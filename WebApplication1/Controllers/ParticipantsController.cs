using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using EventRegistration.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication1.Controllers
{
    public class ParticipantsController : Controller
    {
        private readonly IParticipantService _participantService;
        private readonly IEventService _eventService;

        public ParticipantsController(
            IParticipantService participantService,
            IEventService eventService
        )
        {
            _participantService = participantService;
            _eventService = eventService;
        }

        private async Task<bool> IsEventInPast(Guid eventId)
        {
            var eventDetails = await _eventService.GetEventDetail(eventId);
            return eventDetails?.Event.EventTime <= DateTime.UtcNow;
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

            // Prevent adding participants to past events
            if (await IsEventInPast(eventId))
            {
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var paymentMethods = await _participantService.GetPaymentMethodsAsync();

            var viewModel = new AddOrEditParticipantViewModel
            {
                EventId = eventId,
                EventName = eventDetails.Event.Name,
                PaymentMethods = paymentMethods
                    .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                    .ToList(),
                Individual = new AddIndividualParticipantDto { EventId = eventId },
                Company = new AddCompanyParticipantDto { EventId = eventId },
            };

            return View(viewModel);
        }

        // POST: Participants/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddOrEditParticipantViewModel viewModel)
        {
            // Prevent adding participants to past events
            if (await IsEventInPast(viewModel.EventId))
            {
                return RedirectToAction("Details", "Events", new { id = viewModel.EventId });
            }

            try
            {
                if (viewModel.ParticipantType == "Individual")
                {
                    viewModel.Individual.EventId = viewModel.EventId;

                    // Validate the individual participant
                    var validationResults = new List<ValidationResult>();
                    var validationContext = new ValidationContext(viewModel.Individual);

                    if (
                        Validator.TryValidateObject(
                            viewModel.Individual,
                            validationContext,
                            validationResults,
                            true
                        )
                    )
                    {
                        await _participantService.AddIndividualParticipantAsync(
                            viewModel.Individual
                        );
                        return RedirectToAction(
                            "Details",
                            "Events",
                            new { id = viewModel.EventId }
                        );
                    }
                    else
                    {
                        // Add validation errors to ModelState
                        foreach (var validationResult in validationResults)
                        {
                            foreach (var memberName in validationResult.MemberNames)
                            {
                                ModelState.AddModelError(
                                    $"Individual.{memberName}",
                                    validationResult.ErrorMessage ?? "Validation error"
                                );
                            }
                        }
                    }
                }
                else if (viewModel.ParticipantType == "Company")
                {
                    viewModel.Company.EventId = viewModel.EventId;

                    // Validate the company participant
                    var validationResults = new List<ValidationResult>();
                    var validationContext = new ValidationContext(viewModel.Company);

                    if (
                        Validator.TryValidateObject(
                            viewModel.Company,
                            validationContext,
                            validationResults,
                            true
                        )
                    )
                    {
                        await _participantService.AddCompanyParticipantAsync(viewModel.Company);
                        return RedirectToAction(
                            "Details",
                            "Events",
                            new { id = viewModel.EventId }
                        );
                    }
                    else
                    {
                        // Add validation errors to ModelState
                        foreach (var validationResult in validationResults)
                        {
                            foreach (var memberName in validationResult.MemberNames)
                            {
                                ModelState.AddModelError(
                                    $"Company.{memberName}",
                                    validationResult.ErrorMessage ?? "Validation error"
                                );
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        "ParticipantType",
                        "Please select a participant type."
                    );
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An error occurred while adding the participant. Please try again."
                );
            }

            // If we got this far, something failed
            // Store validation errors in TempData and redirect back to Events/Details
            TempData["ValidationErrors"] = ModelState.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                    kvp.Value?.Errors.Select(e => e.ErrorMessage ?? "Validation error").ToArray()
                    ?? new string[0]
            );

            // Store the submitted form data to repopulate the form
            TempData["ParticipantType"] = viewModel.ParticipantType;
            if (viewModel.ParticipantType == "Individual")
            {
                TempData["Individual.FirstName"] = viewModel.Individual?.FirstName;
                TempData["Individual.LastName"] = viewModel.Individual?.LastName;
                TempData["Individual.PersonalIdCode"] = viewModel.Individual?.PersonalIdCode;
                TempData["Individual.PaymentMethodId"] = viewModel.Individual?.PaymentMethodId;
                TempData["Individual.AdditionalInfo"] = viewModel.Individual?.AdditionalInfo;
            }
            else if (viewModel.ParticipantType == "Company")
            {
                TempData["Company.LegalName"] = viewModel.Company?.LegalName;
                TempData["Company.RegistryCode"] = viewModel.Company?.RegistryCode;
                TempData["Company.NumberOfParticipants"] = viewModel.Company?.NumberOfParticipants;
                TempData["Company.PaymentMethodId"] = viewModel.Company?.PaymentMethodId;
                TempData["Company.AdditionalInfo"] = viewModel.Company?.AdditionalInfo;
            }

            return RedirectToAction("Details", "Events", new { id = viewModel.EventId });
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

            // Prevent deleting participants from past events
            if (await IsEventInPast(eventId))
            {
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            // The participantId is now correctly passed as an int.
            await _participantService.DeleteParticipantAsync(participantId, eventId);

            return RedirectToAction("Details", "Events", new { id = eventId });
        }

        // GET: Participants/Details/{eventId}/{participantId}
        public async Task<IActionResult> Details(Guid eventId, int participantId)
        {
            if (eventId == Guid.Empty)
            {
                return NotFound();
            }

            var participant = await _participantService.GetParticipantDetailsAsync(
                participantId,
                eventId
            );
            if (participant == null)
            {
                return NotFound();
            }

            var eventDetails = await _eventService.GetEventDetail(eventId);
            if (eventDetails == null)
            {
                return NotFound();
            }

            ViewBag.EventName = eventDetails.Event.Name;
            ViewBag.EventTime = eventDetails.Event.EventTime;
            ViewBag.EventLocation = eventDetails.Event.Location;

            return View(participant);
        }

        // GET: Participants/Edit/{eventId}/{participantId}
        public async Task<IActionResult> Edit(Guid eventId, int participantId)
        {
            if (eventId == Guid.Empty)
            {
                return NotFound();
            }

            // Prevent editing participants of past events
            if (await IsEventInPast(eventId))
            {
                return RedirectToAction("Details", "Events", new { id = eventId });
            }

            var participant = await _participantService.GetParticipantDetailsAsync(
                participantId,
                eventId
            );
            if (participant == null)
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
                ParticipantType = participant.ParticipantType,
                PaymentMethods = paymentMethods
                    .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                    .ToList(),
            };

            if (participant.ParticipantType == "Individual")
            {
                viewModel.Individual = new AddIndividualParticipantDto
                {
                    EventId = eventId,
                    FirstName = participant.FirstName ?? string.Empty,
                    LastName = participant.LastName ?? string.Empty,
                    PersonalIdCode = participant.PersonalIdCode ?? string.Empty,
                    PaymentMethodId = participant.PaymentMethodId,
                    AdditionalInfo = participant.EventParticipantAdditionalInfo,
                };
                viewModel.Company = new AddCompanyParticipantDto { EventId = eventId };
            }
            else
            {
                viewModel.Company = new AddCompanyParticipantDto
                {
                    EventId = eventId,
                    LegalName = participant.LegalName ?? string.Empty,
                    RegistryCode = participant.RegistryCode ?? string.Empty,
                    NumberOfParticipants = participant.NumberOfParticipants ?? 1,
                    PaymentMethodId = participant.PaymentMethodId,
                    AdditionalInfo = participant.EventParticipantAdditionalInfo,
                };
                viewModel.Individual = new AddIndividualParticipantDto { EventId = eventId };
            }

            ViewBag.ParticipantId = participantId;
            return View(viewModel);
        }

        // POST: Participants/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int participantId,
            AddOrEditParticipantViewModel viewModel
        )
        {
            // Prevent editing participants of past events
            if (await IsEventInPast(viewModel.EventId))
            {
                return RedirectToAction("Details", "Events", new { id = viewModel.EventId });
            }

            try
            {
                if (viewModel.ParticipantType == "Individual")
                {
                    viewModel.Individual.EventId = viewModel.EventId;

                    // Validate the individual participant
                    var validationResults = new List<ValidationResult>();
                    var validationContext = new ValidationContext(viewModel.Individual);

                    if (
                        Validator.TryValidateObject(
                            viewModel.Individual,
                            validationContext,
                            validationResults,
                            true
                        )
                    )
                    {
                        await _participantService.UpdateIndividualParticipantAsync(
                            participantId,
                            viewModel.EventId,
                            viewModel.Individual
                        );
                        return RedirectToAction(
                            "Details",
                            "Events",
                            new { id = viewModel.EventId }
                        );
                    }
                    else
                    {
                        // Add validation errors to ModelState
                        foreach (var validationResult in validationResults)
                        {
                            foreach (var memberName in validationResult.MemberNames)
                            {
                                ModelState.AddModelError(
                                    $"Individual.{memberName}",
                                    validationResult.ErrorMessage ?? "Validation error"
                                );
                            }
                        }
                    }
                }
                else if (viewModel.ParticipantType == "Company")
                {
                    viewModel.Company.EventId = viewModel.EventId;

                    // Validate the company participant
                    var validationResults = new List<ValidationResult>();
                    var validationContext = new ValidationContext(viewModel.Company);

                    if (
                        Validator.TryValidateObject(
                            viewModel.Company,
                            validationContext,
                            validationResults,
                            true
                        )
                    )
                    {
                        await _participantService.UpdateCompanyParticipantAsync(
                            participantId,
                            viewModel.EventId,
                            viewModel.Company
                        );
                        return RedirectToAction(
                            "Details",
                            "Events",
                            new { id = viewModel.EventId }
                        );
                    }
                    else
                    {
                        // Add validation errors to ModelState
                        foreach (var validationResult in validationResults)
                        {
                            foreach (var memberName in validationResult.MemberNames)
                            {
                                ModelState.AddModelError(
                                    $"Company.{memberName}",
                                    validationResult.ErrorMessage ?? "Validation error"
                                );
                            }
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError(
                        "ParticipantType",
                        "Please select a participant type."
                    );
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    $"An error occurred while updating the participant: {ex.Message}"
                );
            }

            // If we got this far, something failed, reload the form
            var eventDetails = await _eventService.GetEventDetail(viewModel.EventId);
            if (eventDetails != null)
            {
                viewModel.EventName = eventDetails.Event.Name;
            }

            var paymentMethods = await _participantService.GetPaymentMethodsAsync();
            viewModel.PaymentMethods = paymentMethods
                .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                .ToList();

            ViewBag.ParticipantId = participantId;
            return View(viewModel);
        }
    }
}
