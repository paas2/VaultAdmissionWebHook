namespace VaultAdmissionWebHook.Options;

public class VOptions
{
    public VaultOptions Vault { get; set; }
    public HealthCheckOptions HealthCheck { get; set; }
    public string SahabEnvironment{ get; set; }
}