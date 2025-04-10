using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for creating users who do not have a school class.
    /// </summary>
    [Route("create-user-that-has-no-class")]
    [ApiController]
    public class CreateUserFirstLastOrgUidController : ControllerBase
    {
        /// <summary>
        /// Inserts a new user without school class.
        /// </summary>
        /// <param name="userInfo">User information including first name, last name, and organization.</param>
        /// <returns>Returns the newly created user ID along with a success message.</returns>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a user without school class",
            Description = "Inserts user data (first name, last name, organization) into the database without school class."
        )]
        [SwaggerResponse(200, "Data inserted successfully.", typeof(object))]
        [SwaggerResponse(400, "Bad Request - Invalid data provided.")]
        [SwaggerResponse(500, "Internal Server Error.")]
        public async Task<IActionResult> InsertUserInformation([FromBody] CreateUserVariablesFirstLastOrgUid userInfo)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();

                    var query = @"
                        INSERT INTO Userinformation (firstname, lastname, uid, school_class, organisation, early_starter) 
                        VALUES (@firstname, @lastname, @uid, @school_class, @organisation, @early_starter)
                        RETURNING id;";  // PostgreSQL uses RETURNING to fetch the newly inserted ID

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", userInfo.firstname);
                        command.Parameters.AddWithValue("@lastname", userInfo.lastname);
                        command.Parameters.AddWithValue("@uid", userInfo.uid);  
                        command.Parameters.AddWithValue("@school_class", DBNull.Value);
                        command.Parameters.AddWithValue("@early_starter", DBNull.Value); 
                        command.Parameters.AddWithValue("@organisation", userInfo.organisation);

                        var newId = await command.ExecuteScalarAsync();  // Fetch the newly inserted ID

                        // Return the ID along with a success message
                        return Ok(new { Id = newId, Message = "Data inserted successfully." });
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