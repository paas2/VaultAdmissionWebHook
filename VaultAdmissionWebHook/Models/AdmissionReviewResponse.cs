namespace VaultAdmissionWebHook.Models;

public class AdmissionReviewResponse
{
    public string ApiVersion { get; set; } = "admission.k8s.io/v1";
    public string Kind { get; set; } = "AdmissionReview";
    public Response Response { get; set; }
}

public class Response
{
    public string Uid { get; set; }
    public bool Allowed { get; set; }
}

