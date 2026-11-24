using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.Repositories;
using static SharedKernel.Options.JwtOptions;

namespace Web.UI.Services;

internal sealed class DeterministicJwtKeyXmlRepository(IConfiguration configuration) : IXmlRepository
{
    private static readonly XNamespace DpNs = "http://schemas.asp.net/2015/03/dataProtection";
    private readonly XElement _key = CreateKey(configuration[JWT_KEY] ?? throw new InvalidOperationException($"{JWT_KEY} is required."));

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicConstructors, typeof(AuthenticatedEncryptorDescriptorDeserializer))]
    private static XElement CreateKey(string secret)
    {
        var ikm = Encoding.UTF8.GetBytes(secret);
        var id = new Guid(HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 16, [1]));
        var masterKey = Convert.ToBase64String(HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, 64, [2]));

        return new XElement("key",
            new XAttribute("id", id),
            new XAttribute("version", 1),
            new XElement("creationDate", DateTimeOffset.UnixEpoch),
            new XElement("activationDate", DateTimeOffset.UnixEpoch),
            new XElement("expirationDate", DateTimeOffset.MaxValue),
            new XElement("descriptor",
                new XAttribute("deserializerType", typeof(AuthenticatedEncryptorDescriptorDeserializer).AssemblyQualifiedName!),
                new XElement("descriptor",
                    new XElement("encryption", new XAttribute("algorithm", "AES_256_CBC")),
                    new XElement("validation", new XAttribute("algorithm", "HMACSHA256")),
                    new XElement("masterKey",
                        new XAttribute(DpNs + "requiresEncryption", true),
                        new XElement("value", masterKey)))));
    }

    public IReadOnlyCollection<XElement> GetAllElements() => [new XElement(_key)];
    public void StoreElement(XElement element, string friendlyName) { }
}
