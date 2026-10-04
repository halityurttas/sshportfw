# SSH Port Forwarder

Windows Forms tabanlı bir SSH port yönlendirme aracı. Bir gateway (jump host) üzerinden uzak bir porta yerel erişim sağlar; bağlantı koptuğunda otomatik olarak yeniden bağlanır.

## Özellikler

- Birden fazla tünel profili oluşturma ve kaydetme
- Bir profil içinde **birden fazla port yönlendirme** (satır) tanımlama
- Gateway (jump host) üzerinden yerel port yönlendirme; tüm satırlar **tek SSH bağlantısı** üzerinden açılır
- Şifre veya Private Key (+ parola) kimlik doğrulama
- Bağlantı koptuğunda otomatik yeniden bağlanma (gecikme süresi ayarlanabilir)
- Anlık bağlantı durumu göstergesi
- Profiller `%AppData%\SshPortForwarder\profiles.json` dosyasına kaydedilir

## Gereksinimler

- Windows
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Derleme ve Çalıştırma

```bash
git clone <repo-url>
cd sshportfw/SshPortForwarder
dotnet run
```

Yayınlamak için (tek exe):

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## Release Çıkarma

1. Sürüm numarasını [SshPortForwarder.csproj](SshPortForwarder/SshPortForwarder.csproj) içindeki `<Version>` değerinde güncelleyin.
2. Değişiklikleri commit'leyip tag atın. Exe'ye gömülen commit hash HEAD'den alındığı için **önce commit + tag, sonra publish** yapılmalıdır:

   ```bash
   git add -A
   git commit -m "..."
   git tag v1.1.0
   ```

3. Publish edin (çıktı `release/SshPortForwarder/` klasörüne gider):

   ```bash
   dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true \
     -o ./release/SshPortForwarder
   ```

   WPF/WinForms native kütüphaneleri (`D3DCompiler_47_cor3.dll`, `PenImc_cor3.dll`,
   `PresentationNative_cor3.dll`, `vcruntime140_cor3.dll`, `wpfgfx_cor3.dll`) tek dosyaya
   gömülemez; exe'nin yanında kalırlar.

4. `SshPortForwarder.pdb` dosyasını silip klasörün **içeriğini** zip'leyin (klasörün kendisini değil):

   ```powershell
   Remove-Item .\release\SshPortForwarder\SshPortForwarder.pdb
   Compress-Archive -Path .\release\SshPortForwarder\* `
     -DestinationPath .\release\SshPortForwarder-v1.1.0-win-x64.zip -Force
   ```

5. Tag'i push edip zip'i GitHub Releases sayfasına yükleyin. `release/` klasörü `.gitignore` içinde
   olduğu için repoya girmez, sadece GitHub Release asset'i olarak dağıtılır:

   ```bash
   git push origin master --tags
   ```

## Kullanım

1. **+ Ekle** butonuyla yeni bir profil oluşturun.
2. **Gateway** bölümüne jump host adresi, portu ve kullanıcı adını girin.
3. Kimlik doğrulama yöntemini seçin: **Şifre** ya da **Private Key**.
4. **Port Yönlendirmeleri** tablosuna satır ekleyin: her satırda **Yerel Port**, **Uzak Host** ve **Uzak Port** belirtin. Satır eklemek için tablonun sonundaki boş satırı doldurun; kullanmak istemediğiniz satırın **Etkin** kutusunu kaldırın.
5. Gerekirse **Otomatik Yeniden Bağlan** seçeneğini ve bekleme süresini ayarlayın.
6. **Kaydet** ardından **Bağlan**.

### Port yönlendirme şeması

Tek yönlendirme:

```
localhost:<YerelPort>  →  [Gateway SSH]  →  <UzakHost>:<UzakPort>
```

Birden fazla yönlendirme aynı anda (tek SSH bağlantısı):

```
localhost:5432  ─┐
localhost:8080  ─┼→  [Gateway SSH]  →  farklı uzak hostlar/portlar
localhost:6379  ─┘
```

Yerel portlar profil içinde benzersiz olmalıdır; aynı yerel portu iki satırda kullanırsanız bağlanmadan önce uyarı gösterilir.
Profiller `%AppData%\SshPortForwarder\profiles.json` dosyasında `Forwards` listesi olarak saklanır. Tek portlu eski profiller ilk yüklemede otomatik olarak tek satırlı yeni formata taşınır.

## Bağımlılıklar

| Paket | Sürüm |
|---|---|
| [SSH.NET](https://github.com/sshnet/SSH.NET) | 2025.1.0 |
| Newtonsoft.Json | 13.0.4 |
