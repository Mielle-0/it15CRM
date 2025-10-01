using Microsoft.AspNetCore.Mvc;
using AmazonReviewsCRM.Data;
using AmazonReviewsCRM.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AmazonReviewsCRM.Security;

namespace AmazonReviewsCRM.Controllers
{
    // [Authorize]
    public class GamesController : Controller
    {
        private readonly AppDbContext _context;

        public GamesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Games
        // [RequireRoleAccess("Games")]
        public IActionResult Index(string sentiment, string searchTerm, DateTime? date, int page = 1)
        {
            var query = _context.ViewGameList.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(g => g.AppName.Contains(searchTerm) || g.AppId.Contains(searchTerm));

            // Sentiment
            if (!string.IsNullOrEmpty(sentiment))
            {
                if (sentiment == "Positive") query = query.Where(g => g.AvgSentiment >= 0.3m);
                else if (sentiment == "Negative") query = query.Where(g => g.AvgSentiment <= -0.3m);
                else query = query.Where(g => g.AvgSentiment > -0.3m && g.AvgSentiment < 0.3m);
            }

            // Date
            if (date.HasValue)
                query = query.Where(g => g.ReleaseDate.HasValue && g.ReleaseDate.Value.Date == date.Value.Date);

            // Pagination
            var totalItems = query.Count();
            var items = query.OrderBy(g => g.AppName)
                             .Skip((page - 1) * 10)
                             .Take(10)
                             .ToList();

            var model = new PagedResult<ViewGameList>
            {
                Items = items,
                PageNumber = page,
                PageSize = 10,
                TotalItems = totalItems
            };

            ViewBag.Genres = _context.ViewGameList.Select(g => g.Genre).Distinct().ToList();
            ViewBag.Sources = _context.ViewGameList.Select(g => g.Source).Distinct().ToList();

            return View(model);
        }



        // GET: Games/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            return PartialView("_DetailsGameModal", game);
        }



        public IActionResult Create()
        {
            return PartialView("_CreateGameModal");
        }


        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Game game)
        {
            if (ModelState.IsValid)
            {
                _context.Games.Add(game);
                _context.SaveChanges();
                return Json(new { success = true });
            }
            return PartialView("_CreateGameModal", game);
        }


        // GET: Games/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            return PartialView("_EditGameModal", game);
        }


        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Game game)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(game);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Games.Any(e => e.GameId == id))
                        return NotFound();
                    throw;
                }
            }

            return PartialView("_EditGameModal", game);
        }



        // GET: Games/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _context.Games.FindAsync(id);
            if (game == null)
                return NotFound();

            return PartialView("_DeleteGameModal", game); // create this view
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int GameId)
        {
            try
            {


                // Reload from DB to ensure navigation properties are included
                var gameEntity = await _context.Games
                    .Include(g => g.Reviews)
                        .ThenInclude(r => r.Sentiments)
                    .FirstOrDefaultAsync(g => g.GameId == GameId);

                if (gameEntity == null)
                    return NotFound();

                // 2. Copy the game into ArchivedGames
                var archivedGame = new ArchivedGame
                {
                    OriginalGameId = gameEntity.GameId,
                    AppId = gameEntity.AppId,
                    AppName = gameEntity.AppName,
                    Genre = gameEntity.Genre,
                    ReleaseDate = gameEntity.ReleaseDate,
                    Developer = gameEntity.Developer,
                    Publisher = gameEntity.Publisher,
                    Description = gameEntity.Description,
                    Source = gameEntity.Source,
                    ArchivedAt = DateTime.Now
                };
                _context.ArchivedGames.Add(archivedGame);

                // 3. Copy reviews into ArchivedReviews
                foreach (var review in gameEntity.Reviews)
                {
                    var archivedReview = new ArchivedReview
                    {
                        OriginalReviewId = review.ReviewId,
                        ReviewText = review.ReviewText,
                        ReviewVotes = review.ReviewVotes,
                        ReviewDate = review.ReviewDate,
                        ReviewerId = review.ReviewerId,
                        ReviewScoreNumeric = review.ReviewScoreNumeric,
                        ReviewRecommendation = review.ReviewRecommendation,
                        OriginalGameId = review.GameId,
                        ArchivedAt = DateTime.Now
                    };
                    _context.ArchivedReviews.Add(archivedReview);
                }

                // 4. Delete child entities (in correct order)
                foreach (var review in gameEntity.Reviews)
                {
                    _context.ReviewSentiments.RemoveRange(review.Sentiments); // delete sentiments first
                }
                _context.Reviews.RemoveRange(gameEntity.Reviews); // then delete reviews
                _context.Games.Remove(gameEntity); // finally delete the game

                // 5. Commit transaction
                await _context.SaveChangesAsync();
                return Json(new { success = true });

            }
            catch (Exception ex)
            {
                // For debugging: return error details to client
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }

        }


    }
}
