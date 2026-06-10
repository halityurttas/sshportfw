using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SshPortForwarder.Models;
using SshPortForwarder.Services;

namespace SshPortForwarder
{
    public partial class MainForm : Form
    {
        private List<TunnelProfile> _profiles = new();
        private readonly Dictionary<Guid, SshTunnelService> _services = new();

        public MainForm()
        {
            InitializeComponent();
            LoadProfiles();
        }

        // ──────────────── Profil yönetimi ────────────────

        private void LoadProfiles()
        {
            _profiles = ProfileStore.Load();
            RefreshList();
        }

        private void RefreshList()
        {
            listProfiles.Items.Clear();
            foreach (var p in _profiles)
                listProfiles.Items.Add(p);

            if (listProfiles.Items.Count > 0)
                listProfiles.SelectedIndex = 0;
        }

        private void SaveProfiles() => ProfileStore.Save(_profiles);

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var p = new TunnelProfile { Name = "Yeni Profil " + (_profiles.Count + 1) };
            _profiles.Add(p);
            SaveProfiles();
            RefreshList();
            listProfiles.SelectedItem = p;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listProfiles.SelectedItem is not TunnelProfile p) return;

            if (_services.TryGetValue(p.Id, out var svc))
            {
                svc.Stop();
                svc.Dispose();
                _services.Remove(p.Id);
            }

            _profiles.Remove(p);
            SaveProfiles();
            RefreshList();
            ClearForm();
        }

        private TunnelProfile? CurrentProfile =>
            listProfiles.SelectedItem as TunnelProfile;

        private void listProfiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CurrentProfile is { } p)
                FillForm(p);
        }

        // ──────────────── Form ↔ Model ────────────────

        private void FillForm(TunnelProfile p)
        {
            txtName.Text = p.Name;
            txtGatewayHost.Text = p.GatewayHost;
            numGatewayPort.Value = p.GatewayPort;
            txtUsername.Text = p.GatewayUsername;
            txtPassword.Text = p.GatewayPassword;
            txtKeyPath.Text = p.PrivateKeyPath;
            txtKeyPass.Text = p.PrivateKeyPassphrase;
            txtRemoteHost.Text = p.RemoteHost;
            numRemotePort.Value = p.RemotePort;
            numLocalPort.Value = p.LocalPort;
            chkAutoReconnect.Checked = p.AutoReconnect;
            numReconnectDelay.Value = p.ReconnectDelaySeconds;
            rbPassword.Checked = p.AuthMethod == AuthMethod.Password;
            rbKey.Checked = p.AuthMethod == AuthMethod.PrivateKey;
            UpdateAuthUI();
            UpdateButtonState(p);
        }

        private void CollectForm(TunnelProfile p)
        {
            p.Name = txtName.Text.Trim();
            p.GatewayHost = txtGatewayHost.Text.Trim();
            p.GatewayPort = (int)numGatewayPort.Value;
            p.GatewayUsername = txtUsername.Text.Trim();
            p.GatewayPassword = txtPassword.Text;
            p.PrivateKeyPath = txtKeyPath.Text.Trim();
            p.PrivateKeyPassphrase = txtKeyPass.Text;
            p.RemoteHost = txtRemoteHost.Text.Trim();
            p.RemotePort = (int)numRemotePort.Value;
            p.LocalPort = (int)numLocalPort.Value;
            p.AutoReconnect = chkAutoReconnect.Checked;
            p.ReconnectDelaySeconds = (int)numReconnectDelay.Value;
            p.AuthMethod = rbKey.Checked ? AuthMethod.PrivateKey : AuthMethod.Password;
        }

        private void ClearForm()
        {
            txtName.Clear(); txtGatewayHost.Clear(); txtUsername.Clear();
            txtPassword.Clear(); txtKeyPath.Clear(); txtKeyPass.Clear();
            txtRemoteHost.Clear();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (CurrentProfile is not { } p) return;
            CollectForm(p);
            SaveProfiles();
            // listbox text'ini güncelle
            int idx = listProfiles.SelectedIndex;
            listProfiles.Items[idx] = p;
            listProfiles.SelectedIndex = idx;
        }

        // ──────────────── Auth UI ────────────────

        private void rbPassword_CheckedChanged(object sender, EventArgs e) => UpdateAuthUI();
        private void rbKey_CheckedChanged(object sender, EventArgs e) => UpdateAuthUI();

        private void UpdateAuthUI()
        {
            bool useKey = rbKey.Checked;
            lblPassword.Visible = !useKey;
            txtPassword.Visible = !useKey;
            lblKeyPath.Visible = useKey;
            txtKeyPath.Visible = useKey;
            btnBrowseKey.Visible = useKey;
            lblKeyPass.Visible = useKey;
            txtKeyPass.Visible = useKey;
        }

        private void btnBrowseKey_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Private Key Dosyası Seç",
                Filter = "Tüm Dosyalar (*.*)|*.*"
            };
            if (dlg.ShowDialog() == DialogResult.OK)
                txtKeyPath.Text = dlg.FileName;
        }

        // ──────────────── Tünel kontrolü ────────────────

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (CurrentProfile is not { } p) return;
            CollectForm(p);
            SaveProfiles();

            if (!_services.TryGetValue(p.Id, out var svc))
            {
                svc = new SshTunnelService();
                svc.StatusChanged += (_, args) => UpdateStatus(p.Id, args);
                _services[p.Id] = svc;
            }

            svc.Start(p);
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            if (CurrentProfile is not { } p) return;
            if (_services.TryGetValue(p.Id, out var svc))
                svc.Stop();
        }

        private void UpdateStatus(Guid profileId, TunnelStatusEventArgs args)
        {
            if (InvokeRequired)
            {
                Invoke(() => UpdateStatus(profileId, args));
                return;
            }

            // Sadece seçili profil için güncelle
            if (CurrentProfile?.Id != profileId) return;

            lblStatus.Text = args.Message;
            lblStatus.ForeColor = args.State switch
            {
                TunnelState.Connected => Color.Green,
                TunnelState.Reconnecting => Color.DarkOrange,
                TunnelState.Error => Color.Red,
                _ => Color.Gray
            };

            UpdateButtonState(CurrentProfile!);
        }

        private void UpdateButtonState(TunnelProfile p)
        {
            bool running = _services.TryGetValue(p.Id, out var svc) &&
                           (svc.CurrentState == TunnelState.Connected ||
                            svc.CurrentState == TunnelState.Connecting ||
                            svc.CurrentState == TunnelState.Reconnecting);

            btnConnect.Enabled = !running;
            btnDisconnect.Enabled = running;
        }

        // ──────────────── Kapatma ────────────────

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var svc in _services.Values)
            {
                svc.Stop();
                svc.Dispose();
            }
        }
    }
}
