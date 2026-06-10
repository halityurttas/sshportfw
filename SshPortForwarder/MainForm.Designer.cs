namespace SshPortForwarder
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            // ── Sol panel: profil listesi ──
            var pnlLeft = new Panel { Dock = DockStyle.Left, Width = 190, Padding = new Padding(6) };

            listProfiles = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
            listProfiles.SelectedIndexChanged += listProfiles_SelectedIndexChanged;

            var pnlListBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Padding = new Padding(0, 4, 0, 0)
            };

            btnAdd = new Button { Text = "+ Ekle", AutoSize = true };
            btnDelete = new Button { Text = "Sil", AutoSize = true };
            btnAdd.Click += btnAdd_Click;
            btnDelete.Click += btnDelete_Click;

            pnlListBtns.Controls.Add(btnAdd);
            pnlListBtns.Controls.Add(btnDelete);
            pnlLeft.Controls.Add(listProfiles);
            pnlLeft.Controls.Add(pnlListBtns);

            // ── Sağ panel: detay formu ──
            var pnlRight = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                AutoSize = false,
                AutoScroll = true
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 145));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            int row = 0;

            void AddRow(string label, Control ctrl)
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var lbl = new Label
                {
                    Text = label,
                    Anchor = AnchorStyles.Left | AnchorStyles.Top,
                    AutoSize = true,
                    Margin = new Padding(2, 6, 2, 2)
                };
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                ctrl.Margin = new Padding(2, 3, 2, 3);
                tbl.Controls.Add(lbl, 0, row);
                tbl.Controls.Add(ctrl, 1, row);
                row++;
            }

            void AddFullRow(Control ctrl)
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                ctrl.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
                ctrl.Margin = new Padding(2, 3, 2, 3);
                tbl.Controls.Add(ctrl, 0, row);
                tbl.SetColumnSpan(ctrl, 2);
                row++;
            }

            void AddSeparator(string title)
            {
                tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                var lbl = new Label
                {
                    Text = title,
                    Font = new System.Drawing.Font(Font.FontFamily, 9, System.Drawing.FontStyle.Bold),
                    ForeColor = System.Drawing.Color.DarkSlateBlue,
                    AutoSize = true,
                    Margin = new Padding(2, 8, 2, 2)
                };
                tbl.Controls.Add(lbl, 0, row);
                tbl.SetColumnSpan(lbl, 2);
                row++;
            }

            // Profil adı
            txtName = new TextBox();
            AddRow("Profil Adı:", txtName);

            // Gateway bilgileri
            AddSeparator("── Gateway (Jump Host) ──");
            txtGatewayHost = new TextBox();
            AddRow("Host / IP:", txtGatewayHost);

            numGatewayPort = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = 22 };
            AddRow("Port:", numGatewayPort);

            txtUsername = new TextBox();
            AddRow("Kullanıcı Adı:", txtUsername);

            // Kimlik doğrulama yöntemi
            AddSeparator("── Kimlik Doğrulama ──");
            var pnlAuth = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight };
            rbPassword = new RadioButton { Text = "Şifre", Checked = true, AutoSize = true };
            rbKey = new RadioButton { Text = "Private Key", AutoSize = true };
            rbPassword.CheckedChanged += rbPassword_CheckedChanged;
            rbKey.CheckedChanged += rbKey_CheckedChanged;
            pnlAuth.Controls.Add(rbPassword);
            pnlAuth.Controls.Add(rbKey);
            AddFullRow(pnlAuth);

            lblPassword = new Label { Text = "Şifre:", AutoSize = true, Margin = new Padding(2, 6, 2, 2) };
            txtPassword = new TextBox { UseSystemPasswordChar = true };
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblPassword.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            txtPassword.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtPassword.Margin = new Padding(2, 3, 2, 3);
            tbl.Controls.Add(lblPassword, 0, row);
            tbl.Controls.Add(txtPassword, 1, row);
            row++;

            lblKeyPath = new Label { Text = "Key Dosyası:", AutoSize = true, Margin = new Padding(2, 6, 2, 2) };
            var pnlKey = new Panel { Height = 26 };
            txtKeyPath = new TextBox { Dock = DockStyle.Fill };
            btnBrowseKey = new Button { Text = "...", Dock = DockStyle.Right, Width = 28 };
            btnBrowseKey.Click += btnBrowseKey_Click;
            pnlKey.Controls.Add(txtKeyPath);
            pnlKey.Controls.Add(btnBrowseKey);
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblKeyPath.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            pnlKey.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            pnlKey.Margin = new Padding(2, 3, 2, 3);
            tbl.Controls.Add(lblKeyPath, 0, row);
            tbl.Controls.Add(pnlKey, 1, row);
            row++;

            lblKeyPass = new Label { Text = "Key Parolası:", AutoSize = true, Margin = new Padding(2, 6, 2, 2) };
            txtKeyPass = new TextBox { UseSystemPasswordChar = true };
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            lblKeyPass.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            txtKeyPass.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top;
            txtKeyPass.Margin = new Padding(2, 3, 2, 3);
            tbl.Controls.Add(lblKeyPass, 0, row);
            tbl.Controls.Add(txtKeyPass, 1, row);
            row++;

            // Hedef
            AddSeparator("── Yönlendirme ──");
            txtRemoteHost = new TextBox();
            AddRow("Uzak Host:", txtRemoteHost);

            numRemotePort = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = 80 };
            AddRow("Uzak Port:", numRemotePort);

            numLocalPort = new NumericUpDown { Minimum = 1, Maximum = 65535, Value = 8080 };
            AddRow("Yerel Port:", numLocalPort);

            // Yeniden bağlanma
            AddSeparator("── Bağlantı ──");
            chkAutoReconnect = new CheckBox { Text = "Otomatik Yeniden Bağlan", AutoSize = true, Checked = true };
            AddFullRow(chkAutoReconnect);

            numReconnectDelay = new NumericUpDown { Minimum = 1, Maximum = 60, Value = 5 };
            AddRow("Bekleme (sn):", numReconnectDelay);

            // Kaydet butonu
            tbl.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            btnSave = new Button { Text = "Kaydet", AutoSize = true, Margin = new Padding(2, 8, 2, 2) };
            btnSave.Click += btnSave_Click;
            tbl.Controls.Add(btnSave, 1, row);
            row++;

            pnlRight.Controls.Add(tbl);

            // ── Alt durum / kontrol barı ──
            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(6, 6, 6, 0) };
            var pnlBtns = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight
            };
            btnConnect = new Button { Text = "Bağlan", Width = 90, Height = 30 };
            btnDisconnect = new Button { Text = "Kes", Width = 80, Height = 30, Enabled = false };
            btnConnect.BackColor = System.Drawing.Color.FromArgb(40, 167, 69);
            btnConnect.ForeColor = System.Drawing.Color.White;
            btnDisconnect.BackColor = System.Drawing.Color.FromArgb(220, 53, 69);
            btnDisconnect.ForeColor = System.Drawing.Color.White;
            btnConnect.FlatStyle = FlatStyle.Flat;
            btnDisconnect.FlatStyle = FlatStyle.Flat;
            btnConnect.Click += btnConnect_Click;
            btnDisconnect.Click += btnDisconnect_Click;
            pnlBtns.Controls.Add(btnConnect);
            pnlBtns.Controls.Add(btnDisconnect);

            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Text = "Bağlantı yok",
                ForeColor = System.Drawing.Color.Gray
            };

            pnlBottom.Controls.Add(pnlBtns);
            pnlBottom.Controls.Add(lblStatus);

            // ── Splitter ──
            var splitter = new Splitter { Dock = DockStyle.Left, Width = 4 };

            // ── Form ──
            Text = "SSH Port Forwarder";
            Size = new System.Drawing.Size(680, 520);
            MinimumSize = new System.Drawing.Size(580, 440);
            StartPosition = FormStartPosition.CenterScreen;
            FormClosing += MainForm_FormClosing;

            Controls.Add(pnlRight);
            Controls.Add(splitter);
            Controls.Add(pnlLeft);
            Controls.Add(pnlBottom);

            ResumeLayout(false);
        }

        // Kontroller
        private ListBox listProfiles = null!;
        private Button btnAdd = null!, btnDelete = null!;
        private TextBox txtName = null!, txtGatewayHost = null!, txtUsername = null!;
        private TextBox txtPassword = null!, txtKeyPath = null!, txtKeyPass = null!, txtRemoteHost = null!;
        private NumericUpDown numGatewayPort = null!, numRemotePort = null!, numLocalPort = null!, numReconnectDelay = null!;
        private RadioButton rbPassword = null!, rbKey = null!;
        private Label lblPassword = null!, lblKeyPath = null!, lblKeyPass = null!, lblStatus = null!;
        private Button btnBrowseKey = null!, btnSave = null!, btnConnect = null!, btnDisconnect = null!;
        private CheckBox chkAutoReconnect = null!;
    }
}
