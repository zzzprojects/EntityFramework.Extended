using System.Linq;

namespace EntityFramework.Future
{
    /// <summary>
    /// Allows mocking and testing of the Future() ExtensionMethods.
    /// </summary>
    public interface IFutureTestQueryable : IQueryable { }

    /// <summary>
    /// Allows mocking and testing of the Future() ExtensionMethods.
    /// </summary>
    /// <typeparam name="T">Return type</typeparam>
    public interface IFutureTestQueryable<T> : IQueryable<T>, IFutureTestQueryable { }
}
