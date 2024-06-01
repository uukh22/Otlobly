using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.ViewModels;

namespace Otlobly.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Manager,Admin")]
    public class SubCatgoriesController : Controller
    {
        private readonly OtloblyContext _context;
        public SubCatgoriesController(OtloblyContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var subCategory = _context.SubCategories.Include(x => x.Category).ToList();

            return View(subCategory);
        }

        [HttpGet]
        public IActionResult Create()
        {
            SubCategoryViewModel viewModel = new SubCategoryViewModel();
            ViewBag.Category = new SelectList(_context.Categories, "Id", "Title");
            return View(viewModel);

        }
        [HttpPost]
        public IActionResult Create(SubCategoryViewModel viewModel)
        {
            SubCategory Model = new SubCategory();
            if (ModelState.IsValid)
            {
                Model.Title = viewModel.Title;
                Model.Category_Id = viewModel.CartegoryID;
                _context.Add(Model);
                _context.SaveChanges();

                return RedirectToAction("Index");

            }
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var subCategory = _context.SubCategories.Where(x => x.Id == Id).FirstOrDefault();
            SubCategoryViewModel viewModel = new SubCategoryViewModel();
            if (subCategory != null)
            {
                viewModel.Title = subCategory.Title;
                viewModel.Id = subCategory.Id;
                ViewBag.Category = new SelectList(_context.Categories, "Id", "Title", subCategory.Category_Id);
            }
            return View(viewModel);

        }
        [HttpPost]
        public IActionResult Edit(SubCategoryViewModel viewModel)
        {
            var Model = _context.SubCategories.Where(x => x.Id == viewModel.Id).FirstOrDefault();
            if (ModelState.IsValid)
            {
                Model.Title = viewModel.Title;
                Model.Category_Id = viewModel.CartegoryID;
                _context.Update(Model);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Delete(int Id)
        {
            var subCategory = _context.SubCategories.Where(x => x.Id == Id).FirstOrDefault();

            if (subCategory != null)
            {
                _context.SubCategories.Remove(subCategory);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}