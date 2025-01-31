using System.ComponentModel.DataAnnotations;

namespace KursDb.Models
{
    public class CarMaintenance
    {
        [Key]
        public int MaintenanceId { get; set; }
        public int CarId { get; set; } // FK
        public string Description { get; set; }
        public DateTime MaintenanceDate { get; set; }
        public decimal Cost { get; set; }

        // Навігаційна властивість
        public Car Car { get; set; }
    }
}
