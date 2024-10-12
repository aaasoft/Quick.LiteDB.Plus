using System;
using System.ComponentModel;
using System.Reflection;

namespace Quick.LiteDB.Plus
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ModelMetaAttribute : DisplayNameAttribute
    {
        public ModelMetaAttribute(string displayName) : base(displayName) { }
    }
}
