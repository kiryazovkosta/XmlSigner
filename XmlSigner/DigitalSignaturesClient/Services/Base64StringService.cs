using DigitalSignaturesClient.Services.Contracts;
using System.Text;

namespace DigitalSignaturesClient.Services;

public class Base64StringService : IBase64StringService
{
    public string FromBase64String(string base64String)
    {
        var base64EncodedBytes = Convert.FromBase64String(base64String);
        var result = Encoding.UTF8.GetString(base64EncodedBytes);
        return result;
    }

    public string ToBase64String(string value)
    {
        var textBytes = Encoding.UTF8.GetBytes(value);
        var base64String = Convert.ToBase64String(textBytes);
        return base64String;
    }
}
