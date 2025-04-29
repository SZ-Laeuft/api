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

                    // Step 1: Get basic user info
                    const string userQuery = "SELECT * FROM Userinformation WHERE uid = @uid";
                    using var userCommand = new NpgsqlCommand(userQuery, connection);
                    userCommand.Parameters.AddWithValue("@uid", uid);

                    using var reader = await userCommand.ExecuteReaderAsync();

                    if (!await reader.ReadAsync())
                        return NotFound($"User with UID {uid} not found.");

                    var firstName = reader["firstname"]?.ToString();
                    var lastName = reader["lastname"]?.ToString();
                    var fastestLap = reader["fastest_lap"] is DBNull
                        ? null
                        : reader.GetTimeSpan(reader.GetOrdinal("fastest_lap")).ToString(@"hh\:mm\:ss");

                    await reader.CloseAsync(); // Needed before executing a new command on same connection

                    // Step 2: Get round count
                    const string roundCountQuery = "SELECT COUNT(*) FROM Rounds WHERE uid = @uid";
                    using var roundCountCmd = new NpgsqlCommand(roundCountQuery, connection);
                    roundCountCmd.Parameters.AddWithValue("@uid", uid);
                    var roundCountResult = await roundCountCmd.ExecuteScalarAsync();
                    var roundCount = Convert.ToInt32(roundCountResult);

                    // Step 3: Get lap time (difference between last two scan times)
                    const string lapTimeQuery = "SELECT Scantime FROM Rounds WHERE uid = @uid ORDER BY Scantime DESC LIMIT 2";
                    using var lapTimeCommand = new NpgsqlCommand(lapTimeQuery, connection);
                    lapTimeCommand.Parameters.AddWithValue("@uid", uid);

                    var lapTimes = new List<DateTime>();
                    using var lapReader = await lapTimeCommand.ExecuteReaderAsync();
                    while (await lapReader.ReadAsync())
                    {
                        lapTimes.Add(lapReader.GetDateTime(0));
                    }

                    string lapTime = lapTimes.Count >= 2
                        ? (lapTimes[0] - lapTimes[1]).ToString(@"hh\:mm\:ss")
                        : null;

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
