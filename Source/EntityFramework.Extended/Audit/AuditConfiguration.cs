using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using EntityFramework.Reflection;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A class to configure the output of an <see cref="AuditLog"/>.
    /// </summary>
    public class AuditConfiguration
    {
        private const BindingFlags _defaultBinding = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;

        private readonly ConcurrentDictionary<Type, bool> _auditableCache = new ConcurrentDictionary<Type, bool>();
        private readonly ConcurrentDictionary<string, bool> _notAuditedCache = new ConcurrentDictionary<string, bool>();
        private readonly ConcurrentDictionary<string, bool> _alwaysAuditCache = new ConcurrentDictionary<string, bool>();
        private readonly ConcurrentDictionary<string, IMethodAccessor> _formatterCache = new ConcurrentDictionary<string, IMethodAccessor>();
        private readonly ConcurrentDictionary<Type, IMemberAccessor> _displayCache = new ConcurrentDictionary<Type, IMemberAccessor>();

        public AuditConfiguration()
        {
            IncludeInserts = true;
            IncludeDeletes = true;
        }

        internal bool IsAuditable(object entity)
        {
            if (entity == null)
                return false;

            Type entityType = entity.GetType();
            entityType = ObjectContext.GetObjectType(entityType);

            return IsAuditable(entityType);
        }

        internal bool IsAuditable(Type entityType)
        {
            return DefaultAuditable || _auditableCache.GetOrAdd(entityType,
              key => HasAttribute(key, typeof(AuditAttribute)));
        }

        internal bool IsNotAudited(Type entityType, string name)
        {
            string fullName = entityType.FullName + "." + name;

            return _notAuditedCache.GetOrAdd(fullName,
              key => HasAttribute(entityType, name, typeof(NotAuditedAttribute)));
        }

        internal bool IsAlwaysAudited(Type entityType, string name)
        {
            string fullName = entityType.FullName + "." + name;

            return _alwaysAuditCache.GetOrAdd(fullName,
              key => HasAttribute(entityType, name, typeof(AlwaysAuditAttribute)));
        }

        internal IMethodAccessor GetFormatter(Type entityType, string name)
        {
            string fullName = entityType.FullName + "." + name;

            return _formatterCache.GetOrAdd(fullName, key =>
            {
                var formatAttribute = GetAttribute<AuditPropertyFormatAttribute>(entityType, name);
                if (formatAttribute == null)
                    return null;

                var typeAccessor = TypeAccessor.GetAccessor(formatAttribute.FormatType);
                return typeAccessor.FindMethod(formatAttribute.MethodName, typeof(AuditPropertyContext));
            });
        }

        internal IMemberAccessor GetDisplayMember(Type entityType)
        {
            return _displayCache.GetOrAdd(entityType, key =>
            {
                TypeAccessor typeAccessor = TypeAccessor.GetAccessor(entityType);
                IMemberAccessor displayMember = null;

                var displayAttribute = GetAttribute<DisplayColumnAttribute>(entityType);

                // first try DisplayColumnAttribute property
                if (displayAttribute != null)
                    displayMember = typeAccessor.FindProperty(displayAttribute.DisplayColumn);

                if (displayMember != null)
                    return displayMember;

                var properties = typeAccessor.GetProperties().ToList();

                // try first string property        
                displayMember = properties.FirstOrDefault(m => m.MemberType == typeof(string));
                if (displayMember != null)
                    return displayMember;

                // try second property
                return properties.Count > 1 ? properties[1] : null;
            });
        }

        private static bool HasAttribute(Type entityType, string fullName, Type attributeType)
        {
            var info = FindMember(entityType, fullName);
            return HasAttribute(info, attributeType);
        }

        private static bool HasAttribute(MemberInfo memberInfo, Type attributeType)
        {
            if (memberInfo.IsDefined(attributeType, true))
                return true;

            // try the metadata object
            MemberInfo declaringType = memberInfo;
            if (memberInfo.MemberType != MemberTypes.TypeInfo)
                declaringType = memberInfo.DeclaringType;

            var metadataTypeAttribute = declaringType
              .GetCustomAttributes(typeof(MetadataTypeAttribute), true)
              .FirstOrDefault() as MetadataTypeAttribute;

            if (metadataTypeAttribute == null)
                return false;

            Type metadataType = metadataTypeAttribute.MetadataClassType;
            if (memberInfo.MemberType == MemberTypes.TypeInfo)
                return metadataType.IsDefined(attributeType, true);

            MemberInfo metaInfo = metadataType
              .GetMember(memberInfo.Name, _defaultBinding)
              .FirstOrDefault();

            return metaInfo != null && metaInfo.IsDefined(attributeType, true);
        }

        private static TAttribute GetAttribute<TAttribute>(Type entityType, string fullName)
          where TAttribute : Attribute
        {
            var info = FindMember(entityType, fullName);
            return GetAttribute<TAttribute>(info);
        }

        private static TAttribute GetAttribute<TAttribute>(MemberInfo memberInfo)
          where TAttribute : Attribute
        {
            var attributeType = typeof(TAttribute);

            var attribute = memberInfo
              .GetCustomAttributes(attributeType, true)
              .FirstOrDefault() as TAttribute;

            if (attribute != null)
                return attribute;

            // try the metadata object
            var declaringType = memberInfo;
            if (memberInfo.MemberType != MemberTypes.TypeInfo)
                declaringType = memberInfo.DeclaringType;

            var metadataTypeAttribute = declaringType
              .GetCustomAttributes(typeof(MetadataTypeAttribute), true)
              .FirstOrDefault() as MetadataTypeAttribute;

            if (metadataTypeAttribute == null)
                return null;

            var metadataType = metadataTypeAttribute.MetadataClassType;
            if (memberInfo.MemberType == MemberTypes.TypeInfo)
                return metadataType
                  .GetCustomAttributes(attributeType, true)
                  .FirstOrDefault() as TAttribute;

            var metaInfo = metadataType
              .GetMember(memberInfo.Name, _defaultBinding)
              .FirstOrDefault();

            if (metaInfo == null)
                return null;

            return metaInfo
              .GetCustomAttributes(attributeType, true)
              .FirstOrDefault() as TAttribute;
        }

        private static MemberInfo FindMember(Type entityType, string fullName)
        {
            var name = fullName.Split('.').LastOrDefault();
            var member = LateBinder.Find(entityType, name);
            var info = member.MemberInfo;
            return info;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to include inserted entities. Default is <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to include inserted entities; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeInserts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include deleted entities. Default is <c>true</c>.
        /// </summary>
        /// <value>
        ///   <c>true</c> to include deleted entities; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeDeletes { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include relationship properties.
        /// </summary>
        /// <value>
        ///   <c>true</c> to include relationship properties; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeRelationships { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to load a relationship when it is not already
        /// loaded to the context. Please not that this will make database call.
        /// </summary>
        /// <value>
        ///   <c>true</c> to load relationships; otherwise, <c>false</c>.
        /// </value>
        public bool LoadRelationships { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an entity is auditable by default.
        /// </summary>
        /// <value>
        ///   <c>true</c> if default auditable; otherwise, <c>false</c>.
        /// </value>
        public bool DefaultAuditable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the log is maintain across multiple calls to <see cref="M:System.Data.Entity.DbContext.SaveChanges"/>.
        /// Each additional call will add to the <see cref="P:EntityFramework.Audit.AuditLogger.LastLog"/> instance.
        /// </summary>
        /// <value>
        ///   <c>true</c> to maintain log across saves; otherwise, <c>false</c> to create a new log on eash save.
        /// </value>
        public bool MaintainAcrossSaves { get; set; }

        #region Default
        private static readonly Lazy<AuditConfiguration> _default = new Lazy<AuditConfiguration>(() => new AuditConfiguration());

        /// <summary>
        /// Gets the default instance of AuditConfiguration.
        /// </summary>
        /// <value>The default instance.</value>
        public static AuditConfiguration Default
        {
            get { return _default.Value; }
        }
        #endregion

        #region Fluent
        /// <summary>
        /// Fluent method to set whether the entity class is auditable.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="value">if set to <c>true</c> make the entity auditable.</param>
        /// <returns>A fluent object to further configure the auditing for the entity type.</returns>
        public AuditEntityConfiguration<TEntity> IsAuditable<TEntity>(bool value = true)
        {
            Type type = typeof(TEntity);
            _auditableCache.AddOrUpdate(type, value, (k, b) => value);

            return new AuditEntityConfiguration<TEntity>(this);
        }

        /// <summary>
        /// A class to configure the auditing for the entity type.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        public class AuditEntityConfiguration<TEntity>
        {
            private readonly AuditConfiguration _auditConfiguration;

            internal AuditEntityConfiguration(AuditConfiguration auditConfiguration)
            {
                _auditConfiguration = auditConfiguration;
            }

            /// <summary>
            /// Indicates that a field in an audited class should not be included in the audit log.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="propertyExpression">The property expression.</param>
            /// <returns></returns>
            public AuditEntityConfiguration<TEntity> NotAudited<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                var property = ReflectionHelper.ExtractPropertyInfo(propertyExpression);
                var key = property.DeclaringType.FullName + "." + property.Name;
                _auditConfiguration._notAuditedCache.AddOrUpdate(key, true, (k, o) => true);
                return this;
            }

            /// <summary>
            /// Indicates that a field in an audited class should always be included in the audit data even if it hasn't been changed.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="propertyExpression">The property expression.</param>
            /// <returns></returns>
            public AuditEntityConfiguration<TEntity> AlwaysAudited<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                var property = ReflectionHelper.ExtractPropertyInfo(propertyExpression);
                var key = property.DeclaringType.FullName + "." + property.Name;
                _auditConfiguration._alwaysAuditCache.AddOrUpdate(key, true, (k, o) => true);
                return this;
            }

            /// <summary>
            /// Indicates a method to control the output format of the <see cref="AuditProperty"/> values.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="propertyExpression">The property expression.</param>
            /// <param name="formatMethod">The format method.</param>
            /// <returns></returns>
            public AuditEntityConfiguration<TEntity> FormatWith<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression, Expression<Func<AuditPropertyContext, object>> formatMethod)
            {
                var propertyInfo = ReflectionHelper.ExtractPropertyInfo(propertyExpression) as MemberInfo;
                var key = propertyInfo.DeclaringType.FullName + "." + propertyInfo.Name;

                _auditConfiguration._formatterCache.AddOrUpdate(key,
                  k => ExtractMethodInfo(formatMethod),
                  (k, o) => ExtractMethodInfo(formatMethod));

                return this;
            }

            /// <summary>
            /// Indicates that a field in an audited class should be used as the relationship display member.
            /// </summary>
            /// <typeparam name="TProperty">The type of the property.</typeparam>
            /// <param name="propertyExpression">The property expression.</param>
            /// <returns></returns>
            public AuditEntityConfiguration<TEntity> DisplayMember<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                Type type = typeof(TEntity);

                _auditConfiguration._displayCache.AddOrUpdate(type,
                  k => ExtractMember(propertyExpression),
                  (k, o) => ExtractMember(propertyExpression));

                return this;
            }

            private static IMethodAccessor ExtractMethodInfo(Expression<Func<AuditPropertyContext, object>> methodExpression)
            {
                var memberExpression = methodExpression.Body as MethodCallExpression;
                if (memberExpression == null)
                    return null;

                var methodInfo = memberExpression.Method;
                var methodAccess = new MethodAccessor(methodInfo);

                return methodAccess;
            }

            private static IMemberAccessor ExtractMember<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                var accessor = TypeAccessor.GetAccessor<TEntity>();
                var memberAccessor = accessor.FindProperty(propertyExpression);

                return memberAccessor;
            }

        }
        #endregion
    }

}
