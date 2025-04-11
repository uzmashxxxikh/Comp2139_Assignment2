using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error404")]
        public IActionResult Error404()
        {
            return View("~/Views/Shared/Error404.cshtml");
        }

        [Route("Error500")]
        public IActionResult Error500()
        {
            return View("~/Views/Shared/Error500.cshtml");
        }

        [Route("Error/{statusCode}")]
        public IActionResult Error(int statusCode)
        {
            return statusCode switch
            {
                404 => View("~/Views/Shared/Error404.cshtml"),
                500 => View("~/Views/Shared/Error500.cshtml"),
                _ => View("~/Views/Shared/Error500.cshtml")
            };
        }
    }
} 