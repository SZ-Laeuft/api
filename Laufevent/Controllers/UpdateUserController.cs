using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for updating user information based on the provided user ID.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ModifyUserController : ControllerBase
    {
        /// <summary>
        /// Updates user information for a specific user identified by ID.
        /// </summary>
        /// <param name="id">The ID of the user to be updated.</param>
        /// <param name="userInfo">An object containing the updated user details.</param>
        /// <returns>Returns a message indicating the result of the update operation.</returns>
        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Update user information by ID",
            Description = "Updates user details such as first name, last name, uid, school class, and organization."
        )]
        [SwaggerResponse(200, "User details successfully updated.", typeof(string))]
        [SwaggerResponse(404, "User with the specified ID not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> UpdateUserById(int id, [FromBody] UpdateUserModel userInfo)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    var query = @"
                        UPDATE Userinformation 
                        SET 
                            firstname = @firstName, 
                            lastname = @lastName, 
                            uid = @uid, 
                            school_class = @schoolClass, 
                            organisation = @organisation 
                        WHERE id = @id";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@firstName", userInfo.FirstName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@lastName", userInfo.LastName ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@uid", userInfo.uid ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@schoolClass", userInfo.SchoolClass ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@organisation", userInfo.Organisation ?? (object)DBNull.Value);

                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            return Ok($"User with ID {id} successfully updated.");
                        }
                        else
                        {
                            return NotFound($"User with ID {id} not found.");
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