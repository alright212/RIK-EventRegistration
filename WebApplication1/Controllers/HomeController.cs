using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Models; // Assuming ErrorViewModel is here
using EventRegistration.Application; // Added for IEventService
using System.Threading.Tasks; // Added for Task

namespace WebApplication1.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IEventService _eventService; // Added

    public HomeController(ILogger<HomeController> logger, IEventService eventService) // Modified constructor
    {
        _logger = logger;
        _eventService = eventService; // Added
    }

    public async Task<IActionResult> Index() // Made async
    {
        var futureEvents = await _eventService.GetFutureEventsAsync();
        var pastEvents = await _eventService.GetPastEventsAsync();

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
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}