#!/usr/bin/env bash
# One-time AKS setup for the production Kafka cluster: a dedicated node pool with one VM per availability zone,
# tainted so only the Kafka brokers (helm/supportsystem-kafka scheduling.*) run there.
#
#   RESOURCE_GROUP=<rg> AKS_CLUSTER=<cluster> scripts/aks-kafka-nodepool.sh
#
# Needs the Azure CLI, logged in with rights on the cluster. Zones must be available in the cluster's region
# (Central India, East US, West Europe, ... - check: az vm list-skus -l <region> --size Standard_D4s_v5 --zone).
# Cost: 3 x Standard_D4s_v5 (4 vCPU / 16 GiB) + 3 x 100 GiB Premium SSD ZRS disks (created by the chart).
set -euo pipefail

RESOURCE_GROUP="${RESOURCE_GROUP:?set RESOURCE_GROUP}"
AKS_CLUSTER="${AKS_CLUSTER:?set AKS_CLUSTER}"
POOL="${KAFKA_NODE_POOL:-kafkapool}"
VM_SIZE="${KAFKA_NODE_VM_SIZE:-Standard_D4s_v5}"

az aks show -g "$RESOURCE_GROUP" -n "$AKS_CLUSTER" --query "{location:location, kubernetesVersion:kubernetesVersion}" -o table

if az aks nodepool show -g "$RESOURCE_GROUP" --cluster-name "$AKS_CLUSTER" -n "$POOL" >/dev/null 2>&1; then
  echo "Node pool $POOL already exists:"
  az aks nodepool show -g "$RESOURCE_GROUP" --cluster-name "$AKS_CLUSTER" -n "$POOL" \
    --query "{count:count, vmSize:vmSize, zones:availabilityZones, taints:nodeTaints, labels:nodeLabels}" -o json
  exit 0
fi

az aks nodepool add \
  -g "$RESOURCE_GROUP" --cluster-name "$AKS_CLUSTER" -n "$POOL" \
  --mode User \
  --node-count 3 \
  --zones 1 2 3 \
  --node-vm-size "$VM_SIZE" \
  --labels workload=kafka \
  --node-taints dedicated=kafka:NoSchedule \
  --max-pods 30

kubectl get nodes -l workload=kafka -L topology.kubernetes.io/zone
