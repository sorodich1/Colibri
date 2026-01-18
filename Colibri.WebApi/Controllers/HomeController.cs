using Microsoft.AspNetCore.Mvc;

namespace Colibri.WebApi.Controllers
{
    public class HomeController : Controller
    {
        // GET: HomeController
        public ActionResult Index()
        {
            return View();
        }

    }
}
