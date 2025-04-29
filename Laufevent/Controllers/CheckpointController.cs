using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for managing information in the best way understandable for checkpoint-core and client
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class CheckpointController : ControllerBase
    {
        #region Gather Display-Info by uid

        /// <summary>
        /// Retrieves user information for the checkpoint-core and client based on the provided UID.
        /// </summary>
        /// <param name="uid">The UID of the user.</param>
        /// <returns>Returns the user details if found, otherwise a 404 not found error.</returns>
        [HttpGet("ci-by-uid")]
        [SwaggerOperation(
            Summary = "Get user details by UID",
            Description = "Fetches the complete user information for the given UID."
        )]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(object))]
        [SwaggerResponse(404, "User with the specified UID not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetCIByUID([FromQuery] double uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    const string query = @"
                        SELECT 
                            ui.firstname,
                            ui.lastname,
                            ui.fastest_lap,
                            (
                                SELECT COUNT(*) 
                                FROM rounds 
                                WHERE uid = ui.uid
                            ) AS round_count,
                            (
                                SELECT 
                                    r1.scantime - r2.scantime 
                                FROM rounds r1 
                                JOIN rounds r2 ON r1.uid = r2.uid 
                                WHERE r1.uid = ui.uid 
                                ORDER BY r1.scantime DESC 
                                LIMIT 1 OFFSET 1
                            ) AS lap_time
                        FROM userinformation ui
                        WHERE ui.uid = @uid";

                    using var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@uid", uid);

                    using var reader = await command.ExecuteReaderAsync();

                    if (!await reader.ReadAsync())
                        return NotFound($"User with UID {uid} not found.");

                    var firstName = reader["firstname"]?.ToString();
                    var lastName = reader["lastname"]?.ToString();

                    string fastestLap = reader["fastest_lap"] is DBNull
                        ? null
                        : ((TimeSpan)reader["fastest_lap"]).ToString(@"hh\:mm\:ss");

                    string lapTime = reader["lap_time"] is DBNull
                        ? null
                        : ((TimeSpan)reader["lap_time"]).ToString(@"hh\:mm\:ss");

                    int roundCount = reader["round_count"] is DBNull
                        ? 0
                        : Convert.ToInt32(reader["round_count"]);

                    var user = new
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        RoundCount = roundCount,
                        LapTime = lapTime,
                        FastestLap = fastestLap
                    };

                    return Ok(user);
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

        #endregion
    
    }
}
