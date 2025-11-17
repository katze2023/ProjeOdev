# Fitness Center Yönetim ve Randevu Sistemi

## Proje Hakkında
Bu proje, Sakarya Üniversitesi Web Programlama dersi kapsamında geliştirilmiş bir Spor Salonu Yönetim ve Randevu Sistemidir. ASP.NET Core MVC kullanılarak geliştirilmiş olup, spor salonlarının hizmetlerini, antrenörleri ve üye randevularını yönetmeyi amaçlamaktadır.

## Özellikler

### 1. Spor Salonu Yönetimi
- Farklı spor salonları tanımlama
- Çalışma saatleri yönetimi
- Hizmet türleri (fitness, yoga, pilates vb.) tanımlama
- Hizmet süre ve ücret yönetimi

### 2. Antrenör Yönetimi
- Antrenör profil yönetimi
- Uzmanlık alanları tanımlama
- Müsaitlik saatleri belirleme
- Hizmet türü atamaları

### 3. Üye ve Randevu Sistemi
- Kullanıcı kayıt ve giriş sistemi
- Randevu alma ve yönetimi
- Randevu uygunluk kontrolü
- Randevu onay mekanizması

### 4. REST API
- LINQ sorguları ile filtreleme
- Antrenör listeleme API'si
- Uygun antrenör sorgulama
- Randevu listeleme servisleri

### 5. Yapay Zeka Entegrasyonu
- Fotoğraf yükleme ve analiz
- Vücut tipine göre egzersiz önerileri
- Kişiselleştirilmiş diyet planları
- AI destekli fitness danışmanlığı

## Teknolojiler

- **Backend:** ASP.NET Core MVC 8.0
- **Dil:** C# 12
- **Veritabanı:** SQL Server / Entity Framework Core
- **ORM:** Entity Framework Core, LINQ
- **Frontend:** Bootstrap 5, HTML5, CSS3, JavaScript, jQuery
- **API:** RESTful Web Services
- **AI:** OpenAI API / Azure Cognitive Services

## Kurulum

### Gereksinimler
- .NET 8.0 SDK
- SQL Server 2019 veya üzeri
- Visual Studio 2022 / VS Code


## Proje Yapısı

```
FitnessCenter/
├── Controllers/
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── GymController.cs
│   ├── TrainerController.cs
│   ├── AppointmentController.cs
│   └── Api/
│       └── TrainerApiController.cs
├── Models/
│   ├── Gym.cs
│   ├── Trainer.cs
│   ├── Member.cs
│   ├── Appointment.cs
│   ├── Service.cs
│   └── ViewModels/
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbInitializer.cs
├── Views/
│   ├── Shared/
│   ├── Home/
│   ├── Gym/
│   ├── Trainer/
│   └── Appointment/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── images/
└── Services/
    └── AIService.cs
```

## Veritabanı Şeması

### Ana Tablolar
- **Gyms**: Spor salonları  -> Proje tek bir ile spor salonu için yapılabilir.
- **Trainers**: Antrenörler
- **Members**: Üyeler
- **Services**: Hizmetler
- **Appointments**: Randevular
- **Specializations**: Uzmanlık alanları
- **TrainerAvailability**: Antrenör müsaitlik saatleri

## API Endpoints

### Antrenör API
- `GET /api/trainers` - Tüm antrenörleri listele
- `GET /api/trainers/{id}` - Belirli bir antrenörü getir
- `GET /api/trainers/available?date={date}` - Belirli tarihte uygun antrenörler
- `GET /api/trainers/specialization/{spec}` - Uzmanlığa göre antrenörler

### Randevu API
- `GET /api/appointments/member/{memberId}` - Üye randevuları
- `GET /api/appointments/trainer/{trainerId}` - Antrenör randevuları

## Güvenlik

- ASP.NET Core Identity ile kullanıcı yönetimi
- Rol tabanlı yetkilendirme (Admin, Member)
- CSRF koruması
- XSS koruması
- SQL Injection koruması (Entity Framework)

## Yapay Zeka Özellikleri

Sistem, OpenAI API kullanarak şu özellikleri sunar:
- Kullanıcı fotoğraflarından vücut analizi
- Kişiselleştirilmiş egzersiz programı önerileri
- Diyet planı önerileri
- Hedef belirleme ve takip

## Geliştirici Notları

- Tüm controller'larda CRUD işlemleri tam olarak uygulanmıştır
- Client-side ve server-side validation mevcuttur
- Repository pattern kullanılmıştır
- Async/await pattern'leri kullanılmıştır

## Lisans
Bu proje Sakarya Üniversitesi Web Programlama dersi için geliştirilmiştir.

## İletişim
- Öğrenci: Fatih Kaya
- Öğrenci No: G231210072
- Email: fatihkayacinar@gmail.com
- GitHub: https://github.com/katze2023

## Son Güncelleme
2025-2026 Güz Dönemi