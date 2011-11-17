using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Metadata.Edm;
using System.Data.Objects;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using EntityFramework.Extensions;
using EntityFramework.Reflection;

namespace EntityFramework.Audit
{
    public class AuditLogger : IDisposable
    {
        private static readonly Lazy<MethodInfo> _relatedAccessor = new Lazy<MethodInfo>(FindRelatedMethod);
        private const string _nullText = "{null}";
        private const string _errorText = "{error}";

        public AuditLogger(ObjectContext objectContext)
            : this(objectContext, null)
        { }

        public AuditLogger(ObjectContext objectContext, AuditConfiguration configuration)
        {
            if (objectContext == null)
                throw new ArgumentNullException("objectContext");

            _objectContext = objectContext;
            _configuration = configuration ?? AuditConfiguration.Default;

            AttachEvents();
        }

        public AuditLogger(DbContext dbContext)
            : this(dbContext, null)
        { }

        public AuditLogger(DbContext dbContext, AuditConfiguration configuration)
        {
            if (dbContext == null)
                throw new ArgumentNullException("dbContext");

            var adapter = (IObjectContextAdapter)dbContext;
            _objectContext = adapter.ObjectContext;
            _configuration = configuration ?? AuditConfiguration.Default;

            AttachEvents();
        }

        #region Events
        private void AttachEvents()
        {
            _objectContext.SavingChanges += OnSavingChanges;
        }

        private void DetachEvents()
        {
            _objectContext.SavingChanges -= OnSavingChanges;
        }

        protected virtual void OnSavingChanges(object sender, EventArgs e)
        {
            LastLog = CreateLog();
        }
        #endregion

        private readonly ObjectContext _objectContext;
        public ObjectContext ObjectContext
        {
            get { return _objectContext; }
        }

        private readonly AuditConfiguration _configuration;
        public AuditConfiguration Configuration
        {
            get { return _configuration; }
        }

        public AuditLog LastLog { get; private set; }


        public AuditLog CreateLog()
        {
            var auditLog = new AuditLog
            {
                Date = DateTime.Now,
                Username = Environment.UserName
            };

            // must call to make sure changes are detected
            ObjectContext.DetectChanges();

            IEnumerable<ObjectStateEntry> changes = ObjectContext
              .ObjectStateManager
              .GetObjectStateEntries(EntityState.Added | EntityState.Deleted | EntityState.Modified);

            foreach (ObjectStateEntry objectStateEntry in changes)
            {
                if (objectStateEntry.Entity == null)
                    continue;

                Type entityType = objectStateEntry.Entity.GetType();
                entityType = ObjectContext.GetObjectType(entityType);
                if (!Configuration.IsAuditable(entityType))
                    continue;

                var state = new AuditEntryState(objectStateEntry)
                {
                    AuditLog = auditLog,
                    ObjectContext = ObjectContext,
                };

                if (WriteEntity(state))
                    auditLog.Entities.Add(state.AuditEntity);
            }

            return auditLog;
        }


        private bool WriteEntity(AuditEntryState state)
        {
            if (state.EntityType == null)
                return false;

            WriteKeys(state);
            WriteProperties(state);
            WriteRelationships(state);

            return true;

        }

        private void WriteKeys(AuditEntryState state)
        {
            var keys = state.EntityType.KeyMembers;
            if (keys == null)
                return;

            var currentValues = state.ObjectStateEntry.CurrentValues;

            foreach (var keyMember in keys)
            {
                var auditkey = new AuditKey();
                try
                {
                    var name = keyMember.Name;
                    auditkey.Name = name;
                    auditkey.Type = GetType(keyMember);

                    object value = currentValues.GetValue(name);
                    value = FormatValue(state, name, value);

                    auditkey.Value = value;
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    auditkey.Value = _errorText;
                }
                state.AuditEntity.Keys.Add(auditkey);
            }
        }

        private void WriteProperties(AuditEntryState state)
        {
            var properties = state.EntityType.Properties;
            if (properties == null)
                return;

            var modifiedMembers = state.ObjectStateEntry
              .GetModifiedProperties()
              .ToList();

            var type = state.ObjectType;

            CurrentValueRecord currentValues = state.ObjectStateEntry.CurrentValues;
            DbDataRecord originalValues = state.IsModified
              ? state.ObjectStateEntry.OriginalValues : null;

            foreach (EdmProperty edmProperty in properties)
            {
                string name = edmProperty.Name;
                if (Configuration.IsNotAudited(type, name))
                    continue;

                bool isModified = modifiedMembers.Any(m => m == name);

                if (state.IsModified && !isModified
                  && !Configuration.IsAlwaysAudited(type, name))
                    continue; // this means the property was not changed, skip it

                var auditProperty = new AuditProperty();
                try
                {
                    auditProperty.Name = name;
                    auditProperty.Type = GetType(edmProperty);

                    var currentValue = currentValues.GetValue(name);
                    currentValue = FormatValue(state, name, currentValue);

                    if (!state.IsModified && currentValue == null)
                        continue; // ignore null properties?

                    switch (state.AuditEntity.Action)
                    {
                        case AuditAction.Added:
                            auditProperty.Current = currentValue;
                            break;
                        case AuditAction.Modified:
                            auditProperty.Current = currentValue;

                            if (originalValues != null)
                            {
                                object originalValue = originalValues.GetValue(edmProperty.Name);
                                originalValue = FormatValue(state, name, originalValue);

                                auditProperty.Original = originalValue;
                            }
                            break;
                        case AuditAction.Deleted:
                            auditProperty.Original = currentValue;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    if (state.IsDeleted)
                        auditProperty.Original = _errorText;
                    else
                        auditProperty.Current = _errorText;
                }

                state.AuditEntity.Properties.Add(auditProperty);
            } // foreach property

        }

        private void WriteRelationships(AuditEntryState state)
        {
            if (!Configuration.IncludeRelationships)
                return;

            var properties = state.EntityType.NavigationProperties;
            if (properties.Count == 0)
                return;

            var modifiedMembers = state.ObjectStateEntry
              .GetModifiedProperties()
              .ToList();

            var type = state.ObjectType;

            CurrentValueRecord currentValues = state.ObjectStateEntry.CurrentValues;
            DbDataRecord originalValues = state.IsModified
              ? state.ObjectStateEntry.OriginalValues : null;

            foreach (NavigationProperty navigationProperty in properties)
            {
                if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                    continue;

                string name = navigationProperty.Name;
                if (Configuration.IsNotAudited(type, name))
                    continue;

                var accessor = state.EntityAccessor.Find(name);
                IMemberAccessor displayMember = Configuration.GetDisplayMember(accessor.MemberType);
                if (displayMember == null)
                    continue; // no display property, skip

                bool isModified = IsModifed(navigationProperty, modifiedMembers);

                if (state.IsModified && !isModified
                  && !Configuration.IsAlwaysAudited(type, name))
                    continue; // this means the property was not changed, skip it

                bool isLoaded = IsLoaded(state, navigationProperty, accessor);
                if (!isLoaded && !Configuration.LoadRelationships)
                    continue;

                var auditProperty = new AuditProperty();

                try
                {
                    auditProperty.Name = name;
                    auditProperty.Type = accessor.MemberType.FullName;
                    auditProperty.IsRelationship = true;
                    auditProperty.ForeignKey = GetForeignKey(navigationProperty);

                    object currentValue;

                    if (isLoaded)
                    {
                        // get value directly from instance to save db call
                        object valueInstance = accessor.GetValue(state.Entity);
                        currentValue = displayMember.GetValue(valueInstance);
                    }
                    else
                    {
                        // get value from db
                        currentValue = GetDisplayValue(state, navigationProperty, displayMember, currentValues);
                    }

                    // format
                    currentValue = FormatValue(state, name, currentValue);

                    if (!state.IsModified && currentValue == null)
                        continue; // skip null value

                    switch (state.AuditEntity.Action)
                    {
                        case AuditAction.Added:
                            auditProperty.Current = currentValue;
                            break;
                        case AuditAction.Modified:
                            auditProperty.Current = currentValue ?? _nullText;

                            if (Configuration.LoadRelationships)
                            {
                                object originalValue = GetDisplayValue(state, navigationProperty, displayMember, originalValues);
                                originalValue = FormatValue(state, name, originalValue);

                                auditProperty.Original = originalValue;
                            }

                            break;
                        case AuditAction.Deleted:
                            auditProperty.Original = currentValue;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.Message);
                    if (state.IsDeleted)
                        auditProperty.Original = _errorText;
                    else
                        auditProperty.Current = _errorText;
                }

                state.AuditEntity.Properties.Add(auditProperty);
            }
        }


        private object FormatValue(AuditEntryState state, string name, object value)
        {
            if (value == null)
                return null;

            var valueType = value.GetType();

            try
            {
                object returnValue = valueType.IsEnum ? Enum.GetName(valueType, value) : value;

                IMethodAccessor formatMethod = Configuration.GetFormatter(state.ObjectType, name);
                if (formatMethod == null)
                    return returnValue;

                var context = new AuditPropertyContext
                {
                    ValueType = valueType,
                    Entity = state.Entity,
                    Value = returnValue
                };

                try
                {
                    return formatMethod.Invoke(null, new[] { context });
                }
                catch
                {
                    // eat format error?
                    return returnValue;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message);
                return _errorText;
            }
        }

        private static string GetForeignKey(NavigationProperty navigationProperty)
        {
            var association = navigationProperty.RelationshipType as AssociationType;
            if (association == null)
                return null;

            // only support first constraint
            var referentialConstraint = association.ReferentialConstraints.FirstOrDefault();
            if (referentialConstraint == null)
                return null;

            var toProperties = referentialConstraint
              .ToProperties
              .Select(p => p.Name)
              .ToArray();

            return string.Join(",", toProperties);
        }

        private static object GetDisplayValue(AuditEntryState state, NavigationProperty navigationProperty, IMemberAccessor displayMember, DbDataRecord values)
        {
            if (values == null)
                return null;

            var association = navigationProperty.RelationshipType as AssociationType;
            if (association == null)
                return null;

            // only support first constraint
            var referentialConstraint = association.ReferentialConstraints.FirstOrDefault();
            if (referentialConstraint == null)
                return null;

            var toProperties = referentialConstraint
              .ToProperties
              .Select(p => p.Name)
              .ToList();

            var fromProperties = referentialConstraint
              .FromProperties
              .Select(p => p.Name)
              .ToList();

            // make sure key columns match
            if (fromProperties.Count != toProperties.Count)
                return null;

            var edmType = referentialConstraint
              .FromProperties
              .Select(p => p.DeclaringType)
              .FirstOrDefault();

            if (edmType == null)
                return null;

            var eSql = string.Format("SELECT VALUE t FROM {0} AS t", edmType.Name);

            var q = state.ObjectContext.CreateQuery<object>(eSql);
            for (int index = 0; index < fromProperties.Count; index++)
            {
                string fromProperty = fromProperties[index];
                string toProperty = toProperties[index];

                var value = values.GetValue(toProperty);
                var predicate = string.Format("it.{0} == @{0}", fromProperty);
                var parameter = new ObjectParameter(fromProperty, value);

                q = q.Where(predicate, parameter);
            }
            q = q.SelectValue<object>("it." + displayMember.Name);

            return q.FirstOrDefault();
        }

        private static bool IsModifed(NavigationProperty navigationProperty, IEnumerable<string> modifiedMembers)
        {
            var association = navigationProperty.RelationshipType as AssociationType;
            if (association == null)
                return false;

            var referentialConstraint = association.ReferentialConstraints.FirstOrDefault();
            if (referentialConstraint == null)
                return false;

            var toProperties = referentialConstraint
                .ToProperties
                .Select(p => p.Name)
                .ToList();

            return modifiedMembers.Intersect(toProperties).Any();
        }

        private static bool IsLoaded(AuditEntryState state, NavigationProperty navigationProperty, IMemberAccessor accessor)
        {
            var relationshipManager = state.ObjectStateEntry.RelationshipManager;
            var getEntityReference = _relatedAccessor.Value.MakeGenericMethod(accessor.MemberType);
            var parameters = new[]
            {
                navigationProperty.RelationshipType.FullName,
                navigationProperty.ToEndMember.Name
            };

            var entityReference = getEntityReference.Invoke(relationshipManager, parameters) as EntityReference;
            return (entityReference != null && entityReference.IsLoaded);
        }

        private static string GetType(EdmMember edmMember)
        {
            var primitiveType = edmMember.TypeUsage.EdmType as PrimitiveType;
            string type = primitiveType != null && primitiveType.ClrEquivalentType != null
                            ? primitiveType.ClrEquivalentType.FullName
                            : edmMember.TypeUsage.EdmType.FullName;
            return type;
        }

        private static MethodInfo FindRelatedMethod()
        {
            var managerAccessor = TypeAccessor.GetAccessor(typeof(RelationshipManager));
            if (managerAccessor == null)
                return null;

            var methodAccessor = managerAccessor.FindMethod("GetRelatedReference", typeof(string), typeof(string));
            return methodAccessor == null ? null : methodAccessor.MethodInfo;
        }

        #region Dispose
        private bool _disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                DetachEvents();

            _disposed = true;
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="AuditLogger"/> is reclaimed by garbage collection.
        /// </summary>
        ~AuditLogger()
        {
            Dispose(false);
        }
        #endregion
    }
}
