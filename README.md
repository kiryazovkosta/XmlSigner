# Digital Signatures Client

A WPF desktop application that provides digital signature and XML signing capabilities through an embedded ASP.NET Core Web API.

## Overview

**Digital Signatures Client** is a hybrid Windows desktop application that combines:
- **WPF UI**: System tray application for easy access
- **Embedded Web API**: ASP.NET Core service exposing signing functionality via HTTP
- **Digital Signing Services**: X.509 certificate-based XML signing and verification

The application runs as a background service in the Windows system tray and provides REST API endpoints for digital signature operations.

## Features

? **Core Capabilities**
- ?? XML document signing using X.509 certificates
- ? Digital signature verification
- ?? Base64 string encoding/decoding
- ?? Cross-Origin Resource Sharing (CORS) enabled for client integration

?? **Application Features**
- System tray integration with context menu
- Real-time HTTP API documentation via Swagger UI
- Structured logging with Serilog (console and file output)
- Graceful shutdown handling
- Auto-rolling daily log files

## Architecture

### Technology Stack
- **Framework**: .NET 10 (Windows)
- **UI**: Windows Presentation Foundation (WPF) + Windows Forms (system tray)
- **Web Framework**: ASP.NET Core
- **Logging**: Serilog
- **API Documentation**: Swagger/OpenAPI (Swashbuckle)
- **Serialization**: XML Data Contract Serializer

### Project Structure
```
DigitalSignaturesClient/
??? App.xaml / App.xaml.cs          # WPF application entry point
??? Controllers/
?   ??? XmlSignerController.cs       # API endpoints
??? Services/
?   ??? X509CertificateService.cs    # Certificate management
?   ??? SignVerifyEnvelope.cs        # XML signing/verification
?   ??? Base64StringService.cs       # Base64 operations
??? Middleware/
?   ??? LogPostRequestsMiddleware.cs  # HTTP request logging
??? Models/
?   ??? SignXmlRequest.cs            # API request models
??? appsettings.json                 # Configuration
??? DigitalSignaturesClient.csproj   # Project file
```

## Getting Started

### Requirements
- Windows 10 or later
- .NET 10 Runtime
- Valid X.509 certificate for signing operations

### Installation & Running

1. **Clone the Repository**
   ```bash
   git clone https://github.com/kiryazovkosta/XmlSigner.git
   cd XmlSigner/DigitalSignaturesClient
   ```

2. **Build the Project**
   ```bash
   dotnet build
   ```

3. **Run the Application**
   ```bash
   dotnet run
   ```

4. **Access the API**
   - **API Base URL**: `http://localhost:6050`
   - **Swagger UI**: `http://localhost:6050/swagger/index.html`

### System Tray Interaction

Once running, the application appears in the Windows system tray:
- **Right-click menu**:
  - **Running on port 6050**: Status indicator
  - **Logs files**: Opens the logs directory in Explorer
  - **Exit**: Shutdown the application
- **Double-click**: Shows API information balloon tip

## API Endpoints

### POST /api/xmlSigner
Signs an XML document using an X.509 certificate.

**Request**:
```json
{
  "xmlContent": "<xml>...</xml>",
  "certificateThumbprint": "ABCD1234..."
}
```

**Response**:
```json
{
  "signedXml": "<xml>...</xml>"
}
```

### Additional Endpoints
See Swagger UI at `http://localhost:6050/swagger` for complete API documentation.

## Configuration

### appsettings.json
```json
{
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://*:6050"
      }
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "logs/log.txt", "rollingInterval": "Day" } }
    ]
  }
}
```

### Key Configuration Options
- **Port**: Configured in `Kestrel:Endpoints:Http:Url`
- **Log Level**: `Serilog:MinimumLevel:Default`
- **Log Path**: `logs/` directory relative to application directory

## Logging

The application uses **Serilog** for structured logging with two outputs:

1. **Console**: Real-time logs in Visual Studio output window
2. **File Logs**:
   - `logs/xml-signer-*.log` - Application logs (rolling daily)
   - `logs/log.txt` - API request logs (rolling daily)

**View Logs**:
- Use the system tray menu ? "Logs files"
- Or navigate to: `<ApplicationDirectory>/logs/`

**Log Levels**:
- `[INF]` - Information
- `[WRN]` - Warning
- `[ERR]` - Error

Example log entry:
```
2026-01-15 20:55:40.457 +02:00 [INF] Request starting HTTP/1.1 POST http://localhost:6050/api/xmlSigner
```

## Development

### Building from Source
```bash
dotnet build
```

### Running Tests
```bash
dotnet test
```

### Debug Mode
```bash
dotnet run --configuration Debug
```

### Middleware
Custom middleware logs all HTTP POST requests to help with debugging:
```csharp
_webApp.UseMiddleware<LogPostRequestsMiddleware>();
```

## Security Considerations

?? **IMPORTANT**: The current configuration enables:
- **CORS AllowAll**: Allows requests from any origin
- **AllowAnyMethod**: Allows any HTTP method
- **AllowAnyHeader**: Allows any headers

For production use, restrict CORS policies:
```csharp
configurePolicy
    .WithOrigins("https://yourdomain.com")
    .WithMethods("POST")
    .WithHeaders("Content-Type");
```

## Troubleshooting

### Application won't start
- Check if port 6050 is already in use
- Verify .NET 10 runtime is installed
- Check Windows Firewall settings

### API returns 404
- Ensure the API is running (check system tray)
- Verify the endpoint path is correct
- Check logs for detailed error messages

### Certificate not found
- Ensure the certificate thumbprint is correct
- Certificate must be in the Windows certificate store
- Run the application with appropriate permissions

## Logs Location
Default logs directory: `<ApplicationDirectory>/logs/`

Access quickly via system tray ? "Logs files"

## License
Check the repository for license information.

## Support
For issues or feature requests, visit: https://github.com/kiryazovkosta/XmlSigner

---

**Version**: 1.0  
**Target Framework**: .NET 10  
**Last Updated**: January 2026
