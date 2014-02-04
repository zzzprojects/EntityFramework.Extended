
namespace EntityFramework.Caching
{
    /// <summary>
    /// Provider interface for creating cache keys
    /// </summary>
    public interface ICacheKeyProvider
    {
        /// <summary>
        /// Creates the cache key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        string CreateKey(string value);
    }
}
