using System;

namespace EventRegistration.Domain
{
    public class PaymentMethod
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        // Add other relevant properties for payment method, e.g., IsOnline, Description

        public PaymentMethod(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}
