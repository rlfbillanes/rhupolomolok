using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rhupolomolok.Data;
using rhupolomolok.Models;
using Microsoft.EntityFrameworkCore;

namespace rhupolomolok.Areas.Contributor.Controllers
{
    [Area("Contributor")]
    [Authorize(Roles = "Contributor,Administrator")]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ArticlesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ---------------------------------------------------------
        // LIST OF ARTICLES FOR THIS CONTRIBUTOR
        // ---------------------------------------------------------
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            var articles = await _context.Articles
                .Where(a => a.CreatedByUserId == userId)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return View(articles);
        }

        // ---------------------------------------------------------
        // CREATE (GET)
        // ---------------------------------------------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ---------------------------------------------------------
        // CREATE (POST) — FIXED
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Article model)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.CreatedByUserId = _userManager.GetUserId(User);
            model.Status = "Draft";
            model.CreatedDate = DateTime.UtcNow;
            model.IsSubmitted = false;

            _context.Articles.Add(model);
            await _context.SaveChangesAsync();

            // Redirect to Edit so auto-save can begin
            return RedirectToAction("Edit", new { id = model.Id });
        }

        // ---------------------------------------------------------
        // EDIT (GET)
        // ---------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User);

            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.Id == id && a.CreatedByUserId == userId);

            if (article == null)
                return NotFound();

            return View(article);
        }

        // ---------------------------------------------------------
        // EDIT (POST) — Only used if manually saving
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Article model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var article = await _context.Articles.FindAsync(model.Id);
            if (article == null)
                return NotFound();

            if (article.IsSubmitted)
                return RedirectToAction("Index");

            article.Title = model.Title;
            article.Category = model.Category;
            article.Content = model.Content;
            article.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        // ---------------------------------------------------------
        // AUTO-SAVE ENDPOINT
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> AutoSave([FromBody] Article model)
        {
            var article = await _context.Articles.FindAsync(model.Id);
            if (article == null)
                return NotFound();

            if (article.IsSubmitted)
                return BadRequest("Cannot edit submitted article.");

            article.Title = model.Title;
            article.Category = model.Category;
            article.Content = model.Content;
            article.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // ---------------------------------------------------------
        // SUBMIT FOR REVIEW
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> SubmitFinal(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article == null)
                return NotFound();

            article.Status = "Pending";
            article.IsSubmitted = true;
            article.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
