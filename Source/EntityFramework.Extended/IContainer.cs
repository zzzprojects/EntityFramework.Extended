using System;

namespace EntityFramework
{
    /// <summary>
    /// An <see langword="interface"/> for a simple implementation of inversion of control.
    /// </summary>
    public interface IContainer
    {
        /// <summary>
        /// Register the specified <paramref name="factory"/> for resolving <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="factory">The factory <see langword="delegate"/> for resolving.</param>
        void Register<TService>(Func<TService> factory);
        
        /// <summary>
        /// Resolves an instance for the specified <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>A resolved instance of <typeparamref name="TService"/>.</returns>
        TService Resolve<TService>();
    }
}