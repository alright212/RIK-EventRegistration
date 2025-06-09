using System;
using System.Linq;
using System.Threading.Tasks;
using EventRegistration.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    public class EventsController : Controller
    {
        private readonly IEventService _eventService;
        private readonly IParticipantService _participantService;

        public EventsController(IEventService eventService, IParticipantService participantService)
        {
            _eventService = eventService;
            _participantService = participantService;
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

            // Get payment methods for the add participant form
            var paymentMethods = await _participantService.GetPaymentMethodsAsync();
            ViewBag.PaymentMethods = paymentMethods
                .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                .ToList();
            ViewBag.EventId = id;

            // FIX: The view model for details is EventDetailViewModel, which contains the event info.
            ViewBag.EventName = eventDetails.Event.Name;
            return View(eventDetails);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            var eventDto = await _eventService.GetEventForEdit(id);
            if (eventDto == null)
            {
                return NotFound();
            }

            return View(eventDto);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateEventDto updateEventDto)
        {
            if (id == Guid.Empty)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _eventService.UpdateEvent(id, updateEventDto);
                    return RedirectToAction("Details", new { id = id });
                }
                catch (KeyNotFoundException)
                {
                    return NotFound();
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            return View(updateEventDto);
        }
    }
}
