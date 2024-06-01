using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Otlobly.Models;
using Otlobly.Repository;


namespace Otlobly.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Manager,Admin")]
    public class CouponController : Controller
    {
        private readonly OtloblyContext _dbContext;

        public CouponController(OtloblyContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var promocode = _dbContext.Coupons.ToList();
            return View(promocode);
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var files = Request.Form.Files;

                if (files.Count > 0)
                {
                    var supportedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };

                    var file = files[0];

                    // Check if the uploaded file is an image
                    if (file.ContentType.StartsWith("image/") && supportedImageTypes.Contains(file.ContentType))
                    {
                        using (var fileStream = file.OpenReadStream())
                        {
                            using (var memoryStream = new MemoryStream())
                            {
                                await fileStream.CopyToAsync(memoryStream);
                                coupon.Coupon_Picture = memoryStream.ToArray();
                            }
                        }

                        _dbContext.Coupons.Add(coupon);
                        await _dbContext.SaveChangesAsync();
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        // Unsupported file type, return validation error
                        ModelState.AddModelError("Coupon_Picture", "Please select a profile picture of type .jpg, .jpeg, .png, or .gif");
                    }
                }
                else
                {
                    // No file uploaded, return validation error
                    ModelState.AddModelError("Coupon_Picture", "Please select a profile picture.");
                }
            }

            // If model state is not valid, return to the view with errors
            return View(coupon);

        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var coupon = _dbContext.Coupons.FirstOrDefault(x => x.Coupon_Id == id);
            if (coupon != null)
            {
                _dbContext.Coupons.Remove(coupon);
                _dbContext.SaveChanges();
            }
            else
            {
                return NotFound();
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            var copun = _dbContext.Coupons.Where(x => x.Coupon_Id == Id).FirstOrDefault();
            if (copun != null)
            {
                return View(copun);
            }
            else return NotFound();

        }
        [HttpPost]
        public IActionResult Edit(Coupon model)

        {
            var copun = _dbContext.Coupons.Where(x => x.Coupon_Id == model.Coupon_Id).FirstOrDefault();

            byte[] Photo = null;
            if (ModelState.IsValid)
            {
                var files = Request.Form.Files;
                if (files.Count > 0)
                {
                    using (var fileStream = files[0].OpenReadStream())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            fileStream.CopyTo(memoryStream);
                            Photo = memoryStream.ToArray();
                        }
                    }
                    copun.Coupon_Picture = Photo;
                }

                copun.MinimumAmount = model.MinimumAmount;
                copun.Discount = model.Discount;
                copun.IsActive = model.IsActive;
                copun.Title = model.Title;
                copun.Coupon_Type = model.Coupon_Type;

                _dbContext.Coupons.Update(copun);
                _dbContext.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(copun);
        }

    }

}