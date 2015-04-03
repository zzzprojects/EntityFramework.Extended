using System;
using System.Collections.Concurrent;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EntityFramework.Future
{
    /// <summary>
    /// A class to store the collection of <see cref="FutureContext"/> class that are linked to an <see cref="ObjectContext"/>.
    /// </summary>
    public class FutureStore
    {
        private readonly ConcurrentDictionary<int, FutureContext> _futureContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="FutureStore"/> class.
        /// </summary>
        protected FutureStore()
        {
            _futureContext = new ConcurrentDictionary<int, FutureContext>();
            Threshold = 70;
        }

        /// <summary>
        /// Gets or create the future context that is linked to the underlying <see cref="ObjectContext"/>.
        /// </summary>
        /// <param name="objectQuery">The query source to get the future context from.</param>
        /// <returns>An instance of <see cref="IFutureContext"/> to store waiting future queries.</returns>
        public IFutureContext GetOrCreate(ObjectQuery objectQuery)
        {
            if (objectQuery == null)
                throw new ArgumentNullException("objectQuery");

            var objectContext = objectQuery.Context;
            if (objectContext == null)
                throw new ArgumentException("The source ObjectContext can not be null.", "objectQuery");

            MaybeCleanup();

            int key = RuntimeHelpers.GetHashCode(objectContext);
            return _futureContext.GetOrAdd(key, k => new FutureContext(objectContext));
        }

        /// <summary>
        /// A percentage value between 0 and 100 inclusive.  When the percentage of Values collected is greater than
        /// or equal to this percentage then a collection will occur and the underlying structure
        /// will be shrunk to only valid values.
        /// </summary>
        public int Threshold { get; set; }

        /// <summary>
        /// Cleanup values that have been disposed.
        /// </summary>
        public void Cleanup()
        {
            var removeKeys = _futureContext
              .Where(p => p.Value.IsAlive == false)
              .Select(p => p.Key);

            foreach (int key in removeKeys)
            {
                FutureContext value;
                _futureContext.TryRemove(key, out value);
            }
        }

        private void MaybeCleanup()
        {
            if (!ShouldCleanup())
                return;

            Cleanup();
        }

        private bool ShouldCleanup()
        {
            int countCollected = _futureContext.Values.Count(x => !x.IsAlive);
            double diff = (double)countCollected / _futureContext.Count;
            int percent = (int)(100 * diff);

            return percent >= MakeFit(Threshold, 0, 100);
        }

        private static int MakeFit(int value, int low, int high)
        {
            return Math.Min(high, Math.Max(low, value));
        }

        #region Default
        private static readonly Lazy<FutureStore> _current = new Lazy<FutureStore>(() => new FutureStore());

        /// <summary>
        /// Gets the current default instance of <see cref="FutureStore"/>.
        /// </summary>
        public static FutureStore Default
        {
            get { return _current.Value; }
        }
        #endregion
    }
}