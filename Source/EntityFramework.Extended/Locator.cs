using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EntityFramework.Batch;
using EntityFramework.Caching;
using EntityFramework.Future;
using EntityFramework.Mapping;

namespace EntityFramework
{
    /// <summary>
    /// Service locator pattern.
    /// </summary>
    /// <example>
    /// Replace cache provider with Memcached provider
    /// <code><![CDATA[
    /// Locator.Current.Register<ICacheProvider>(() => new MemcachedProvider());
    /// ]]>
    /// </code>
    /// Replace the built in <see cref="IContainer"/> with other Locator framework.
    /// <code><![CDATA[
    /// var container = new OtherContainer();
    /// // register all default providers
    /// Locator.RegisterDefaults(container);
    /// // overide cache provider
    /// container.Register<ICacheProvider>(() => new MemcachedProvider());
    /// // make Locator use new custom container
    /// Locator.SetContainer(container);
    /// ]]>
    /// </code>
    /// </example>
    public class Locator
    {
        private static readonly Locator _instance = new Locator();
        private IContainer _container;

        /// <summary>
        /// Initializes a new instance of the <see cref="Locator"/> class.
        /// </summary>
        public Locator()
        {
            _container = new Container();
            RegisterDefaults(_container);
        }

        /// <summary>
        /// Gets the current Locator <see cref="IContainer"/>.
        /// </summary>
        public static IContainer Current
        {
            get { return _instance.Container; }
        }

        /// <summary>
        /// Sets the <see cref="Current"/> <see cref="IContainer"/>.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to set.</param>
        public static void SetContainer(IContainer container)
        {
            _instance.SetInnerContainer(container);
        }

        /// <summary>
        /// Gets the <see cref="IContainer"/> for this instance.
        /// </summary>
        protected IContainer Container
        {
            get { return _container; }
        }

        /// <summary>
        /// Sets the <see cref="IContainer"/> for this instance.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> for this instance.</param>
        protected void SetInnerContainer(IContainer container)
        {
            if (container == null)
                throw new ArgumentNullException("container");

            _container = container;
        }

        /// <summary>
        /// Registers the default service provider resolvers.
        /// </summary>
        /// <param name="container">The <see cref="IContainer"/> to register the default service resolvers with.</param>
        public static void RegisterDefaults(IContainer container)
        {
            container.Register<IMappingProvider>(() => new MetadataMappingProvider());
            container.Register<IBatchRunner>(() => new SqlServerBatchRunner());
            container.Register<IFutureRunner>(() => new FutureRunner());

            container.Register<ICacheProvider>(() => new MemoryCacheProvider());
            container.Register(() => CacheManager.Current);
        }
    }
}
