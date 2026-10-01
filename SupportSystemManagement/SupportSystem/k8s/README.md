# Kubernetes manifests

Equivalent of [`docker-compose.yml`](../docker-compose.yml) for a Kubernetes cluster. Kubernetes
can't build images from a Dockerfile the way `docker-compose up --build` does, so images must be
built and pushed to a registry first.

## 1. Build and push images

From this folder's parent (`SupportSystemManagement/SupportSystem`, where `docker-compose.yml` is),
build and push all six images with the names/build contexts already defined in compose:

```bash
docker compose build ss-auth-server-api ss-user-api ss-ticket-api ss-email-api ss-gateway-api ss-react-ui
docker compose push ss-auth-server-api ss-user-api ss-ticket-api ss-email-api ss-gateway-api ss-react-ui
```

Every .NET image is built from the solution folder (`context: .`) because the APIs reference the
shared `SS.Base.*` projects. The React UI needs no build args: it calls `/api` on its own origin,
which the Ingress routes to the gateway.

Replace `narendransekar` with your own registry/repo if needed - just keep the image names in
`docker-compose.yml` and `supportsystem.yaml` in sync.

## 2. Install the ingress controller (once per cluster)

The `supportsystem-ingress` uses `ingressClassName: webapprouting.kubernetes.azure.com`, the class of
the AKS **application routing** add-on (Microsoft-managed NGINX). Enable it once:

```bash
az aks approuting enable -g <resource-group> -n <cluster>
kubectl get ingressclass   # should list webapprouting.kubernetes.azure.com
```

(It can also be ticked in the portal when creating the cluster - Networking tab.) Off AKS, e.g. on
minikube (`minikube addons enable ingress`) or with community ingress-nginx, change
`ingressClassName` back to `nginx`.

## 3. Apply the manifests

```bash
kubectl apply -f k8s/supportsystem.yaml
```

This creates everything in the `supportsystem` namespace: a `ConfigMap`/`Secret` for the env vars
that were hardcoded in `docker-compose.yml`, a `Deployment` + `Service` per compose service, and the
`Ingress`. Create the real secret first - see **Secrets** below.

## 4. Access the app

Every Service is `ClusterIP` (internal only). The single public entry point is the Ingress, which
gets one public IP from the controller's Azure load balancer:

| Path | Routed to |
|---|---|
| `/api/...` | `ss-gateway-api:8080` (Ocelot, which forwards to the other APIs) |
| everything else | `ss-react-ui:80` |

```bash
kubectl get ingress -n supportsystem   # ADDRESS column = public IP (can take a minute or two)
```

Open `http://<ADDRESS>/`. Because the UI and API share that origin, no CORS or gateway URL setup is
needed.

Without an ingress controller (e.g. `kind`), port-forward the UI and gateway instead - the UI then
calls `/api` on `localhost:3000`, so use the gateway directly for API testing:

```bash
kubectl -n supportsystem port-forward svc/ss-react-ui 3000:80
kubectl -n supportsystem port-forward svc/ss-gateway-api 5145:8080
```

Inside the cluster, the gateway and APIs reach each other by Service name (`ss-user-api`,
`ss-auth-server-api`, ...), which is what `ocelot.Docker.json` already uses.

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
    --from-literal=RABBITMQ_PASSWORD='...' \
    --from-literal=APPLICATIONINSIGHTS_CONNECTION_STRING='InstrumentationKey=...;IngestionEndpoint=...'
  ```
  then delete the `Secret` block from `supportsystem.yaml` (or `kubectl apply` will overwrite it
  with the placeholders) before running `kubectl apply -f k8s/supportsystem.yaml`.

## Logging & monitoring

Every .NET service logs to stdout; `LOG_CONSOLE_FORMAT: "json"` in the ConfigMap makes each entry one
structured JSON line (with `TraceId`/`SpanId` scopes), and `LOG_LEVEL_DEFAULT` sets the minimum level
without rebuilding images:

```bash
kubectl -n supportsystem logs deploy/ss-ticket-api -f
```

When the `APPLICATIONINSIGHTS_CONNECTION_STRING` secret key is set, all services (and the React UI)
also export logs, distributed traces and metrics via OpenTelemetry to Azure Application Insights -
see the "Observability" section of `ARCHITECTURE.md`. After changing the secret, restart the pods so
they pick it up:

```bash
kubectl -n supportsystem rollout restart deploy
```

## Metrics: Prometheus + Grafana (Azure managed)

Every API serves Prometheus metrics at `/metrics` on a **separate port 9464** (the `metrics` container
port; the app stays on 8080). No Service exposes 9464, so metrics can't be read through the public
Ingress. RabbitMQ exposes its built-in metrics on 15692 (`prometheus` port).

What you get:

| Source | Examples |
|---|---|
| ASP.NET Core / Kestrel | `http_server_request_duration_seconds` (rate, latency, status codes per route), `http_server_active_requests`, `kestrel_active_connections` |
| HttpClient | `http_client_request_duration_seconds` (Gateway/Ocelot → downstream, Auth → User API) |
| .NET runtime | `process_runtime_dotnet_gc_*`, `..._thread_pool_*`, `..._exceptions_count` |
| MassTransit | publish/send/consume counts and durations (once messages flow) |
| RabbitMQ | `rabbitmq_queue_messages_ready`, publish/deliver rates, connections |
| Cluster (from the add-on itself) | node/pod CPU & memory, restarts, HPA replicas, kube-state-metrics |

### 1. Create the Azure resources (once)

```bash
az monitor account create -g <resource-group> -n <amw-name> -l <location>          # Azure Monitor workspace (stores Prometheus data)
az grafana create -g <resource-group> -n <grafana-name>                            # Azure Managed Grafana
```

### 2. Enable managed Prometheus on the cluster and link Grafana

```bash
az aks update -g <resource-group> -n <cluster> --enable-azure-monitor-metrics \
  --azure-monitor-workspace-resource-id $(az monitor account show -g <resource-group> -n <amw-name> --query id -o tsv) \
  --grafana-resource-id $(az grafana show -g <resource-group> -n <grafana-name> --query id -o tsv)
```

This installs the `ama-metrics` pods in `kube-system`, adds the Prometheus data source to Grafana and
creates the built-in Kubernetes dashboards (Grafana → Dashboards → *Azure Managed Prometheus*).

### 3. Scrape the Support System pods

After the images are rebuilt/pushed (step 1) and `supportsystem.yaml` is applied:

```bash
kubectl apply -f k8s/monitoring.yaml
```

`monitoring.yaml` holds `PodMonitor`s for the APIs and RabbitMQ. It's a separate file because its CRD
only exists once step 2 is done. Metrics show up in Grafana within a couple of minutes, with
`job` = service name (`ss-ticket-api`, ...) and `pod` = replica.

Check an endpoint by hand:

```bash
kubectl -n supportsystem port-forward deploy/ss-ticket-api 9464:9464
curl http://localhost:9464/metrics
```

### 4. Dashboards

Grafana → Dashboards → New → Import, pick the Managed Prometheus data source:

- **19924** - ASP.NET Core (request rate, latency, errors, connections per `job`/`instance`)
- **19925** - ASP.NET Core Endpoint (drill-down per route)
- **10991** - RabbitMQ Overview

Useful queries (Explore):

```promql
# requests/s per service
sum by (job) (rate(http_server_request_duration_seconds_count[5m]))
# p95 latency per service
histogram_quantile(0.95, sum by (job, le) (rate(http_server_request_duration_seconds_bucket[5m])))
# 5xx ratio per service
sum by (job) (rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]))
  / sum by (job) (rate(http_server_request_duration_seconds_count[5m]))
# messages waiting in RabbitMQ (all queues - /metrics is aggregated; per-queue needs /metrics/per-object)
sum(rabbitmq_queue_messages_ready)
```

Locally (`dotnet run` / docker-compose) there's no metrics port - `/metrics` is served on the normal
app port, e.g. `http://localhost:<port>/metrics`.

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
| Node or zone goes down | Deployments recreate pods elsewhere; `topologySpreadConstraints` keep replicas on different nodes/zones so the other copy keeps serving meanwhile |
| Cluster upgrade / node drain / scale-in | `PodDisruptionBudget`s (`minAvailable: 1`) stop all replicas of a service being evicted at once |

### Known gap: RabbitMQ is not persistent (future work)

`rabbitmq` is still a single-replica Deployment with no volume and no probes. Kubernetes recreates
it if it crashes, but **any messages still in its queues are lost on restart**, and while it's down
publishing fails and the API pods go not-ready. Planned fix, deliberately deferred for now:

- Run it as a **StatefulSet** (stable hostname `rabbitmq-0` - RabbitMQ names its node and data
  directory after the hostname, so a Deployment + volume would not find its old queues) with a
  `volumeClaimTemplates` volume mounted at `/var/lib/rabbitmq`, `fsGroup: 999` and a 60s
  `terminationGracePeriodSeconds`.
- Probes: `rabbitmq-diagnostics -q ping` for startup/liveness, TCP 5672 for readiness.
- Storage is cluster-specific. On AKS with nodes in several zones use a zone-redundant disk
  (`disk.csi.azure.com`, `skuName: StandardSSD_ZRS`) - the disk is created in the cluster's `MC_...`
  node resource group and billed as a managed disk (5Gi rounds up to the 8 GiB E2 tier). With
  `reclaimPolicy: Retain` it keeps billing after the PVC is deleted until removed manually.
- Switching from the Deployment needs a one-time `kubectl -n supportsystem delete deployment rabbitmq`
  after applying, done while the queues are empty.
- For zero-downtime messaging: a 3-node cluster with quorum queues (e.g. the
  [RabbitMQ Cluster Operator](https://www.rabbitmq.com/kubernetes/operator/operator-overview)) or a managed broker.
