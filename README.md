# Web API Full Implementation

This is a template meant to setup a fully implemented Web API application using `.NET 7`.

## Table of contents

- [What to expect](#what-to-expect)
- [Technologies](#technologies)
- [Usage](#usage)
- [Swap DBMS](#swap-dbms)
- [User roles](#user-roles)
- [User credentials validators](#user-credentials-validators)
- [Uninstall](#uninstall)
- [License](#license)

## What to expect

- Fully customizable and automated logs that change files on a daily interval for readability.

  > Edit values in *appsettings.json* file to personalize.

  - Routes called
  - Users' actions
  - Exceptions (with unique exception IDs for easier debugging)
  - Server setup steps
- Versioned services
- User credentials validations
  - Customizable password validation

    > Edit values in *appsettings.json* file to personalize.
  - Email validation
  - Username validation
- JWT authentication
  - User creation
  - Login and sessions
  - Password encryption
  - Both public and authenticated services
- Role-based authorization
  - Admin and member accounts
  - Admin-only services
  - Default admin user account

    > Edit values in *appsettings.json* file to personalize.
- Database-ready
  - *SQLite* already setup
  - Easily swap DBMS

[⬆ Back to the Table of contents](#table-of-contents)

## Technologies

- [.NET 7](https://dotnet.microsoft.com)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core)
- [BCrypt](https://github.com/BcryptNet/bcrypt.net)
- [Entity Framework](https://learn.microsoft.com/en-us/ef/)
- [Jwt](https://jwt.io)
- [Serilog](https://serilog.net)
- [SQLite](https://sqlite.org)
- [Swagger](https://swagger.io)

[⬆ Back to the Table of contents](#table-of-contents)

## Usage

1. [Clone the repository](https://docs.github.com/en/repositories/creating-and-managing-repositories/cloning-a-repository)
2. Open terminal into template's folder

   ```zsh
   cd webapi-full
   ```

3. Install the template to use for project creation
   - For Windows

     ```bash
     dotnet new install .\
     ```

   - For MacOS / Linux

     ```zsh
     dotnet new install ./
     ```

   > You might need to uninstall one version to install an updated one.
   >
   > To avoid this inconvenience, simply add the `--force` option in the above command.

4. Create the project

   ```zsh
   dotnet new webapi-full
   ```

   > Remember to run this command into an empty directory as it'll be used as the project's folder.
   >
   > Name the directory as you like, the project's namespace will inherit that name.
5. Add the migrations and create the database

   ```zsh
   dotnet ef migrations add CreateUser
   ```

   and

   ```zsh
   dotnet ef database update
   ```

   > If you don't have Entity Framework installed, run the following first:
   >
   > ```zsh
   > dotnet tool install --global dotnet-ef
   > ```

6. Run the project and try out using [Swagger](https://swagger.io/docs/)

   ```zsh
   dotnet watch run
   ```

[⬆ Back to the Table of contents](#table-of-contents)

## Swap DBMS

There are 2 **DataBase Management Systems** that I will include.

- [SQL Server](#sql-server)
- [PostgreSQL](#postgresql)

> Of course, there are more but these are the ones I've worked with the most.
>
> For more *Data Providers*, visit [the official EF Core documentation](https://learn.microsoft.com/en-us/ef/core/providers).

### SQL Server

1. Install the Entity Framework connector package.

   ```zsh
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   ```

2. Change the connection string in *appsettings.json* to something like this:

   ```json
   {
     "ConnectionStrings": {
       "Demo": "Server=<SERVER_NAME>;Database=<DATABASE_NAME>;Trusted_Connection=true;MultipleActiveResultSets=true;Trust Server Certificate=true"
     },
     ...
   }
   ```

3. Modify your `ApplicationDbContext` service to use **SQL Server**.

   > This is done inside *Program.cs*.

   ```c#
   builder.Services.AddDbContext<ApplicationDbContext>(options => 
     options.UseSqlServer(builder.Configuration.GetConnectionString("Demo"))
   );
   ```

### PostgreSQL

1. Install the Entity Framework connector package.

   ```zsh
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   ```

2. Change the connection string in *appsettings.json* to something like this:

   ```json
   {
     "ConnectionStrings": {
       "Demo": "Host=localhost:5432;Database=<DATABASE>;Username=<USERNAME>"
     },
     ...
   }
   ```

3. Modify your `ApplicationDbContext` service to use **PostgreSQL**.

   > This is done inside *Program.cs*.

   ```c#
   builder.Services.AddDbContext<ApplicationDbContext>(options => 
     options.UseNpgsql(builder.Configuration.GetConnectionString("Demo"))
   );
   ```
   
4. **PostgreSQL** requires some extra steps for this solution to run.

   1. Change the `Is_Deleted` property of class `IndexedObject` as this DBMS does not support the `bit` type.
   
      ```csharp
      [Required]
      [Column("Is_Deleted")]
      [JsonIgnore]
      public bool IsDeleted { get; set; } = false;
      ```
      
   2. Add the following inside *Program.cs* right after step 3's code.
   
      ```csharp
      AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
      AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
      ```
      
      > This is essential for **Postgre** to support our `DateTime` properties.

[⬆ Back to the Table of contents](#table-of-contents)

## User roles

### Declaring roles

User roles are defined in 2 places for the app to work:

1. The `Role` *enum*

   This *enum* holds the available roles of the app as well as each role's index.

   This index is important to the app's functionality and works in a simple manner: The higher the index, the more privileged the role.

   The default enum holds the following values:

   ```json
   {
     "User": 1,
     "Admin": 2
   }
   ```

   > Notice that the admin hes a higher index cause it's the superior role.

2. Inside *Program.cs* to translate the enum values into "*policies*" that are used by the app.

   ```c#
   builder.Services.AddAuthorization(options =>
   {
       options.AddPolicy("admin", policy => policy.Requirements.Add(new RoleRequirement(Role.Admin)));
       options.AddPolicy("user", policy => policy.Requirements.Add(new RoleRequirement(Role.User)));
   });
   ```

   > After adding a role to the Enum, it is mandatory that it's also added it here.

### User-authenticated services

To authenticate any service for a user, simple add the `[Authorize]` attribute.

It is important that the role is declared as well for the authorization to work.

```c#
[Authorize(Policy = "user")]
[HttpGet]
public IActionResult GetLoggedUser()
{
    User user = this.userUtils.GetLoggedUser(this.User);

    Log.Information($"Retrieved user '{user.UserName}'.");

    return Ok(user);
}
```

> This is a service that can be used by any user.
>
> Remember that it's still not a public service and you have to be logged in to use it. It's just that no special role is needed.

### Default administrator account

Inside *appsettings.json*, there is the default administrator's credentials and important information.

The application uses this information to automatically create the user on database's creation.

The data is declared in the form of the following JSON:

```json
"DefaultAdmin": {
  "Email": "admin@user.com",
  "UserName": "admin_user",
  "FirstName": "Admin",
  "LastName": "User",
  "Password": "123"
}
```

> **It is recommended** that you change at least the email and password before proceeding.

[⬆ Back to the Table of contents](#table-of-contents)

## User credentials validators

### Username

Username validation is simple but opinionated.

Usernames:

- Must not include any *whitespace* characters.
- Must have a maximum length of 40 characters.
- Must have a minimum length of 6 characters.
- Only allow specific non-alphanumeric characters:
  - `_`
  - `-`
- Cannot have any uppercase letters.

To return the whole list of rules and mark the invalid ones, the formatted message is returned as an ***HTML*** `<ul></ul>` tag.

More specifically, here is an example of the validation having failed:

```html
<ul class='username-validation'>
  <li class='valid'>Username cannot contain whitespaces.</li>
  <li class='invalid'>Username cannot exceed 40 characters.</li>
  <li class='valid'>Username must be at least 6 characters long.</li>
  <li class='valid'>The only allowed special characters are the following: -, _</li>
  <li class='valid'>Username must be lowercase.</li>
</ul>
```

> To edit this validator, you'll have to edit the code as the rules were hardcoded. This happened due to the username validation following a standard.
>
> In the above example, all validators passed except for the one enforcing a maximum length of 40 characters.

### Email

The email validation is pretty straightforward.

All the addresses provided by the user are valid as long as they stick to the format.

I see no point in changing this validator but you are free to do so in your code.

> Both `ValidateEmail` and `ValidateUserName` methods are defined as part of the `IUserUtils` interface.

### Password

Password validation is the most complex and easily customizable and so its rules are defined in the *appsettings.json* file.

On the app's generation, you will find this part in the previously mentioned file:

```json
"PasswordValidator": {
  "AllowedNonAlphanumeric": "!@#$._-",
  "MaxLength": 16,
  "MinLength": 8,
  "RequireDigit": false,
  "RequireLowercase": true,
  "RequireNonAlphanumeric": true,
  "RequireUppercase": true
},
```

What this does is:

- #### Allowed Non-Alphanumeric

  This rule sets the allowed non-alphanumeric characters that the password string in allowed to have.

  The characters must not be separated by anything. Just put them all in the json string.

  > To not allow any non-alphanumeric, make the value an empty string.

- #### Max Length

  The max length does what its name implies.

  If you set it to a number, the password string cannot exceed that many characters in length.

  > To stop enforcing a maximum length for passwords, set the value to 0.

- #### Min Length

  Works like its [Max Length](#max-length) counterpart.

  Passwords cannot have less characters than the number set for this field.

  > To stop enforcing a minimum length for passwords, set the value to 0.

- #### Require Digit

  This is a `Boolean` variable.

  If set to true, the user will have to use at least 1 digit (`[0-9]`) for their password.

  > To not enforce this rule, set its value to `false`.

- #### Require Lowercase

  This is a `Boolean` variable.

  If set to true, the user will have to use at least 1 lowercase letter for their password.

  > To not enforce this rule, set its value to `false`.

- #### Require Non-Alphanumeric

  This is a `Boolean` variable.

  If set to true, the user will have to use at least 1 of the [Allowed Non-Alphanumeric](#allowed-non-alphanumeric) characters for their password.

  > To not enforce this rule, set its value to `false`.

- #### Require Uppercase

  This is a `Boolean` variable.

  If set to true, the user will have to use at least 1 uppercase letter for their password.

  > To not enforce this rule, set its value to `false`.

- Also, by default, passwords cannot contain any whitespace characters.

To return the whole list of rules and mark the invalid ones, the formatted message is returned as an ***HTML*** `<ul></ul>` tag.

More specifically, here is an example of the validation having failed:

```html
<ul class='password-validation'>
  <li class='valid'>Password cannot contain whitespaces.</li>
  <li class='invalid'>The only allowed special characters are the following: !, @, #, $, ., _, -</li>
  <li class='valid'>Password cannot exceed 16 characters.</li>
  <li class='invalid'>Password must be at least 8 characters long.</li>
  <li class='valid'>Password must contain at least one digit.</li>
  <li class='valid'>Password must contain at least one lowercase letter.</li>
</ul>
```

> In the above example, the password was almost valid but it:
>
> - contained an not-allowed non-alphanumeric character and
> - was too short.

To edit the password validation, you (may) have to do 3 things:

- Edit the rules inside the *appsettings.json* file.
- Edit the `PasswordValidator` class to match those rules.

  > If you just change the values of the existing rules, there's no need for it.
- Edit the logic of the validation declared as a part of the `IPasswordUtils` interface inside the provided implementation class or by creating your own.

  > If you just change the values of the existing rules, there's no need for it.

[⬆ Back to the Table of contents](#table-of-contents)

## Uninstall

To uninstall the template, simply do the following:

1. Open terminal into template's folder

   ```zsh
   cd webapi-full
   ```

2. Uninstall the template by running:
   - For Windows

     ```bash
     dotnet new uninstall .\
     ```

   - For MacOS / Linux

     ```zsh
     dotnet new uninstall ./
     ```

[⬆ Back to the Table of contents](#table-of-contents)

## License

**dotnet-template-webapi-full** is licensed under [GNU General Public License v3.0](https://github.com/Stratis-Dermanoutsos/dotnet-template-webapi-full/blob/main/LICENSE).

[⬆ Back to the Table of contents](#table-of-contents)
