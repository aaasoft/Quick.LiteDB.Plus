using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using LiteDB;

namespace Quick.LiteDB.Plus
{
    public class ModelBuilder
    {
        private DbContext dbContext;
        internal ModelBuilder(DbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public void Entity<T>(Action<ILiteCollection<T>> collectionAction)
        {
            var type = typeof(T);
            var collectionName = type.FullName;
            var tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
                collectionName = tableAttribute.Name;
            dbContext.RegisterCollection(type, collectionName);
            var collection = dbContext.GetCollection<T>();
            collectionAction(collection);
        }
    }
}