

namespace Booking.Controllers
{
    public class ListingController : Controller
    {
        private readonly HotelDbContext _db;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWebHostEnvironment _env;

        public ListingController(HotelDbContext db, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _env = env;
        }

        // GET: Listing/Create
        public IActionResult Create()
        {
            ViewBag.Countries = new SelectList(_db.Countries, "Id", "Name");
            ViewBag.ListingTypes = new SelectList(_db.ListingTypes, "Id", "Name");
            ViewBag.Services = new MultiSelectList(_db.Services, "Id", "Description");
            ViewBag.Amenities = new MultiSelectList(_db.Amenities, "Id", "Name");
            ViewBag.ServiceTypes = Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() });
            return View(new ListingVM());
        }

        // POST: Listing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ListingVM model)
        {
            if (ModelState.IsValid)
            {
                // Validate CityId
                if (!_db.Cities.Any(c => c.Id == model.CityId))
                {
                    ModelState.AddModelError("CityId", "The selected city does not exist.");
                    ViewBag.Countries = new SelectList(_db.Countries, "Id", "Name", model.CityId);
                    ViewBag.ListingTypes = new SelectList(_db.ListingTypes, "Id", "Name", model.ListingTypeId);
                    ViewBag.Services = new MultiSelectList(_db.Services, "Id", "Description", model.SelectedServices);
                    ViewBag.Amenities = new MultiSelectList(_db.Amenities, "Id", "Name", model.SelectedAmenities);
                    ViewBag.ServiceTypes = Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() });
                    return View(model);
                }

                // Get the current user
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                // Check if the user already has a pending JoinUs request
                var existingRequest = _db.JoinUses.FirstOrDefault(j => j.UserId == user.Id && j.Status == Status.Pending);
                if (existingRequest != null)
                {
                    ModelState.AddModelError(string.Empty, "لديك طلب انضمام معلق سابق.");
                    ViewBag.Countries = new SelectList(_db.Countries, "Id", "Name", model.CityId);
                    ViewBag.ListingTypes = new SelectList(_db.ListingTypes, "Id", "Name", model.ListingTypeId);
                    ViewBag.Services = new MultiSelectList(_db.Services, "Id", "Description", model.SelectedServices);
                    ViewBag.Amenities = new MultiSelectList(_db.Amenities, "Id", "Name", model.SelectedAmenities);
                    ViewBag.ServiceTypes = Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() });
                    return View(model);
                }

                #region Handling JoinUs
                var joinUs = new JoinUs
                {
                    UserId = user.Id,
                    Status = Status.Pending,
                    Reason = model.Reason
                };
                if (model.OwnershipProof != null)
                {
                    joinUs.OwnerShipProof = await UploadFile(model.OwnershipProof, "OwnershipProof");
                }
                if (model.GovernmentCertificate != null)
                {
                    joinUs.GovermentCertificate = await UploadFile(model.GovernmentCertificate, "GovernmentCertificate");
                }
                if (model.IdentityProof != null)
                {
                    joinUs.IdentityProof = await UploadFile(model.IdentityProof, "IdentityProof");
                }
                _db.JoinUses.Add(joinUs);
                await _db.SaveChangesAsync();
                #endregion

                #region Handling Listing
                var listing = new Listing
                {
                    Name = model.Name,
                    Description = model.Description,
                    CityId = model.CityId,
                    Street = model.Street,
                    TotalRoom = model.TotalRoom,
                    TotalFloor = model.TotalFloor,
                    PhoneNo = model.PhoneNo,
                    Rating = model.Rating,
                    ListingTypeId = model.ListingTypeId,
                    Longitude = model.Longitude,
                    Latitude = model.Latitude,
                    AgentId = user.Id,
                    JoinUsId = joinUs.Id,
                    IsActive = false
                };
                _db.Listings.Add(listing);
                await _db.SaveChangesAsync();
                #endregion

                #region Handling Listing Images
                if (model.ListingImages != null && model.ListingImages.Count > 0)
                {
                    foreach (var image in model.ListingImages)
                    {
                        var imagePath = await UploadFile(image, "ListingImages");
                        var listingImage = new ListingImage
                        {
                            ListingId = listing.Id,
                            ImagePath = imagePath
                        };
                        _db.ListingImages.Add(listingImage);
                    }
                    await _db.SaveChangesAsync();
                }
                #endregion

                #region Handling Custom Services
                foreach (var (description, type) in model.CustomServices)
                {
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        var service = new Service
                        {
                            Description = description,
                            Type = type
                        };
                        _db.Services.Add(service);
                        await _db.SaveChangesAsync();
                        model.SelectedServices.Add(service.Id); // Add the new service to the selected list
                    }
                }
                #endregion

                #region Adding Services to the Listing
                foreach (var serviceId in model.SelectedServices)
                {
                    _db.ListingServices.Add(new ListingService { ListingId = listing.Id, ServiceId = serviceId });
                }
                await _db.SaveChangesAsync();
                #endregion

                #region Handling Custom Amenities
                foreach (var amenityName in model.CustomAmenities)
                {
                    if (!string.IsNullOrWhiteSpace(amenityName))
                    {
                        var amenity = new Amenity
                        {
                            Name = amenityName
                        };
                        _db.Amenities.Add(amenity);
                        await _db.SaveChangesAsync();
                        model.SelectedAmenities.Add(amenity.Id); // Add the new amenity to the selected list
                    }
                }
                #endregion

                #region Adding Amenities to RoomClasses
                foreach (var roomClassVM in model.RoomClasses)
                {
                    var roomClass = new RoomClass
                    {
                        Name = roomClassVM.Name,
                        DiscountId = roomClassVM.DiscountId
                    };
                    foreach (var amenityId in roomClassVM.SelectedAmenities)
                    {
                        roomClass.RoomClassAmenities.Add(new RoomClassAmenity
                        {
                            AmenityId = amenityId
                        });
                    }
                    listing.RoomClasses.Add(roomClass);
                }
                await _db.SaveChangesAsync();
                #endregion

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Countries = new SelectList(_db.Countries, "Id", "Name", model.CityId);
            ViewBag.ListingTypes = new SelectList(_db.ListingTypes, "Id", "Name", model.ListingTypeId);
            ViewBag.Services = new MultiSelectList(_db.Services, "Id", "Description", model.SelectedServices);
            ViewBag.Amenities = new MultiSelectList(_db.Amenities, "Id", "Name", model.SelectedAmenities);
            ViewBag.ServiceTypes = Enum.GetValues(typeof(ServiceType)).Cast<ServiceType>().Select(s => new SelectListItem { Text = s.ToString(), Value = ((int)s).ToString() });
            return View(model);
        }

        // Admin Approval Action
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var joinUsRequest = await _db.JoinUses.Include(j => j.Listing).FirstOrDefaultAsync(j => j.Id == id);
            if (joinUsRequest == null) return NotFound();

            // Activate the listing
            if (joinUsRequest.Listing != null)
            {
                joinUsRequest.Listing.IsActive = true;
                _db.Listings.Update(joinUsRequest.Listing);
            }

            // Update the user's role to Agent
            var user = await _userManager.FindByIdAsync(joinUsRequest.UserId);
            if (user != null)
            {
                if (!await _userManager.IsInRoleAsync(user, "Agent"))
                {
                    await _userManager.AddToRoleAsync(user, "Agent");
                }
            }

            // Update the status of the JoinUs request
            joinUsRequest.Status = Status.Approved;
            _db.JoinUses.Update(joinUsRequest);

            // Send a message to the user
            await SendMessageToUser(user, "تم الموافقة على إقامتتك. يمكنك الآن إضافة الغرف.");

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Admin Reject Action
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var joinUsRequest = await _db.JoinUses.Include(j => j.Listing).FirstOrDefaultAsync(j => j.Id == id);
            if (joinUsRequest == null) return NotFound();

            // Optionally, delete the listing if not approved
            if (joinUsRequest.Listing != null)
            {
                _db.Listings.Remove(joinUsRequest.Listing);
            }

            // Update the status of the JoinUs request
            joinUsRequest.Status = Status.Rejected;
            _db.JoinUses.Update(joinUsRequest);

            // Send a message to the user
            var user = await _userManager.FindByIdAsync(joinUsRequest.UserId);
            if (user != null)
            {
                await SendMessageToUser(user, "تم رفض طلب إقامتتك.");
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Admin Cleanup Action (Delete pending requests older than 30 days)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CleanupPendingRequests()
        {
            var cutoffDate = DateTime.Now.AddDays(-30);
            var pendingRequests = _db.JoinUses.Where(j => j.Status == Status.Pending && j.DateOfSubmission < cutoffDate).ToList();
            foreach (var request in pendingRequests)
            {
                // Optionally, delete the listing if not approved
                if (request.Listing != null)
                {
                    _db.Listings.Remove(request.Listing);
                }
                _db.JoinUses.Remove(request);
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Listing/AddRooms/{id}
        [Authorize(Roles = "Agent")]
        public IActionResult AddRooms(int id)
        {
            var listing = _db.Listings.Include(l => l.RoomClasses).FirstOrDefault(l => l.Id == id);
            if (listing == null || !listing.IsActive) return NotFound();
            ViewBag.RoomClasses = new SelectList(listing.RoomClasses, "Id", "Name");
            return View(new AddRoomVM { ListingId = id });
        }

        // POST: Listing/AddRooms/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> AddRooms(int id, AddRoomVM model)
        {
            var listing = await _db.Listings.Include(l => l.RoomClasses).FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null || !listing.IsActive) return NotFound();

            // Add rooms to the listing
            foreach (var roomDto in model.Rooms)
            {
                var room = new Room
                {
                    RoomNo = roomDto.RoomNo,
                    PricePerNight = roomDto.PricePerNight,
                    AdultsCapacity = roomDto.AdultsCapacity,
                    ChildrenCapacity = roomDto.ChildrenCapacity,
                    RoomSize = roomDto.RoomSize,
                    RoomClassId = roomDto.RoomClassId
                };

                // Handle room image uploads
                if (roomDto.RoomImages != null && roomDto.RoomImages.Count > 0)
                {
                    foreach (var image in roomDto.RoomImages)
                    {
                        var imagePath = await UploadFile(image, "RoomImages");
                        room.Images.Add(new RoomImage
                        {
                            ImagePath = imagePath
                        });
                    }
                }

                _db.Rooms.Add(room);
            }
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = listing.Id });
        }

        // GET: Listing/EditRoom/{id}
        [Authorize(Roles = "Agent")]
        public IActionResult EditRoom(int id)
        {
            var room = _db.Rooms
                .Include(r => r.RoomClass)
                .Include(r => r.Images)
                .FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();

            var roomVM = new RoomVM
            {
                Id = room.Id,
                RoomNo = room.RoomNo,
                PricePerNight = room.PricePerNight,
                AdultsCapacity = room.AdultsCapacity,
                ChildrenCapacity = room.ChildrenCapacity,
                RoomSize = room.RoomSize,
                RoomClassId = room.RoomClassId,
                ExistingRoomImages = room.Images.Select(i => i.ImagePath).ToList()
            };

            ViewBag.RoomClasses = new SelectList(_db.RoomClasses, "Id", "Name", room.RoomClassId);
            return View(roomVM);
        }

        // POST: Listing/EditRoom/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> EditRoom(int id, RoomVM roomVM, List<IFormFile> roomImages)
        {
            var room = _db.Rooms
                .Include(r => r.RoomClass)
                .Include(r => r.Images)
                .FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();

            // Update room details
            room.RoomNo = roomVM.RoomNo;
            room.PricePerNight = roomVM.PricePerNight;
            room.AdultsCapacity = roomVM.AdultsCapacity;
            room.ChildrenCapacity = roomVM.ChildrenCapacity;
            room.RoomSize = roomVM.RoomSize;
            room.RoomClassId = roomVM.RoomClassId;

            // Handle new room image uploads
            if (roomImages != null && roomImages.Count > 0)
            {
                foreach (var image in roomImages)
                {
                    var imagePath = await UploadFile(image, "RoomImages");
                    room.Images.Add(new RoomImage
                    {
                        ImagePath = imagePath
                    });
                }
            }

            _db.Rooms.Update(room);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = room.ListingId });
        }

        // GET: Listing/DeleteRoom/{id}
        [Authorize(Roles = "Agent")]
        public IActionResult DeleteRoom(int id)
        {
            var room = _db.Rooms
                .Include(r => r.RoomClass)
                .Include(r => r.Images)
                .FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();

            ViewBag.RoomClassId = room.RoomClassId;
            return View(room);
        }

        // POST: Listing/DeleteRoom/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteRoom")]
        [Authorize(Roles = "Agent")]
        public async Task<IActionResult> DeleteRoomConfirmed(int id)
        {
            var room = _db.Rooms
                .Include(r => r.RoomClass)
                .Include(r => r.Images)
                .FirstOrDefault(r => r.Id == id);
            if (room == null) return NotFound();

            // Remove room images
            if (room.Images != null && room.Images.Count > 0)
            {
                foreach (var image in room.Images)
                {
                    var imagePath = Path.Combine(_env.WebRootPath, image.ImagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    _db.RoomImages.Remove(image);
                }
            }

            _db.Rooms.Remove(room);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = room.ListingId });
        }

        // GET: Listing/Index
        public IActionResult Index()
        {
            var listings = _db.Listings
                .Include(l => l.City)
                .Include(l => l.ListingType)
                .Include(l => l.RoomClasses)
                .Include(l => l.RoomClasses).ThenInclude(rc => rc.Rooms)
                .Include(l => l.RoomClasses).ThenInclude(rc => rc.Rooms).ThenInclude(r => r.Images)
                .Include(l => l.RoomClasses).ThenInclude(rc => rc.RoomClassAmenities).ThenInclude(rca => rca.Amenity)
                .Include(l => l.Services).ThenInclude(ls => ls.Service)
                .Include(l => l.Images)
                .Include(l => l.Agent)
                .Include(l => l.Reviews).ThenInclude(r => r.User)
                .Where(l => l.RoomClasses.Any())
                .ToList();
            return View(listings);
        }

        // GET: Listing/Details/{id}
        public IActionResult Details(int id)
        {
            var listing = _db.Listings
                .Include(l => l.City)
                .Include(l => l.ListingType)
                .Include(l => l.RoomClasses)
                .Include(l => l.RoomClasses).ThenInclude(rc => rc.Rooms)
                .Include(l => l.RoomClasses).ThenInclude(rc => rc.Rooms).ThenInclude(r => r.Images)
                .Include(l => l.RoomClasses).ThenInclude(rc => rc.RoomClassAmenities).ThenInclude(rca => rca.Amenity)
                .Include(l => l.Services).ThenInclude(ls => ls.Service)
                .Include(l => l.Images)
                .Include(l => l.Agent)
                .Include(l => l.Reviews).ThenInclude(r => r.User)
                .FirstOrDefault(l => l.Id == id);
            if (listing == null) return NotFound();
            return View(listing);
        }

        // Helper Method: Upload File
        private async Task<string> UploadFile(IFormFile file, string folderName)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExtension = Path.GetExtension(file.FileName).ToLower();
            if (!allowedExtensions.Contains(fileExtension))
            {
                throw new InvalidOperationException("امتداد الملف غير صالح. يرجى تحميل ملف JPG، JPEG، PNG، أو PDF.");
            }
            long maxSizeInBytes = 5 * 1024 * 1024;
            if (file.Length > maxSizeInBytes)
            {
                throw new InvalidOperationException("حجم الملف كبير جدًا. الحجم الأقصى المسموح به هو 5 ميجابايت.");
            }
            var folder = $"Images/{folderName}";
            var fileName = Guid.NewGuid().ToString() + fileExtension;
            var filePath = Path.Combine(_env.WebRootPath, folder, fileName);
            if (!Directory.Exists(Path.Combine(_env.WebRootPath, folder)))
            {
                Directory.CreateDirectory(Path.Combine(_env.WebRootPath, folder));
            }
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            return Path.Combine(folder, fileName).Replace("\\", "/");
        }

        // Helper Method: Send Message to User
        private async Task SendMessageToUser(AppUser user, string message)
        {
            // Implement your messaging logic here (e.g., email, notification)
            // For demonstration, we'll just log the message
            Console.WriteLine($"رسالة إلى {user.Email}: {message}");
        }
    }

    // ViewModel for Listing
    public class ListingVM
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        public int CityId { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public int TotalRoom { get; set; }
        [Required]
        public int TotalFloor { get; set; }
        [Phone]
        public string PhoneNo { get; set; }
        [Required, Range(1, 5)]
        public int Rating { get; set; }
        [Required]
        public int ListingTypeId { get; set; }
        [Required]
        public List<int> SelectedServices { get; set; } = new List<int>();
        [Required]
        public List<int> SelectedAmenities { get; set; } = new List<int>();
        [Required]
        public List<RoomClassDto> RoomClasses { get; set; } = new List<RoomClassDto>();
        public List<string> CustomAmenities { get; set; } = new List<string>();
        public List<(string Description, ServiceType Type)> CustomServices { get; set; } = new List<(string Description, ServiceType Type)>();
        public IFormFile OwnershipProof { get; set; }
        public IFormFile GovernmentCertificate { get; set; }
        public IFormFile IdentityProof { get; set; }
        public List<IFormFile> ListingImages { get; set; } = new List<IFormFile>();
        [Required]
        public string Reason { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }

    // DTO for Room Class
    public class RoomClassDto
    {
        [Required, MaxLength(50)]
        public string Name { get; set; }
        public int? DiscountId { get; set; }
        public List<int> SelectedAmenities { get; set; } = new List<int>();
        public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
    }

    // DTO for Room
    public class RoomDto
    {
        [Required, MaxLength(10)]
        public string RoomNo { get; set; }
        [Required, Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int AdultsCapacity { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int ChildrenCapacity { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int RoomSize { get; set; }
        [Required]
        public int RoomClassId { get; set; }
        public List<IFormFile> RoomImages { get; set; } = new List<IFormFile>();
    }

    // ViewModel for Adding Rooms
    public class AddRoomVM
    {
        public int ListingId { get; set; }
        public List<RoomDto> Rooms { get; set; } = new List<RoomDto>();
    }

    // ViewModel for Editing Rooms
    public class RoomVM
    {
        public int Id { get; set; }
        [Required, MaxLength(10)]
        public string RoomNo { get; set; }
        [Required, Range(0, double.MaxValue)]
        public decimal PricePerNight { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int AdultsCapacity { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int ChildrenCapacity { get; set; }
        [Required, Range(0, int.MaxValue)]
        public int RoomSize { get; set; }
        [Required]
        public int RoomClassId { get; set; }
        public List<string> ExistingRoomImages { get; set; } = new List<string>();
    }
}