using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for managing user donations and updating gift collection status.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SetGiftCollectedController : ControllerBase
    {
        /// <summary>
        /// Updates the collection status for gifts for a specific user identified by UID.
        /// Checks if gift 1, 2, or 3 are set to true, and updates the corresponding collection status for each gift.
        /// </summary>
        /// <param name="uid">The UID of the user whose gifts are to be updated.</param>
        /// <returns>Returns a message indicating the result of the operation.</returns>
        [HttpPut("{uid}")]
        [SwaggerOperation(
            Summary = "Update collection status for gifts by UID",
            Description = "Checks if gift 1, 2, or 3 are set to true. If they are, updates the corresponding collection status for each gift."
        )]
        [SwaggerResponse(200, "Gift collection status successfully updated.", typeof(string))]
        [SwaggerResponse(404, "User with the specified UID not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> UpdateUserGiftCollection(long uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    // Use separate commands for checking and updating.
                    NpgsqlCommand checkCommand = null;
                    NpgsqlDataReader reader = null;
                    NpgsqlCommand updateCommand = null;

                    try
                    {
                        // Check if user exists and retrieve gift data
                        checkCommand = new NpgsqlCommand(@"SELECT gift_1, gift_2, gift_3 FROM user_gifts WHERE uid = @uid", connection);
                        checkCommand.Parameters.AddWithValue("@uid", uid);
                        reader = await checkCommand.ExecuteReaderAsync();

                        // Ensure the reader is fully read before moving to the update
                        if (await reader.ReadAsync())
                        {
                            await reader.DisposeAsync(); // Dispose the reader before continuing

                            // Moved the update logic into a separate command
                            updateCommand = new NpgsqlCommand(@"
                                    UPDATE user_gifts
                                    SET 
                                        gift_1_collected = CASE WHEN gift_1 THEN true ELSE gift_1_collected END,
                                        gift_2_collected = CASE WHEN gift_2 THEN true ELSE gift_2_collected END,
                                        gift_3_collected = CASE WHEN gift_3 THEN true ELSE gift_3_collected END
                                    WHERE uid = @uid", connection);
                            updateCommand.Parameters.AddWithValue("@uid", uid);
                            int rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                            if (rowsAffected > 0)
                            {
                                return Ok($"Gift collection status for user with UID {uid} has been updated.");
                            }
                            else
                            {
                                return NotFound($"User with UID {uid} not found.");
                            }
                        }
                        else
                        {
                            return NotFound($"User with UID {uid} not found.");
                        }
                    }
                    finally
                    {
                        // Ensure that commands and readers are disposed of, even if exceptions occur.
                        if (reader != null)
                        {
                            await reader.DisposeAsync();
                        }
                        if (checkCommand != null)
                        {
                            await checkCommand.DisposeAsync();
                        }
                        if (updateCommand != null)
                        {
                            await updateCommand.DisposeAsync();
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
