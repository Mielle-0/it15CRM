using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmazonReviewsCRM.Controllers
{
    // [Authorize]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult UpdateRoles(string userName, string role)
        {
            TempData["Message"] = $"✅ Role for {userName} updated to {role}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateApi(string apiKey, string endpoint)
        {
            TempData["Message"] = $"🔑 API settings updated (Endpoint: {endpoint})";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateThreshold(double threshold)
        {
            TempData["Message"] = $"⚡ Sentiment alert threshold set to {threshold}";
            return RedirectToAction("Index");
        }
    }
}
