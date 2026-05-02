using Booking.Enums;
using Booking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Booking.Data
{
    public class Context : IdentityDbContext<AppUser>
    {
        public Context(DbContextOptions<Context> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            #region Change Identity table names

            modelBuilder.Entity<AppUser>().ToTable("Users", "sec");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "sec");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "sec");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "sec");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "sec");
            modelBuilder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims", "sec");
            modelBuilder.Entity<IdentityUserToken<string>>().ToTable("UserTokens", "sec");
            #endregion

            #region Seed Data
            var adminRole = new IdentityRole { Id = "f3776bee-aa31-4560-a259-03afe71b4b83", Name = "Admin", NormalizedName = "ADMIN", ConcurrencyStamp = "6b7eda24-b7df-4b98-9e87-aa9663bb7808" };
            var userRole = new IdentityRole { Id = "27c0908b-e09c-4a68-906d-e42c3295dec9", Name = "User", NormalizedName = "USER", ConcurrencyStamp = "9b52f8d9-cfbe-4f9c-bdfb-41423f3face9" };
            var agentRole = new IdentityRole { Id = "86343a28-20ab-450e-b401-5fa16c073688", Name = "Agent", NormalizedName = "AGENT", ConcurrencyStamp = "9348b5af-c885-44d5-8f55-28610173c272" };

            var hasher = new PasswordHasher<AppUser>();
            var adminUser = new AppUser
            {
                Id = "8709e6d5-6c9e-442e-98d6-49d39f80014f",
                UserName = "Mohammad",
                NormalizedUserName = "MOHAMMAD",
                Email = "Mohammadabuaisheh40@gmail.com",
                NormalizedEmail = "MOHAMMADABUAISHEH40@GMAIL.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Mohammad1012004"),
                SecurityStamp = "f73eb42e-7153-44a4-8a5e-aa8f5d0b184c",
                JoinDate = DateTime.Now,
                FirstName = "Mohammad",
                LastName = "Abu Aisheh",
                CityId = 1,
                DOB = new DateOnly(2004, 1, 10)
            };

            var adminUserRole = new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = adminRole.Id };
            var adminUserRole2 = new IdentityUserRole<string> { UserId = adminUser.Id, RoleId = userRole.Id };
            modelBuilder.Entity<IdentityRole>().HasData(adminRole, userRole, agentRole);
            modelBuilder.Entity<AppUser>().HasData(adminUser);
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(adminUserRole, adminUserRole2);


            modelBuilder.Entity<RoomClass>().HasData(
                new RoomClass { Id = 1, Name = "Single Room" },
                new RoomClass { Id = 2, Name = "Double Room" },
                new RoomClass { Id = 3, Name = "Triple Room" },
                new RoomClass { Id = 4, Name = "Quad Room" },
                new RoomClass { Id = 5, Name = "Queen Room" },
                new RoomClass { Id = 6, Name = "King Room" },
                new RoomClass { Id = 7, Name = "Hollywood Twin Room" },
                new RoomClass { Id = 8, Name = "Suite" },
                new RoomClass { Id = 9, Name = "Family Room" },
                new RoomClass { Id = 10, Name = "Connected Room" },
                new RoomClass { Id = 11, Name = "Delux Room" },
                new RoomClass { Id = 12, Name = "Executive Room" },
                new RoomClass { Id = 13, Name = "Presidential Suite" }
                );


            modelBuilder.Entity<Amenity>().HasData(
           new Amenity { Id = 1, Name = "Free Wi-Fi" },
           new Amenity { Id = 2, Name = "Swimming Pool" },
           new Amenity { Id = 3, Name = "Air Conditioning" },
           new Amenity { Id = 4, Name = "Parking" },
           new Amenity { Id = 5, Name = "Gym/Fitness Center" },
           new Amenity { Id = 6, Name = "Restaurant" },
           new Amenity { Id = 7, Name = "Bar/Lounge" },
           new Amenity { Id = 8, Name = "24/7 Room Service" },
           new Amenity { Id = 9, Name = "Spa & Wellness Center" },
           new Amenity { Id = 10, Name = "Conference Room" },
           new Amenity { Id = 11, Name = "Airport Shuttle" },
           new Amenity { Id = 12, Name = "Business Center" },
           new Amenity { Id = 13, Name = "Breakfast Included" },
           new Amenity { Id = 14, Name = "Pet-Friendly" },
           new Amenity { Id = 15, Name = "Laundry Service" },
           new Amenity { Id = 16, Name = "Non-Smoking Rooms" },
           new Amenity { Id = 17, Name = "Children's Playground" },
           new Amenity { Id = 18, Name = "Jacuzzi/Hot Tub" },
           new Amenity { Id = 19, Name = "Private Beach Access" },
           new Amenity { Id = 20, Name = "Car Rental Service" }
       );

            modelBuilder.Entity<ListingType>().HasData(
    new ListingType { Id = 1, Name = "Luxury Hotel" },
    new ListingType { Id = 2, Name = "Resort" },
    new ListingType { Id = 3, Name = "Business Hotel" },
    new ListingType { Id = 4, Name = "Boutique Hotel" },
    new ListingType { Id = 5, Name = "Budget Hotel" },
    new ListingType { Id = 6, Name = "Hostel" },
    new ListingType { Id = 7, Name = "Bed & Breakfast" },
    new ListingType { Id = 8, Name = "Apartment Hotel" },
    new ListingType { Id = 9, Name = "Extended Stay Hotel" },
    new ListingType { Id = 10, Name = "Eco-Friendly Hotel" },
    new ListingType { Id = 11, Name = "Historic Hotel" },
    new ListingType { Id = 12, Name = "Motel" },
    new ListingType { Id = 13, Name = "Capsule Hotel" },
    new ListingType { Id = 14, Name = "Beachfront Hotel" },
    new ListingType { Id = 15, Name = "Ski Resort" },
    new ListingType { Id = 16, Name = "Casino Hotel" },
    new ListingType { Id = 17, Name = "Waterpark Resort" },
    new ListingType { Id = 18, Name = "Wellness & Spa Hotel" },
    new ListingType { Id = 19, Name = "Guesthouse" },
    new ListingType { Id = 20, Name = "Floating Hotel" }
);


            modelBuilder.Entity<Service>().HasData(
           // Language Services
           new Service { Id = 1, Description = "English-speaking staff", Type = ServiceType.Language },
           new Service { Id = 2, Description = "Spanish-speaking staff", Type = ServiceType.Language },
           new Service { Id = 3, Description = "French-speaking staff", Type = ServiceType.Language },
           new Service { Id = 4, Description = "German-speaking staff", Type = ServiceType.Language },

           // Activities
           new Service { Id = 5, Description = "Guided City Tours", Type = ServiceType.Activities },
           new Service { Id = 6, Description = "Hiking Trips", Type = ServiceType.Activities },
           new Service { Id = 7, Description = "Scuba Diving Classes", Type = ServiceType.Activities },
           new Service { Id = 8, Description = "Cultural Workshops", Type = ServiceType.Activities },

           // Payment Methods
           new Service { Id = 9, Description = "Credit Card Accepted", Type = ServiceType.Payment },
           new Service { Id = 10, Description = "PayPal Accepted", Type = ServiceType.Payment },
           new Service { Id = 11, Description = "Apple Pay Accepted", Type = ServiceType.Payment },
           new Service { Id = 12, Description = "Cryptocurrency Payments", Type = ServiceType.Payment },

           // General Services
           new Service { Id = 13, Description = "24-hour Front Desk", Type = ServiceType.Services },
           new Service { Id = 14, Description = "Daily Housekeeping", Type = ServiceType.Services },
           new Service { Id = 15, Description = "Airport Pickup/Drop-off", Type = ServiceType.Services },
           new Service { Id = 16, Description = "Car Rental Service", Type = ServiceType.Services },

           // Safety & Security
           new Service { Id = 17, Description = "CCTV Surveillance", Type = ServiceType.Safety },
           new Service { Id = 18, Description = "Secure Lockers", Type = ServiceType.Safety },
           new Service { Id = 19, Description = "Fire Extinguishers Available", Type = ServiceType.Safety },
           new Service { Id = 20, Description = "24-hour Security", Type = ServiceType.Safety }

       );

            modelBuilder.Entity<Country>().HasData(
           new Country { Id = 1, Name = "Saudi Arabia" },
           new Country { Id = 2, Name = "United Arab Emirates" },
           new Country { Id = 3, Name = "Qatar" },
           new Country { Id = 4, Name = "Kuwait" },
           new Country { Id = 5, Name = "Oman" },
           new Country { Id = 6, Name = "Bahrain" },
           new Country { Id = 7, Name = "Egypt" },
           new Country { Id = 8, Name = "Jordan" },
           new Country { Id = 9, Name = "Lebanon" },
           new Country { Id = 10, Name = "Turkey" },
           new Country { Id = 11, Name = "Morocco" },
           new Country { Id = 12, Name = "Tunisia" },
           new Country { Id = 13, Name = "Algeria" },
           new Country { Id = 14, Name = "United States" },
           new Country { Id = 15, Name = "United Kingdom" },
           new Country { Id = 16, Name = "France" },
           new Country { Id = 17, Name = "Germany" },
           new Country { Id = 18, Name = "Canada" },
           new Country { Id = 19, Name = "Australia" },
           new Country { Id = 20, Name = "Japan" }
       );

            // Seed Cities
            modelBuilder.Entity<City>().HasData(
                // Cities in Saudi Arabia
                new City { Id = 1, Name = "Riyadh", CountryId = 1, VisitCount = 0, ImageUrl = "riyadh.jpg" },
                new City { Id = 2, Name = "Jeddah", CountryId = 1, VisitCount = 0, ImageUrl = "jeddah.jpg" },
                new City { Id = 3, Name = "Mecca", CountryId = 1, VisitCount = 0, ImageUrl = "mecca.jpg" },
                new City { Id = 4, Name = "Medina", CountryId = 1, VisitCount = 0, ImageUrl = "medina.jpg" },

                // Cities in UAE
                new City { Id = 5, Name = "Dubai", CountryId = 2, VisitCount = 0, ImageUrl = "dubai.jpg" },
                new City { Id = 6, Name = "Abu Dhabi", CountryId = 2, VisitCount = 0, ImageUrl = "abudhabi.jpg" },

                // Cities in Qatar
                new City { Id = 7, Name = "Doha", CountryId = 3, VisitCount = 0, ImageUrl = "doha.jpg" },

                // Cities in Kuwait
                new City { Id = 8, Name = "Kuwait City", CountryId = 4, VisitCount = 0, ImageUrl = "kuwait.jpg" },

                // Cities in Oman
                new City { Id = 9, Name = "Muscat", CountryId = 5, VisitCount = 0, ImageUrl = "muscat.jpg" },

                // Cities in Bahrain
                new City { Id = 10, Name = "Manama", CountryId = 6, VisitCount = 0, ImageUrl = "manama.jpg" },

                // Cities in Egypt
                new City { Id = 11, Name = "Cairo", CountryId = 7, VisitCount = 0, ImageUrl = "cairo.jpg" },
                new City { Id = 12, Name = "Alexandria", CountryId = 7, VisitCount = 0, ImageUrl = "alexandria.jpg" },

                // Cities in Jordan
                new City { Id = 13, Name = "Amman", CountryId = 8, VisitCount = 0, ImageUrl = "amman.jpg" },

                // Cities in Lebanon
                new City { Id = 14, Name = "Beirut", CountryId = 9, VisitCount = 0, ImageUrl = "beirut.jpg" },

                // Cities in Turkey
                new City { Id = 15, Name = "Istanbul", CountryId = 10, VisitCount = 0, ImageUrl = "istanbul.jpg" },
                new City { Id = 16, Name = "Ankara", CountryId = 10, VisitCount = 0, ImageUrl = "ankara.jpg" },

                // Cities in Morocco
                new City { Id = 17, Name = "Marrakech", CountryId = 11, VisitCount = 0, ImageUrl = "marrakech.jpg" },
                new City { Id = 18, Name = "Casablanca", CountryId = 11, VisitCount = 0, ImageUrl = "casablanca.jpg" },

                // Cities in Tunisia
                new City { Id = 19, Name = "Tunis", CountryId = 12, VisitCount = 0, ImageUrl = "tunis.jpg" },

                // Cities in Algeria
                new City { Id = 20, Name = "Algiers", CountryId = 13, VisitCount = 0, ImageUrl = "algiers.jpg" },

                // Cities in the United States
                new City { Id = 21, Name = "New York", CountryId = 14, VisitCount = 0, ImageUrl = "newyork.jpg" },
                new City { Id = 22, Name = "Los Angeles", CountryId = 14, VisitCount = 0, ImageUrl = "losangeles.jpg" },

                // Cities in the UK
                new City { Id = 23, Name = "London", CountryId = 15, VisitCount = 0, ImageUrl = "london.jpg" },

                // Cities in France
                new City { Id = 24, Name = "Paris", CountryId = 16, VisitCount = 0, ImageUrl = "paris.jpg" },

                // Cities in Germany
                new City { Id = 25, Name = "Berlin", CountryId = 17, VisitCount = 0, ImageUrl = "berlin.jpg" },

                // Cities in Canada
                new City { Id = 26, Name = "Toronto", CountryId = 18, VisitCount = 0, ImageUrl = "toronto.jpg" },

                // Cities in Australia
                new City { Id = 27, Name = "Sydney", CountryId = 19, VisitCount = 0, ImageUrl = "sydney.jpg" },

                // Cities in Japan
                new City { Id = 28, Name = "Tokyo", CountryId = 20, VisitCount = 0, ImageUrl = "tokyo.jpg" }
            );

            #endregion

            modelBuilder.Entity<RoomClass>()
      .Property(r => r.Id)
      .ValueGeneratedOnAdd();

            modelBuilder.Entity<ListingRoomClass>()
            .HasKey(rca => new { rca.RoomClassId, rca.ListingId });

            modelBuilder.Entity<Favorite>()
            .HasOne(f => f.Listing)
            .WithMany(l => l.Favorites)
            .HasForeignKey(f => f.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Listing>().HasMany(f => f.Favorites)
           .WithOne(l => l.Listing)
           .HasForeignKey(f => f.ListingId)
           .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bookings>()
            .HasOne(r => r.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bookings>()
            .HasOne(b => b.Listing)
            .WithMany(l => l.Bookings)
            .HasForeignKey(b => b.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bookings>()
            .HasOne(b => b.User)
            .WithMany(u => u.Bookings)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>().HasOne(r => r.Listing)
            .WithMany(h => h.Reviews)
            .HasForeignKey(r => r.ListingId)
            .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Room>()
            .HasOne(r => r.RoomClass)
            .WithMany(rc => rc.Rooms)
            .HasForeignKey(r => r.RoomClassId);



            modelBuilder.Entity<Room>()
                .HasOne(r => r.RoomClass)
                .WithMany(rc => rc.Rooms)
                .HasForeignKey(r => r.RoomClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomClassAmenity>()
                .HasKey(rca => new { rca.RoomClassId, rca.AmenityId, rca.ListingId });

            modelBuilder.Entity<RoomClassAmenity>()
                .HasOne(rca => rca.RoomClass)
                .WithMany(rc => rc.RoomClassAmenities)
                .HasForeignKey(rca => rca.RoomClassId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RoomClassAmenity>()
                .HasOne(rca => rca.Amenity)
                .WithMany(a => a.RoomClassAmenities)
                .HasForeignKey(rca => rca.AmenityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ListingImage>()
                .HasOne(li => li.Listing)
                .WithMany(l => l.Images)
                .HasForeignKey(li => li.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListingService>()
                .HasKey(ls => new { ls.ListingId, ls.ServiceId });

            modelBuilder.Entity<ListingService>()
                .HasOne(ls => ls.Listing)
                .WithMany(l => l.ListingServices)
                .HasForeignKey(ls => ls.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ListingService>()
             .HasOne(ls => ls.Service)
             .WithMany(s => s.ListingServices)
             .HasForeignKey(ls => ls.ServiceId)
             .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Booking)
                .WithMany(b => b.Invoices)
                .HasForeignKey(i => i.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invoice>()
                .HasOne(r => r.Room)
                .WithMany(i => i.Invoices)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BookingRoom>()
                .HasKey(br => new { br.BookingId, br.RoomId });


            modelBuilder.Entity<BookingRoom>()
                .HasOne(br => br.Room)
                .WithMany(r => r.BookingRooms)
                .HasForeignKey(br => br.RoomId)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<RoomImage>()
                .HasOne(ri => ri.Room)
                .WithMany(r => r.Images)
                .HasForeignKey(ri => ri.RoomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Agent)
                .WithMany(u => u.Listings)
                .HasForeignKey(l => l.AgentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<JoinUs>()
                .HasOne(j => j.Agent)
                .WithMany(u => u.JoinUsRequests)
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.AppUser)
                .WithMany(u => u.Favorites)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Listing)
                .WithMany(l => l.Favorites)
                .HasForeignKey(f => f.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Listing)
                .WithMany(l => l.Reviews)
                .HasForeignKey(r => r.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bookings>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Bookings>()
                .HasOne(b => b.Listing)
                .WithMany(l => l.Bookings)
                .HasForeignKey(b => b.ListingId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(co => co.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);





        }

        public DbSet<Blogs> Blogs { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Bookings> Bookings { get; set; }
        public DbSet<BookingRoom> BookingRooms { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<JoinUs> JoinUs { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<ListingImage> ListingImages { get; set; }
        public DbSet<ListingService> ListingServices { get; set; }
        public DbSet<ListingType> ListingTypes { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<RoomClass> RoomClasses { get; set; }
        public DbSet<RoomClassAmenity> RoomClassAmenities { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ListingRoomClass> ListingRoomClasses { get; set; }
        public DbSet<AmenityRequest> AmenityRequests { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

    }
}
