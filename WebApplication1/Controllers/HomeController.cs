using System.Diagnostics;
using System.Threading.Tasks;
using EventRegistration.Application;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IEventService _eventService;

        public HomeController(ILogger<HomeController> logger, IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            // FIX: Corrected method names to match the IEventService interface.
            var futureEvents = await _eventService.GetUpcomingEvents();
            var pastEvents = await _eventService.GetPastEvents();

            ViewBag.FutureEvents = futureEvents;
            ViewBag.PastEvents = pastEvents;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }
    }
}
