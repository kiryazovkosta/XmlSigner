using Microsoft.AspNetCore.Builder;
using Serilog;
using DigitalSignaturesClient;
using DigitalSignaturesClient.Services.Contracts;
using DigitalSignaturesClient.Services;
using System.Runtime.InteropServices;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File(Path.Combine(AppContext.BaseDirectory, "logs", "xml-signer-.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddScoped<IX509CertificateService, X509CertificateService>();
builder.Services.AddScoped<ISignVerifyEnvelope, SignVerifyEnvelope>();
builder.Services.AddScoped<IBase64StringService, Base64StringService>();
builder.Services.AddHostedService<Worker>();

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "Digital Sign Client";
    });
}

builder.Services.AddControllers().AddXmlDataContractSerializerFormatters();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", configurePolicy =>
    {
        configurePolicy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.MapControllers();
app.Run();