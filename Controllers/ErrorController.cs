using Microsoft.AspNetCore.Mvc;

namespace CargoParcelTracker.Controllers
{
    /// <summary>
    /// Handles application errors and displays appropriate error pages
    /// </summary>
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Displays 404 Not Found error page
        /// </summary>
        [Route("/Error/404")]
        [HttpGet]
        public IActionResult NotFound()
        {
            Response.StatusCode = 404;
            _logger.LogWarning("404 Not Found: {Path}", HttpContext.Request.Path);
            return View();
        }

        /// <summary>
        /// Displays 500 Internal Server Error page
        /// </summary>
        [Route("/Error/500")]
        [HttpGet]
        public IActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            _logger.LogError("500 Internal Server Error occurred");
            return View();
        }

        /// <summary>
        /// Generic error handler
        /// </summary>
        [Route("/Error/{code:int}")]
        [HttpGet]
        public IActionResult Index(int code)
        {
            Response.StatusCode = code;
            _logger.LogWarning("Error {StatusCode} occurred", code);

            return code switch
            {
                404 => View("NotFound"),
                500 => View("InternalServerError"),
                _ => View("GenericError")
            };
        }
    }
}
