{{/*
Chart name and version, as used by the helm.sh/chart label.
*/}}
{{- define "supportsystem.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Labels common to every resource.
*/}}
{{- define "supportsystem.labels" -}}
helm.sh/chart: {{ include "supportsystem.chart" . }}
app.kubernetes.io/part-of: supportsystem
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end -}}

{{/*
Labels for one component. Call with (dict "root" $ "name" "<component>").
`app` is the only selector label - same as k8s/supportsystem.yaml, and what monitoring.yaml selects on.
*/}}
{{- define "supportsystem.componentLabels" -}}
app: {{ .name }}
app.kubernetes.io/name: {{ .name }}
{{ include "supportsystem.labels" .root }}
{{- end -}}

{{/*
Full image reference. Call with (dict "root" $ "image" <svc.image>).
*/}}
{{- define "supportsystem.image" -}}
{{- $tag := toString (default .root.Values.image.tag .image.tag) -}}
{{- if .root.Values.image.registry -}}
{{- printf "%s/%s:%s" (trimSuffix "/" .root.Values.image.registry) .image.repository $tag -}}
{{- else -}}
{{- printf "%s:%s" .image.repository $tag -}}
{{- end -}}
{{- end -}}

{{- define "supportsystem.configMapName" -}}supportsystem-config{{- end -}}

{{- define "supportsystem.secretName" -}}
{{- default "supportsystem-secrets" .Values.secrets.existingSecret -}}
{{- end -}}

{{/*
Pod annotations that roll the pods when the ConfigMap or chart-managed Secret changes
(env vars from them are only read at container start).
*/}}
{{- define "supportsystem.podChecksums" -}}
checksum/config: {{ include (print .Template.BasePath "/configmap.yaml") . | sha256sum }}
checksum/secret: {{ include (print .Template.BasePath "/secret.yaml") . | sha256sum }}
{{- end -}}

{{/*
topologySpreadConstraints for one component. Call with (dict "root" $ "name" "<component>").
*/}}
{{- define "supportsystem.topologySpread" -}}
{{- if .root.Values.topologySpread.enabled }}
topologySpreadConstraints:
{{- range .root.Values.topologySpread.topologyKeys }}
  - maxSkew: 1
    topologyKey: {{ . }}
    whenUnsatisfiable: ScheduleAnyway
    labelSelector:
      matchLabels:
        app: {{ $.name }}
{{- end }}
{{- end }}
{{- end -}}

{{/*
HorizontalPodAutoscaler + PodDisruptionBudget for one component.
Call with (dict "root" $ "name" "<component>" "svc" <values of the component>).
*/}}
{{- define "supportsystem.scaling" -}}
{{- if .svc.autoscaling.enabled }}
---
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: {{ .name }}
  labels:
    {{- include "supportsystem.componentLabels" (dict "root" .root "name" .name) | nindent 4 }}
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: {{ .name }}
  minReplicas: {{ .svc.autoscaling.minReplicas }}
  maxReplicas: {{ .svc.autoscaling.maxReplicas }}
  metrics:
    - type: Resource
      resource:
        name: cpu
        target:
          type: Utilization
          averageUtilization: {{ .svc.autoscaling.targetCPUUtilizationPercentage }}
  {{- with .root.Values.autoscalingBehavior }}
  behavior:
    {{- toYaml . | nindent 4 }}
  {{- end }}
{{- end }}
{{- if .svc.pdb.enabled }}
---
apiVersion: policy/v1
kind: PodDisruptionBudget
metadata:
  name: {{ .name }}
  labels:
    {{- include "supportsystem.componentLabels" (dict "root" .root "name" .name) | nindent 4 }}
spec:
  minAvailable: 1
  selector:
    matchLabels:
      app: {{ .name }}
{{- end }}
{{- end -}}
