using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Otlobly.Models;
using Otlobly.Repository;
using Otlobly.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;

namespace Otlobly.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Manager,Admin")]
    public class CategoriesController : Controller
    {
        private readonly OtloblyContext _context;
        public CategoriesController(OtloblyContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            var ListFromDb = _context.Categories.ToList().Select(
                x => new CategoryViewModel()
                {
                    Id = x.Id,
                    Title = x.Title
                }).ToList();

            return View(ListFromDb);
        }
        [HttpGet]
        public IActionResult Create()
        {
            CategoryViewModel Categoty = new CategoryViewModel();
            return View(Categoty);
        }
        [HttpPost]
        public IActionResult Create(CategoryViewModel Categoty)
        {
            Category model=new Category();
            model.Title = Categoty.Title;
            _context.Categories.Add(model);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Edit(int Id)
        {
            var catViewModel = _context.Categories.Where(x => x.Id == Id)
                .Select(x => new CategoryViewModel()
                {
                    Id = x.Id,
                    Title = x.Title

                }).FirstOrDefault();

            return View(catViewModel);
        }
        [HttpPost]
        public IActionResult Edit(CategoryViewModel Categoty)
        {

            if (ModelState.IsValid)
            {
                var categoryFromDb = _context.Categories.FirstOrDefault(x => x.Id == Categoty.Id);
                if (categoryFromDb != null)
                {
                    categoryFromDb.Title = Categoty.Title;
                    _context.Categories.Update(categoryFromDb);
                    _context.SaveChanges();
                }
            }
        return RedirectToAction("Index");   
        }
        [HttpGet]
        public IActionResult Delete(int Id)
        {
            var Category = _context.Categories.Where(x => x.Id == Id).FirstOrDefault();
            var subcategory=_context.SubCategories.Where(x=>x.Category_Id==Id).ToList();
            var item = _context.Items.Where(x => x.CategoryId == Id).ToList();
            foreach(var it in item)
            {
                if (it != null)
                {
                    _context.Items.Remove(it);
                    _context.SaveChanges();
                }
            }
            foreach (var Sub in subcategory)
            {
                if (Sub != null)
                {
                    _context.SubCategories.Remove(Sub);
                    _context.SaveChanges();
                }
            }
            if(Category!=null)
            {
                _context.Categories.Remove(Category);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

    }
}
