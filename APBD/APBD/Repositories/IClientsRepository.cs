using APBD.Models.DTOs;

namespace APBD.Repositories;

public interface IClientsRepository
{
    Task<ClientDto> GetClientWithRentalsAsync(int clientId);

    Task<(bool IsSuccess, string ErrorMessage, int ClientId)> AddClientWithRentalAsync(NewClientRentalDto newClientRental);
}