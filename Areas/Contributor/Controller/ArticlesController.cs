using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rhupolomolok.Data;
using rhupolomolok.Models;

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
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return View();
        }

        // ---------------------------------------------------------
        // CREATE (POST)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Article model,
            List<IFormFile> ImageFiles,
            IFormFile? VideoFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            model.CreatedByUserId = _userManager.GetUserId(User);
            model.CreatedDate = DateTime.UtcNow;
            model.Status = "Draft";

            _context.Articles.Add(model);
            await _context.SaveChangesAsync();

            var folder = Path.Combine("wwwroot/uploads/articles", model.Id.ToString());
            Directory.CreateDirectory(folder);

            // IMAGE UPLOADS
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                if (ImageFiles.Count > 3)
                {
                    ModelState.AddModelError("", "Maximum of 3 images allowed.");
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    return View(model);
                }

                foreach (var img in ImageFiles)
                {
                    if (!img.FileName.ToLower().EndsWith(".jpg"))
                    {
                        ModelState.AddModelError("", "Only JPG images are allowed.");
                        ViewBag.Categories = await _context.Categories.ToListAsync();
                        return View(model);
                    }

                    var fileName = Guid.NewGuid() + ".jpg";
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }

                    _context.ArticleMedia.Add(new ArticleMedia
                    {
                        ArticleId = model.Id,
                        MediaType = "Image",
                        FilePath = $"/uploads/articles/{model.Id}/{fileName}",
                        FileSize = img.Length
                    });
                }
            }

            // VIDEO UPLOAD
            if (VideoFile != null)
            {
                if (!VideoFile.FileName.ToLower().EndsWith(".mp4"))
                {
                    ModelState.AddModelError("", "Only MP4 videos are allowed.");
                    ViewBag.Categories = await _context.Categories.ToListAsync();
                    return View(model);
                }

                var fileName = Guid.NewGuid() + ".mp4";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(stream);
                }

                _context.ArticleMedia.Add(new ArticleMedia
                {
                    ArticleId = model.Id,
                    MediaType = "Video",
                    FilePath = $"/uploads/articles/{model.Id}/{fileName}",
                    FileSize = VideoFile.Length
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Record was saved successfully.";
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

            ViewBag.Categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            ViewBag.Media = await _context.ArticleMedia
                .Where(m => m.ArticleId == id)
                .ToListAsync();

            return View(article);
        }

        // ---------------------------------------------------------
        // EDIT (POST)
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Article model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = await _context.Categories.ToListAsync();
                return View(model);
            }

            var article = await _context.Articles.FindAsync(model.Id);
            if (article == null)
                return NotFound();

            if (article.IsSubmitted)
                return RedirectToAction("Index");

            article.Title = model.Title;
            article.CategoryId = model.CategoryId;
            article.Content = model.Content;
            article.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Changes saved successfully.";
            return RedirectToAction("Edit", new { id = model.Id });
        }

        // ---------------------------------------------------------
        // UPLOAD MEDIA
        // ---------------------------------------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMedia(
            int articleId,
            List<IFormFile> ImageFiles,
            IFormFile? VideoFile)
        {
            var article = await _context.Articles.FindAsync(articleId);
            if (article == null)
                return NotFound();

            var folder = Path.Combine("wwwroot/uploads/articles", articleId.ToString());
            Directory.CreateDirectory(folder);

            var existingImages = await _context.ArticleMedia
                .Where(m => m.ArticleId == articleId && m.MediaType == "Image")
                .CountAsync();

            var existingVideos = await _context.ArticleMedia
                .Where(m => m.ArticleId == articleId && m.MediaType == "Video")
                .CountAsync();

            // IMAGE LIMIT
            if (ImageFiles != null && ImageFiles.Count > 0)
            {
                int remainingSlots = 3 - existingImages;

                if (remainingSlots <= 0)
                {
                    TempData["Success"] = "You already uploaded the maximum of 3 images.";
                    return RedirectToAction("Edit", new { id = articleId });
                }

                if (ImageFiles.Count > remainingSlots)
                {
                    TempData["Success"] = $"You can only upload {remainingSlots} more image(s).";
                    return RedirectToAction("Edit", new { id = articleId });
                }

                foreach (var img in ImageFiles)
                {
                    if (!img.FileName.ToLower().EndsWith(".jpg"))
                        continue;

                    var fileName = Guid.NewGuid() + ".jpg";
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await img.CopyToAsync(stream);
                    }

                    _context.ArticleMedia.Add(new ArticleMedia
                    {
                        ArticleId = articleId,
                        MediaType = "Image",
                        FilePath = $"/uploads/articles/{articleId}/{fileName}",
                        FileSize = img.Length
                    });
                }
            }

            // VIDEO LIMIT
            if (VideoFile != null)
            {
                if (existingVideos >= 1)
                {
                    TempData["Success"] = "You already uploaded a video.";
                    return RedirectToAction("Edit", new { id = articleId });
                }

                var fileName = Guid.NewGuid() + ".mp4";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(stream);
                }

                _context.ArticleMedia.Add(new ArticleMedia
                {
                    ArticleId = articleId,
                    MediaType = "Video",
                    FilePath = $"/uploads/articles/{articleId}/{fileName}",
                    FileSize = VideoFile.Length
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Media uploaded successfully.";
            return RedirectToAction("Edit", new { id = articleId });
        }

        // ---------------------------------------------------------
        // DELETE MEDIA
        // ---------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> DeleteMedia(int id)
        {
            var media = await _context.ArticleMedia.FindAsync(id);
            if (media == null)
                return NotFound();

            var physicalPath = Path.Combine("wwwroot", media.FilePath.TrimStart('/'));
            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);

            _context.ArticleMedia.Remove(media);
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
