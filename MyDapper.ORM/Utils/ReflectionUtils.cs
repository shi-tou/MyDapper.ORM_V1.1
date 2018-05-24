using MyDapper.ORM.Mapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace MyDapper.ORM.Utils
{
    /// <summary>
    /// 反射处理类：用于获取Model的Properties集合、Attributes集合
    /// </summary>
    public class ReflectionUtils
    {
        /// <summary>
        /// Model对象Properties集合
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
        public static List<PropertyInfo> TypePropertiesCache(Type type)
        {
            IEnumerable<PropertyInfo> pis;
            if (TypeProperties.TryGetValue(type.TypeHandle, out pis))
            {
                return pis.ToList();
            }

            var properties = type.GetProperties().ToArray();
            TypeProperties[type.TypeHandle] = properties;
            return properties.ToList();
        }
        /// <summary>
        /// Model对象自定义Attributes集合
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, TableAttribute> CustomAttributes = new ConcurrentDictionary<RuntimeTypeHandle, TableAttribute>();
        public static TableAttribute CustomAttributesCache(Type type)
        {
            TableAttribute attr;
            if (CustomAttributes.TryGetValue(type.TypeHandle, out attr))
            {
                return attr;
            }

            attr = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault();
            //if (attr == null)
            //{
            //    throw new Exception("类" + type.Name + "必须添加'_TableAttribute'属性");
            //}
            CustomAttributes[type.TypeHandle] = attr;
            return attr;
        }
    }
}
