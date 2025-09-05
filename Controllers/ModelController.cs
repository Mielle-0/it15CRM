using Microsoft.AspNetCore.Mvc;

namespace AmazonReviewsCRM.Controllers
{
    public class ModelController : Controller
    {
        // GET: /Model
        public IActionResult Index()
        {
            // Mock model performance metrics
            var metrics = new
            {
                Version = "v1.2.3",
                Precision = 0.87,
                Recall = 0.82,
                F1 = 0.845
            };

            return View(metrics);
        }

        // POST: /Model/Upload
        [HttpPost]
        public IActionResult Upload(IFormFile modelFile)
        {
            if (modelFile != null && modelFile.Length > 0)
            {
                TempData["Message"] = $"✅ Model '{modelFile.FileName}' uploaded successfully!";
            }
            else
            {
                TempData["Message"] = "⚠️ Please select a file before uploading.";
            }

            return RedirectToAction("Index");
        }

        // POST: /Model/Test
        [HttpPost]
        public IActionResult Test(string sampleText)
        {
            // Mock prediction result
            var result = new
            {
                Text = sampleText,
                Prediction = "Positive",
                Confidence = 0.91
            };

            return Json(result);
        }
    }
}
