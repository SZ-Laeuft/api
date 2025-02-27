using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    [Route("create-user-that-has-everything")]
    [ApiController]
    public class CreateUserFirstLastOrgClassUidController : ControllerBase
    {
        private bool early_starter = false;

        /// <summary>
        /// Inserts a new user who has all required information (UID, school class, organisation, etc.)
        /// </summary>
        /// <param name="userInfo">User information (first name, last name, UID, school class, organization, early starter)</param>
        /// <returns>Returns the newly created user ID along with a success message.</returns>
        [HttpPost]
        [SwaggerOperation(Summary = "Create a user with all information", 
                          Description = "Inserts user data (first name, last name, uid, school class, organization, early starter) into the database.")]
        [SwaggerResponse(200, "Data inserted successfully.", typeof(object))]
        [SwaggerResponse(400, "Bad Request - Invalid data provided.")]
        [SwaggerResponse(500, "Internal Server Error.")]
        public async Task<IActionResult> InsertUserInformation([FromBody] CreateUserVariablesFirstLastOrgClassUid userInfo)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = @"
                        INSERT INTO Userinformation (firstname, lastname, uid, school_class, organisation, early_starter) 
                        VALUES (@firstname, @lastname, @uid, @school_class, @organisation, @early_starter)
                        RETURNING id;"; // PostgreSQL equivalent of SCOPE_IDENTITY()

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", userInfo.firstname);
                        command.Parameters.AddWithValue("@lastname", userInfo.lastname);
                        command.Parameters.AddWithValue("@uid", userInfo.uid);
                        command.Parameters.AddWithValue("@school_class", userInfo.school_class);
                        command.Parameters.AddWithValue("@organisation", userInfo.organisation );
                        command.Parameters.AddWithValue("@early_starter", DBNull.Value); // Adjust if needed

                        var newUserId = await command.ExecuteScalarAsync();  // Fetch the newly inserted ID
                        return Ok(new { Id = newUserId, Message = "Data inserted successfully." });
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
