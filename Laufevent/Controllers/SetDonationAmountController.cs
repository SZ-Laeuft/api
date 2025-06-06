using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;

namespace Laufevent.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SetDonationAmountController : ControllerBase
    {
        [HttpPut("{uid}")]
        [SwaggerOperation(
            Summary = "Update donation amount by UID",
            Description = "Adds the input donation amount to the current amount for a specific user identified by their UID."
        )]
        [SwaggerResponse(200, "Donation amount successfully updated for the user.", typeof(object))]
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

                    
                    const string selectQuery = "SELECT amount FROM USER_DONATIONS WHERE uid = @uid";
                    decimal currentAmount;

                    using (var selectCommand = new NpgsqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@uid", uid);

                        var result = await selectCommand.ExecuteScalarAsync();
                        if (result == null)
                        {
                            return NotFound($"User with UID {uid} not found.");
                        }

                        currentAmount = Convert.ToDecimal(result);
                    }

                    // Calculate the new amount
                    decimal newAmount = currentAmount + Convert.ToDecimal(userInfo.Amount);

                    const string updateQuery = @"
                        UPDATE USER_DONATIONS
                        SET amount = @amount
                        WHERE uid = @uid";

                    using (var updateCommand = new NpgsqlCommand(updateQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@uid", uid);
                        updateCommand.Parameters.AddWithValue("@amount", newAmount);

                        var rowsAffected = await updateCommand.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok(new
                            {
                                Message = $"User with UID {uid} successfully updated.",
                                PreviousAmount = currentAmount,
                                InputAmount = userInfo.Amount,
                                NewAmount = newAmount
                            });
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

    public class ChangeAmount
    {
        public double Amount { get; set; }
    }
}