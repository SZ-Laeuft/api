using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for handling round completion, lap duration calculations, and round counts.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class LapController : ControllerBase
    {
        /// <summary>
        /// Inserts user information into the database.
        /// </summary>
        /// <param name="userInfo">The user information to be inserted.</param>
        /// <returns>A response indicating the result of the operation.</returns>
        [HttpPost("CompleteRound")]
        [SwaggerOperation(Summary = "Inserts user information into the database", 
            Description = "This endpoint inserts the user's UID and scan time into the Rounds table.")]
        [SwaggerResponse(200, "Data inserted successfully.", typeof(string))]
        [SwaggerResponse(500, "Internal Server Error.")]
        public async Task<IActionResult> InsertUserInformation([FromBody] CompleteRoundVariables userInfo)
        {
            try
            {
                DateTime currentTime = DateTime.UtcNow;

                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = "INSERT INTO Rounds (UID, Scantime) VALUES (@UID, @Scantime);";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@UID", userInfo.uid);
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

        /// <summary>
        /// Retrieves the lap duration based on the last two scan times for the given user ID.
        /// </summary>
        /// <param name="uid">The ID of the user to fetch the lap times for.</param>
        /// <returns>Returns the lap duration (time difference between the last two laps) or an error message.</returns>
        [HttpGet("LapDuration/{uid}")]
        [SwaggerOperation(
            Summary = "Get the lap duration based on the last two scan times",
            Description = "Fetches the last two scan times for the given user ID and calculates the lap duration."
        )]
        [SwaggerResponse(200, "Lap duration successfully calculated.", typeof(object))]
        [SwaggerResponse(404, "Not enough data to calculate lap duration.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetLastLapById(long uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    var query = @"
                WITH last_two_rounds AS (
                    SELECT
                        scantime,
                        ROW_NUMBER() OVER (ORDER BY scantime DESC) AS rn
                    FROM public.rounds
                    WHERE uid = @uid
                )
                SELECT 
                    t1.scantime - t2.scantime AS laptime
                FROM last_two_rounds t1
                JOIN last_two_rounds t2 ON t1.rn = 1 AND t2.rn = 2;
            ";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@uid", uid);

                        var result = await command.ExecuteScalarAsync();

                        if (result != null && result != DBNull.Value)
                        {
                            if (result != null && result != DBNull.Value)
                            {
                                var laptime = (TimeSpan)result;
                                string formattedLapTime = laptime.ToString(@"hh\:mm\:ss"); 
                                return Ok(formattedLapTime);
                            }
                            return NotFound("Not enough rounds to calculate lap time.");
                        }
                        else
                        {
                            return NotFound("Not enough rounds to calculate lap time.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves the count of rounds for a given user Uid.
        /// </summary>
        /// <param name="Uid">The Uid of the user whose rounds count you want to retrieve.</param>
        /// <returns>Returns the count of rounds for the specified user Uid or an error message if no rounds exist.</returns>
        [HttpGet("RoundsCount/{Uid}")]
        [SwaggerOperation(
            Summary = "Get the rounds count for a given user Uid",
            Description = "Fetches the total number of rounds for a specific user Uid."
        )]
        [SwaggerResponse(200, "Rounds count successfully retrieved.", typeof(object))]
        [SwaggerResponse(404, "No rounds found for the given user Uid.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetRoundsCountByUid(long Uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = "SELECT COUNT(*) AS RoundsCount FROM Rounds WHERE Uid = @Uid";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Uid", Uid);

                        var roundsCount = await command.ExecuteScalarAsync();

                        if (roundsCount != null && Convert.ToInt32(roundsCount) > 0)
                        {
                            return Ok(new { RoundsCount = roundsCount });
                        }
                        else
                        {
                            return NotFound($"No entries found for Uid {Uid}.");
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
