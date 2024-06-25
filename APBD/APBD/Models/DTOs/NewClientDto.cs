using System.ComponentModel.DataAnnotations;

namespace APBD.Models.DTOs;

public class NewClientDto
{
    [MaxLength(50)] public string FirstName { get; set; }
    [MaxLength(100)] public string LastName { get; set; }
    [MaxLength(100)] public string Address { get; set; }
}