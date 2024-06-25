namespace APBD.Models.DTOs;

public class ClientDto
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public List<RentalDto> Rentals { get; set; } = new List<RentalDto>();
}