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
        where T : IndexedObject => entities.Where(entity => !entity.IsDeleted);

    /// <summary>
    /// <paramref name="entities" />
    /// <param name="entities">: The entities to query.</param>
    /// <br />
    /// <paramref name="id" />
    /// <param name="id">: The id to search for.</param>
    /// <br />
    /// <returns>
    /// Returns an <paramref name="Entity" /> with the given <paramref name="id" />.
    /// If the <paramref name="Entity" /> is not found, null is returned instead.
    /// </returns>
    /// </summary>
    public static T? Get<T>(this IQueryable<T> entities, int id)
        where T : IndexedObject => entities.SingleOrDefault(entity => entity.Id == id);

    /// <summary>
    /// <paramref name="entities" />
    /// <param name="entities">: The entities to query.</param>
    /// <br />
    /// <paramref name="id" />
    /// <param name="id">: The id to search for.</param>
    /// <br />
    /// <returns>Returns an <paramref name="Entity" /> with the given <paramref name="id" />.</returns>
    /// </summary>
    public static T GetAssured<T>(this IQueryable<T> entities, int id)
        where T : IndexedObject => entities.Single(entity => entity.Id == id);
}