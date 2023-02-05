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

## Technologies

- [.NET 7](https://dotnet.microsoft.com)
- [ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core)
- [BCrypt](https://github.com/BcryptNet/bcrypt.net)
- [Entity Framework](https://learn.microsoft.com/en-us/ef/)
- [Jwt](https://jwt.io)
- [Serilog](https://serilog.net)
- [SQLite](https://sqlite.org)
- [Swagger](https://swagger.io)

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

To return the whole list of rules and mark the invalid ones, the formatted message returned in an ***HTML*** `<ul></ul>` tag.

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

> In the above example, all validators passed except for the one enforcing a maximum length of 40 characters.

### Email

The email validation is pretty straightforward.

All the addresses provided by the user are valid as long as they stick to the format.

> Both `ValidateEmail` and `ValidateUserName` methods are defined as part of the `IUserUtils` interface.

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

## License

**dotnet-template-webapi-full** is licensed under [GNU General Public License v3.0](https://github.com/Stratis-Dermanoutsos/dotnet-template-webapi-full/blob/main/LICENSE).
