# SSH Port Forwarder

Windows Forms tabanlı bir SSH port yönlendirme aracı. Bir gateway (jump host) üzerinden uzak bir porta yerel erişim sağlar; bağlantı koptuğunda otomatik olarak yeniden bağlanır.

## Özellikler

- Birden fazla tünel profili oluşturma ve kaydetme
- Gateway (jump host) üzerinden yerel port yönlendirme
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

## Kullanım

1. **+ Ekle** butonuyla yeni bir profil oluşturun.
2. **Gateway** bölümüne jump host adresi, portu ve kullanıcı adını girin.
3. Kimlik doğrulama yöntemini seçin: **Şifre** ya da **Private Key**.
4. **Yönlendirme** bölümünde uzak host/port ile yerel portu belirtin.
5. Gerekirse **Otomatik Yeniden Bağlan** seçeneğini ve bekleme süresini ayarlayın.
6. **Kaydet** ardından **Bağlan**.

### Port yönlendirme şeması

```
localhost:<YerelPort>  →  [Gateway SSH]  →  <UzakHost>:<UzakPort>
```

## Bağımlılıklar

| Paket | Sürüm |
|---|---|
| [SSH.NET](https://github.com/sshnet/SSH.NET) | 2025.1.0 |
| Newtonsoft.Json | 13.0.4 |
