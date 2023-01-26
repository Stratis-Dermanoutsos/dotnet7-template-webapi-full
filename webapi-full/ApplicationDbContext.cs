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
    /// <item>set the default values for the <paramref name="Date_In" /> and <paramref name="Date_Edit" /> on creation</item>
    /// <item>update <paramref name="Date_Edit" /> on update</item>
    /// </list>
    /// automatically.
    /// </summary>
    public override int SaveChanges()
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity is IndexedObject && (e.State == EntityState.Modified || e.State == EntityState.Added));

        foreach (var entityEntry in entries)
        {
            ((IndexedObject)entityEntry.Entity).DateEdit = DateTime.Now;

            if (entityEntry.State == EntityState.Added)
                ((IndexedObject)entityEntry.Entity).DateIn = DateTime.Now;
        }

        return base.SaveChanges();
    }
}