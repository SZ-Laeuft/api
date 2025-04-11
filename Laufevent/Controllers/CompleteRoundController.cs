using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for handling round completion operations.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CompleteRoundController : ControllerBase
    {
        /// <summary>
        /// Inserts user information into the database.
        /// </summary>
        /// <param name="userInfo">The user information to be inserted.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Inserts user information into the database", 
            Description = "This endpoint inserts the user's UID and scan time into the Rounds table.")]
        [SwaggerResponse(200, "Data inserted successfully.", typeof(string))]
        [SwaggerResponse(500, "Internal Server Error.")]
        public async Task<IActionResult> InsertUserInformation([FromBody] CompleteRoundVariables userInfo)
        {
            try
            {
                DateTime currentTime = DateTime.UtcNow; // Use UTC for consistency

                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = "INSERT INTO Rounds (UID, Scantime) VALUES (@UID, @Scantime);";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UID", userInfo.uid); // PostgreSQL supports bigints, use appropriate type
                        command.Parameters.AddWithValue("@Scantime", currentTime);

                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        return Ok($"Data inserted successfully. Rows affected: {rowsAffected}");
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