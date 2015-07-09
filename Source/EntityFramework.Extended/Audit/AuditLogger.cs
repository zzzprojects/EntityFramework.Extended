using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using EntityFramework.Extensions;
using EntityFramework.Reflection;

namespace EntityFramework.Audit
{
    /// <summary>
    /// A class used to create an AuditLog.
    /// </summary>
    public class AuditLogger : IDisposable
    {
        private static readonly Lazy<MethodInfo> _relatedAccessor = new Lazy<MethodInfo>(FindRelatedMethod);
        private const string _nullText = "{null}";
        private const string _errorText = "{error}";

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogger"/> class.
        /// </summary>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to create the <see cref="AuditLog"/> from.</param>
        public AuditLogger(ObjectContext objectContext)
            : this(objectContext, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogger"/> class.
        /// </summary>
        /// <param name="objectContext">The <see cref="ObjectContext"/> to create the <see cref="AuditLog"/> from.</param>
        /// <param name="configuration">The <see cref="AuditConfiguration"/> to use when creating the <see cref="AuditLog"/>.</param>
        public AuditLogger(ObjectContext objectContext, AuditConfiguration configuration)
        {
            if (objectContext == null)
                throw new ArgumentNullException("objectContext");

            _objectContext = objectContext;
            _configuration = configuration ?? AuditConfiguration.Default;

            AttachEvents();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogger"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="DbContext"/> to create the <see cref="AuditLog"/> from.</param>
        public AuditLogger(DbContext dbContext)
            : this(dbContext, null)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogger"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="DbContext"/> to create the <see cref="AuditLog"/> from.</param>
        /// <param name="configuration">The <see cref="AuditConfiguration"/> to use when creating the <see cref="AuditLog"/>.</param>
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

        /// <summary>
        /// Called when OjbectContext is saving changes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void OnSavingChanges(object sender, EventArgs e)
        {
            if (LastLog == null || !Configuration.MaintainAcrossSaves)
                LastLog = CreateLog();
            else
                UpdateLog(LastLog);
        }
        #endregion

        private readonly ObjectContext _objectContext;
        /// <summary>
        /// Gets the ObjectContext to create the <see cref="AuditLog"/> from.
        /// </summary>
        public ObjectContext ObjectContext
        {
            get { return _objectContext; }
        }

        private readonly AuditConfiguration _configuration;
        /// <summary>
        /// Gets the <see cref="AuditConfiguration"/> to use when creating the <see cref="AuditLog"/>.
        /// </summary>
        public AuditConfiguration Configuration
        {
            get { return _configuration; }
        }

        /// <summary>
        /// Gets the last <see cref="AuditLog"/> created.
        /// </summary>
        public AuditLog LastLog { get; private set; }

        /// <summary>
        /// Creates the <see cref="AuditLog"/> from the current <see cref="ObjectContext"/>.
        /// </summary>
        /// <returns></returns>
        public AuditLog CreateLog()
        {
            var auditLog = new AuditLog
            {
                Date = DateTime.Now,
                Username = Environment.UserName
            };

            return UpdateLog(auditLog);
        }

        /// <summary>
        /// Updates the <see cref="AuditLog"/> from the current <see cref="ObjectContext"/> changes. 
        /// </summary>
        /// <param name="auditLog">The audit log.</param>
        /// <returns></returns>
        public AuditLog UpdateLog(AuditLog auditLog)
        {
            if (auditLog == null)
                throw new ArgumentNullException("auditLog");

            // must call to make sure changes are detected
            ObjectContext.DetectChanges();

            var entityState = EntityState.Modified;
            if (Configuration.IncludeInserts)
                entityState = entityState | EntityState.Added;
            if (Configuration.IncludeDeletes)
                entityState = entityState | EntityState.Deleted;

            IEnumerable<ObjectStateEntry> changes = ObjectContext
                .ObjectStateManager
                .GetObjectStateEntries(entityState);

            foreach (ObjectStateEntry objectStateEntry in changes)
            {
                if (objectStateEntry.Entity == null)
                    continue;

                Type entityType = objectStateEntry.Entity.GetType();
                entityType = ObjectContext.GetObjectType(entityType);
                if (!Configuration.IsAuditable(entityType))
                    continue;

                var state = new AuditEntryState(ObjectContext, objectStateEntry)
                {
                    AuditLog = auditLog,
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

            var currentValues = state.IsDeleted
                ? state.ObjectStateEntry.OriginalValues
                : state.ObjectStateEntry.CurrentValues;

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

            var currentValues = state.IsDeleted
                ? state.ObjectStateEntry.OriginalValues
                : state.ObjectStateEntry.CurrentValues;

            var originalValues = state.IsModified
                ? state.ObjectStateEntry.OriginalValues
                : null;

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

            var currentValues = state.IsDeleted
                ? state.ObjectStateEntry.OriginalValues
                : state.ObjectStateEntry.CurrentValues;

            var originalValues = state.IsModified
                ? state.ObjectStateEntry.OriginalValues
                : null;

            foreach (NavigationProperty navigationProperty in properties)
            {
                if (navigationProperty.ToEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many
                    || navigationProperty.FromEndMember.RelationshipMultiplicity != RelationshipMultiplicity.Many)
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

                    object currentValue = null;

                    if (isLoaded)
                    {
                        // get value directly from instance to save db call
                        object valueInstance = accessor.GetValue(state.Entity);
                        if(valueInstance != null)
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

            var entitySet = state.ObjectContext.GetEntitySet(edmType.FullName);

            var sql = new StringBuilder();
            sql.Append("SELECT VALUE t.")
                .Append(displayMember.Name)
                .Append(" FROM ")
                .Append(entitySet.Name)
                .Append(" as t")
                .Append(" WHERE ");

            var parameters = new List<ObjectParameter>();
            for (int index = 0; index < fromProperties.Count; index++)
            {
                if (index > 0)
                    sql.Append(" AND ");

                string fromProperty = fromProperties[index];
                string toProperty = toProperties[index];
                var value = values.GetValue(toProperty);
                var name = "@" + fromProperty;

                sql.Append(" t.").Append(fromProperty);
                if (value != null)
                {
                    sql.Append(" == ").Append(name);
                    parameters.Add(new ObjectParameter(fromProperty, value));
                }
                else
                {
                    sql.Append(" is null");
                }
            }

            var q = state.ObjectContext.CreateQuery<object>(
                sql.ToString(),
                parameters.ToArray());

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
