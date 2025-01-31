namespace KursDb.Models.ViewModels
{
    public class AdminStatisticsViewModel
    {
        public int TotalReservations { get; set; }
        public int MonthlyReservations { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal MonthlyIncome { get; set; }
        public List<(Car Car, int Bookings)> MostBookedCars { get; set; }
        public int TotalCars { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
    }
}
