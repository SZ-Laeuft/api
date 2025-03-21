using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for deleting users by their unique UID.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DeleteUserByUUidController : ControllerBase
    {
        /// <summary>
        /// Deletes a user by their Uid.
        /// </summary>
        /// <param name="Uid">The Uid of the user to be deleted.</param>
        /// <returns>Returns a success or error message based on the outcome of the deletion.</returns>
        [HttpDelete("{Uid}")]
        [SwaggerOperation(
            Summary = "Delete a user by Uid",
            Description = "Deletes a user from the database based on their unique user Uid."
        )]
        [SwaggerResponse(200, "User successfully deleted.", typeof(string))]
        [SwaggerResponse(404, "User not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> DeleteUserByUid(double Uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = "DELETE FROM Userinformation WHERE Uid = @Uid";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Uid", Uid);

                        // Execute the delete command and check how many rows were affected
                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok($"User with Uid {Uid} has been deleted.");
                        }
                        else
                        {
                            return NotFound($"User with Uid {Uid} not found.");
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