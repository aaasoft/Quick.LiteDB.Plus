using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using LiteDB;
using System.IO;
using System.Linq;

namespace Quick.LiteDB.Plus
{
    public class LiteDatabaseBackupContext
    {
        private const string DB_DATA_ENTRY_NAME = "DATA";
        private static readonly Encoding dbBackupDataEncoding = Encoding.UTF8;
        private Dictionary<string, string> typeNameTableNameDict;
        private Dictionary<string, string> tableNameTypeNameDict;
        private Action<int, string> progressNotify;
        private Action<string> stateNotify;
        private LiteDatabaseBackupContextTextResource textResource;

        public LiteDatabaseBackupContext(
            Dictionary<string, string> typeNameTableNameDict,
            Action<int, string> progressNotify = null,
            Action<string> stateNotify = null,
            LiteDatabaseBackupContextTextResource textResource = null)
        {
            this.typeNameTableNameDict = typeNameTableNameDict;
            tableNameTypeNameDict = typeNameTableNameDict.ToDictionary(t => t.Value, t => t.Key);

            this.progressNotify = progressNotify;
            this.stateNotify = stateNotify;
            this.textResource = textResource;
        }

        public void Backup(LiteDatabase db, string backupFile)
        {
            using (var stream = File.OpenWrite(backupFile))
                Backup(db, stream);
        }

        public void Backup(LiteDatabase db, Stream backupStream)
        {
            using (var zipArchive = new ZipArchive(backupStream, ZipArchiveMode.Create, true))
            {
                var dataEntry = zipArchive.CreateEntry(DB_DATA_ENTRY_NAME);
                using (var stream = dataEntry.Open())
                using (var writer = new StreamWriter(stream, dbBackupDataEncoding))
                {
                    var collectionNames = db.GetCollectionNames().ToArray();
                    var i = 1;
                    foreach (var collectionName in collectionNames)
                    {
                        var collection = db.GetCollection(collectionName);
                        var typeName = collectionName;
                        tableNameTypeNameDict.TryGetValue(collectionName, out typeName);
                        progressNotify?.Invoke(i * 100 / collectionNames.Length, $"({i}/{collectionNames.Length}) {typeName}({collectionName}))");
                        i++;
                        try
                        {
                            writer.WriteLine($"#{typeName}");
                            foreach (var item in collection.FindAll())
                            {
                                writer.WriteLine(item.ToString());
                            }
                        }
                        catch { }
                    }
                }
            }
        }

        public void Restore(LiteDatabase db, string backupFile)
        {
            using (var stream = File.OpenRead(backupFile))
                Restore(db, stream);
        }

        public void Restore(LiteDatabase db, Stream backupStream)
        {
            //读取元信息
            using (ZipArchive zipArchive = new ZipArchive(backupStream, ZipArchiveMode.Read, true))
            {
                var dataEntry = zipArchive.GetEntry(DB_DATA_ENTRY_NAME);
                if (dataEntry == null)
                    throw new ApplicationException(textResource.DatabaseBackupFileHasNoData);

                var totalLength = dataEntry.Length;
                //开始导入
                using (var stream = dataEntry.Open())
                using (var reader = new StreamReader(stream, dbBackupDataEncoding))
                {
                    stateNotify?.Invoke(textResource.DeletingTableSchema);
                    foreach (var collectionName in db.GetCollectionNames())
                        db.DropCollection(collectionName);

                    stateNotify?.Invoke(textResource.RestoringData);

                    string currentTypeName = null;
                    ILiteCollection<BsonDocument> currentCollection = null;
                    long position = 0;
                    Action updateProgress = () =>
                    {
                        progressNotify?.Invoke(
                           Convert.ToInt32(position * 100 / totalLength),
                           currentTypeName == null ?
                               null : currentTypeName);
                    };

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        position += reader.CurrentEncoding.GetByteCount(line) + 1;
                        if (string.IsNullOrEmpty(line))
                            continue;
                        if (line.StartsWith("#"))
                        {
                            currentTypeName = line.Substring(1);
                            updateProgress();
                            if (currentTypeName == null)
                                continue;
                            currentCollection = db.GetCollection(typeNameTableNameDict[currentTypeName]);
                        }
                        else if (line.StartsWith("{"))
                        {
                            if (currentTypeName == null)
                                continue;
                            updateProgress();
                            var item = JsonSerializer.Deserialize(line);
                            currentCollection.Insert((BsonDocument)item);
                        }
                    }
                    stateNotify?.Invoke(textResource.SavingChanges);
                }
            }
        }

        /// <summary>
        /// 更新结构
        /// </summary>
        /// <param name="db"></param>
        public void UpdateSchema(LiteDatabase db)
        {
            using (var ms = new MemoryStream())
            {
                //备份
                Backup(db, ms);
                //还原
                ms.Position = 0;
                Restore(db, ms);
            }
        }
    }
}
