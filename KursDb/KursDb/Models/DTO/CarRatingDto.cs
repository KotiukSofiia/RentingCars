public class CarRatingDto
{
    public int CarId { get; set; }
    public string UserId { get; set; }
    public int Rating { get; set; }
}

public class CarDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal PricePerDay { get; set; }
    public int Year { get; set; }
    public string Category { get; set; }
    public double AverageRating { get; set; }
}