using System;
using System.Collections.Generic;
using System.Reflection;
using LiteDB;

namespace Quick.LiteDB.Plus
{
    public abstract class DbContext : IDisposable
    {
        public LiteDatabase Database { get; private set; }
        private Dictionary<Type, string> collectionNameDict = new Dictionary<Type, string>();

        public DbContext(string connectionString)
        {
            Database = new LiteDatabase(connectionString);
            OnModelCreating(new ModelBuilder(this));
        }

        internal void RegisterCollection(Type type, string collectionName)
        {
            collectionNameDict[type] = collectionName;
        }


        public ILiteCollection<T> GetCollection<T>()
        {
            var type = typeof(T);
            if (!collectionNameDict.TryGetValue(type, out var collectionName))
                throw new LiteException(-1, $"Type[{type.FullName}] not mapped!");
            return Database.GetCollection<T>(collectionName);
        }

        protected virtual void OnModelCreating(ModelBuilder modelBuilder) { }

        public virtual BsonValue Add<T>(T entity)
        {
            return GetCollection<T>().Insert(entity);
        }

        public virtual int AddRange<T>(IEnumerable<T> entities)
        {
            return GetCollection<T>().InsertBulk(entities);
        }

        public virtual bool Remove<T>(T entity)
        {
            var collection = GetCollection<T>();

            var id = collection.EntityMapper.Id.Getter.Invoke(entity);
            return collection.Delete(new BsonValue(id));
        }

        public virtual bool Update<T>(T entity)
        {
            return GetCollection<T>().Update(entity);
        }

        public virtual void Dispose()
        {
            Database?.Dispose();
        }

        public virtual IEnumerable<ILiteCollection<BsonDocument>> GetCollections()
        {
            foreach (var collectionName in Database.GetCollectionNames())
            {
                yield return Database.GetCollection(collectionName);
            }
        }
    }
}