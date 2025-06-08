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
                    // FIX: Corrected method name to match the IEventService interface.
                    await _eventService.CreateEvent(createEventDto);
                    return RedirectToAction("Index", "Home");
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

            // FIX: Corrected method name to match the IEventService interface.
            var eventDetails = await _eventService.GetEventDetail(id);
            if (eventDetails == null)
            {
                return NotFound();
            }
            
            // FIX: The view model for details is EventDetailViewModel, which contains the event info.
            ViewBag.EventName = eventDetails.Event.Name;
            return View(eventDetails);
        }
    }
}