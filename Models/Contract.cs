using Logistics.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;


namespace Logistics.Models
{
    public class Contract
    {
        public int Id { get; set; }

        [Display(Name = "Client Name")]
        public int ClientId { get; set; }

        public Client? Client { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public ContractStatus Status { get; set; }

        public string? SignedAgreementPath { get; set; }

        [NotMapped]
        public IFormFile? AgreementFile { get; set; }

        public ICollection<ServiceRequest>? ServiceRequests { get; set; }
    }
}
