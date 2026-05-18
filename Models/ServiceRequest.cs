namespace Logistics.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        public int ContractId { get; set; }

        public Contract? Contract { get; set; }

        public string Description { get; set; }

        public decimal Cost { get; set; }

        public string Currency { get; set; }

        public string Status { get; set; }
    }
}
