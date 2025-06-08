using System;

namespace EventRegistration.Domain
{
    public class PaymentMethod
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        

        public PaymentMethod(string name)
        {
            Id = Guid.NewGuid();
            Name = name;
        }
    }
}
