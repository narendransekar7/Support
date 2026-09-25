# Kubernetes manifests

Equivalent of [`docker-compose.yml`](../docker-compose.yml) for a Kubernetes cluster. Kubernetes
can't build images from a Dockerfile the way `docker-compose up --build` does, so images must be
built and pushed to a registry first.

## 1. Build and push images

```bash
docker build -t narendransekar/ss-auth-server-api:latest ./SS.Auth.Server.API
docker build -f SS.User.API/Dockerfile -t narendransekar/ss-user-api:latest .
docker build -f SS.Ticket.API/Dockerfile -t narendransekar/ss-ticket-api:latest .
docker build -f SS.Email.API/SS.Email.API/Dockerfile -t narendransekar/ss-email-api:latest .
docker build -t narendransekar/ss-gateway-api:latest ./SS.Gateway.API
docker build --build-arg REACT_APP_GATEWAY_URL=http://localhost:5145 -t narendransekar/ss-react-ui:latest ./supportsystem.reactui

docker push narendransekar/ss-auth-server-api:latest
docker push narendransekar/ss-user-api:latest
docker push narendransekar/ss-ticket-api:latest
docker push narendransekar/ss-email-api:latest
docker push narendransekar/ss-gateway-api:latest
docker push narendransekar/ss-react-ui:latest
```

Replace `narendransekar` with your own registry/repo if needed - just keep the image names in
`supportsystem.yaml` in sync.

## 2. Apply the manifests

```bash
kubectl apply -f k8s/supportsystem.yaml
```

This creates everything in the `supportsystem` namespace: a `ConfigMap`/`Secret` for the env vars
that were hardcoded in `docker-compose.yml`, and a `Deployment` + `Service` per compose service.

## 3. Access the app

- `ss-gateway-api` and `ss-react-ui` are `LoadBalancer` Services (compose's `5145:8080` and
  `3000:80` host port mappings). On a cloud cluster they get an external IP; on `minikube` run
  `minikube tunnel` or `minikube service -n supportsystem ss-react-ui`; on `kind`, port-forward
  instead:
  ```bash
  kubectl -n supportsystem port-forward svc/ss-react-ui 3000:3000
  kubectl -n supportsystem port-forward svc/ss-gateway-api 5145:5145
  ```
- All other services (`rabbitmq`, `ss-auth-server-api`, `ss-user-api`, `ss-ticket-api`,
  `ss-email-api`) are `ClusterIP` (internal only), matching how the gateway/react-ui talk to them
  in `ocelot.Docker.json` - those routes already use the plain service names (`ss-user-api`,
  `ss-auth-server-api`, etc.), which is exactly what Kubernetes DNS resolves inside the cluster, so
  no changes were needed there.

## Notes / deliberate differences from docker-compose.yml

- **No `depends_on` equivalent**: Kubernetes doesn't guarantee startup order. Pods will restart
  and retry on crash (`CrashLoopBackOff`) until dependencies like `rabbitmq` are ready.
- **`extra_hosts: host.docker.internal:host-gateway`** on `ss-user-api`/`ss-ticket-api` was dropped
  - the connection string points at Azure SQL, not the host machine, so it isn't needed here.
- **Secrets**: `supportsystem-secrets` in the manifest only contains `REPLACE_ME` placeholders so no
  credentials are committed. Before applying, create the real secret with the same values
  `docker-compose.yml` uses (Azure SQL connection string, RabbitMQ user/password):
  ```bash
  kubectl create namespace supportsystem
  kubectl -n supportsystem create secret generic supportsystem-secrets \
    --from-literal=DB_CONNECTION_STRING='...' \
    --from-literal=RABBITMQ_USERNAME='...' \
    --from-literal=RABBITMQ_PASSWORD='...'
  ```
  then delete the `Secret` block from `supportsystem.yaml` (or `kubectl apply` will overwrite it
  with the placeholders) before running `kubectl apply -f k8s/supportsystem.yaml`.

## Autoscaling

Each app Deployment has a `HorizontalPodAutoscaler` (bottom of `supportsystem.yaml`) that scales on
CPU at 70% of the container's `resources.requests.cpu`:

| Deployment           | min | max |
|----------------------|-----|-----|
| `ss-gateway-api`     | 2   | 10  |
| `ss-auth-server-api` | 2   | 6   |
| `ss-user-api`        | 2   | 6   |
| `ss-ticket-api`      | 2   | 6   |
| `ss-email-api`       | 1   | 3   |
| `ss-react-ui`        | 2   | 5   |

`rabbitmq` is not autoscaled - extra replicas would be separate brokers, not a cluster (see Self-healing below).

- **metrics-server** must be running (it is by default on AKS). Check with `kubectl top pods -n supportsystem`;
  on minikube run `minikube addons enable metrics-server`.
- **Health probes**: every API exposes `/health/live` (liveness/startup, no dependency checks) and
  `/health/ready` (readiness, includes MassTransit's RabbitMQ bus check). **Rebuild and push all
  images (step 1) before applying**, or the probes will fail against old images.
- **Nodes**: HPA only adds pods. On AKS, enable the cluster autoscaler so new nodes are added when
  pods are `Pending`:
  ```bash
  az aks update -g <resource-group> -n <cluster> --enable-cluster-autoscaler --min-count 1 --max-count 5
  ```

Watch it work:
```bash
kubectl get hpa -n supportsystem -w
```

## Self-healing

| Failure | What recovers it |
|---|---|
| Container crashes | kubelet restarts it (`restartPolicy: Always`) |
| API hangs (process up, not responding) | liveness probe on `/health/live` -> container restarted |
| Pod not ready / lost its RabbitMQ connection | readiness probe on `/health/ready` -> removed from the Service until it recovers |
| RabbitMQ Erlang node unresponsive | liveness probe `rabbitmq-diagnostics -q ping` -> container restarted |
| RabbitMQ pod restarted or rescheduled | StatefulSet keeps the hostname (`rabbitmq-0`) and its persistent volume, so durable queues and persistent messages survive |
| Node or zone goes down | Deployments/StatefulSet recreate pods elsewhere; `topologySpreadConstraints` keep replicas on different nodes/zones so the other copy keeps serving; the RabbitMQ disk is zone-redundant (ZRS) so it can re-attach in the other zone |
| AKS upgrade / node drain / scale-in | `PodDisruptionBudget`s (`minAvailable: 1`) stop all replicas of a service being evicted at once |

Notes:
- **One-time migration** - rabbitmq changed from a Deployment to a StatefulSet. After applying,
  delete the old Deployment, otherwise the `rabbitmq` Service load-balances between two separate brokers:
  ```bash
  kubectl -n supportsystem delete deployment rabbitmq
  ```
  Messages on the old (non-persistent) broker are lost at this point, so do it when the queues are empty.
- The `supportsystem-zrs` StorageClass uses `StandardSSD_ZRS` disks. If your region doesn't support
  ZRS managed disks, change `skuName` to `StandardSSD_LRS` (the broker then can't move across zones).
  It uses `reclaimPolicy: Retain`, so the Azure disk is kept even after the PVC is deleted - remove it
  manually in the portal if you tear the environment down.
- RabbitMQ is still a single broker: while it restarts (~30-60s), publishers get errors and API pods
  go not-ready. For zero-downtime messaging, run a 3-node cluster with quorum queues (e.g. the
  [RabbitMQ Cluster Operator](https://www.rabbitmq.com/kubernetes/operator/operator-overview)) or a managed broker.
