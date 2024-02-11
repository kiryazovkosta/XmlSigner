namespace DigitalSignaturesClient.Services.Contracts;

public interface IBase64StringService
{
    string ToBase64String(string value);
    string FromBase64String(string base64String);
}
