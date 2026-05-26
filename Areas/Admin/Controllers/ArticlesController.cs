using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rhupolomolok.Data;
using rhupolomolok.Models;

namespace rhupolomolok.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrator")]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // List all pending articles
        public async Task<IActionResult> Pending()
        {
            var pending = await _context.Articles
                .Where(a => a.Status == "Pending")
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return View(pending);
        }

        // Review a single article
        public async Task<IActionResult> Review(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            return View(article);
        }

        // Approve article
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            article.Status = "Approved";
            article.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Pending");
        }

        // Reject article
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            article.Status = "Rejected";
            article.IsSubmitted = false; // allow editing again
            article.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Pending");
        }
    }
}
