namespace SshPortForwarder.Models
{
    /// <summary>
    /// Tek bir yerel→uzak port yönlendirmesi. Bir profil birden fazla satır barındırabilir;
    /// tümü aynı SSH bağlantısı üzerinden açılır.
    /// </summary>
    public class PortForward
    {
        public bool Enabled { get; set; } = true;
        public int LocalPort { get; set; } = 8080;
        public string RemoteHost { get; set; } = "127.0.0.1";
        public int RemotePort { get; set; } = 80;
    }
}
