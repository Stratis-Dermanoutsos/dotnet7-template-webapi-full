using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using webapi_full.Entities.Request;
using webapi_full.Exceptions;
using webapi_full.Extensions;
using webapi_full.IServices;
using webapi_full.IUtils;
using webapi_full.Models;

namespace webapi_full.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext dbContext;
    private readonly IPasswordUtils passwordUtils;
    private readonly IConfiguration configuration;
    private readonly IUserUtils userUtils;

    public UserService(ApplicationDbContext dbContext, IPasswordUtils passwordUtils, IConfiguration configuration, IUserUtils userUtils)
    {
        this.dbContext = dbContext;
        this.passwordUtils = passwordUtils;
        this.configuration = configuration;
        this.userUtils = userUtils;
    }

    #region GET
    /// <inheritdoc/>
    public IQueryable<User> GetAll()
    {
        IQueryable<User> users = this.dbContext.Users.GetAll();

        Log.Information($"Retrieved {users.Count()} users.");

        return users;
    }

    /// <inheritdoc/>
    public User GetLoggedUser(ClaimsPrincipal principal)
    {
        User user = this.userUtils.GetLoggedUser(principal);

        Log.Information($"Retrieved logged user {user.UserName}.");

        return user;
    }

    /// <inheritdoc/>
    public User GetByUsername(string username)
    {
        User? user = this.userUtils.GetByUserName(username);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the username {username}.");

        return user;
    }

    /// <inheritdoc/>
    public User GetById(int id)
    {
        User? user = this.dbContext.Users.Get(id);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the id {id}.");

        return user;
    }
    #endregion

    #region DELETE
    /// <inheritdoc/>
    public User Delete(int id, ClaimsPrincipal principal)
    {
        if (id == this.userUtils.GetLoggedUserId(principal))
            throw new BadRequestException("You cannot delete your own account.");

        User user = this.GetById(id);

        string usernameOld = user.UserName;

        this.dbContext.Users.Remove(user);
        this.dbContext.SaveChanges();

        Log.Information($"Deleted user {usernameOld}.");

        return user;
    }
    #endregion

    #region POST
    /// <inheritdoc/>
    public User Register(UserToCreate entity)
    {
        //* Validate credentials
        try {
            this.userUtils.ValidateEmail(entity.Email);
            this.passwordUtils.Validate(entity.Password);
            this.userUtils.ValidateUserName(entity.UserName);
        } catch (Exception exception) {
            throw new BadRequestException(exception.Message);
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

        Log.Information($"Registered user {user.UserName}.");

        return user;
    }

    /// <inheritdoc/>
    public string Login(UserCredentials credentials)
    {
        User? user = this.userUtils.GetByEmail(credentials.Email);

        if (user is null)
            throw new NotFoundException($"There is no user account associated with the email {credentials.Email}.");

        if (!passwordUtils.Check(credentials.Password, user.Password))
            throw new BadRequestException("Wrong email or password.");

        //* Create claims details based on the user information
        var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, this.configuration["Jwt:Subject"] ??
                    throw new ArgumentNullException("JWT subject is null")),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, ((int)user.Role).ToString())
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Jwt:Key"] ??
                    throw new ArgumentNullException("JWT key is null")));
        var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        var token = new JwtSecurityToken(
            this.configuration["Jwt:Issuer"],
            this.configuration["Jwt:Audience"],
            claims,
            expires: DateTime.UtcNow.AddMinutes(25),
            signingCredentials: signIn);

        Log.Information($"User {user.UserName} logged in.");

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    #endregion

    #region PUT
    /// <inheritdoc/>
    public User UpdateUser(int id, UserToUpdate entity)
    {
        //* Validate user's information
        try {
            this.userUtils.ValidateEmail(entity.Email);
            this.userUtils.ValidateUserName(entity.UserName);
        } catch (Exception exception) {
            throw new BadRequestException(exception.Message);
        }

        User user = this.GetById(id);

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

        return user;
    }

    /// <inheritdoc/>
    public User UpdateLoggedUser(UserToUpdate entity, ClaimsPrincipal principal)
    {
        int id = this.userUtils.GetLoggedUserId(principal);

        return this.UpdateUser(id, entity);
    }

    /// <inheritdoc/>
    public User UpdatePassword(int id, PasswordConfirm entity)
    {
        if (!entity.Password.Equals(entity.PasswordConfirmation))
            throw new BadRequestException("Passwords do not match.");

        //* Validate the password
        try {
            this.passwordUtils.Validate(entity.Password);
        } catch (Exception exception) {
            throw new BadRequestException(exception.Message);
        }

        User user = this.GetById(id);

        if (passwordUtils.Check(entity.Password, user.Password))
            throw new BadRequestException("The new password must be different from the old one.");

        //* Set the user's new password
        user.Password = this.passwordUtils.Encrypt(entity.Password);

        //* Update the user
        this.dbContext.Users.Update(user);
        this.dbContext.SaveChanges();

        return user;
    }

    /// <inheritdoc/>
    public User UpdateLoggedUserPassword(PasswordToUpdate entity, ClaimsPrincipal principal)
    {
        User user = this.userUtils.GetLoggedUser(principal);

        if (!passwordUtils.Check(entity.OldPassword, user.Password))
            throw new UnauthorizedException("Wrong password.");

        return this.UpdatePassword(user.Id, entity);
    }
    #endregion
}