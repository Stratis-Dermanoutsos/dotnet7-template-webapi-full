using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using webapi_full.Attributes;
using webapi_full.Entities.Request;
using webapi_full.Exceptions;
using webapi_full.Extensions;
using webapi_full.IUtils;
using webapi_full.Models;

namespace webapi_full.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Logged]
public class UserController : ControllerBase
{
    private readonly ApplicationDbContext dbContext;
    private readonly IPasswordUtils passwordUtils;
    private readonly IConfiguration configuration;
    private readonly IUserUtils userUtils;

    public UserController(IConfiguration configuration, ApplicationDbContext dbContext, IPasswordUtils passwordUtils, IUserUtils userUtils)
    {
        this.configuration = configuration;
        this.dbContext = dbContext;
        this.passwordUtils = passwordUtils;
        this.userUtils = userUtils;
    }

    #region GET methods
    /// <summary>
    /// Get all users.
    /// </summary>
    [HttpGet]
    [Route("all")]
    public IActionResult GetAll()
    {
        IQueryable<User> users = this.dbContext.Users.GetAll();

        Log.Information($"Retrieved {users.Count()} users.");

        return Ok(users);
    }

    /// <summary>
    /// Get the logged user.
    /// </summary>
    [Authorize]
    [HttpGet]
    public IActionResult GetLoggedUser()
    {
        User user = this.userUtils.GetLoggedUser(this.User);

        Log.Information($"Retrieved user '{user.UserName}'.");

        return Ok(user);
    }

    /// <summary>
    /// Get user by username.
    /// </summary>
    [HttpGet]
    [Route("{userName}")]
    public IActionResult GetByUserName([FromRoute] string userName)
    {
        User? user = this.userUtils.GetByUserName(userName);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the username '{userName}'.");

        return Ok(user);
    }

    /// <summary>
    /// Get user by id.
    /// </summary>
    [Authorize]
    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetById([FromRoute] int id)
    {
        User? user = this.dbContext.Users.Get(id);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the id '{id}'.");

        return Ok(user);
    }
    #endregion

    #region DELETE methods
    /// <summary>
    /// Delete a user by id.
    /// </summary>
    [Authorize]
    [HttpDelete]
    [Route("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
    {
        User? user = this.dbContext.Users.GetAll().Get(id);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the id '{id}'.");

        if (user.Id == this.userUtils.GetLoggedUserId(this.User))
            throw new BadRequestException("You cannot delete your own account.");

        string usernameOld = user.UserName;

        this.dbContext.Users.Remove(user);
        this.dbContext.SaveChanges();

        Log.Information($"Deleted user '{usernameOld}'.");

        return Ok(user.FullName);
    }
    #endregion

    #region POST methods
    /// <summary>
    /// Create a new user account.
    /// <br/>
    /// <paramref name="entity" />: The user's information.
    /// </summary>
    [HttpPost]
    [Route("register")]
    public IActionResult Register([FromBody] UserToCreate entity)
    {
        try {
            this.passwordUtils.Validate(entity.Password);
        } catch (Exception ex) {
            throw new BadRequestException(ex.Message);
        }

        //* Check if email exists
        if (this.userUtils.GetByEmail(entity.Email) is not null)
            throw new ConflictException($"Email {entity.Email} belongs to another user.");

        //* Check if userName exists
        if (this.userUtils.GetByUserName(entity.UserName) is not null)
            throw new ConflictException($"Username {entity.UserName} belongs to another user.");

        //* Get the User object
        User user = entity.ToUser();

        //* Encrypt the user's password
        user.Password = this.passwordUtils.Encrypt(user.Password);

        //* Add the user to the database
        this.dbContext.Users.Add(user);
        this.dbContext.SaveChanges();

        Log.Information($"Registered user '{user.UserName}'.");

        return Ok(user.FullName);
    }

    /// <summary>
    /// Create a new session for a registered user.
    /// <br/>
    /// <paramref name="credentials" />: The user's login credentials.
    /// </summary>
    [HttpPost]
    [Route("login")]
    public IActionResult Login([FromBody] UserCredentials credentials)
    {
        User? user = this.userUtils.GetByEmail(credentials.Email);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the email address '{credentials.Email}'.");

        if (!passwordUtils.Check(credentials.Password, user.Password))
            throw new BadRequestException("Wrong email or password.");

        //* Create claims details based on the user information
        var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, configuration["Jwt:Subject"] ??
                    throw new ArgumentNullException("JWT subject is null")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ??
                    throw new ArgumentNullException("JWT key is null")));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var token = new JwtSecurityToken(
            configuration["Jwt:Issuer"],
            configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(25),
            signingCredentials: signIn);

        Log.Information($"User '{user.UserName}' logged in.");

        return Ok(new JwtSecurityTokenHandler().WriteToken(token));
    }
    #endregion

    #region PUT methods
    /// <summary>
    /// Update a user's information.
    /// <br/>
    /// <paramref name="id" />: The user's id.
    /// <br/>
    /// <paramref name="entity" />: The user's new information.
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    public IActionResult UpdateUser([FromRoute] int id, [FromBody] UserToUpdate entity)
    {
        User? user = this.dbContext.Users.Get(id);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the id '{id}'.");

        //* Check if email exists
        if (entity.Email != user.Email && this.userUtils.GetByEmail(entity.Email) is not null)
            throw new ConflictException($"Email {entity.Email} belongs to another user.");

        //* Check if userName exists
        if (entity.UserName != user.UserName && this.userUtils.GetByUserName(entity.UserName) is not null)
            throw new ConflictException($"Username {entity.UserName} belongs to another user.");

        //* Update the user's information
        user = entity.MergeToUser(user);

        //* Update the user
        this.dbContext.Users.Update(user);
        this.dbContext.SaveChanges();

        return Ok(user.FullName);
    }

    /// <summary>
    /// Update the logged user's information.
    /// <br/>
    /// <paramref name="entity" />: The user's new information.
    /// </summary>
    [Authorize]
    [HttpPut]
    public IActionResult UpdateLoggedUser([FromBody] UserToUpdate entity)
    {
        int id = this.userUtils.GetLoggedUserId(this.User);

        return this.UpdateUser(id, entity);
    }

    /// <summary>
    /// Update a user's password.
    /// <br/>
    /// <paramref name="id" />: The user's id.
    /// <br/>
    /// <paramref name="entity" />: The new passwords.
    /// </summary>
    [Authorize]
    [HttpPut("password/{id:int}")]
    public IActionResult UpdatePassword([FromRoute] int id, [FromBody] PasswordConfirm entity)
    {
        if (!entity.Password.Equals(entity.PasswordConfirmation))
            throw new BadRequestException("Passwords do not match.");

        try {
            this.passwordUtils.Validate(entity.Password);
        } catch (Exception ex) {
            throw new BadRequestException(ex.Message);
        }

        User? user = this.dbContext.Users.Get(id);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the id '{id}'.");

        if (passwordUtils.Check(entity.Password, user.Password))
            throw new BadRequestException("The new password must be different from the old one.");

        //* Set the user's new password
        user.Password = this.passwordUtils.Encrypt(entity.Password);

        //* Update the user
        this.dbContext.Users.Update(user);
        this.dbContext.SaveChanges();

        return Ok(user.FullName);
    }

    /// <summary>
    /// Update the logged user's password.
    /// <br/>
    /// <paramref name="entity" />: The user's old and new passwords.
    /// </summary>
    [Authorize]
    [HttpPut("password")]
    public IActionResult UpdateLoggedUserPassword([FromBody] PasswordToUpdate entity)
    {
        User user = this.userUtils.GetLoggedUser(this.User);

        if (!passwordUtils.Check(entity.OldPassword, user.Password))
            throw new UnauthorizedException("Wrong password.");

        return this.UpdatePassword(user.Id, entity);
    }
    #endregion
}