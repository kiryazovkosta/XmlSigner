using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows;
using DigitalSignaturesClient.Middleware;
using DigitalSignaturesClient.Services;
using DigitalSignaturesClient.Services.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Forms = System.Windows.Forms;
using WpfApplication = System.Windows.Application;

namespace DigitalSignaturesClient;

public partial class App : WpfApplication
{
    private WebApplication? _webApp;
    private Forms.NotifyIcon? _trayIcon;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        SetupSerilog();
        ShowStartupInfo();
        SetupTrayIcon();
        await StartWebApiAsync();
    }

    private void SetupSerilog()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, "logs", "xml-signer-.log"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();
    }

    private void ShowStartupInfo()
    {
        System.Windows.MessageBox.Show(
            "Digital Signatures Client is starting...\n\n" +
            "API Endpoint: http://localhost:6050\n" +
            "Swagger UI: http://localhost:6050/swagger/index.html\n\n" +
            "The application will run in the system tray.",
            "Digital Signatures Client",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void SetupTrayIcon()
    {
        _trayIcon = new Forms.NotifyIcon
        {
            Icon = LoadIcon(),
            Visible = true,
            Text = "Digital Sign Client"
        };

        var contextMenu = new Forms.ContextMenuStrip();

        var statusItem = new Forms.ToolStripMenuItem("Running on port 6050")
        {
            Enabled = false
        };
        contextMenu.Items.Add(statusItem);

        var logsItem = new Forms.ToolStripMenuItem("Logs files");
        logsItem.Click += (_, _) => ShowLogsFilesFolder();
        contextMenu.Items.Add(logsItem);

        contextMenu.Items.Add(new Forms.ToolStripSeparator());

        var exitItem = new Forms.ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) => Shutdown();
        contextMenu.Items.Add(exitItem);

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.DoubleClick += (_, _) => ShowBalloonTip();
    }

    private Icon LoadIcon()
    {
        var iconPath = Path.Combine(AppContext.BaseDirectory, "Public", "app.ico");
        if (File.Exists(iconPath))
        {
            return new Icon(iconPath);
        }

        return SystemIcons.Application;
    }

    private void ShowLogsFilesFolder()
    {
        Log.Error(Directory.GetCurrentDirectory());
        Log.Error(AppContext.BaseDirectory);
        string logFilesFolder = Path.Combine(AppContext.BaseDirectory, "logs");
        Log.Error(logFilesFolder);
        Process.Start("explorer.exe", logFilesFolder);
    }

    private void ShowBalloonTip()
    {
        _trayIcon?.ShowBalloonTip(
            3000,
            "Digital Sign Client",
            "API running on http://localhost:6050",
            Forms.ToolTipIcon.Info);
    }

    private async Task StartWebApiAsync()
    {
        try
        {
            var builder = WebApplication.CreateBuilder();

            builder.Host.UseSerilog();

            builder.Services.AddScoped<IX509CertificateService, X509CertificateService>();
            builder.Services.AddScoped<ISignVerifyEnvelope, SignVerifyEnvelope>();
            builder.Services.AddScoped<IBase64StringService, Base64StringService>();

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

            _webApp = builder.Build();

            _webApp.UseSwagger();
            _webApp.UseSwaggerUI();
            _webApp.UseCors("AllowAll");
            _webApp.MapControllers();
            _webApp.UseMiddleware<LogPostRequestsMiddleware>();

            Log.Information("Starting Web API on port 6050...");

            await _webApp.RunAsync("http://localhost:6050");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start Web API");
            Forms.MessageBox.Show(
                $"Failed to start Web API: {ex.Message}",
                "Error",
                Forms.MessageBoxButtons.OK,
                Forms.MessageBoxIcon.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Shutting down...");

        if (_webApp != null)
        {
            await _webApp.StopAsync();
            await _webApp.DisposeAsync();
        }

        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
        }

        Log.CloseAndFlush();

        base.OnExit(e);
    }
}
