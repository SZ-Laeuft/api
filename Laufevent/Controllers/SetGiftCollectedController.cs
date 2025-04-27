using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;

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
        public async Task<IActionResult> UpdateUserGiftCollection(double uid)
        {
            try
            {
                // Create the connection object and ensure it's disposed of correctly.
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    // First, check if the user exists in the USER_GIFTS table.
                    var checkQuery = @"
                        SELECT gift_1, gift_2, gift_3
                        FROM user_gifts
                        WHERE uid = @uid";

                    // Execute the check command and await its completion before proceeding.
                    using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@uid", uid);

                        // Ensure the reader is fully consumed before proceeding with the update.
                        using (var reader = await checkCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                // Prepare the update query to set the 'gift_X_collected' flags based on 'gift_X' values.
                                var updateQuery = @"
                                    UPDATE user_gifts
                                    SET 
                                        gift_1_collected = CASE WHEN gift_1 THEN true ELSE gift_1_collected END,
                                        gift_2_collected = CASE WHEN gift_2 THEN true ELSE gift_2_collected END,
                                        gift_3_collected = CASE WHEN gift_3 THEN true ELSE gift_3_collected END
                                    WHERE uid = @uid";

                                // Execute the update command after the check has completed.
                                using (var updateCommand = new NpgsqlCommand(updateQuery, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@uid", uid);

                                    var rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                                    // Return appropriate responses based on the number of affected rows.
                                    if (rowsAffected > 0)
                                    {
                                        return Ok($"Gift collection status for user with UID {uid} has been updated.");
                                    }
                                    else
                                    {
                                        return NotFound($"User with UID {uid} not found.");
                                    }
                                }
                            }
                            else
                            {
                                // User was not found.
                                return NotFound($"User with UID {uid} not found.");
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
