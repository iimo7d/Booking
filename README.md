# 🏨 Booking — Multipurpose Hotel Booking Platform

> 🎓 **This is my very first project** — built as a final bootcamp capstone to apply everything I learned about ASP.NET Core MVC, Entity Framework, authentication, and full-stack web development from scratch.

A full-featured hotel booking web application built with **ASP.NET Core MVC (.NET 8)**. Users can discover hotels, reserve rooms, write reviews, and manage their bookings — while agents manage their own listings and admins control the entire platform.

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Roles & Permissions](#roles--permissions)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Setup & Installation](#setup--installation)
- [Configuration](#configuration)
- [Database Migrations](#database-migrations)
- [Running the App](#running-the-app)
- [Seeded Data](#seeded-data)
- [Domain Models](#domain-models)

---

## ✨ Features

### 👥 Guest / Visitor
- Browse the home page with featured hotels, most-visited cities, and testimonials
- Search and filter hotels by country, city, listing type, price range, star rating, and room class
- View hotel details: rooms, amenities, reviews, and gallery
- Read blog posts
- Paginated load-more review section

### 🙋 Registered User
- Register / login / logout with email notification on registration & login
- Edit profile (name, DOB, city, avatar upload)
- Change password
- Add hotels to favorites and manage them
- Add rooms to a session-based cart and complete checkout
- Booking lifecycle: Pending → Confirmed → Cancelled/Completed
- Receive email confirmation at each booking stage
- View all bookings (upcoming, cancelled, completed)
- Post, edit, and delete reviews with optional image upload *(requires a confirmed booking for that hotel)*
- Submit a "Join Us" request to become an agent (with document/image proof upload)

### 🏢 Agent
- Personal dashboard showing own listing stats (rooms, bookings, visits)
- Manage rooms: add, edit (name, price, capacity, class, images), delete
- Assign amenities per room class for their listing

### 🔑 Admin
- Full admin dashboard with widgets: recent users, rooms, hotels, reviews, bookings, invoices
- Manage **Guests**: list, view booking history, contact details
- Manage **Agents**: list, view agent details, approve/reject listings
- Manage **Listings**: activate/deactivate, edit, upload images
- Manage **Cities** and **Countries** (with image uploads)
- Manage **Room Classes**, **Amenities**, **Services**, **Listing Types**
- Manage **Blog** posts (image + video upload)
- Review and action **Join Us** requests
- Nightly Quartz.NET job cleans up stale/expired requests automatically

---

## 🛠 Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core MVC .NET 8 |
| ORM | Entity Framework Core 8 (SQL Server) |
| Authentication | ASP.NET Core Identity |
| Email | MailKit 4.10 + SendGrid 9.29 |
| Background Jobs | Quartz.NET 3.13 (ASP.NET Core hosted service) |
| JSON | Newtonsoft.Json 13.0 |
| Session | `Microsoft.AspNetCore.Session` (distributed memory cache) |
| Frontend | Bootstrap 5, jQuery, Dropzone.js, noUiSlider, Choices.js |
| View Engine | Razor (`.cshtml`) |

### Full NuGet Dependency List

| Package | Version |
|---------|---------|
| MailKit | 4.10.0 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 8.0.12 |
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 8.0.13 |
| Microsoft.AspNetCore.Session | 2.3.0 |
| Microsoft.EntityFrameworkCore | 8.0.12 |
| Microsoft.EntityFrameworkCore.Design | 8.0.12 |
| Microsoft.EntityFrameworkCore.Relational | 8.0.12 |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.12 |
| Microsoft.EntityFrameworkCore.Tools | 8.0.12 |
| Newtonsoft.Json | 13.0.3 |
| Quartz.AspNetCore | 3.13.1 |
| SendGrid | 9.29.3 |

---

## 🔐 Roles & Permissions

| Action | Guest | User | Agent | Admin |
|--------|:-----:|:----:|:-----:|:-----:|
| Browse listings & blog | ✅ | ✅ | ✅ | ✅ |
| Register / login | ✅ | — | — | — |
| Edit profile & avatar | — | ✅ | ✅ | ✅ |
| Add to cart & checkout | — | ✅ | — | — |
| Confirm / cancel bookings | — | ✅ | — | — |
| Favorites | — | ✅ | — | — |
| Post reviews | — | ✅* | — | — |
| Submit Join Us request | — | ✅ | — | — |
| Agent dashboard | — | — | ✅ | — |
| Manage own rooms | — | — | ✅ | — |
| Assign amenities | — | — | ✅ | ✅ |
| Admin dashboard | — | — | — | ✅ |
| Manage all entities | — | — | — | ✅ |

> \* Requires a **confirmed** booking for the hotel being reviewed.

---

## 📁 Project Structure

```
Booking/
├── Areas/
│   ├── Admin/                   # Admin area (dashboard, guest/agent mgmt)
│   │   ├── Controllers/
│   │   │   ├── AdminDashboardController.cs
│   │   │   ├── AgentController.cs
│   │   │   └── GuestController.cs
│   │   └── Views/
│   └── Agent/                   # Agent area (own listing/rooms)
│       ├── Controllers/
│       │   └── AgentController.cs
│       └── Views/
├── Controllers/                 # Main MVC controllers
│   ├── AccountController.cs     # Auth, profile, password
│   ├── BookingController.cs     # Cart, checkout, confirm, cancel
│   ├── ListingController.cs     # Hotel search, detail, admin CRUD
│   ├── RoomController.cs        # Room CRUD + images
│   ├── ReviewController.cs      # Reviews + image upload
│   ├── FavoriteController.cs    # Favorites
│   ├── JoinUsController.cs      # Agent application workflow
│   ├── BlogController.cs        # Blog CMS
│   ├── RoomClassController.cs   # Room classes + amenity assignment
│   ├── CityController.cs        # City CRUD
│   ├── CountryController.cs     # Country CRUD
│   ├── AmenityController.cs     # Amenity CRUD
│   ├── ServiceController.cs     # Service CRUD
│   └── ListingTypeController.cs # Listing type CRUD
├── Data/
│   └── Context.cs               # EF Core DbContext + seed data
├── Enums/
│   ├── BookingStatus.cs         # Pending, Confirmed, Cancelled, Completed
│   ├── Status.cs                # Pending, Approved, Rejected
│   ├── ServiceType.cs           # Language, Activities, Payment, Services, Safety
│   └── PaymentMethod.cs         # CreditCard, DebitCard
├── Migrations/                  # EF Core migrations
├── Models/                      # Domain entities
├── Services/
│   └── EmailSender.cs           # IEmailSender implementation (SendGrid/MailKit)
├── ViewComponents/
│   ├── CityList/                # Most-visited cities widget
│   └── RandomHotels/            # Featured hotels widget
├── ViewModels/                  # View-specific models
├── Views/                       # Razor views
│   ├── Booking/                 # Cart, checkout, confirm, my-bookings
│   ├── Listing/                 # Search, detail, create/edit
│   ├── Room/                    # Room add/edit partials
│   ├── Account/                 # Register, login, profile
│   ├── Favorite/                # My favorites
│   ├── Blog/                    # Blog index + content
│   ├── JoinUs/                  # Agent application pages
│   └── Shared/                  # _Layout, partials
├── wwwroot/
│   ├── assets/                  # Theme CSS, JS, images, vendor libs
│   │   └── vendor/              # Dropzone, noUiSlider, Choices.js, etc.
│   ├── uploads/                 # User-uploaded files (avatars, listings, blogs)
│   └── lib/                     # bootstrap, jquery, validation
├── appsettings.json
├── Program.cs
└── Booking.csproj
```

---

## 🧩 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/) (Express or higher) or SQL Server LocalDB
- A SendGrid API key **or** SMTP credentials (for email functionality)
- (Optional) [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extension

---

## 🚀 Setup & Installation

### 1. Clone the repository

```bash
git clone https://github.com/your-username/Booking.git
cd Booking
```

### 2. Install dependencies

```bash
dotnet restore
```

### 3. Configure the application

Copy or edit `Booking/appsettings.json` and fill in your values (see [Configuration](#configuration) below).

### 4. Apply database migrations

```bash
dotnet ef database update --project Booking/Booking.csproj
```

### 5. Run the application

```bash
dotnet run --project Booking/Booking.csproj
```

The app starts at `https://localhost:7118` (HTTPS) or `http://localhost:5XXX` (HTTP).

---

## ⚙️ Configuration

Edit `Booking/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ConnectionString": "Server=YOUR_SERVER;Database=BookingDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "SendGrid": {
    "SecretKey": "YOUR_SENDGRID_API_KEY"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

| Key | Description |
|-----|-------------|
| `ConnectionStrings:ConnectionString` | SQL Server connection string |
| `SendGrid:SecretKey` | SendGrid API key for transactional email |

> **Tip:** Use `dotnet user-secrets` or environment variables to keep secrets out of source control in production.

---

## 🗄️ Database Migrations

The project uses EF Core Code-First migrations.

```bash
# Apply all pending migrations to the database
dotnet ef database update --project Booking/Booking.csproj

# Create a new migration after model changes
dotnet ef migrations add <MigrationName> --project Booking/Booking.csproj

# Revert to a specific migration
dotnet ef database update <MigrationName> --project Booking/Booking.csproj
```

Current migrations range: `20250226205526_132` → `20250226211056_321`

---

## ▶️ Running the App

```bash
# Development (hot reload)
dotnet watch run --project Booking/Booking.csproj

# Production-like
dotnet run --project Booking/Booking.csproj --configuration Release
```

---

## 🌱 Seeded Data

On first run the database is seeded automatically with:

| Data | Details |
|------|---------|
| **Roles** | Admin, User, Agent |
| **Admin user** | username: `admin` / email: `admin@booking.com` — **no password is set by default**; after running migrations update the admin account with a strong password directly in the database or via the Identity reset-password flow |
| **Countries** | 20 countries (Saudi Arabia, UAE, USA, UK, France, etc.) |
| **Cities** | 28 cities mapped to their countries |
| **Room Classes** | 13 types (Single, Double, Suite, Presidential Suite, etc.) |
| **Amenities** | 20 amenities (Wi-Fi, Pool, Parking, Spa, etc.) |
| **Listing Types** | 20 types (Luxury Hotel, Resort, Boutique Hotel, etc.) |
| **Services** | 20 services across Language, Activities, Payment, Safety categories |

> ⚠️ **Admin password**: the seeded admin account has no password hash. Use `dotnet ef` to update the user record or implement a first-run setup page before deploying.

---

## 🗂️ Domain Models

| Model | Key Fields |
|-------|-----------|
| `Listing` | Name, Description, City, Street, StarterPrice, Rating, IsActive, AgentId |
| `Room` | RoomNo, PricePerNight, AdultsCapacity, ChildrenCapacity, BookedCount, RoomClassId |
| `Bookings` | Status, CheckInDate, CheckOutDate, TotalPrice, ConfirmationNo, PaymentMethod |
| `Review` | Content, Rate, ImagePath, AgentReply, CreationTime |
| `AppUser` | FirstName, LastName, DOB, AvatarUrl, CityId |
| `City` | Name, CountryId, VisitCount, ImageUrl |
| `Amenity` | Name |
| `RoomClass` | Name |
| `Blogs` | Title, Content, ImagePath, VideoPath |
| `JoinUs` | Status (Pending/Approved/Rejected), documents/proofs |

### Enumerations

```csharp
BookingStatus  { Pending, Confirmed, Cancelled, Completed }
Status         { Pending, Approved, Rejected }         // JoinUs requests
ServiceType    { Language, Activities, Payment, Services, Safety }
PaymentMethod  { CreditCard, DebitCard }
```

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m "feat: add your feature"`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

---

## 📄 License

This is my very first full-stack project, built as a final bootcamp capstone. Feel free to use and adapt it for learning or educational purposes.
