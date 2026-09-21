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
