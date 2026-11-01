
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP306_Lab03_LearningPortal.Data;
using COMP306_Lab03_LearningPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;


namespace COMP306_Lab03_LearningPortal.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly LearningPortalContext _context;

        public ResourcesController(LearningPortalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string sort = "newest",
            string title = "",
            string author = "")
        {
            title = (title ?? "").Trim();
            author = (author ?? "").Trim();

            var query = from r in _context.LearningResources
                        join c in _context.Categories
                            on r.CategoryID equals c.CategoryID
                        join u in _context.Users
                            on r.AuthorID equals u.UserID
                        select new ResourceListViewModel
                        {
                            ResourceID = r.ResourceID,
                            AuthorID = r.AuthorID,
                            Title = r.Title,
                            Description = r.Description,
                            CategoryName = c.CategoryName,
                            AuthorName = u.Username,
                            CreatedDate = r.CreatedDate,
                            ViewCount = r.ViewCount
                        };

            // Search by title or topic
            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(r =>
                    r.Title.Contains(title) ||
                    r.Description.Contains(title));
            }

            // Search by author
            if (!string.IsNullOrEmpty(author))
            {
                query = query.Where(r =>
                    r.AuthorName.Contains(author));
            }

            // Sort resources
            if (sort == "popular")
            {
                query = query.OrderByDescending(r => r.ViewCount)
                             .ThenByDescending(r => r.ResourceID);
            }
            else
            {
                query = query.OrderByDescending(r => r.CreatedDate)
                             .ThenByDescending(r => r.ResourceID);
            }

            ViewBag.Sort = sort == "popular" ? "popular" : "newest";
            ViewBag.SearchTitle = title;
            ViewBag.SearchAuthor = author;

            var resources = await query.ToListAsync();

            return View(resources);
        }


        // Display the Add Resource page
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create()
        {
            await LoadCategories();
            return View();
        }

        // Save a new resource
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ResourceCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == model.CategoryID);

            if (!categoryExists)
            {
                ModelState.AddModelError("CategoryID", "Invalid category.");
                await LoadCategories();
                return View(model);
            }

            var resource = new LearningResource
            {
                Title = model.Title.Trim(),
                Description = model.Description.Trim(),
                CategoryID = model.CategoryID,
                AuthorID = userId,
                CreatedDate = DateTime.UtcNow,
                ViewCount = 0,
                S3Key = null
            };

            _context.LearningResources.Add(resource);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Load categories for the dropdown
        private async Task LoadCategories()
        {
            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            ViewBag.Categories = new SelectList(
                categories, "CategoryID", "CategoryName");
        }

        // Show Edit Resource page
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(int id)
        {
            var resource = await _context.LearningResources.FindAsync(id);

            if (resource == null)
                return NotFound();

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            if (resource.AuthorID != userId)
                return Forbid();

            var model = new ResourceCreateViewModel
            {
                Title = resource.Title,
                Description = resource.Description,
                CategoryID = resource.CategoryID
            };

            await LoadCategories();
            return View(model);
        }

        // Save edited resource
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id, ResourceCreateViewModel model)
        {
            var resource = await _context.LearningResources.FindAsync(id);

            if (resource == null)
                return NotFound();

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            if (resource.AuthorID != userId)
                return Forbid();

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryID == model.CategoryID);

            if (!categoryExists)
                ModelState.AddModelError("CategoryID", "Invalid category.");

            if (!ModelState.IsValid)
            {
                await LoadCategories();
                return View(model);
            }

            resource.Title = model.Title.Trim();
            resource.Description = model.Description.Trim();
            resource.CategoryID = model.CategoryID;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // Show Delete confirmation page
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int id)
        {
            var resource = await _context.LearningResources.FindAsync(id);

            if (resource == null)
                return NotFound();

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            if (resource.AuthorID != userId)
                return Forbid();

            return View(resource);
        }

        // Delete resource after confirmation
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var resource = await _context.LearningResources.FindAsync(id);

            if (resource == null)
                return NotFound();

            int userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            if (resource.AuthorID != userId)
                return Forbid();

            _context.LearningResources.Remove(resource);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }


        // View resource details and update view count
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var resource = await _context.LearningResources
                .FindAsync(id);

            if (resource == null)
                return NotFound();

            // Increase views by 1
            resource.ViewCount++;

            await _context.SaveChangesAsync();

            var category = await _context.Categories
                .FindAsync(resource.CategoryID);

            var author = await _context.Users
                .FindAsync(resource.AuthorID);

            var model = new ResourceListViewModel
            {
                ResourceID = resource.ResourceID,
                AuthorID = resource.AuthorID,
                Title = resource.Title,
                Description = resource.Description,
                CategoryName = category?.CategoryName ?? "",
                AuthorName = author?.Username ?? "",
                CreatedDate = resource.CreatedDate,
                ViewCount = resource.ViewCount
            };

            return View(model);
        }


    }
}
