using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Linq;
using EntityFramework.Reflection;

namespace EntityFramework.Audit
{
    internal class AuditEntryState
    {
        public AuditEntryState(ObjectContext objectContext, ObjectStateEntry objectStateEntry)
        {
            if (objectStateEntry == null)
                throw new ArgumentNullException("objectStateEntry");

            if (objectStateEntry.Entity == null)
                throw new ArgumentException("The Entity property is null for the specified ObjectStateEntry.", "objectStateEntry");

            if (objectContext == null)
            {
                throw new ArgumentNullException("objectContext");
            }

            ObjectStateEntry = objectStateEntry;
            ObjectContext = objectContext;
            ObjectType = ObjectContext.GetObjectType(objectStateEntry.Entity.GetType());

            Entity = objectStateEntry.Entity;
            EntityType = objectContext.MetadataWorkspace.GetItems(DataSpace.OSpace).OfType<EntityType>().FirstOrDefault(x => x.Name == ObjectType.Name);
            EntityAccessor = TypeAccessor.GetAccessor(ObjectType);

            AuditEntity = new AuditEntity(objectStateEntry.Entity)
            {
                Action = GetAction(objectStateEntry),
            };
        }

        public ObjectContext ObjectContext { get; set; }
        public AuditLog AuditLog { get; set; }

        public object Entity { get; private set; }
        public Type ObjectType { get; private set; }
        public EntityType EntityType { get; private set; }
        public ObjectStateEntry ObjectStateEntry { get; private set; }
        public TypeAccessor EntityAccessor { get; private set; }

        public AuditEntity AuditEntity { get; private set; }

        public bool IsAdded
        {
            get { return AuditEntity.Action == AuditAction.Added; }
        }
        public bool IsDeleted
        {
            get { return AuditEntity.Action == AuditAction.Deleted; }
        }
        public bool IsModified
        {
            get { return AuditEntity.Action == AuditAction.Modified; }
        }

        private static AuditAction GetAction(ObjectStateEntry entity)
        {
            switch (entity.State)
            {
                case EntityState.Added:
                    return AuditAction.Added;
                case EntityState.Deleted:
                    return AuditAction.Deleted;
                default:
                    return AuditAction.Modified;
            }
        }
    }
}