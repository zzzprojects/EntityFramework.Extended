using System;
using System.Collections.Concurrent;
using EntityFramework.Caching;

namespace EntityFramework
{
    /// <summary>
    /// The default <see cref="IContainer"/> for resolving dependencies.
    /// </summary>
    public class Container : IContainer
    {
        private readonly ConcurrentDictionary<Type, object> _factories;

        /// <summary>
        /// Initializes a new instance of the <see cref="Container"/> class.
        /// </summary>
        public Container()
        {
            _factories = new ConcurrentDictionary<Type, object>();
        }

        /// <summary>
        /// Resolves an instance for the specified <typeparamref name="TService"/> type.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <returns>
        /// A resolved instance of <typeparamref name="TService"/>.
        /// </returns>
        public virtual TService Resolve<TService>()
        {
            object factory;

            if (_factories.TryGetValue(typeof(TService), out factory))
                return ((Func<TService>)factory)();

            Type serviceType = typeof(TService);
            if (serviceType.IsInterface || serviceType.IsAbstract)
                return default(TService);

            try
            {
                return (TService)Activator.CreateInstance(serviceType);
            }
            catch
            {
                return default(TService);
            }
        }

        /// <summary>
        /// Register the specified <paramref name="factory"/> for resolving <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="factory">The factory <see langword="delegate"/> for resolving.</param>
        public virtual void Register<TService>(Func<TService> factory)
        {
            Type key = typeof(TService);
            _factories[key] = factory;
        }

        /// <summary>
        /// Register the specified <paramref name="factory"/> for resolving <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="factory">The factory <see langword="delegate"/> for resolving.</param>
        public void Register<TService>(Func<IContainer, TService> factory)
        {
            Func<TService> partialFactory = () => factory(this);
            Register(partialFactory);
        }

        /// <summary>
        /// Register the specified <paramref name="factory"/> for resolving <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TArg">The type of the constructor argument.</typeparam>
        /// <param name="factory">The factory <see langword="delegate"/> for resolving.</param>
        public void Register<TService, TArg>(Func<TArg, TService> factory)
        {
            Func<TService> partialFactory = () => factory(Resolve<TArg>());
            Register(partialFactory);
        }

        /// <summary>
        /// Register the specified <paramref name="factory"/> for resolving <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TArg1">The type of the first constructor argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second constructor argument.</typeparam>
        /// <param name="factory">The factory <see langword="delegate"/> for resolving.</param>
        public void Register<TService, TArg1, TArg2>(Func<TArg1, TArg2, TService> factory)
        {
            Func<TService> partialFactory = () => factory(Resolve<TArg1>(), Resolve<TArg2>());
            Register(partialFactory);
        }

        /// <summary>
        /// Register the specified <paramref name="factory"/> for resolving <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TArg1">The type of the first constructor argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second constructor argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third constructor argument.</typeparam>
        /// <param name="factory">The factory <see langword="delegate"/> for resolving.</param>
        public void Register<TService, TArg1, TArg2, TArg3>(Func<TArg1, TArg2, TArg3, TService> factory)
        {
            Func<TService> partialFactory = () => factory(Resolve<TArg1>(), Resolve<TArg2>(), Resolve<TArg3>());
            Register(partialFactory);
        }

        /// <summary>
        /// Register the specified <paramref name="factory"/> for resolving <typeparamref name="TService"/>.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TArg1">The type of the first constructor argument.</typeparam>
        /// <typeparam name="TArg2">The type of the second constructor argument.</typeparam>
        /// <typeparam name="TArg3">The type of the third constructor argument.</typeparam>
        /// <typeparam name="TArg4">The type of the fourth constructor argument.</typeparam>
        /// <param name="factory">The factory <see langword="delegate"/> for resolving.</param>
        public void Register<TService, TArg1, TArg2, TArg3, TArg4>(Func<TArg1, TArg2, TArg3, TArg4, TService> factory)
        {
            Func<TService> partialFactory = () => factory(Resolve<TArg1>(), Resolve<TArg2>(), Resolve<TArg3>(), Resolve<TArg4>());
            Register(partialFactory);
        }
    }
}