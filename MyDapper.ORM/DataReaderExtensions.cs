using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace MyDapper.ORM
{
    /// <summary>
    /// DataReader 扩展
    /// </summary>
    public static class DataReaderExtensions
    {

        #region 把需要的IDataReader里面的方法先反射出来，做好缓存，避免为每个T反射一次
        private static readonly MethodInfo DataRecord_ItemGetter_String =
            typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(string) });
        private static readonly MethodInfo DataRecord_ItemGetter_Int =
            typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo DataRecord_GetOrdinal =
            typeof(IDataRecord).GetMethod("GetOrdinal");
        private static readonly MethodInfo DataReader_Read =
            typeof(IDataReader).GetMethod("Read");
        private static readonly MethodInfo Convert_IsDBNull =
            typeof(Convert).GetMethod("IsDBNull");
        private static readonly MethodInfo DataRecord_GetDateTime =
            typeof(IDataRecord).GetMethod("GetDateTime");
        private static readonly MethodInfo DataRecord_GetDecimal =
            typeof(IDataRecord).GetMethod("GetDecimal");
        private static readonly MethodInfo DataRecord_GetDouble =
            typeof(IDataRecord).GetMethod("GetDouble");
        private static readonly MethodInfo DataRecord_GetInt32 =
            typeof(IDataRecord).GetMethod("GetInt32");
        private static readonly MethodInfo DataRecord_GetInt64 =
            typeof(IDataRecord).GetMethod("GetInt64");
        private static readonly MethodInfo DataRecord_GetString =
            typeof(IDataRecord).GetMethod("GetString");
        private static readonly MethodInfo DataRecord_IsDBNull =
            typeof(IDataRecord).GetMethod("IsDBNull");
        #endregion

        #region 公开对外方法

        /// <summary>
        /// 把结果集流转换成数据实体列表
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="reader">结果集流</param>
        /// <returns>数据实体列表</returns>
        public static List<T> Select<T>(this IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            return EntityConverter<T>.Select(reader);
        }

        /// <summary>
        /// 把结果集流转换成数据实体序列（延迟）
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        /// <param name="reader">结果集流</param>
        /// <returns>数据实体序列（延迟）</returns>
        public static IEnumerable<T> SelectLazy<T>(this IDataReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            return EntityConverter<T>.SelectDelay(reader);
        }

        #endregion

        /// <summary>
        /// 实体转换器
        /// </summary>
        /// <typeparam name="T">数据实体类型</typeparam>
        private class EntityConverter<T>
        {
            #region 转换器
            /// <summary>
            /// 单个对象转换器
            /// </summary>
            private static Converter<IDataReader, T> dataLoader;
            private static Converter<IDataReader, T> DataLoader
            {
                get
                {
                    if (dataLoader == null)
                        dataLoader = CreateDataLoader(new List<PropertyInfo>(GetTypeProperties()));
                    return dataLoader;
                }
            }
            /// <summary>
            /// 列表对象转换器
            /// </summary>
            private static Converter<IDataReader, List<T>> batchDataLoader;
            private static Converter<IDataReader, List<T>> BatchDataLoader
            {
                get
                {
                    if (batchDataLoader == null)
                        batchDataLoader = CreateBatchDataLoader(new List<PropertyInfo>(GetTypeProperties()));
                    return batchDataLoader;
                }
            }
            #endregion

            #region Emit处理方法
            /// <summary>
            /// 获取类型属性集合
            /// </summary>
            /// <returns></returns>
            private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> TypeProperties = new ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>>();
            private static IEnumerable<PropertyInfo> GetTypeProperties()
            {
                Type type = typeof(T);
                IEnumerable<PropertyInfo> pis;
                if (TypeProperties.TryGetValue(type.TypeHandle, out pis))
                {
                    return pis;
                }
                var properties = type.GetProperties();
                TypeProperties[type.TypeHandle] = properties;
                return properties.ToList();
            }

            /// <summary>
            /// 创建单个数据装载器
            /// </summary>
            /// <param name="propertyInfoes"></param>
            /// <returns></returns>
            private static Converter<IDataReader, T> CreateDataLoader(List<PropertyInfo> propertyInfoes)
            {
                DynamicMethod dm = new DynamicMethod(string.Empty, typeof(T),
                    new Type[] { typeof(IDataReader) }, typeof(EntityConverter<T>));
                ILGenerator il = dm.GetILGenerator();
                LocalBuilder item = il.DeclareLocal(typeof(T));
                //获取类内属性索引
                LocalBuilder[] propertyIndices = GetPropertyIndices(il, propertyInfoes);
                //创建数据对象
                BuildItem(il, propertyInfoes, item, propertyIndices);
                //返回对象
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ret);
                return (Converter<IDataReader, T>)dm.CreateDelegate(typeof(Converter<IDataReader, T>));
            }

            /// <summary>
            /// 创建列表数据装载器
            /// </summary>
            /// <param name="propertyInfoes"></param>
            /// <returns></returns>
            private static Converter<IDataReader, List<T>> CreateBatchDataLoader(List<PropertyInfo> propertyInfoes)
            {
                DynamicMethod dm = new DynamicMethod(string.Empty, typeof(List<T>),
                    new Type[] { typeof(IDataReader) }, typeof(EntityConverter<T>));
                ILGenerator il = dm.GetILGenerator();
                LocalBuilder list = il.DeclareLocal(typeof(List<T>));
                LocalBuilder item = il.DeclareLocal(typeof(T));
                Label exit = il.DefineLabel();
                Label loop = il.DefineLabel();
                // List<T> list = new List<T>();
                il.Emit(OpCodes.Newobj, typeof(List<T>).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc_S, list);
                //获取类内属性索引
                LocalBuilder[] propertyIndices = GetPropertyIndices(il, propertyInfoes);
                //开始循环创建数据对象，并添加到List<T>中
                il.MarkLabel(loop);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Callvirt, DataReader_Read);
                il.Emit(OpCodes.Brfalse, exit);
                //创建数据对象
                BuildItem(il, propertyInfoes, item, propertyIndices);
                //添加到List<T>
                il.Emit(OpCodes.Ldloc_S, list);
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Callvirt, typeof(List<T>).GetMethod("Add"));
                //结束循环
                il.Emit(OpCodes.Br, loop);
                il.MarkLabel(exit);
                //返回List<T>
                il.Emit(OpCodes.Ldloc_S, list);
                il.Emit(OpCodes.Ret);
                return (Converter<IDataReader, List<T>>)dm.CreateDelegate(typeof(Converter<IDataReader, List<T>>));
            }

            /// <summary>
            /// 获取对象属性在类内的索引
            /// </summary>
            /// <param name="il"></param>
            /// <param name="propertyInfoes"></param>
            /// <returns></returns>
            private static LocalBuilder[] GetPropertyIndices(ILGenerator il, List<PropertyInfo> propertyInfoes)
            {
                LocalBuilder[] propertyIndices = new LocalBuilder[propertyInfoes.Count];
                for (int i = 0; i < propertyIndices.Length; i++)
                {
                    propertyIndices[i] = il.DeclareLocal(typeof(int));

                    //il.BeginExceptionBlock();
                    il.Emit(OpCodes.Ldarg_0);
                    il.Emit(OpCodes.Ldstr, propertyInfoes[i].Name);
                    il.Emit(OpCodes.Callvirt, DataRecord_GetOrdinal);
                    il.Emit(OpCodes.Stloc_S, propertyIndices[i]);
                    //可选数据列的处理
                    //if (propertyInfoes[i].IsOptional)
                    //{
                    //    Label exit = il.DefineLabel();
                    //    il.Emit(OpCodes.Leave_S, exit);
                    //    // } catch (IndexOutOfRangeException) {
                    //    il.BeginCatchBlock(typeof(IndexOutOfRangeException));
                    //    // //forget the exception
                    //    il.Emit(OpCodes.Pop);
                    //    // int %index% = -1; // if not found, -1
                    //    il.Emit(OpCodes.Ldc_I4_M1);
                    //    il.Emit(OpCodes.Stloc_S, propertyIndices[i]);
                    //    il.Emit(OpCodes.Leave_S, exit);
                    //    // } catch (ArgumentException) {
                    //    il.BeginCatchBlock(typeof(ArgumentException));
                    //    // forget the exception
                    //    il.Emit(OpCodes.Pop);
                    //    // int %index% = -1; // if not found, -1
                    //    il.Emit(OpCodes.Ldc_I4_M1);
                    //    il.Emit(OpCodes.Stloc_S, propertyIndices[i]);
                    //    il.Emit(OpCodes.Leave_S, exit);
                    //    // }
                    //    il.EndExceptionBlock();
                    //    il.MarkLabel(exit);
                    //}
                }
                return propertyIndices;
            }
            /// <summary>
            /// 创建对象，并赋值
            /// </summary>
            /// <param name="il"></param>
            /// <param name="propertyInfoes"></param>
            /// <param name="item"></param>
            /// <param name="propertyIndices"></param>
            private static void BuildItem(ILGenerator il, List<PropertyInfo> propertyInfoes, LocalBuilder item, LocalBuilder[] propertyIndices)
            {
                il.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Stloc_S, item);
                //Label skip = new Label();
                for (int i = 0; i < propertyIndices.Length; i++)
                {
                    //可选数据列的处理
                    //if (propertyInfoes[i].IsOptional)
                    //{
                    //    // if %index% == -1 then goto skip;
                    //    skip = il.DefineLabel();
                    //    il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                    //    il.Emit(OpCodes.Ldc_I4_M1);
                    //    il.Emit(OpCodes.Beq, skip);
                    //}
                    //声明成员的类型
                    Type declaringType = propertyInfoes[i].PropertyType;
                    if (IsCompatibleType(declaringType, typeof(int)))
                    {
                        ReadInt32(il, item, propertyInfoes, propertyIndices, i);
                    }
                    else if (IsCompatibleType(declaringType, typeof(int?)))
                    {
                        ReadNullableInt32(il, item, propertyInfoes, propertyIndices, i);
                    }
                    else if (IsCompatibleType(declaringType, typeof(long)))
                    {
                        ReadInt64(il, item, propertyInfoes, propertyIndices, i);
                    }
                    else if (IsCompatibleType(declaringType, typeof(long?)))
                    {
                        ReadNullableInt64(il, item, propertyInfoes, propertyIndices, i);
                    }
                    else if (IsCompatibleType(declaringType, typeof(decimal)))
                    {
                        ReadDecimal(il, item, propertyInfoes[i].GetSetMethod(), propertyIndices[i]);
                    }
                    else if (declaringType == typeof(decimal?))
                    {
                        ReadNullableDecimal(il, item, propertyInfoes[i].GetSetMethod(), propertyIndices[i]);
                    }
                    else if (declaringType == typeof(DateTime))
                    {
                        ReadDateTime(il, item, propertyInfoes[i].GetSetMethod(), propertyIndices[i]);
                    }
                    else if (declaringType == typeof(DateTime?))
                    {
                        ReadNullableDateTime(il, item, propertyInfoes[i].GetSetMethod(), propertyIndices[i]);
                    }
                    else
                    {
                        ReadObject(il, item, propertyInfoes, propertyIndices, i);
                    }
                    //可选数据列的处理
                    //if (propertyInfoes[i].IsOptional)
                    //{
                    //    // :skip
                    //    il.MarkLabel(skip);
                    //}
                }
            }
            /// <summary>
            /// 判断类型是否一致
            /// </summary>
            /// <param name="t1"></param>
            /// <param name="t2"></param>
            /// <returns></returns>
            private static bool IsCompatibleType(Type t1, Type t2)
            {
                if (t1 == t2)
                    return true;
                if (t1.IsEnum && Enum.GetUnderlyingType(t1) == t2)
                    return true;
                var u1 = Nullable.GetUnderlyingType(t1);
                var u2 = Nullable.GetUnderlyingType(t2);
                if (u1 != null && u2 != null)
                    return IsCompatibleType(u1, u2);
                return false;
            }
            #endregion

            #region 读取不同类型的值
            private static void ReadInt32(ILGenerator il, LocalBuilder item,
                List<PropertyInfo> propertyInfoes, LocalBuilder[] propertyIndices, int i)
            {
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                il.Emit(OpCodes.Callvirt, DataRecord_GetInt32);
                il.Emit(OpCodes.Callvirt, propertyInfoes[i].GetSetMethod());
            }

            private static void ReadNullableInt32(ILGenerator il, LocalBuilder item,
                List<PropertyInfo> propertyInfoes, LocalBuilder[] propertyIndices, int i)
            {
                Type type = propertyInfoes[i].PropertyType;
                var local = il.DeclareLocal(type);
                Label intNull = il.DefineLabel();
                Label intCommon = il.DefineLabel();
                il.Emit(OpCodes.Ldloca, local);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                il.Emit(OpCodes.Callvirt, DataRecord_IsDBNull);
                il.Emit(OpCodes.Brtrue_S, intNull);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                il.Emit(OpCodes.Callvirt, DataRecord_GetInt32);
                il.Emit(OpCodes.Call, type.GetConstructor(
                    new Type[] { Nullable.GetUnderlyingType(type) }));
                il.Emit(OpCodes.Br_S, intCommon);
                il.MarkLabel(intNull);
                il.Emit(OpCodes.Initobj, type);
                il.MarkLabel(intCommon);
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Callvirt, propertyInfoes[i].GetSetMethod());
            }

            private static void ReadInt64(ILGenerator il, LocalBuilder item,
                List<PropertyInfo> propertyInfoes, LocalBuilder[] propertyIndices, int i)
            {
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                il.Emit(OpCodes.Callvirt, DataRecord_GetInt64);
                il.Emit(OpCodes.Callvirt, propertyInfoes[i].GetSetMethod());
            }

            private static void ReadNullableInt64(ILGenerator il, LocalBuilder item,
                List<PropertyInfo> propertyInfoes, LocalBuilder[] propertyIndices, int i)
            {
                Type type = propertyInfoes[i].PropertyType;
                var local = il.DeclareLocal(type);
                Label intNull = il.DefineLabel();
                Label intCommon = il.DefineLabel();
                il.Emit(OpCodes.Ldloca, local);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                il.Emit(OpCodes.Callvirt, DataRecord_IsDBNull);
                il.Emit(OpCodes.Brtrue_S, intNull);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                il.Emit(OpCodes.Callvirt, DataRecord_GetInt64);
                il.Emit(OpCodes.Call, type.GetConstructor(
                    new Type[] { Nullable.GetUnderlyingType(type) }));
                il.Emit(OpCodes.Br_S, intCommon);
                il.MarkLabel(intNull);
                il.Emit(OpCodes.Initobj, type);
                il.MarkLabel(intCommon);
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Callvirt, propertyInfoes[i].GetSetMethod());
            }

            private static void ReadDecimal(ILGenerator il, LocalBuilder item,
                MethodInfo setMethod, LocalBuilder colIndex)
            {
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, colIndex);
                il.Emit(OpCodes.Callvirt, DataRecord_GetDecimal);
                il.Emit(OpCodes.Callvirt, setMethod);
            }

            private static void ReadNullableDecimal(ILGenerator il, LocalBuilder item,
                MethodInfo setMethod, LocalBuilder colIndex)
            {
                var local = il.DeclareLocal(typeof(decimal?));
                Label decimalNull = il.DefineLabel();
                Label decimalCommon = il.DefineLabel();
                il.Emit(OpCodes.Ldloca, local);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, colIndex);
                il.Emit(OpCodes.Callvirt, DataRecord_IsDBNull);
                il.Emit(OpCodes.Brtrue_S, decimalNull);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, colIndex);
                il.Emit(OpCodes.Callvirt, DataRecord_GetDecimal);
                il.Emit(OpCodes.Call, typeof(decimal?).GetConstructor(new Type[] { typeof(decimal) }));
                il.Emit(OpCodes.Br_S, decimalCommon);
                il.MarkLabel(decimalNull);
                il.Emit(OpCodes.Initobj, typeof(decimal?));
                il.MarkLabel(decimalCommon);
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Callvirt, setMethod);
            }

            private static void ReadDateTime(ILGenerator il, LocalBuilder item,
                MethodInfo setMethod, LocalBuilder colIndex)
            {
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, colIndex);
                il.Emit(OpCodes.Callvirt, DataRecord_GetDateTime);
                il.Emit(OpCodes.Callvirt, setMethod);
            }

            private static void ReadNullableDateTime(ILGenerator il, LocalBuilder item,
                MethodInfo setMethod, LocalBuilder colIndex)
            {
                var local = il.DeclareLocal(typeof(DateTime?));
                Label dtNull = il.DefineLabel();
                Label dtCommon = il.DefineLabel();
                il.Emit(OpCodes.Ldloca, local);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, colIndex);
                il.Emit(OpCodes.Callvirt, DataRecord_IsDBNull);
                il.Emit(OpCodes.Brtrue_S, dtNull);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, colIndex);
                il.Emit(OpCodes.Callvirt, DataRecord_GetDateTime);
                il.Emit(OpCodes.Call, typeof(DateTime?).GetConstructor(new Type[] { typeof(DateTime) }));
                il.Emit(OpCodes.Br_S, dtCommon);
                il.MarkLabel(dtNull);
                il.Emit(OpCodes.Initobj, typeof(DateTime?));
                il.MarkLabel(dtCommon);
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Callvirt, setMethod);
            }

            private static void ReadObject(ILGenerator il, LocalBuilder item,
                List<PropertyInfo> propertyInfoes, LocalBuilder[] propertyIndices, int i)
            {
                Type type = propertyInfoes[i].PropertyType;
                Label common = il.DefineLabel();
                il.Emit(OpCodes.Ldloc_S, item);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc_S, propertyIndices[i]);
                il.Emit(OpCodes.Callvirt, DataRecord_ItemGetter_Int);
                il.Emit(OpCodes.Dup);
                il.Emit(OpCodes.Call, Convert_IsDBNull);
                il.Emit(OpCodes.Brfalse_S, common);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ldnull);
                il.MarkLabel(common);
                il.Emit(OpCodes.Unbox_Any, type);
                il.Emit(OpCodes.Callvirt, propertyInfoes[i].GetSetMethod());
            }

            #endregion

            #region 内部方法
            /// <summary>
            /// 懒加载
            /// </summary>
            /// <param name="reader"></param>
            /// <returns></returns>
            internal static IEnumerable<T> SelectDelay(IDataReader reader)
            {
                while (reader.Read())
                    yield return DataLoader(reader);
            }
            /// <summary>
            /// 即时加载
            /// </summary>
            /// <param name="reader"></param>
            /// <returns></returns>
            internal static List<T> Select(IDataReader reader)
            {
                return BatchDataLoader(reader);
            }

            #endregion

        }
    }
}
