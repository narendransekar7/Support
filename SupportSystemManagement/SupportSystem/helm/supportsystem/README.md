# Support System Helm chart

Helm version of [`k8s/supportsystem.yaml`](../../k8s/supportsystem.yaml) (plus, optionally,
[`k8s/monitoring.yaml`](../../k8s/monitoring.yaml)). Both are maintained: the raw manifest for plain
`kubectl apply`, this chart for parameterised installs/upgrades/rollbacks. **When you change one,
change the other.**

Prerequisites are the same as for the raw manifests - images built and pushed, an ingress controller,
metrics-server (see [`k8s/README.md`](../../k8s/README.md) steps 1-2).

## Install / upgrade

From `SupportSystemManagement/SupportSystem`:

```bash
cp helm/supportsystem/values.secret.example.yaml helm/supportsystem/values.secret.yaml   # then fill it in
helm upgrade --install supportsystem ./helm/supportsystem \
  -n supportsystem --create-namespace \
  -f helm/supportsystem/values.secret.yaml
```

`values.secret.yaml` is git-ignored. Alternatively create the Secret yourself and point the chart at it:

```bash
kubectl -n supportsystem create secret generic supportsystem-secrets \
  --from-literal=DB_CONNECTION_STRING='...' \
  --from-literal=RABBITMQ_USERNAME='...' \
  --from-literal=RABBITMQ_PASSWORD='...' \
  --from-literal=APPLICATIONINSIGHTS_CONNECTION_STRING='...'
helm upgrade --install supportsystem ./helm/supportsystem -n supportsystem \
  --set secrets.existingSecret=supportsystem-secrets
```

Other useful commands:

```bash
helm lint ./helm/supportsystem -f helm/supportsystem/values.secret.yaml
helm template supportsystem ./helm/supportsystem -n supportsystem -f helm/supportsystem/values.secret.yaml   # render without installing
helm diff upgrade ...                 # with the helm-diff plugin
helm history supportsystem -n supportsystem
helm rollback supportsystem <REVISION> -n supportsystem
helm uninstall supportsystem -n supportsystem
```

## Common overrides

| Value | Default | Purpose |
|---|---|---|
| `image.registry` | `narendransekar` | Image prefix (Docker Hub org or `myacr.azurecr.io`) |
| `image.tag` | `latest` | Tag for all app images; per service: `apis.<name>.image.tag`, `reactUi.image.tag` |
| `imagePullSecrets` | `[]` | For a private registry |
| `secrets.existingSecret` | `""` | Use a Secret created outside the chart |
| `config.logLevelDefault` | `Information` | .NET minimum log level |
| `config.corsReactUiOrigin` | `http://localhost:3000` | Extra origin allowed to call the APIs |
| `apis.<name>.autoscaling.*` | see values | HPA min/max/CPU target |
| `apis.<name>.resources` | `apiDefaults.resources` | Per-service requests/limits |
| `ingress.className` | `nginx` | `webapprouting.kubernetes.azure.com` for the AKS add-on |
| `ingress.host` / `ingress.tls` | none | Host name and TLS |
| `rabbitmq.enabled` | `true` | `false` + `config.rabbitmqHost` to use an external broker |
| `monitoring.podMonitors.enabled` | `false` | Managed Prometheus PodMonitors (needs the AKS metrics add-on) |

Example - pin a version and use the AKS application routing add-on:

```bash
helm upgrade --install supportsystem ./helm/supportsystem -n supportsystem \
  -f helm/supportsystem/values.secret.yaml \
  --set image.tag=1.4.0 \
  --set ingress.className=webapprouting.kubernetes.azure.com
```

## Differences from `kubectl apply -f k8s/supportsystem.yaml`

- **No `Namespace` object** - Helm installs into `-n <namespace>` (`--create-namespace` creates it).
- **Pods roll automatically when config changes**: the ConfigMap/Secret checksums are pod annotations,
  so changing a value and upgrading restarts the affected pods (not for `existingSecret`).
- **Resource names are fixed** (`ss-user-api`, `rabbitmq`, `supportsystem-config`, ...), not prefixed
  with the release name, because `ocelot.Docker.json` routes by Service name. Install **one release per
  namespace**.
- **Don't mix both in one namespace.** Helm refuses to adopt resources created by `kubectl apply`.
  To switch an existing namespace to Helm, `kubectl delete -f k8s/supportsystem.yaml` first (or install
  into a new namespace).
