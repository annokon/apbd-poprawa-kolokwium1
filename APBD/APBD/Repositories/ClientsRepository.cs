using APBD.Models.DTOs;
using Microsoft.Data.SqlClient;

namespace APBD.Repositories;

public class ClientsRepository : IClientsRepository
{
    private readonly IConfiguration _configuration;

    public ClientsRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<ClientDto> GetClientWithRentalsAsync(int clientId)
    {
        ClientDto client = null;

        using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await connection.OpenAsync();

            var command = new SqlCommand(@"
            SELECT c.ID, c.FirstName, c.LastName, c.Address,
                   car.VIN, col.Name AS Color, m.Name AS Model, 
                   cr.DateFrom, cr.DateTo, cr.TotalPrice
            FROM clients c
            LEFT JOIN car_rentals cr ON c.ID = cr.ClientID
            LEFT JOIN cars car ON cr.CarID = car.ID
            LEFT JOIN models m ON car.ModelID = m.ID
            LEFT JOIN colors col ON car.ColorID = col.ID
            WHERE c.ID = @ClientId", connection);

            command.Parameters.AddWithValue("@ClientId", clientId);

            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    client = new ClientDto
                    {
                        Id = (int)reader["ID"],
                        FirstName = reader["FirstName"].ToString(),
                        LastName = reader["LastName"].ToString(),
                        Address = reader["Address"].ToString(),
                        Rentals = new List<RentalDto>()
                    };

                    do
                    {
                        if (reader["VIN"] != DBNull.Value)
                        {
                            client.Rentals.Add(new RentalDto
                            {
                                Vin = reader["VIN"].ToString(),
                                Color = reader["Color"].ToString(),
                                Model = reader["Model"].ToString(),
                                DateFrom = (DateTime)reader["DateFrom"],
                                DateTo = (DateTime)reader["DateTo"],
                                TotalPrice = (int)reader["TotalPrice"]
                            });
                        }
                    } while (await reader.ReadAsync());
                }
            }
        }

        return client;
    }

    public async Task<(bool IsSuccess, string ErrorMessage, int ClientId)> AddClientWithRentalAsync(
        NewClientRentalDto newClientRental)
    {
        using (var connection = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await connection.OpenAsync();

            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var addClientCommand = new SqlCommand(@"
                        INSERT INTO clients (FirstName, LastName, Address)
                        OUTPUT INSERTED.ID
                        VALUES (@FirstName, @LastName, @Address)", connection, transaction);

                    addClientCommand.Parameters.AddWithValue("@FirstName", newClientRental.Client.FirstName);
                    addClientCommand.Parameters.AddWithValue("@LastName", newClientRental.Client.LastName);
                    addClientCommand.Parameters.AddWithValue("@Address", newClientRental.Client.Address);

                    var clientId = (int)await addClientCommand.ExecuteScalarAsync();


                    var checkCarCommand = new SqlCommand(@"
                        SELECT COUNT(*) FROM cars WHERE ID = @CarId", connection, transaction);
                    checkCarCommand.Parameters.AddWithValue("@CarId", newClientRental.CarId);

                    var exists = (int)await checkCarCommand.ExecuteScalarAsync();
                    if (exists == 0)
                    {
                        throw new Exception("Car not found.");
                    }


                    var addRentalCommand = new SqlCommand(@"
                        INSERT INTO car_rentals (ClientID, CarID, DateFrom, DateTo, TotalPrice)
                        VALUES (@ClientId, @CarId, @DateFrom, @DateTo, DATEDIFF(DAY, @DateFrom, @DateTo) * (SELECT PricePerDay FROM cars WHERE ID = @CarId))",
                        connection, transaction);

                    addRentalCommand.Parameters.AddWithValue("@ClientId", clientId);
                    addRentalCommand.Parameters.AddWithValue("@CarId", newClientRental.CarId);
                    addRentalCommand.Parameters.AddWithValue("@DateFrom", newClientRental.DateFrom);
                    addRentalCommand.Parameters.AddWithValue("@DateTo", newClientRental.DateTo);

                    await addRentalCommand.ExecuteNonQueryAsync();

                    transaction.Commit();
                    return (true, null, clientId);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return (false, ex.Message, 0);
                }
            }
        }
    }
}