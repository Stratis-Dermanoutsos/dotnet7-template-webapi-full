using Microsoft.EntityFrameworkCore;
using webapi_full.Enums;
using webapi_full.IUtils;
using webapi_full.Models;

namespace webapi_full;

public class ApplicationDbContext : DbContext
{
    private readonly IConfiguration configuration;
    private readonly IPasswordUtils passwordUtils;

    /// <inheritdoc />
    public ApplicationDbContext(IConfiguration configuration, IPasswordUtils passwordUtils, DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        this.configuration = configuration;
        this.passwordUtils = passwordUtils;

        this.CreateDefaultAdmin();
    }

    //? Entries used to add data to the database
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// <inheritdoc />
    /// <br />
    /// Overriden to
    /// <br />
    /// <list type="bullet">
    /// <item>set <paramref name="DateIn" /> on creation</item>
    /// <item>update <paramref name="DateEdit" /> on any change</item>
    /// <item>set <paramref name="IsDeleted" /> flag to 1 on deletion</item>
    /// </list>
    /// automatically.
    /// </summary>
    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IndexedObject && (
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted));

        foreach (var entityEntry in entries)
        {
            //* Change the DateEdit on any change
            ((IndexedObject)entityEntry.Entity).DateEdit = DateTime.Now;

            //* Set the DateIn on creation of the entity
            if (entityEntry.State == EntityState.Added)
                ((IndexedObject)entityEntry.Entity).DateIn = DateTime.Now;

            //* Set the IsDeleted flag and keep the entity on deletion
            if (entityEntry.State == EntityState.Deleted) {
                /**
                 * If the entity is a user,
                 * - remove the password and
                 * - change the secondary keys
                 */
                if (entityEntry.Entity is User user) {
                    user.Password = string.Empty;
                    user.Email = $"(deleted-{user.Id}){user.Email}";
                    user.UserName = $"(deleted-{user.Id}){user.UserName}";
                }

                entityEntry.State = EntityState.Modified;
                ((IndexedObject)entityEntry.Entity).IsDeleted = true;
            }
        }

        return base.SaveChanges();
    }

    /// <summary>
    /// Creates the default admin user account if it doesn't already exist.
    /// </summary>
    private void CreateDefaultAdmin()
    {
        if (this.Users.Any(u => u.Role == Role.Admin))
            return;

        var userInfo = this.configuration.GetSection("DefaultAdmin");

        if (!userInfo.Exists())
            return;

        this.Users.Add(new()
        {
            Email = userInfo.GetValue<string>("Email") ?? string.Empty,
            UserName = userInfo.GetValue<string>("UserName") ?? string.Empty,
            FirstName = userInfo.GetValue<string>("FirstName") ?? string.Empty,
            LastName = userInfo.GetValue<string>("LastName") ?? string.Empty,
            Password = this.passwordUtils.Encrypt(userInfo.GetValue<string>("Password") ?? string.Empty),
            Role = Role.Admin
        });
        this.SaveChanges();

        Log.Information("Default admin created.");
    }
}