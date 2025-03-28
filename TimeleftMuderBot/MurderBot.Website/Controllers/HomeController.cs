using Microsoft.AspNetCore.Mvc;
using MurderBot.Website.Models;
using System.Diagnostics;

namespace MurderBot.Website.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult GetContactFile()
        {
            var contact = @"BEGIN:VCARD
VERSION:3.0
FN;CHARSET=UTF-8: Timeleft Murder Bot
N;CHARSET=UTF-8:Murder Bot;Timeleft;;;
TEL;TYPE=CELL:+16785902487

END:VCARD";

            return Content(contact, "text/vcard");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
