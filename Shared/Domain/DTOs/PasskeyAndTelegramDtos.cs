using System;

namespace Domain.DTOs
{
    public class PasskeyRegisterDto
    {
        public string? UserId { get; set; }
        public string Id { get; set; } = string.Empty;
        public string RawId { get; set; } = string.Empty;
        public string ClientDataJSON { get; set; } = string.Empty;
        public string AttestationObject { get; set; } = string.Empty;
    }

    public class PasskeyLoginDto
    {
        public string Id { get; set; } = string.Empty;
        public string RawId { get; set; } = string.Empty;
        public string ClientDataJSON { get; set; } = string.Empty;
        public string AuthenticatorData { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string? UserHandle { get; set; }
    }

    public class TelegramAuthDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? PhotoUrl { get; set; }
        public long AuthDate { get; set; }
        public string Hash { get; set; } = string.Empty;
    }
}
