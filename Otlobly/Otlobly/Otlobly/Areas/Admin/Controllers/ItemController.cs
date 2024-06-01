using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.ViewModels;
using Stripe;
using System;
using System.IO;

namespace Otlobly.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Manager")]
    [Authorize(Roles = "Manager,Admin")]
    public class ItemController : Controller
    {
        private readonly OtloblyContext _context;
        private IWebHostEnvironment _webHostEnvironment;

        public ItemController(OtloblyContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var Items = _context.Items.Include(x => x.Category).Include(y => y.SubCategory).Select(model => new ItemViewModel()
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Image = model.Image,
                price = model.Price,
                CategoryTitle = model.Category.Title,
                SubCategoryTitle = model.SubCategory.Title

            }).ToList();
            return View(Items);
        }


        //[HttpGet]
        //public IActionResult GetSubCategory(int CategoryId)
        //{
        //   var SubCategory=_context.SubCategories.Where(x=> x.Category_Id==CategoryId).FirstOrDefault();  
        //    return Json(SubCategory);

        //}

        [HttpGet]
        public IActionResult Create()
        {
            ItemViewModel viewModel = new ItemViewModel();
            ViewBag.Category = new SelectList(_context.Categories, "Id", "Title");
            ViewBag.SubCategory = new SelectList(_context.SubCategories, "Id", "Title");
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ItemViewModel itemViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var img_file = Request.Form.Files;
                    if (img_file != null && img_file.Count > 0)
                    {
                        var supportedImageTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };

                        var file = img_file[0];

                        // Check if the uploaded file is an image
                        if (file.ContentType.StartsWith("image/") && supportedImageTypes.Contains(file.ContentType))
                        {
                            using (var fileStream = file.OpenReadStream())
                            {
                                using (var memoryStream = new MemoryStream())
                                {
                                    await fileStream.CopyToAsync(memoryStream);
                                    itemViewModel.Image = memoryStream.ToArray();
                                }
                            }
                        }
                        else
                        {
                            // Unsupported file type, return validation error
                            ModelState.AddModelError("img_file", "Please select a profile picture of type .jpg, .jpeg, .png, or .gif");
                            // Return the view with validation errors
                            return View(itemViewModel);
                        }
                    }
                    else
                    {
                        // No file uploaded, return validation error
                        ModelState.AddModelError("img_file", "Please select a profile picture.");
                        // Return the view with validation errors
                        return View(itemViewModel);
                    }

                    var model = new Item
                    {
                        Title = itemViewModel.Title,
                        Description = itemViewModel.Description,
                        Price = itemViewModel.price,
                        CategoryId = itemViewModel.CategoryId,
                        SubcategoryId = itemViewModel.SubcategoryId,
                        Image = itemViewModel.Image // Assign the image data to the model
                    };

                    await _context.Items.AddAsync(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.exc = ex.Message;
                    ViewBag.Category = new SelectList(_context.Categories, "Id", "Title");
                    ViewBag.SubCategory = new SelectList(_context.SubCategories, "Id", "Title");
                    return View(itemViewModel);
                }
            }

            ViewBag.Category = new SelectList(_context.Categories, "Id", "Title");
            ViewBag.SubCategory = new SelectList(_context.SubCategories, "Id", "Title");
            return View(itemViewModel);
        }


        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var item = _context.Items.Where(x => x.Id == Id).FirstOrDefault();
            ItemViewModel viewModel = new ItemViewModel();
           
            if (item != null)
            {
                viewModel.Title = item.Title;
                viewModel.Description = item.Description;
                viewModel.price = item.Price;
                viewModel.Image = item.Image;
                viewModel.CategoryId = item.CategoryId;
                viewModel.SubcategoryId = item.SubcategoryId;

                ViewBag.Category = new SelectList(_context.Categories, "Id", "Title", item.CategoryId);
                ViewBag.SubCategory = new SelectList(_context.SubCategories, "Id", "Title", item.SubcategoryId);
            }
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(ItemViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Return the view with the current model to display validation errors
            }

            var itemm = await _context.Items.FirstOrDefaultAsync(x => x.Id == model.Id);
            if (itemm == null)
            {
                return NotFound($"Item with ID {model.Id} was not found.");
            }

            var files = Request.Form.Files;
            if (files.Count > 0)
            {
                var file = files[0];
                var permittedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!permittedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Invalid file type. Only JPG, JPEG, PNG, and GIF are allowed.");
                    return View(model);
                }

                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    itemm.Image = memoryStream.ToArray();
                }
            }

            // Update other properties
            itemm.Price = model.price;
            itemm.Title = model.Title;
            itemm.Description = model.Description;
            itemm.CategoryId = model.CategoryId;
            itemm.SubcategoryId = model.SubcategoryId;

            try
            {
                _context.Items.Update(itemm);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving the item. Please try again.");
                return View(model);
            }
        }



        [HttpGet]
        public IActionResult Delete(int Id)
        {
            
            var item = _context.Items.Where(x => x.Id == Id).FirstOrDefault();
            
                    _context.Items.Remove(item);
                    _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
