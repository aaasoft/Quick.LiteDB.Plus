using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LiteDB;

namespace Quick.LiteDB.Plus
{
    /// <summary>
    /// 配置数据库上下文
    /// </summary>
    public partial class ConfigDbContext : DbContext
    {
        private static Action<ModelBuilder> ModelBuilderHandler;
        public static DbCacheContext<ConfigDbContext> CacheContext { get; } = new DbCacheContext<ConfigDbContext>();
        /// <summary>
        /// 配置处理器
        /// </summary>
        public static string ConfigConnectionString { get; set; }

        public static void Init(string connectionString, Action<ModelBuilder> modelBuilderHandler)
        {
            ConfigConnectionString = connectionString;
            ModelBuilderHandler = modelBuilderHandler;
        }

        public ConfigDbContext() : base(ConfigConnectionString) { }

        public ConfigDbContext(string connectionString) : base(connectionString) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ModelBuilderHandler(modelBuilder);
        }
    }
}
