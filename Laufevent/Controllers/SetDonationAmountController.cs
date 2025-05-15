using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for adding or updating a user's donation amount.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class SetDonationAmountController : ControllerBase
    {
        /// <summary>
        /// Updates the donation amount for a user identified by their UID.
        /// </summary>
        /// <param name="uid">The UID of the user whose donation amount will be updated.</param>
        /// <param name="userInfo">An object containing the donation amount to be added to the user's current donation amount.</param>
        /// <returns>Returns a message indicating the result of the operation.</returns>
        [HttpPut("{uid}")]
        [SwaggerOperation(
            Summary = "Update donation amount by UID",
            Description = "Updates the donation amount for a specific user identified by their UID. The donation amount is provided in the request body."
        )]
        [SwaggerResponse(200, "Donation amount successfully updated for the user.", typeof(string))]
        [SwaggerResponse(400, "Invalid input data provided.")]
        [SwaggerResponse(404, "User with the specified UID not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> UpdateUserByUId(decimal uid, [FromBody] ChangeAmount userInfo)
        {
            if (userInfo == null || userInfo.Amount <= 0)
            {
                return BadRequest("Invalid input data. Donation amount must be greater than 0.");
            }

            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    var query = @"
                        UPDATE USER_DONATIONS 
                        SET 
                           amount = @amount
                        WHERE uid = @uid";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@uid", uid);
                        command.Parameters.AddWithValue("@amount", userInfo.Amount);

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok($"User with UID {uid} successfully updated. New donation amount: {userInfo.Amount}");
                        }
                        else
                        {
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

    /// <summary>
    /// Model for updating the user's donation amount.
    /// </summary>
    public class ChangeAmount
    {
        /// <summary>
        /// The amount to be added to the user's donation.
        /// </summary>
        public double Amount { get; set; }
    }
}
