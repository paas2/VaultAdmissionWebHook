using Newtonsoft.Json;

namespace VaultAdmissionWebHook.Models;

public class KubernetesRoleRequest {
    [JsonProperty("bound_service_account_names")]
    public string? BoundServiceAccountNames { get; set; }

    [JsonProperty("bound_service_account_namespaces")]
    public string? BoundServiceAccountNamespaces { get; set; }

    [JsonProperty("policies")]
    public string[]? Policies { get; set; }

    [JsonProperty("max_ttl")]
    public long MaxTtl { get; set; }
}