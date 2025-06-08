using EventRegistration.Application;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEventDto createEventDto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _eventService.CreateEventAsync(createEventDto);
                    return RedirectToAction("Index", "Home"); // Or to a success page, or event details
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }
            return View(createEventDto);
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var eventDetails = await _eventService.GetEventDetailsAsync(id);
            if (eventDetails == null)
            {
                return NotFound();
            }

            // For now, we'll pass the EventViewModel. Later we might need a more specific ViewModel for this page.
            // We also need to fetch participants for this event. This will be added later.
            ViewBag.EventName = eventDetails.Name; // For the breadcrumb/title
            return View(eventDetails);
        }
    }
}
