using System;
using System.Net;
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
        private ForwardedPortLocal? _port;
        private CancellationTokenSource? _cts;
        private TunnelProfile? _profile;
        private volatile bool _disposed;
        private volatile bool _userStopped;

        public event EventHandler<TunnelStatusEventArgs>? StatusChanged;

        public TunnelState CurrentState { get; private set; } = TunnelState.Disconnected;

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
                    SetState(TunnelState.Connected,
                        $"Bağlı — localhost:{_profile!.LocalPort} → {_profile.RemoteHost}:{_profile.RemotePort}");

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

            _port = new ForwardedPortLocal(
                IPAddress.Loopback.ToString(),
                (uint)p.LocalPort,
                p.RemoteHost,
                (uint)p.RemotePort);

            _client.AddForwardedPort(_port);
            _port.Start();
        }

        private void Cleanup()
        {
            try { _port?.Stop(); } catch { }
            try { _port?.Dispose(); } catch { }
            try { _client?.Disconnect(); } catch { }
            try { _client?.Dispose(); } catch { }
            _port = null;
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
