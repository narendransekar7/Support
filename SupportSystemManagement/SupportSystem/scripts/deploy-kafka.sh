#!/usr/bin/env bash
# Installs/upgrades the Strimzi operator and the Support System's Kafka release (helm/supportsystem-kafka), then
# waits until the Kafka cluster, its topics and its users are Ready - so the app release that follows finds the
# per-service password Secrets and the CA Secret it needs. Used by the Jenkins and Azure DevOps deploys.
#
#   scripts/deploy-kafka.sh <namespace> [extra helm args for the Kafka chart, e.g. -f helm/supportsystem-kafka/values.staging.yaml]
#
# Environment (all optional):
#   STRIMZI_VERSION           Strimzi operator version (default 1.2.0 - Kafka 4.2/4.3, CRD API kafka.strimzi.io/v1)
#   STRIMZI_NAMESPACE         where the operator runs (default kafka-operator)
#   STRIMZI_WATCH_NAMESPACES  namespaces it manages, comma-separated (default: <namespace>)
#   KAFKA_RELEASE / KAFKA_CHART / KAFKA_CLUSTER   (defaults supportsystem-kafka / helm/supportsystem-kafka / support-kafka)
#
# Run from SupportSystemManagement/SupportSystem with kubectl/helm pointed at the cluster.
set -euo pipefail

NAMESPACE="${1:?usage: scripts/deploy-kafka.sh <namespace> [helm args...]}"
shift
STRIMZI_VERSION="${STRIMZI_VERSION:-1.2.0}"
STRIMZI_NAMESPACE="${STRIMZI_NAMESPACE:-kafka-operator}"
STRIMZI_WATCH_NAMESPACES="${STRIMZI_WATCH_NAMESPACES:-$NAMESPACE}"
KAFKA_RELEASE="${KAFKA_RELEASE:-supportsystem-kafka}"
KAFKA_CHART="${KAFKA_CHART:-helm/supportsystem-kafka}"
KAFKA_CLUSTER="${KAFKA_CLUSTER:-support-kafka}"
OPERATOR_CHART="oci://quay.io/strimzi-helm/strimzi-kafka-operator"

echo "== Strimzi CRDs ($STRIMZI_VERSION)"
# Helm installs a chart's CRDs only on the first install and never upgrades them, so apply them every time
# (server-side: the Kafka CRD is too large for client-side apply's annotation).
helm show crds "$OPERATOR_CHART" --version "$STRIMZI_VERSION" | kubectl apply --server-side --force-conflicts -f -

echo "== Strimzi operator -> namespace $STRIMZI_NAMESPACE, watching $STRIMZI_WATCH_NAMESPACES"
# The operator chart creates RoleBindings in every watched namespace, so they must all exist first
# (on a new cluster e.g. "supportsystem" when only staging is being deployed).
for ns in $(echo "$NAMESPACE,$STRIMZI_WATCH_NAMESPACES" | tr ',' ' ' | xargs -n1 | sort -u); do
  kubectl create namespace "$ns" --dry-run=client -o yaml | kubectl apply -f -
done
# A first install that failed leaves a release Helm can't upgrade ("has no deployed releases") - clear it.
if helm -n "$STRIMZI_NAMESPACE" status strimzi-kafka-operator 2>/dev/null | grep -q "^STATUS: failed"; then
  echo "Removing the previous failed operator install"
  helm -n "$STRIMZI_NAMESPACE" uninstall strimzi-kafka-operator --wait
fi
# --skip-crds: the CRDs were applied above (and must stay managed in one place, or the two field managers conflict).
helm upgrade --install strimzi-kafka-operator "$OPERATOR_CHART" \
  --version "$STRIMZI_VERSION" --skip-crds \
  -n "$STRIMZI_NAMESPACE" --create-namespace \
  --set "watchNamespaces={$STRIMZI_WATCH_NAMESPACES}" \
  --wait --timeout 10m

echo "== Kafka release $KAFKA_RELEASE -> namespace $NAMESPACE"
# No --wait here: Kafka UI only becomes ready once Strimzi has created the cluster and its user - waited for below.
helm upgrade --install "$KAFKA_RELEASE" "$KAFKA_CHART" -n "$NAMESPACE" "$@"

echo "== Waiting for Kafka cluster $KAFKA_CLUSTER (first install takes a few minutes)"
kubectl -n "$NAMESPACE" wait "kafka/$KAFKA_CLUSTER" --for=condition=Ready --timeout=20m
kubectl -n "$NAMESPACE" wait kafkatopic -l "strimzi.io/cluster=$KAFKA_CLUSTER" --for=condition=Ready --timeout=10m
kubectl -n "$NAMESPACE" wait kafkauser -l "strimzi.io/cluster=$KAFKA_CLUSTER" --for=condition=Ready --timeout=10m

if kubectl -n "$NAMESPACE" get deploy "$KAFKA_CLUSTER-ui" >/dev/null 2>&1; then
  kubectl -n "$NAMESPACE" rollout status "deploy/$KAFKA_CLUSTER-ui" --timeout=5m || echo "warning: Kafka UI not ready (not fatal)"
fi

kubectl -n "$NAMESPACE" get kafka,kafkanodepool
kubectl -n "$NAMESPACE" get pods -l "strimzi.io/cluster=$KAFKA_CLUSTER" -o wide
