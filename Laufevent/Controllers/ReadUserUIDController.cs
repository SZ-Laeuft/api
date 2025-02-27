using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for retrieving user information based on the provided UID.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReadUserUIDController : ControllerBase
    {
        /// <summary>
        /// Retrieves user information based on the provided UID.
        /// </summary>
        /// <param name="uid">The UID of the user.</param>
        /// <returns>Returns the user details if found, otherwise a 404 not found error.</returns>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get user details by uid",
            Description = "Fetches the complete user information for the given uid."
        )]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(object))]
        [SwaggerResponse(404, "User with the specified uid not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetUserByUID(double uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    const string query = "SELECT * FROM Userinformation WHERE uid = @uid";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@uid", uid);

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
                            return NotFound($"User with UID {uid} not found.");
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