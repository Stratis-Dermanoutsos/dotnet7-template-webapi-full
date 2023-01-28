# Web API Full Implementation

This is a template meant to setup a fully implemented Web API application using `.NET 7`.

## Table of contents

- [What to expect](#what-to-expect)
- [Technologies](#technologies)
- [Usage](#usage)
- [Uninstall](#uninstall)
- [License](#license)

## What to expect

- Fully customizable and automated logs that change files on a daily interval for readability.

  This includes:
  - Route calls
  - Exceptions (with unique exception IDs for easier debugging)
  - Server setup steps
- Versioned services
- User credentials validations
  - Customizable password validation (edit values in *appsettings.json* file to personalize)
- JWT authentication

  This includes:
  - User creation
  - Login and sessions
  - Password encryption
  - Both public and authenticated services
- Database-ready
  - *SQLite* already setup
  - Easily change to another DBMS.

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
