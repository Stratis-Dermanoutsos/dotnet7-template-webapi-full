using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapi_full.Attributes;
using webapi_full.Entities.Request;
using webapi_full.IServices;

namespace webapi_full.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Logged]
public class UserController : ControllerBase
{
    private readonly IUserService userService;

    public UserController(IUserService userService) => this.userService = userService;

    #region GET methods
    /// <summary>
    /// Get all users.
    /// </summary>
    [HttpGet]
    [Route("all")]
    public IActionResult GetAll()
        => Ok(this.userService.GetAll());

    /// <summary>
    /// Get the logged user.
    /// </summary>
    [Authorize(Policy = "user")]
    [HttpGet]
    public IActionResult GetLoggedUser()
        => Ok(this.userService.GetLoggedUser(this.User));

    /// <summary>
    /// Get a user by username.
    /// </summary>
    [HttpGet]
    [Route("{userName}")]
    public IActionResult GetByUserName([FromRoute] string userName)
        => Ok(this.userService.GetByUsername(userName));

    /// <summary>
    /// Get a user by id.
    /// </summary>
    [Authorize(Policy = "admin")]
    [HttpGet]
    [Route("{id:int}")]
    public IActionResult GetById([FromRoute] int id)
        => Ok(this.userService.GetById(id));
    #endregion

    #region DELETE methods
    /// <summary>
    /// Delete a user by id.
    /// </summary>
    [Authorize(Policy = "admin")]
    [HttpDelete]
    [Route("{id:int}")]
    public IActionResult Delete([FromRoute] int id)
        => Ok(this.userService.Delete(id, this.User));
    #endregion

    #region POST methods
    /// <summary>
    /// Create a new user account.
    /// <br/>
    /// <paramref name="entity" />: The user's information.
    /// </summary>
    [HttpPost]
    public IActionResult Register([FromBody] UserToCreate entity)
        => Ok(this.userService.Register(entity));

    /// <summary>
    /// Create a new session for a registered user.
    /// <br/>
    /// <paramref name="credentials" />: The user's login credentials.
    /// </summary>
    [HttpPost]
    [Route("login")]
    public IActionResult Login([FromBody] UserCredentials credentials)
        => Ok(this.userService.Login(credentials));
    #endregion

    #region PUT methods
    /// <summary>
    /// Update a user's information.
    /// <br/>
    /// <paramref name="id" />: The user's id.
    /// <br/>
    /// <paramref name="entity" />: The user's new information.
    /// </summary>
    [Authorize(Policy = "admin")]
    [HttpPut]
    [Route("{id:int}")]
    public IActionResult UpdateUser([FromRoute] int id, [FromBody] UserToUpdate entity)
        => Ok(this.userService.UpdateUser(id, entity));

    /// <summary>
    /// Update the logged user's information.
    /// <br/>
    /// <paramref name="entity" />: The user's new information.
    /// </summary>
    [Authorize(Policy = "user")]
    [HttpPut]
    public IActionResult UpdateLoggedUser([FromBody] UserToUpdate entity)
        => Ok(this.userService.UpdateLoggedUser(entity, this.User));

    /// <summary>
    /// Update a user's password.
    /// <br/>
    /// <paramref name="id" />: The user's id.
    /// <br/>
    /// <paramref name="entity" />: The new passwords.
    /// </summary>
    [Authorize(Policy = "admin")]
    [HttpPut]
    [Route("password/{id:int}")]
    public IActionResult UpdatePassword([FromRoute] int id, [FromBody] PasswordConfirm entity)
        => Ok(this.userService.UpdatePassword(id, entity));

    /// <summary>
    /// Update the logged user's password.
    /// <br/>
    /// <paramref name="entity" />: The user's old and new passwords.
    /// </summary>
    [Authorize(Policy = "user")]
    [HttpPut]
    [Route("password")]
    public IActionResult UpdateLoggedUserPassword([FromBody] PasswordToUpdate entity)
        => Ok(this.userService.UpdateLoggedUserPassword(entity, this.User));
    #endregion
}