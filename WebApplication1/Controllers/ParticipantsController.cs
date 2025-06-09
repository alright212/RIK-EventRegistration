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
            return eventDetails?.Event.EventTime <= DateTime.Now;
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
            if (await IsEventInPast(viewModel.EventId))
            {
                TempData["ErrorMessage"] = "Cannot add participants to past events.";
                return RedirectToAction("Details", "Events", new { id = viewModel.EventId });
            }

            // Clear ModelState errors for the unused participant type
            if (viewModel.ParticipantType == "Individual")
            {
                // Remove all Company-related validation errors
                var companyKeys = ModelState.Keys.Where(k => k.StartsWith("Company.")).ToList();
                foreach (var key in companyKeys)
                {
                    ModelState.Remove(key);
                }
            }
            else if (viewModel.ParticipantType == "Company")
            {
                // Remove all Individual-related validation errors
                var individualKeys = ModelState
                    .Keys.Where(k => k.StartsWith("Individual."))
                    .ToList();
                foreach (var key in individualKeys)
                {
                    ModelState.Remove(key);
                }
            }

            try
            {
                if (viewModel.ParticipantType == "Individual")
                {
                    viewModel.Individual.EventId = viewModel.EventId;
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
                        TempData["SuccessMessage"] = "Participant added successfully.";
                        return RedirectToAction(
                            "Details",
                            "Events",
                            new { id = viewModel.EventId }
                        );
                    }
                    else
                    {
                        foreach (var vr in validationResults)
                        {
                            foreach (var memberName in vr.MemberNames.DefaultIfEmpty(string.Empty))
                            {
                                ModelState.AddModelError(
                                    $"Individual.{memberName}".TrimEnd('.'),
                                    vr.ErrorMessage ?? "Validation error"
                                );
                            }
                        }
                    }
                }
                else if (viewModel.ParticipantType == "Company")
                {
                    viewModel.Company.EventId = viewModel.EventId;
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
                        TempData["SuccessMessage"] = "Participant added successfully.";
                        return RedirectToAction(
                            "Details",
                            "Events",
                            new { id = viewModel.EventId }
                        );
                    }
                    else
                    {
                        foreach (var vr in validationResults)
                        {
                            foreach (var memberName in vr.MemberNames.DefaultIfEmpty(string.Empty))
                            {
                                ModelState.AddModelError(
                                    $"Company.{memberName}".TrimEnd('.'),
                                    vr.ErrorMessage ?? "Validation error"
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
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Sama isikukoodiga osavõtja on juba sellele üritusele registreeritud. Ühe isikukoodi saab üritusele registreerida ainult ühe korra."
                );
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Osavõtja lisamisel tekkis viga. Palun proovige uuesti."
                );
            }

            // If we reach here, validation failed or an exception was caught.
            // Store validation errors in TempData and redirect back to Events/Details page
            var validationErrors = new List<string>();
            foreach (var modelError in ModelState.Values.SelectMany(v => v.Errors))
            {
                validationErrors.Add(modelError.ErrorMessage);
            }

            if (validationErrors.Any())
            {
                TempData["ValidationErrors"] = validationErrors;
            }

            // Store form data in TempData to preserve user input
            TempData["ParticipantType"] = viewModel.ParticipantType;
            if (viewModel.ParticipantType == "Individual" && viewModel.Individual != null)
            {
                TempData["Individual.FirstName"] = viewModel.Individual.FirstName;
                TempData["Individual.LastName"] = viewModel.Individual.LastName;
                TempData["Individual.PersonalIdCode"] = viewModel.Individual.PersonalIdCode;
                TempData["Individual.PaymentMethodId"] = viewModel.Individual.PaymentMethodId;
                TempData["Individual.AdditionalInfo"] = viewModel.Individual.AdditionalInfo;
            }
            else if (viewModel.ParticipantType == "Company" && viewModel.Company != null)
            {
                TempData["Company.LegalName"] = viewModel.Company.LegalName;
                TempData["Company.RegistryCode"] = viewModel.Company.RegistryCode;
                TempData["Company.NumberOfParticipants"] = viewModel.Company.NumberOfParticipants;
                TempData["Company.PaymentMethodId"] = viewModel.Company.PaymentMethodId;
                TempData["Company.AdditionalInfo"] = viewModel.Company.AdditionalInfo;
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

            // Clear ModelState errors for the unused participant type
            if (viewModel.ParticipantType == "Individual")
            {
                // Remove all Company-related validation errors
                var companyKeys = ModelState.Keys.Where(k => k.StartsWith("Company.")).ToList();
                foreach (var key in companyKeys)
                {
                    ModelState.Remove(key);
                }
            }
            else if (viewModel.ParticipantType == "Company")
            {
                // Remove all Individual-related validation errors
                var individualKeys = ModelState
                    .Keys.Where(k => k.StartsWith("Individual."))
                    .ToList();
                foreach (var key in individualKeys)
                {
                    ModelState.Remove(key);
                }
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
            catch (InvalidOperationException ex) when (ex.Message.Contains("already registered"))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Sama isikukoodiga osavõtja on juba sellele üritusele registreeritud. Ühe isikukoodi saab üritusele registreerida ainult ühe korra."
                );
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Osavõtja muutmisel tekkis viga. Palun proovige uuesti."
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

        // API endpoint to lookup participant by personal ID code
        [HttpGet]
        public async Task<IActionResult> LookupByPersonalIdCode(string personalIdCode)
        {
            if (string.IsNullOrWhiteSpace(personalIdCode))
            {
                return BadRequest("Personal ID code is required");
            }

            var participant = await _participantService.GetIndividualByPersonalIdCodeAsync(
                personalIdCode
            );
            if (participant == null)
            {
                return NotFound();
            }

            return Json(participant);
        }

        // API endpoint to lookup company by registry code
        [HttpGet]
        public async Task<IActionResult> LookupByRegistryCode(string registryCode)
        {
            if (string.IsNullOrWhiteSpace(registryCode))
            {
                return BadRequest("Registry code is required");
            }

            var company = await _participantService.GetCompanyByRegistryCodeAsync(registryCode);
            if (company == null)
            {
                return NotFound();
            }

            return Json(company);
        }

        // POST: Participants/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddOrEditParticipantViewModel model)
        {
            // Clear ModelState errors for the unused participant type
            if (model.ParticipantType == "Individual")
            {
                // Remove all Company-related validation errors
                var companyKeys = ModelState.Keys.Where(k => k.StartsWith("Company.")).ToList();
                foreach (var key in companyKeys)
                {
                    ModelState.Remove(key);
                }
            }
            else if (model.ParticipantType == "Company")
            {
                // Remove all Individual-related validation errors
                var individualKeys = ModelState
                    .Keys.Where(k => k.StartsWith("Individual."))
                    .ToList();
                foreach (var key in individualKeys)
                {
                    ModelState.Remove(key);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ParticipantType == "Individual")
                    {
                        var dto = new AddIndividualParticipantDto
                        {
                            EventId = model.EventId,
                            FirstName = model.Individual.FirstName,
                            LastName = model.Individual.LastName,
                            PersonalIdCode = model.Individual.PersonalIdCode,
                            PaymentMethodId = model.Individual.PaymentMethodId,
                            AdditionalInfo = model.Individual.AdditionalInfo,
                        };
                        await _participantService.AddIndividualParticipantAsync(dto);
                    }
                    else // It's a Company
                    {
                        var dto = new AddCompanyParticipantDto
                        {
                            EventId = model.EventId,
                            LegalName = model.Company.LegalName,
                            RegistryCode = model.Company.RegistryCode,
                            NumberOfParticipants = model.Company.NumberOfParticipants,
                            PaymentMethodId = model.Company.PaymentMethodId,
                            AdditionalInfo = model.Company.AdditionalInfo,
                        };
                        await _participantService.AddCompanyParticipantAsync(dto);
                    }
                    return RedirectToAction("Details", "Events", new { id = model.EventId });
                }
                catch (InvalidOperationException ex)
                {
                    if (ex.Message.Contains("Participant is already registered for this event."))
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            "Sama isikukoodiga osavõtja on juba sellele üritusele registreeritud. Ühe isikukoodi saab üritusele registreerida ainult ühe korra."
                        );
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "An unexpected error occurred.");
                    }
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
            }

            var paymentMethods = await _participantService.GetPaymentMethodsAsync();
            model.PaymentMethods = paymentMethods
                .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
                .ToList();
            var eventDetails = await _eventService.GetEventDetail(model.EventId);
            if (eventDetails != null)
            {
                model.EventName = eventDetails.Event.Name;
            }
            return View(model);
        }
    }
}
