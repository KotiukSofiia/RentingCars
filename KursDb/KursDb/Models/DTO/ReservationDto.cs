public class ReservationDto
{
    public int CarId { get; set; }
    public string CarName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
}