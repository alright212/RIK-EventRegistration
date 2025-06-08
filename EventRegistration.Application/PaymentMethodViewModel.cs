using System;

namespace EventRegistration.Application
{
    public class PaymentMethodViewModel
    {
        // FIX: Changed Id from Guid to int to match the domain entity.
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}