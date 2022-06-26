using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using VaultAdmissionWebHook.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.All;
    // logging.RequestHeaders.Add("sec-ch-ua");
    // logging.ResponseHeaders.Add("MyResponseHeader");
    // logging.MediaTypeOptions.AddText("application/javascript");
    // logging.RequestBodyLogLimit = 4096;
    // logging.ResponseBodyLogLimit = 4096;

});

builder.Services.AddHttpClient();

X509Certificate2? x509Certificate2 = null;
builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        // for local tests
        // var certPath = Path.Combine(builder.Environment.ContentRootPath, "cert", "tls.crt");
        // var keyPath = Path.Combine(builder.Environment.ContentRootPath, "cert", "tls.key");
        
        var certPath = builder.Configuration.GetValue<string>("TlsCertPath");
        var keyPath = builder.Configuration.GetValue<string>("TlsKeyPath");
        
        // httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        if (certPath != null && keyPath != null) 
            x509Certificate2 = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        httpsOptions.ServerCertificate = x509Certificate2;
    });
});

// Add services to the container.
builder.Services.Configure<VOptions>(builder.Configuration);
var healthCheckActive = builder.Configuration.GetValue<bool>("HealthCheck:IsActive");

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//builder.Services.AddHealthChecks();
builder.Services.AddHealthChecks()
    .AddCheck("Sample", () =>
    {
        if (healthCheckActive)
        {
            if (x509Certificate2 != null && x509Certificate2.NotAfter < DateTime.Now)
            {
                Console.WriteLine("...Certificate is expired...");
                Environment.Exit(0);
            }
        }
        
        return HealthCheckResult.Healthy("A healthy result.");
    });

var app = builder.Build();

app.UseHttpLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();