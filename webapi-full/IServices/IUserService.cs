using System.Security.Claims;
using webapi_full.Entities.Request;
using webapi_full.Exceptions;
using webapi_full.Models;

namespace webapi_full.IServices;

/// <summary>
/// Service to encapsulate all user related operations.
/// </summary>
public interface IUserService
{
    #region GET
    /// <summary>
    /// Get all users that have not been deleted.
    /// </summary>
    /// <returns>The users.</returns>
    public IQueryable<User> GetAll();

    /// <summary>
    /// Get the logged user.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="principal" />: The logged user.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The logged user.</returns>
    public User GetLoggedUser(ClaimsPrincipal principal);

    /// <summary>
    /// Get a user by username.
    /// </summary>
    /// <param name="username">The username.</param>
    /// <returns>The user.</returns>
    /// <exception cref="NotFoundException">Thrown if the user is not found.</exception>
    public User GetByUsername(string username);

    /// <summary>
    /// Get a user by id.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="id" />: The user's id.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The user.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown if the user is not found.
    /// </exception>
    public User GetById(int id);
    #endregion

    #region DELETE
    /// <summary>
    /// Delete a user by id.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="id" />: The user's id.<br/>
    /// </item>
    /// <item>
    /// <paramref name="principal" />: The logged user.
    /// </item>
    /// </list>
    /// </summary>
    /// <exception cref="NotFoundException">
    /// Thrown if the user is not found.
    /// </exception>
    /// <exception cref="BadRequestException">
    /// Thrown if the user is the logged user.
    /// </exception>
    public User Delete(int id, ClaimsPrincipal principal);
    #endregion

    #region POST
    /// <summary>
    /// Create a new user account.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="entity" />: The user's information.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The created user.</returns>
    /// <exception cref="BadRequestException">
    /// Thrown if the user is invalid.
    /// </exception>
    /// <exception cref="ConflictException">
    /// Thrown if the username or email is already taken.
    /// </exception>
    public User Register(UserToCreate entity);

    /// <summary>
    /// Create a new session for a registered user.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="credentials" />: The user's login credentials.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The JWT token.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown if the user is not found.
    /// </exception>
    /// <exception cref="BadRequestException">
    /// Thrown if the credentials are invalid.
    /// </exception>
    public string Login(UserCredentials credentials);
    #endregion

    #region PUT
    /// <summary>
    /// Update a user's information by id.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="id" />: The user's id.<br/>
    /// <paramref name="entity" />: The user's information.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The updated user.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown if the user is not found.
    /// </exception>
    /// <exception cref="ConflictException">
    /// Thrown if the username or email is already taken.
    /// </exception>
    public User UpdateUser(int id, UserToUpdate entity);

    /// <summary>
    /// Update the logged user's information.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="entity" />: The user's information.<br/>
    /// </item>
    /// <item>
    /// <paramref name="principal" />: The logged user.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The updated user.</returns>
    /// <exception cref="NotFoundException">
    /// Thrown if the user is not found.
    /// </exception>
    /// <exception cref="ConflictException">
    /// Thrown if the username or email is already taken.
    /// </exception>
    public User UpdateLoggedUser(UserToUpdate entity, ClaimsPrincipal principal);

    /// <summary>
    /// Update a user's password by id.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="id" />: The user's id.<br/>
    /// <paramref name="entity" />: The user's password.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The updated user.</returns>
    /// <exception cref="BadRequestException">
    /// Thrown if the password is invalid.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown if the user is not found.
    /// </exception>
    /// <exception cref="BadRequestException">
    /// Thrown if the new password is the same as the old one.
    /// </exception>
    public User UpdatePassword(int id, PasswordConfirm entity);

    /// <summary>
    /// Update the logged user's password.<br/>Params:
    /// <list>
    /// <item>
    /// <paramref name="entity" />: The user's password.<br/>
    /// </item>
    /// <item>
    /// <paramref name="principal" />: The logged user.
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>The updated user.</returns>
    /// <exception cref="UnauthorizedException">
    /// Thrown if the provided password is wrong.
    /// </exception>
    /// <exception cref="BadRequestException">
    /// Thrown if the password is invalid.
    /// </exception>
    /// <exception cref="NotFoundException">
    /// Thrown if the user is not found.
    /// </exception>
    /// <exception cref="BadRequestException">
    /// Thrown if the new password is the same as the old one.
    /// </exception>
    public User UpdateLoggedUserPassword(PasswordToUpdate entity, ClaimsPrincipal principal);
    #endregion
}