using System.ComponentModel.DataAnnotations;

namespace APBD.Models.DTOs;

public class NewClientDto
{
    [MaxLength(50)] [Required] public string FirstName { get; set; }
    [MaxLength(100)] [Required] public string LastName { get; set; }
    [MaxLength(100)] [Required] public string Address { get; set; }
}