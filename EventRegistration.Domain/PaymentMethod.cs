using System;

namespace EventRegistration.Domain
{
    public class PaymentMethod
    {
        // Changed Id from Guid to int.
        // The database will be responsible for generating this value.
        public int Id { get; private set; }
        public string Name { get; private set; }


        public PaymentMethod(string name)
        {
            // Removed Id initialization.
            Name = name;
        }
    }
}