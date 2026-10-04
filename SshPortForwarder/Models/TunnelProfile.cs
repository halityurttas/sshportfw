using System;
using System.Collections.Generic;

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

        // Port yönlendirmeleri (her satır ayrı bir localhost→uzak hedef eşlemesi)
        public List<PortForward> Forwards { get; set; } = new();

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
