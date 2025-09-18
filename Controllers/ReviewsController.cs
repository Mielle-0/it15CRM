using Microsoft.AspNetCore.Mvc;
using AmazonReviewsCRM.Data;
using Microsoft.EntityFrameworkCore;
using AmazonReviewsCRM.Models;

namespace AmazonReviewsCRM.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly AppDbContext _context;

        public ReviewsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Reviews/
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? sentiment,
            DateTime? date,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.ReviewOverview.AsNoTracking();

            // Apply filters
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(r => r.ReviewText.Contains(searchTerm) || r.AppName.Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(sentiment))
            {
                query = query.Where(r => r.Sentiment == sentiment);
            }

            if (date.HasValue)
            {
                query = query.Where(r => r.ReviewDate >= date.Value);
            }

            // Pagination
            var totalCount = await query.CountAsync();
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;

            return View(reviews);
        }


        // GET: /Reviews/Details/{id}
        // If you need details later, update your view & model to include review_id
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var review = await _context.Reviews
                    .Include(r => r.Sentiments)
                    .Include(r => r.Game)
                    .FirstOrDefaultAsync(r => r.ReviewId == id);

                if (review == null)
                    return NotFound();

                var sentiment = review.Sentiments.FirstOrDefault();

                var vm = new ReviewDetailViewModel
                {
                    AppName = review.Game?.AppName,
                    Genre = review.Game?.Genre,
                    ReleaseDate = review.Game?.ReleaseDate,
                    Developer = review.Game?.Developer,
                    Publisher = review.Game?.Publisher,
                    Source = review.Game?.Source,
                    ReviewId = review.ReviewId,
                    ReviewText = review.ReviewText,
                    ReviewScoreNumeric = review.ReviewScoreNumeric,
                    ReviewRecommendation = review.ReviewRecommendation,
                    ReviewVotes = review.ReviewVotes,
                    ReviewDate = review.ReviewDate,
                    ReviewerId = review.ReviewerId,
                    PredictedSentiment = sentiment?.PredictedSentiment,
                    ConfidenceScore = sentiment?.ConfidenceScore
                };

                return PartialView("_ViewReviewModal", vm);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }



        // GET: Reviews/Create
        public IActionResult Create()
        {
            ViewBag.Games = _context.Games
                .Select(g => new { g.GameId, g.AppName })
                .ToList();

            return PartialView("_CreateReviewModal", new Review());
        }


        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Review review)
        {
            if (ModelState.IsValid)
            {
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Games = _context.Games
                .Select(g => new { g.GameId, g.AppName })
                .ToList();

            return PartialView("_CreateReviewModal", review);
        }

        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            ViewBag.Games = _context.Games
                .Select(g => new { g.GameId, g.AppName })
                .ToList();

            return PartialView("_EditReviewModal", review);
        }


        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Review review)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(review);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Reviews.Any(e => e.ReviewId == id))
                        return NotFound();
                    throw;
                }
            }

            ViewBag.Games = _context.Games
                .Select(g => new { g.GameId, g.AppName })
                .ToList();

            return PartialView("_EditReviewModal", review);
        }

    }
}
