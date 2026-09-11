using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    public class ErrorController : Controller
    {
        [HttpGet]
        public IActionResult Index(int code)
        {
            if (code == 404)
            {
                return View("NotFound");
            }

            if (code == 403)
            {
                return View("AccessDenied");
            }

            return View("Error");
        }

        [HttpGet]
        public IActionResult ServerError()
        {
            return View("Error");
        }
    }
}