using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
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
        /// <returns>Returns a message indicating the result of the operation and which gifts were updated.</returns>
        [HttpPut("{uid}")]
        [SwaggerOperation(
            Summary = "Update collection status for gifts by UID",
            Description = "Checks if gift 1, 2, or 3 are set to true. If they are, updates the corresponding collection status for each gift."
        )]
        [SwaggerResponse(200, "Gift collection status successfully updated.", typeof(object))]
        [SwaggerResponse(404, "User with the specified UID not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> UpdateUserGiftCollection(decimal uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    const string checkQuery = @"SELECT gift_1, gift_2, gift_3, gift_1_collected, gift_2_collected, gift_3_collected
                                                FROM user_gifts WHERE uid = @uid";

                    using (var checkCommand = new NpgsqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@uid", uid);

                        using (var reader = await checkCommand.ExecuteReaderAsync())
                        {
                            if (!await reader.ReadAsync())
                                return NotFound($"User with UID {uid} not found.");

                            bool gift1 = reader.GetBoolean(reader.GetOrdinal("gift_1"));
                            bool gift2 = reader.GetBoolean(reader.GetOrdinal("gift_2"));
                            bool gift3 = reader.GetBoolean(reader.GetOrdinal("gift_3"));

                            bool gift1Collected = reader.GetBoolean(reader.GetOrdinal("gift_1_collected"));
                            bool gift2Collected = reader.GetBoolean(reader.GetOrdinal("gift_2_collected"));
                            bool gift3Collected = reader.GetBoolean(reader.GetOrdinal("gift_3_collected"));

                            await reader.CloseAsync();

                            const string updateQuery = @"
                                UPDATE user_gifts
                                SET
                                    gift_1_collected = CASE WHEN gift_1 THEN true ELSE gift_1_collected END,
                                    gift_2_collected = CASE WHEN gift_2 THEN true ELSE gift_2_collected END,
                                    gift_3_collected = CASE WHEN gift_3 THEN true ELSE gift_3_collected END
                                WHERE uid = @uid";

                            using (var updateCommand = new NpgsqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@uid", uid);
                                await updateCommand.ExecuteNonQueryAsync();
                            }

                            var updatedGifts = new
                            {
                                Gift1Collected = gift1 && !gift1Collected,
                                Gift2Collected = gift2 && !gift2Collected,
                                Gift3Collected = gift3 && !gift3Collected
                            };

                            return Ok(new
                            {
                                Message = $"Gift collection status for user with UID {uid} has been updated.",
                                UpdatedGifts = updatedGifts
                            });
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