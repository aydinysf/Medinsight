# ADR-016: MVP Kimlik Doğrulama — E-posta/Parola + JWT

**Durum:** Kabul edildi
**Karar tarihi:** 2026-07-23, WP2

## Bağlam

`security-architecture.md` kimlik katmanını "JWT, RBAC, kaynak bazlı yetki"
olarak tanımlar ve JWT claim'lerinde `userId` ve `role` taşınmasını şart koşar;
ancak token'ın nasıl üretileceği (kimlik sağlayıcı) tanımsızdı ve
`erd-identity-case.md`'deki `USERS` tablosunda kimlik bilgisi alanı yoktu.

## Karar

1. **MVP'de e-posta + parola ile birinci taraf kimlik doğrulama.** Harici IdP
   (Auth0/Keycloak/Azure AD B2C) MVP'ye alınmaz (ADR-008 YAGNI ilkesi); geçiş
   yolu açık kalır çünkü token üretimi `IJwtTokenGenerator` soyutlamasının
   arkasındadır.
2. `USERS` tablosuna `role` ve `password_hash` kolonları eklenir. Bir
   kullanıcının tek rolü vardır (`Patient | Caregiver | Doctor | Admin`) —
   ERD'deki "bir kullanıcı hem hasta hem doktor olamaz" kuralıyla tutarlı.
3. Parola, ASP.NET Core Identity'nin `PasswordHasher`'ı ile hash'lenir
   (kanıtlanmış PBKDF2 implementasyonu); özel kripto yazılmaz.
4. Token: kısa ömürlü JWT access token (varsayılan 60 dk). Refresh token
   MVP dışıdır — süre dolunca yeniden login.
5. Yetkilendirme iki katman olarak uygulanır:
   - **Rol katmanı** API'de (`[Authorize(Roles = ...)]`),
   - **Kaynak katmanı** Application handler'larında (`ICurrentUser` ile vaka
     üyeliği / sahiplik kontrolü) — test edilebilirlik için handler seviyesinde.
6. Caregiver'ın "hasta adına vaka açması" (Analiz Raporu §14), hasta-caregiver
   bağ modeli henüz tanımlanmadığı için MVP'de **vaka üyeliği üzerinden**
   çalışır: caregiver ancak üyesi olduğu vakalarda yetkilidir; vaka oluşturma
   Patient (kendisi için) ve Admin ile sınırlıdır. Bağ modeli tanımlandığında
   bu karar genişletilir.

## Alternatifler

1. **Harici IdP (Keycloak vb.)** — Reddedildi (MVP için). Kurulum/operasyon
   yükü MVP karmaşıklık bütçesini aşıyor; soyutlama geçişe izin veriyor.
2. **Parolasız (magic link/OTP)** — Reddedildi. E-posta altyapısı bağımlılığı
   yaratır; Notification Engine MVP'de tek kanal ve kritik akışlara ayrılmış.
3. **ASP.NET Core Identity'nin tamamı (UserManager/IdentityDbContext)** —
   Reddedildi. Şema kontrolünü ERD'den koparır; sadece PasswordHasher bileşeni
   kullanılır.

## Sonuç

- `users` tablosu: + `role`, + `password_hash` (bkz. güncellenen
  `erd-identity-case.md`).
- Yeni endpoint: `POST /api/v1/auth/login` → `{ accessToken, expiresAt }`.
- JWT imza anahtarı geliştirmede `appsettings.Development.json`'da durur;
  üretimde secrets manager'dan gelir (bkz. `security-architecture.md`).

## İlgili Dosyalar

- `../architecture/security-architecture.md`
- `../domain/erd-identity-case.md`
- `../backend/error-handling.md`
