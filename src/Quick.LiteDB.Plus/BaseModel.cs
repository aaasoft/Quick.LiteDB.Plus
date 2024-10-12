
namespace Quick.LiteDB.Plus
{
    /// <summary>
    /// 基础模型类
    /// </summary>
    public abstract class BaseModel
    {
        /// <summary>
        /// 编号
        /// </summary>
        public virtual string Id { get; set; }

        public override int GetHashCode()
        {
            return this.GetHashCode(
                t => t.Id);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj,
                t => t.Id);
        }
    }
}
