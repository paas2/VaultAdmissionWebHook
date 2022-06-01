using Microsoft.AspNetCore.HttpLogging;
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

// builder.WebHost.ConfigureKestrel(options =>
// {
//     options.ConfigureHttpsDefaults(httpsOptions =>
//     {
//         var certPath = Path.Combine(builder.Environment.ContentRootPath, "cert", "tls.crt");
//         var keyPath = Path.Combine(builder.Environment.ContentRootPath, "cert", "tls.key");
//
//         httpsOptions.ServerCertificate = X509Certificate2.CreateFromPemFile(certPath, keyPath);
//     });
// });

// Add services to the container.
builder.Services.Configure<VOptions>(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

app.Run();