using webapi_full.Models;

namespace webapi_full.Extensions;

public static class DbContextExtension
{
    /// <summary>
    /// <paramref name="entities" />
    /// <param name="entities">: The entities to get.</param>
    /// <br />
    /// <returns>Returns an <paramref name="IQueryable" /> containing all objects of type
    /// <paramref name="T" /> not marked as deleted.
    /// </returns>
    /// </summary>
    public static IQueryable<T> GetAll<T>(this IQueryable<T> entities)
        where T : IndexedObject => entities.Where(i => !i.IsDeleted);

    /// <summary>
    /// <paramref name="entities" />
    /// <param name="entities">: The entities to query.</param>
    /// <br />
    /// <paramref name="id" />
    /// <param name="id">: The id to search for.</param>
    /// <br />
    /// <returns>Returns an <paramref name="Entity" /> with the given <paramref name="id" />.</returns>
    /// </summary>
    public static T? Get<T>(this IQueryable<T> entities, int id)
        where T : IndexedObject => entities.FirstOrDefault(i => i.Id == id);
}