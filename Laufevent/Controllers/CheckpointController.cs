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

                    // Query user base info
                    const string userQuery = "SELECT * FROM Userinformation WHERE uid = @uid";
                    using (var userCommand = new NpgsqlCommand(userQuery, connection))
                    {
                        userCommand.Parameters.AddWithValue("@uid", uid);
                        using (var reader = await userCommand.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                                return NotFound($"User with UID {uid} not found.");

                            var firstName = reader["firstname"]?.ToString();
                            var lastName = reader["lastname"]?.ToString();
                            var fastestLap = reader["fastest_lap"] is DBNull ? null :
                                reader.GetTimeSpan(reader.GetOrdinal("fastest_lap")).ToString(@"hh\:mm\:ss");

                            reader.Close();

                            // Query Round Count
                            const string countQuery = "SELECT COUNT(*) FROM Rounds WHERE uid = @uid";
                            int roundCount = 0;
                            using (var countCommand = new NpgsqlCommand(countQuery, connection))
                            {
                                countCommand.Parameters.AddWithValue("@uid", uid);
                                roundCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                            }

                            // Query Last Two Lap Times
                            const string lapQuery = "SELECT Scantime FROM Rounds WHERE uid = @uid ORDER BY Scantime DESC LIMIT 2";
                            TimeSpan? lapDuration = null;
                            using (var lapCommand = new NpgsqlCommand(lapQuery, connection))
                            {
                                lapCommand.Parameters.AddWithValue("@uid", uid);
                                using (var lapReader = await lapCommand.ExecuteReaderAsync())
                                {
                                    var times = new List<DateTime>();
                                    while (await lapReader.ReadAsync())
                                        times.Add(lapReader.GetDateTime(0));

                                    if (times.Count >= 2)
                                        lapDuration = times[0] - times[1];
                                }
                            }

                            var result = new
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                RoundCount = roundCount,
                                LapTime = lapDuration?.ToString(@"hh\:mm\:ss"),
                                FastestLap = fastestLap
                            };

                            return Ok(result);
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


        #endregion
    
    }
}
