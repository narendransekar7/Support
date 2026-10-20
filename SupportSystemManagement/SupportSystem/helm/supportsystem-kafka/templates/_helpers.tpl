{{/* Common labels for every resource of this chart. */}}
{{- define "supportsystem-kafka.labels" -}}
app.kubernetes.io/part-of: supportsystem
app.kubernetes.io/component: kafka
app.kubernetes.io/managed-by: {{ .Release.Service }}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version }}
{{- end -}}

{{/* The label Strimzi's entity operators use to pick up KafkaTopic/KafkaUser/KafkaNodePool resources. */}}
{{- define "supportsystem-kafka.clusterLabel" -}}
strimzi.io/cluster: {{ .Values.clusterName }}
{{- end -}}

{{/* "<prefix>.<name>", e.g. support.ticket-created */}}
{{- define "supportsystem-kafka.topic" -}}
{{ .root.Values.topics.prefix }}.{{ .name }}
{{- end -}}
