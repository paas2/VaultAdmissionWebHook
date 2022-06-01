namespace VaultAdmissionWebHook.Models;

public class AdmissionReviewRequest
{
    public string ApiVersion { get; set; }
    public string Kind { get; set; }
    public Request Request { get; set; }
}

public class Request
{
    public string Uid { get; set; }
    public KindObject Kind { get; set; }
    public ResourceObject Resource { get; set; }
    public RequestKind RequestKind { get; set; }
    public RequestResource RequestResource { get; set; }
    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Operation { get; set; }
    public UserInfo UserInfo { get; set; }
    public Object? Object { get; set; }
    public Object? OldObject { get; set; }
    public bool DryRun { get; set; }
    public Options Options { get; set; }
}

public class RequestKind
{
    public string group { get; set; }
    public string version { get; set; }
    public string kind { get; set; }
}

public class RequestResource
{
    public string Group { get; set; }
    public string Version { get; set; }
    public string Resource { get; set; }
}

public class ResourceObject
{
    public string Group { get; set; }
    public string Version { get; set; }
    public string Resource { get; set; }
}

public class Root
{
    public string Kind { get; set; }
    public string ApiVersion { get; set; }
    public Request Request { get; set; }
}

public class Spec
{
    public List<AccessDefinition> AccessDefinitions { get; set; }
}

public class UserInfo
{
    public string Username { get; set; }
    public List<string> Groups { get; set; }
}

public class AccessDefinition
{
    public string SecretPath { get; set; }
    public string ServiceAccountName { get; set; }
    public string ServiceAccountNamespace { get; set; }
}

public class KindObject
{
    public string Group { get; set; }
    public string Version { get; set; }
    public string Kind { get; set; }
}

public class Object
{
    public string ApiVersion { get; set; }
    public string Kind { get; set; }
    public Spec Spec { get; set; }
}

public class Options
{
    public string Kind { get; set; }
    public string ApiVersion { get; set; }
    // public string FieldManager { get; set; }
}