using System;

namespace SshPortForwarder.Models
{
    public class TunnelProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = "Yeni Profil";

        // Gateway (jump host) bilgileri
        public string GatewayHost { get; set; } = "";
        public int GatewayPort { get; set; } = 22;
        public string GatewayUsername { get; set; } = "";
        public string GatewayPassword { get; set; } = "";
        public string PrivateKeyPath { get; set; } = "";
        public string PrivateKeyPassphrase { get; set; } = "";
        public AuthMethod AuthMethod { get; set; } = AuthMethod.Password;

        // Forward hedef bilgileri
        public string RemoteHost { get; set; } = "127.0.0.1";
        public int RemotePort { get; set; } = 80;

        // Yerel port
        public int LocalPort { get; set; } = 8080;

        // Otomatik yeniden bağlanma
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectDelaySeconds { get; set; } = 5;

        public override string ToString() => Name;
    }

    public enum AuthMethod
    {
        Password,
        PrivateKey
    }
}
