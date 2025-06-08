using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EventRegistration.Application
{
    /// <summary>
    /// ViewModel to handle both adding and editing participants.
    /// It contains the necessary data for the form, including event info
    /// and a list of available payment methods.
    /// </summary>
    public class AddOrEditParticipantViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;

        // The type of participant, either "Individual" or "Company"
        [Required]
        public string ParticipantType { get; set; } = "Individual";

        // This property will hold the specific details for an individual participant
        public AddIndividualParticipantDto Individual { get; set; }

        // This property will hold the specific details for a company participant
        public AddCompanyParticipantDto Company { get; set; }

        // A list of payment methods to populate a dropdown in the view
        public IEnumerable<SelectListItem> PaymentMethods { get; set; }

        public AddOrEditParticipantViewModel()
        {
            Individual = new AddIndividualParticipantDto();
            Company = new AddCompanyParticipantDto();
            PaymentMethods = new List<SelectListItem>();
        }
    }
}
