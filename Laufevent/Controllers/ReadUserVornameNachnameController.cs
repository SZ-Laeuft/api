using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for retrieving user information based on the provided first and last name.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReadUserByNameController : ControllerBase
    {
        /// <summary>
        /// Retrieves user information based on the provided first and last name.
        /// </summary>
        /// <param name="firstName">The user's first name.</param>
        /// <param name="lastName">The user's last name.</param>
        /// <returns>Returns the user details if found, otherwise a 404 not found error.</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get user details by first and last name",
            Description = "Fetches the complete user information based on the first and last name."
        )]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(object))]
        [SwaggerResponse(404, "User with the specified first and last name not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetUserByName([FromQuery] string firstName, [FromQuery] string lastName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    const string query = "SELECT * FROM Userinformation WHERE firstname = @firstName AND lastname = @lastName";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstName", firstName);
                        command.Parameters.AddWithValue("@lastName", lastName);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    Id = reader["id"] is DBNull ? 0 : Convert.ToInt32(reader["id"]),
                                    FirstName = reader["firstname"]?.ToString(),
                                    LastName = reader["lastname"]?.ToString(),
                                    Uid = reader["uid"] is DBNull ? 0.0 : Convert.ToDouble(reader["uid"]),
                                    SchoolClass = reader["school_class"]?.ToString(),
                                    Organisation = reader["organisation"]?.ToString(),
                                    FastestLap = reader["fastest_lap"] is DBNull ? null : reader.GetTimeSpan(reader.GetOrdinal("fastest_lap")).ToString(@"hh\:mm\:ss"),
                                    EarlyStarter = reader["early_starter"] is DBNull ? (bool?)null : Convert.ToBoolean(reader["early_starter"])
                                };
                                return Ok(user);
                            }
                            return NotFound($"User with FirstName '{firstName}' and LastName '{lastName}' not found.");
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