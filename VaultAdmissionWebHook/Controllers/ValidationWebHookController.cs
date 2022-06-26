using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using VaultAdmissionWebHook.Models;
using VaultAdmissionWebHook.Options;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Kubernetes;
using VaultSharp.V1.SystemBackend;

namespace VaultAdmissionWebHook.Controllers;

[ApiController]
[Route("[controller]")]
public class ValidationWebHookController : ControllerBase
{
    private readonly ILogger<ValidationWebHookController> _logger;
    private readonly VaultClient _vaultClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptions<VOptions> _options;

    public ValidationWebHookController(ILogger<ValidationWebHookController> logger, IOptions<VOptions> options, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = options;

        var authMethod = 
            string.IsNullOrEmpty(options.Value.Vault.AuthMountPoint) ? 
                new KubernetesAuthMethodInfo(options.Value.Vault.RoleName, options.Value.Vault.Token) : 
                new KubernetesAuthMethodInfo(options.Value.Vault.AuthMountPoint, options.Value.Vault.RoleName, options.Value.Vault.Token);
        
        var vaultClientSettings = new VaultClientSettings(options.Value.Vault.Server, authMethod);

        _vaultClient = new VaultClient(vaultClientSettings);
    }

    [HttpPost("Validate")]
    [Produces(typeof(AdmissionReviewResponse))]
    public async Task<ActionResult> Validate([FromBody] AdmissionReviewRequest admissionReviewRequest)
    {
        var response = new AdmissionReviewResponse()
        {
            Response = new Response()
            {
                Uid = admissionReviewRequest.Request.Uid,
                Allowed = true
            }
        };

        // Console.WriteLine("AdmissionReviewRequest: " + JsonConvert.SerializeObject(admissionReviewRequest));
        
        switch(admissionReviewRequest.Request.Operation)
        {
            case "CREATE":
                await CreatePolicyAndAppRole(admissionReviewRequest.Request);
                break;
            case "UPDATE":
                await DeletePolicyAndAppRole(admissionReviewRequest.Request);
                await CreatePolicyAndAppRole(admissionReviewRequest.Request);
                break;
            case "DELETE":
                await DeletePolicyAndAppRole(admissionReviewRequest.Request);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return Ok(response);
    }

    private async Task CreatePolicyAndAppRole(Request request)
    {
        var newObject = request.Object;
        
        foreach (var accessDefinition in newObject.Spec.AccessDefinitions)
        {
            var policy = new ACLPolicy
            {
                Name = $"{_options.Value.SahabEnvironment}-{request.Name}-{accessDefinition.ServiceAccountName}-{accessDefinition.ServiceAccountNamespace}",
                Policy = $"path \"{accessDefinition.SecretPath}\" {{capabilities = [\"read\"]}}"
            };

            // create read policy
            await _vaultClient.V1.System.WriteACLPolicyAsync(policy);
            
            // create app role
            await CreateAppRole(accessDefinition.ServiceAccountName, accessDefinition.ServiceAccountNamespace, policy.Name);
        }
    }

    private async Task CreateAppRole(string serviceAccountName, string serviceAccountNamespace, string policyName)
    {
        var uri = $"{_vaultClient.Settings.VaultServerUriWithPort}/v1/auth/{_options.Value.Vault.AuthMountPoint}/role/{serviceAccountName}-{serviceAccountNamespace}";
        var tokenInfo = await _vaultClient.V1.Auth.Token.LookupSelfAsync();

        var kubernetesRoleRequest = new KubernetesRoleRequest()
        {
            BoundServiceAccountNames = serviceAccountName,
            BoundServiceAccountNamespaces = serviceAccountNamespace,
            Policies = new[] {policyName},
            MaxTtl = 1800000
        };
        
        var content = new StringContent(
            JsonConvert.SerializeObject(kubernetesRoleRequest),
            Encoding.UTF8,
            MediaTypeNames.Application.Json);
          
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Headers =
            {
                { HeaderNames.UserAgent, "VaultAdmissionWebHook" },
                { "X-Vault-Token", tokenInfo.Data.Id }
            },
            Content = content
        };
        
        var httpClient = _httpClientFactory.CreateClient();
        await httpClient.SendAsync(httpRequestMessage);
    }

    private async Task DeletePolicyAndAppRole(Request request)
    {
        var oldObject = request.OldObject;
        
        foreach (var accessDefinition in oldObject.Spec.AccessDefinitions)
        {
            var name = $"{_options.Value.SahabEnvironment}-{request.Name}-{accessDefinition.ServiceAccountName}-{accessDefinition.ServiceAccountNamespace}";
            
            // delete read policy
            await _vaultClient.V1.System.DeleteACLPolicyAsync(name);
            
            // delete app role
            await DeleteAppRole(accessDefinition.ServiceAccountName, accessDefinition.ServiceAccountNamespace, name);
        }
    }
    
    private async Task DeleteAppRole(string serviceAccountName, string serviceAccountNamespace, string policyName)
    {
        var uri = $"{_vaultClient.Settings.VaultServerUriWithPort}/v1/auth/{_options.Value.Vault.AuthMountPoint}/role/{serviceAccountName}-{serviceAccountNamespace}";
        var tokenInfo = await _vaultClient.V1.Auth.Token.LookupSelfAsync();

        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri)
        {
            Headers =
            {
                { HeaderNames.UserAgent, "VaultAdmissionWebHook" },
                { "X-Vault-Token", tokenInfo.Data.Id }
            }
        };
        
        var httpClient = _httpClientFactory.CreateClient();
        await httpClient.SendAsync(httpRequestMessage);
    }
 }