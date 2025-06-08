using EventRegistration.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly IParticipantService _participantService;
        private readonly IEventService _eventService;

        public ParticipantsController(IParticipantService participantService, IEventService eventService)
        {
            _participantService = participantService;
            _eventService = eventService;
        }

        // GET: Participants/Add/{eventId}
        public async Task<IActionResult> Add(Guid eventId)
        {
            if (eventId == Guid.Empty)
            {
                return NotFound();
            }

            var eventDetails = await _eventService.GetEventDetail(eventId);
            if (eventDetails == null)
            {
                return NotFound();
            }

            var paymentMethods = await _participantService.GetPaymentMethodsAsync();

            var viewModel = new AddOrEditParticipantViewModel
            {
                EventId = eventId,
                EventName = eventDetails.Event.Name,
                PaymentMethods = paymentMethods.Select(p => new SelectListItem(p.Name, p.Id.ToString())).ToList(),
                Individual = new AddIndividualParticipantDto { EventId = eventId },
                Company = new AddCompanyParticipantDto { EventId = eventId }
            };

            return View(viewModel);
        }

        // POST: Participants/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddOrEditParticipantViewModel viewModel)
        {
            ModelState.Clear(); 
            
            if (viewModel.ParticipantType == "Individual")
            {
                viewModel.Individual.EventId = viewModel.EventId;
                if (TryValidateModel(viewModel.Individual))
                {
                    await _participantService.AddIndividualParticipantAsync(viewModel.Individual);
                    return RedirectToAction("Details", "Events", new { id = viewModel.EventId });
                }
            }
            else if (viewModel.ParticipantType == "Company")
            {
                viewModel.Company.EventId = viewModel.EventId;
                 if (TryValidateModel(viewModel.Company))
                {
                    await _participantService.AddCompanyParticipantAsync(viewModel.Company);
                    return RedirectToAction("Details", "Events", new { id = viewModel.EventId });
                }
            }
            
            var eventDetails = await _eventService.GetEventDetail(viewModel.EventId);
            var paymentMethods = await _participantService.GetPaymentMethodsAsync();
            viewModel.EventName = eventDetails?.Event?.Name ?? "Event";
            viewModel.PaymentMethods = paymentMethods.Select(p => new SelectListItem(p.Name, p.Id.ToString())).ToList();

            return View(viewModel);
        }

        // POST: Participants/Delete/{eventId}/{participantId}
        [HttpPost]
        [ValidateAntiForgeryToken]
        // FIX: Changed participantId from Guid to int to match the service layer.
        public async Task<IActionResult> Delete(Guid eventId, int participantId)
        {
            if (eventId == Guid.Empty)
            {
                return NotFound();
            }
            
            // The participantId is now correctly passed as an int.
            await _participantService.DeleteParticipantAsync(participantId, eventId);

            return RedirectToAction("Details", "Events", new { id = eventId });
        }
    }
}