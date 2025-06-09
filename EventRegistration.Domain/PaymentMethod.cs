using System;

namespace EventRegistration.Domain
{
    public class PaymentMethod
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public PaymentMethod(string name)
        {
            Name = name;
        }
    }
}
