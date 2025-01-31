using System.ComponentModel.DataAnnotations;

namespace KursDb.Models
{
    public class CarCategory
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; }

        // Навігаційна властивість
        public ICollection<Car> Cars { get; set; }
    }

}
