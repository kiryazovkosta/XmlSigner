using Microsoft.AspNetCore.Builder;
using DigitalSignaturesClient;
using DigitalSignaturesClient.Services.Contracts;
using DigitalSignaturesClient.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IX509CertificateService, X509CertificateService>();
builder.Services.AddScoped<ISignVerifyEnvelope, SignVerifyEnvelope>();
builder.Services.AddScoped<IBase64StringService, Base64StringService>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddWindowsService();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var host = builder.Build();

if (host.Environment.IsDevelopment())
{
    host.UseSwagger();
    host.UseSwaggerUI();
}

host.MapControllers();
host.Run();
