using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using webapi_full.Models;

namespace webapi_full;

public class ApplicationDbContext : DbContext
{
    /// <inheritdoc />
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    { }

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
}