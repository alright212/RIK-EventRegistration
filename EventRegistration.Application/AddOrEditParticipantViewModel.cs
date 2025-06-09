using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventRegistration.Application
{
    public class AddOrEditParticipantViewModel
    {
        public Guid EventId { get; set; }
        public string EventName { get; set; } = string.Empty;

        [Required]
        public string ParticipantType { get; set; } = "Individual";

        public AddIndividualParticipantDto Individual { get; set; }

        public AddCompanyParticipantDto Company { get; set; }

        public IEnumerable<SelectListItem> PaymentMethods { get; set; }

        public AddOrEditParticipantViewModel()
        {
            Individual = new AddIndividualParticipantDto();
            Company = new AddCompanyParticipantDto();
            PaymentMethods = new List<SelectListItem>();
        }
    }
}
