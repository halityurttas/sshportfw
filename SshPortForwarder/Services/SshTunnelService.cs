using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Renci.SshNet;
using SshPortForwarder.Models;

namespace SshPortForwarder.Services
{
    public enum TunnelState { Disconnected, Connecting, Connected, Reconnecting, Error }

    public class TunnelStatusEventArgs : EventArgs
    {
        public TunnelState State { get; }
        public string Message { get; }
        public TunnelStatusEventArgs(TunnelState state, string message)
        {
            State = state;
            Message = message;
        }
    }

    public class SshTunnelService : IDisposable
    {
        private SshClient? _client;
        private readonly List<ForwardedPortLocal> _ports = new();
        private CancellationTokenSource? _cts;
        private TunnelProfile? _profile;
        private volatile bool _disposed;
        private volatile bool _userStopped;

        public event EventHandler<TunnelStatusEventArgs>? StatusChanged;

        public TunnelState CurrentState { get; private set; } = TunnelState.Disconnected;

        /// <summary>Profilin bağlanmaya uygun olup olmadığını kontrol eder; hata mesajı ya da null döner.</summary>
        public static string? Validate(TunnelProfile p)
        {
            var forwards = ActiveForwards(p);
            if (forwards.Count == 0)
                return "En az bir etkin port yönlendirme satırı olmalı. Tabloya satır ekleyip 'Etkin' kutusunu işaretleyin.";

            var duplicate = forwards.GroupBy(f => f.LocalPort).FirstOrDefault(g => g.Count() > 1);
            if (duplicate != null)
                return $"Yerel port {duplicate.Key} birden fazla satırda kullanılıyor. Her satır için farklı bir yerel port seçin.";

            foreach (var f in forwards)
            {
                if (string.IsNullOrWhiteSpace(f.RemoteHost))
                    return $"Yerel port {f.LocalPort} satırında uzak host boş olamaz.";

                if (f.LocalPort is < 1 or > 65535 || f.RemotePort is < 1 or > 65535)
                    return "Portlar 1-65535 aralığında olmalıdır.";
            }

            return null;
        }

        private static List<PortForward> ActiveForwards(TunnelProfile p) =>
            p.Forwards?.Where(f => f.Enabled).ToList() ?? new List<PortForward>();

        public void Start(TunnelProfile profile)
        {
            if (CurrentState == TunnelState.Connected || CurrentState == TunnelState.Connecting)
                return;

            _profile = profile;
            _userStopped = false;
            _cts = new CancellationTokenSource();

            Task.Run(() => ConnectLoop(_cts.Token));
        }

        public void Stop()
        {
            _userStopped = true;
            _cts?.Cancel();
            Cleanup();
            SetState(TunnelState.Disconnected, "Bağlantı kesildi.");
        }

        private async Task ConnectLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested && !_disposed && !_userStopped)
            {
                try
                {
                    SetState(TunnelState.Connecting, "Bağlanılıyor...");
                    Connect();
                    SetState(TunnelState.Connected, $"Bağlı — {DescribeForwards()}");

                    // Bağlantı kesilene kadar bekle
                    while (!token.IsCancellationRequested && _client != null && _client.IsConnected)
                    {
                        await Task.Delay(2000, token).ConfigureAwait(false);
                    }

                    if (token.IsCancellationRequested || _userStopped) break;

                    // Beklenmedik kopuş
                    Cleanup();
                    if (_profile!.AutoReconnect)
                    {
                        SetState(TunnelState.Reconnecting,
                            $"Bağlantı koptu, {_profile.ReconnectDelaySeconds}s içinde yeniden denenecek...");
                        await Task.Delay(_profile.ReconnectDelaySeconds * 1000, token).ConfigureAwait(false);
                    }
                    else
                    {
                        SetState(TunnelState.Disconnected, "Bağlantı koptu.");
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Cleanup();
                    if (_userStopped || token.IsCancellationRequested) break;

                    if (_profile!.AutoReconnect)
                    {
                        SetState(TunnelState.Reconnecting,
                            $"Hata: {ex.Message} — {_profile.ReconnectDelaySeconds}s içinde yeniden denenecek...");
                        try
                        {
                            await Task.Delay(_profile.ReconnectDelaySeconds * 1000, token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) { break; }
                    }
                    else
                    {
                        SetState(TunnelState.Error, $"Hata: {ex.Message}");
                        break;
                    }
                }
            }

            if (!_userStopped && CurrentState != TunnelState.Disconnected)
                SetState(TunnelState.Disconnected, "Durduruldu.");
        }

        private void Connect()
        {
            var p = _profile!;
            var forwards = ActiveForwards(p);
            if (forwards.Count == 0)
                throw new InvalidOperationException("Etkin port yönlendirme tanımlı değil.");

            ConnectionInfo connInfo;
            if (p.AuthMethod == AuthMethod.PrivateKey)
            {
                PrivateKeyFile keyFile = string.IsNullOrEmpty(p.PrivateKeyPassphrase)
                    ? new PrivateKeyFile(p.PrivateKeyPath)
                    : new PrivateKeyFile(p.PrivateKeyPath, p.PrivateKeyPassphrase);

                connInfo = new ConnectionInfo(p.GatewayHost, p.GatewayPort, p.GatewayUsername,
                    new PrivateKeyAuthenticationMethod(p.GatewayUsername, keyFile));
            }
            else
            {
                connInfo = new ConnectionInfo(p.GatewayHost, p.GatewayPort, p.GatewayUsername,
                    new PasswordAuthenticationMethod(p.GatewayUsername, p.GatewayPassword));
            }

            _client = new SshClient(connInfo);
            _client.Connect();

            // Tüm yönlendirmeler aynı SSH bağlantısı üzerinden açılır.
            foreach (var f in forwards)
            {
                var port = new ForwardedPortLocal(
                    IPAddress.Loopback.ToString(),
                    (uint)f.LocalPort,
                    f.RemoteHost,
                    (uint)f.RemotePort);

                _client.AddForwardedPort(port);
                port.Start();
                _ports.Add(port);
            }
        }

        private string DescribeForwards()
        {
            var forwards = ActiveForwards(_profile!);
            var sb = new StringBuilder();
            foreach (var f in forwards)
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append($"localhost:{f.LocalPort} → {f.RemoteHost}:{f.RemotePort}");
            }

            return forwards.Count > 1
                ? $"{forwards.Count} yönlendirme: {sb}"
                : sb.ToString();
        }

        private void Cleanup()
        {
            foreach (var port in _ports)
            {
                try { port.Stop(); } catch { }
                try { port.Dispose(); } catch { }
            }
            _ports.Clear();

            try { _client?.Disconnect(); } catch { }
            try { _client?.Dispose(); } catch { }
            _client = null;
        }

        private void SetState(TunnelState state, string message)
        {
            CurrentState = state;
            StatusChanged?.Invoke(this, new TunnelStatusEventArgs(state, message));
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
            _cts?.Dispose();
        }
    }
}
