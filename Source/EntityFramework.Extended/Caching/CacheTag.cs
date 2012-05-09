using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EntityFramework.Caching
{
    public class CacheTag : IEquatable<CacheTag>
    {
        private readonly string _tag;

        public CacheTag(string tag)
        {
            _tag = tag;
        }

        public bool Equals(CacheTag other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return string.Equals(other._tag, _tag, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj.GetType() != typeof(CacheTag))
                return false;

            return Equals((CacheTag)obj);
        }

        public static bool operator ==(CacheTag left, CacheTag right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(CacheTag left, CacheTag right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            return (_tag != null ? _tag.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return _tag;
        }
    }
}
