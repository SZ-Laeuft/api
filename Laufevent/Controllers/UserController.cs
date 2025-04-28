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
            Description =
                "Inserts user data (first name, last name, uid, school class, organization, early starter) into the database."
        )]
        [SwaggerResponse(200, "Data inserted successfully.", typeof(object))]
        [SwaggerResponse(400, "Bad Request - Invalid data provided.")]
        [SwaggerResponse(500, "Internal Server Error.")]
        public async Task<IActionResult> InsertUserWithClass(
            [FromBody] CreateUserVariablesFirstLastOrgClassUid userInfo)
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
            Description =
                "Inserts user data (first name, last name, organization) into the database without school class."
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
                        command.Parameters.AddWithValue("@school_class", DBNull.Value); // No school class
                        command.Parameters.AddWithValue("@early_starter", DBNull.Value); // Early starter is also optional
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

    /// <summary>
    /// Controller for retrieving user information based on the provided UID.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReadUIDController : ControllerBase
    {
        /// <summary>
        /// Retrieves user information based on the provided UID.
        /// </summary>
        /// <param name="uid">The UID of the user.</param>
        /// <returns>Returns the user details if found, otherwise a 404 not found error.</returns>
        [HttpGet("ReadByUID")]
        [SwaggerOperation(
            Summary = "Get user details by uid",
            Description = "Fetches the complete user information for the given uid."
        )]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(object))]
        [SwaggerResponse(404, "User with the specified uid not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetUserByUID(double uid)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    const string query = "SELECT * FROM Userinformation WHERE uid = @uid";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@uid", uid);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    Id = reader["id"] is DBNull ? 0 : Convert.ToInt32(reader["id"]),
                                    FirstName = reader["firstname"]?.ToString(),
                                    LastName = reader["lastname"]?.ToString(),
                                    Uid = reader["uid"] is DBNull ? 0.0 : Convert.ToDouble(reader["uid"]),
                                    SchoolClass = reader["school_class"]?.ToString(),
                                    Organisation = reader["organisation"]?.ToString(),
                                    FastestLap = reader["fastest_lap"] is DBNull
                                        ? null
                                        : reader.GetTimeSpan(reader.GetOrdinal("fastest_lap"))
                                            .ToString(@"hh\:mm\:ss"),
                                    EarlyStarter = reader["early_starter"] is DBNull
                                        ? (bool?)null
                                        : Convert.ToBoolean(reader["early_starter"])
                                };
                                return Ok(user);
                            }

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
    /// Controller for retrieving user information based on the provided first and last name.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ReadByNameController : ControllerBase
    {
        /// <summary>
        /// Retrieves user information based on the provided first and last name.
        /// </summary>
        /// <param name="firstName">The user's first name.</param>
        /// <param name="lastName">The user's last name.</param>
        /// <returns>Returns the user details if found, otherwise a 404 not found error.</returns>
        [HttpGet("ReadByName")]
        [SwaggerOperation(
            Summary = "Get user details by first and last name",
            Description = "Fetches the complete user information based on the first and last name."
        )]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(object))]
        [SwaggerResponse(404, "User with the specified first and last name not found.")]
        [SwaggerResponse(500, "Internal Server Error - Database issue or unexpected error.")]
        public async Task<IActionResult> GetUserByName([FromQuery] string firstName,
            [FromQuery] string lastName)
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString.connectionstring))
                {
                    await connection.OpenAsync();
                    const string query =
                        "SELECT * FROM Userinformation WHERE firstname = @firstName AND lastname = @lastName";

                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@firstName", firstName);
                        command.Parameters.AddWithValue("@lastName", lastName);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var user = new
                                {
                                    Id = reader["id"] is DBNull ? 0 : Convert.ToInt32(reader["id"]),
                                    FirstName = reader["firstname"]?.ToString(),
                                    LastName = reader["lastname"]?.ToString(),
                                    Uid = reader["uid"] is DBNull ? 0.0 : Convert.ToDouble(reader["uid"]),
                                    SchoolClass = reader["school_class"]?.ToString(),
                                    Organisation = reader["organisation"]?.ToString(),
                                    FastestLap = reader["fastest_lap"] is DBNull
                                        ? null
                                        : reader.GetTimeSpan(reader.GetOrdinal("fastest_lap"))
                                            .ToString(@"hh\:mm\:ss"),
                                    EarlyStarter = reader["early_starter"] is DBNull
                                        ? (bool?)null
                                        : Convert.ToBoolean(reader["early_starter"])
                                };
                                return Ok(user);
                            }

                            return NotFound(
                                $"User with FirstName '{firstName}' and LastName '{lastName}' not found.");
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
