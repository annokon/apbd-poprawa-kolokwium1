using APBD.Models.DTOs;
using APBD.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace APBD.Controllers;

[ApiController]
[Route("api/[controller]")]
public class clientsController : ControllerBase
{
    private readonly IClientsRepository _clientsRepository;

    public clientsController(IClientsRepository clientsRepository)
    {
        _clientsRepository = clientsRepository;
    }

    [HttpGet("{clientId}")]
    public async Task<ActionResult<ClientDto>> GetClientWithRentals(int clientId)
    {
        var client = await _clientsRepository.GetClientWithRentalsAsync(clientId);
        if (client == null)
            return NotFound($"Client with ID {clientId} not found.");
        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult> CreateClientWithRental(NewClientRentalDto newClientRental)
    {
        if (!ModelState.IsValid)
            return BadRequest();

        var result = await _clientsRepository.AddClientWithRentalAsync(newClientRental);
        if (!result.IsSuccess)
            return BadRequest(result.ErrorMessage);

        return CreatedAtAction(nameof(GetClientWithRentals), new { clientId = result.ClientId }, null);
    }
}