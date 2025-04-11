using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for retrieving user information based on the provided user ID.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReadUserIDController : ControllerBase
    {
        /// <summary>
        /// Retrieves user information based on the provided user ID.
        /// </summary>
        /// <param name="id">The unique ID of the user.</param>
        /// <returns>Returns the user details if found, otherwise a 404 not found error.</returns>
        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Get user details by ID",
            Description = "Fetches the complete user information for the given user ID."
        )]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(object))]
        [SwaggerResponse(404, "User with the specified ID not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = "SELECT * FROM Userinformation WHERE id = @id";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    Id = reader["id"],
                                    FirstName = reader["firstname"],
                                    LastName = reader["lastname"],
                                    Uid = reader["uid"],
                                    SchoolClass = reader["school_class"],
                                    Organisation = reader["organisation"],
                                    FastestLap = reader["fastest_lap"],
                                    EarlyStarter = reader["early_starter"]
                                };
                                return Ok(user);
                            }
                            else
                            {
                                return NotFound($"User with ID {id} not found.");
                            }
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}