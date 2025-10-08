using Microsoft.AspNetCore.Mvc;

namespace AmazonReviewsCRM.Controllers
{
    public class AlertsController : Controller
    {
        // GET: /Alerts
        public IActionResult Index()
        {
            var alerts = new List<dynamic>
            {
                new { Id = 1, Game = "Space Explorer", Severity = "High", Date = DateTime.Now.AddDays(-1), Message = "Sentiment dropped 15% in the last 7 days." },
                new { Id = 2, Game = "Dungeon Quest", Severity = "Medium", Date = DateTime.Now.AddDays(-2), Message = "Spike in negative reviews mentioning 'bugs'." },
                new { Id = 3, Game = "Farm Tycoon", Severity = "Low", Date = DateTime.Now.AddDays(-3), Message = "Unusual number of neutral reviews detected." },
                new { Id = 4, Game = "City Builder Pro", Severity = "High", Date = DateTime.Now.AddHours(-12), Message = "Sudden 20% drop in recommended reviews." }
            };

            return View(alerts);
        }

        // POST: /Alerts/Assign/{id}
        [HttpPost]
        public IActionResult Assign(int id)
        {
            // TODO: Implement alert assignment logic
            // This would typically assign the alert to a user or team
            return Json(new { success = true, message = "Alert assigned successfully" });
        }

        // POST: /Alerts/Dismiss/{id}
        [HttpPost]
        public IActionResult Dismiss(int id)
        {
            // TODO: Implement alert dismissal logic
            // This would typically mark the alert as resolved or dismissed
            return Json(new { success = true, message = "Alert dismissed successfully" });
        }
    }
}

