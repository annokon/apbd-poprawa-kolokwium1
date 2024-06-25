namespace APBD.Models.DTOs;

public class NewClientRentalDto
{
    public NewClientDto Client { get; set; }
    public int CarId { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime DateTo { get; set; }
}