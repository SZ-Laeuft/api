using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Laufevent.Controllers
{
    /// <summary>
    /// Controller for creating users with or without a school class.
    /// </summary>
    [Route("create-user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// Inserts a new user with all required information (UID, school class, organisation, etc.)
        /// </summary>
        /// <param name="userInfo">User information including first name, last name, UID, school class, organization, and early starter status.</param>
        /// <returns>Returns the newly created user ID along with a success message.</returns>
        [HttpPost("with-class")]
        [SwaggerOperation(
            Summary = "Create a user with all information",
            Description = "Inserts user data (first name, last name, uid, school class, organization, early starter) into the database."
        )]
        [SwaggerResponse(200, "Data inserted successfully.", typeof(object))]
        [SwaggerResponse(400, "Bad Request - Invalid data provided.")]
        [SwaggerResponse(500, "Internal Server Error.")]
        public async Task<IActionResult> InsertUserWithClass([FromBody] CreateUserVariablesFirstLastOrgClassUid userInfo)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = @"
                        INSERT INTO Userinformation (firstname, lastname, uid, school_class, organisation, early_starter) 
                        VALUES (@firstname, @lastname, @uid, @school_class, @organisation, @early_starter)
                        RETURNING id;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", userInfo.firstname);
                        command.Parameters.AddWithValue("@lastname", userInfo.lastname);
                        command.Parameters.AddWithValue("@uid", userInfo.uid);
                        command.Parameters.AddWithValue("@school_class", userInfo.school_class);
                        command.Parameters.AddWithValue("@organisation", userInfo.organisation);
                        command.Parameters.AddWithValue("@early_starter", DBNull.Value);

                        var newUserId = await command.ExecuteScalarAsync();
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

        /// <summary>
        /// Inserts a new user without a school class.
        /// </summary>
        /// <param name="userInfo">User information including first name, last name, and organization.</param>
        /// <returns>Returns the newly created user ID along with a success message.</returns>
        [HttpPost("without-class")]
        [SwaggerOperation(
            Summary = "Create a user without school class",
            Description = "Inserts user data (first name, last name, organization) into the database without school class."
        )]
        [SwaggerResponse(200, "Data inserted successfully.", typeof(object))]
        [SwaggerResponse(400, "Bad Request - Invalid data provided.")]
        [SwaggerResponse(500, "Internal Server Error.")]
        public async Task<IActionResult> InsertUserWithoutClass([FromBody] CreateUserVariablesFirstLastOrgUid userInfo)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    var query = @"
                        INSERT INTO Userinformation (firstname, lastname, uid, school_class, organisation, early_starter) 
                        VALUES (@firstname, @lastname, @uid, @school_class, @organisation, @early_starter)
                        RETURNING id;";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstname", userInfo.firstname);
                        command.Parameters.AddWithValue("@lastname", userInfo.lastname);
                        command.Parameters.AddWithValue("@uid", userInfo.uid);
                        command.Parameters.AddWithValue("@school_class", DBNull.Value);  // No school class
                        command.Parameters.AddWithValue("@early_starter", DBNull.Value);  // Early starter is also optional
                        command.Parameters.AddWithValue("@organisation", userInfo.organisation);

                        var newId = await command.ExecuteScalarAsync();
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
