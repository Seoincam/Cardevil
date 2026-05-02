using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace Database
{
    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1>
    {
        public T1 Item1;

        public DatabaseTuple(T1 item1)
        {
            Item1 = item1;
        }

        public void Deconstruct(out T1 item1)
        {
            item1 = Item1;
        }

        public static implicit operator ValueTuple<T1>(DatabaseTuple<T1> value) => new ValueTuple<T1>(value.Item1);
        public static implicit operator DatabaseTuple<T1>(ValueTuple<T1> value) => new DatabaseTuple<T1>(value.Item1);
        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1, T2>
    {
        public T1 Item1;
        public T2 Item2;

        public DatabaseTuple(T1 item1, T2 item2)
        {
            Item1 = item1;
            Item2 = item2;
        }

        public void Deconstruct(out T1 item1, out T2 item2)
        {
            item1 = Item1;
            item2 = Item2;
        }

        public static implicit operator (T1, T2)(DatabaseTuple<T1, T2> value) => (value.Item1, value.Item2);
        public static implicit operator DatabaseTuple<T1, T2>((T1, T2) value) => new DatabaseTuple<T1, T2>(value.Item1, value.Item2);
        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1, T2, T3>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;

        public DatabaseTuple(T1 item1, T2 item2, T3 item3)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
        }

        public static implicit operator (T1, T2, T3)(DatabaseTuple<T1, T2, T3> value) => (value.Item1, value.Item2, value.Item3);
        public static implicit operator DatabaseTuple<T1, T2, T3>((T1, T2, T3) value) => new DatabaseTuple<T1, T2, T3>(value.Item1, value.Item2, value.Item3);
        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1, T2, T3, T4>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;

        public DatabaseTuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
        }

        public static implicit operator (T1, T2, T3, T4)(DatabaseTuple<T1, T2, T3, T4> value) => (value.Item1, value.Item2, value.Item3, value.Item4);
        public static implicit operator DatabaseTuple<T1, T2, T3, T4>((T1, T2, T3, T4) value) => new DatabaseTuple<T1, T2, T3, T4>(value.Item1, value.Item2, value.Item3, value.Item4);
        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1, T2, T3, T4, T5>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;

        public DatabaseTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
        }

        public static implicit operator (T1, T2, T3, T4, T5)(DatabaseTuple<T1, T2, T3, T4, T5> value) => (value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
        public static implicit operator DatabaseTuple<T1, T2, T3, T4, T5>((T1, T2, T3, T4, T5) value) => new DatabaseTuple<T1, T2, T3, T4, T5>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5);
        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1, T2, T3, T4, T5, T6>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;

        public DatabaseTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
            item6 = Item6;
        }

        public static implicit operator (T1, T2, T3, T4, T5, T6)(DatabaseTuple<T1, T2, T3, T4, T5, T6> value) => (value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6);
        public static implicit operator DatabaseTuple<T1, T2, T3, T4, T5, T6>((T1, T2, T3, T4, T5, T6) value) => new DatabaseTuple<T1, T2, T3, T4, T5, T6>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6);
        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1, T2, T3, T4, T5, T6, T7>
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;
        public T7 Item7;

        public DatabaseTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
            item6 = Item6;
            item7 = Item7;
        }

        public static implicit operator (T1, T2, T3, T4, T5, T6, T7)(DatabaseTuple<T1, T2, T3, T4, T5, T6, T7> value) => (value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
        public static implicit operator DatabaseTuple<T1, T2, T3, T4, T5, T6, T7>((T1, T2, T3, T4, T5, T6, T7) value) => new DatabaseTuple<T1, T2, T3, T4, T5, T6, T7>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7);
        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    [Serializable]
    [JsonConverter(typeof(DatabaseTupleJsonConverter))]
    public struct DatabaseTuple<T1, T2, T3, T4, T5, T6, T7, TRest>
        where TRest : struct
    {
        public T1 Item1;
        public T2 Item2;
        public T3 Item3;
        public T4 Item4;
        public T5 Item5;
        public T6 Item6;
        public T7 Item7;
        public TRest Rest;

        public DatabaseTuple(T1 item1, T2 item2, T3 item3, T4 item4, T5 item5, T6 item6, T7 item7, TRest rest)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
            Item6 = item6;
            Item7 = item7;
            Rest = rest;
        }

        public void Deconstruct(out T1 item1, out T2 item2, out T3 item3, out T4 item4, out T5 item5, out T6 item6, out T7 item7, out TRest rest)
        {
            item1 = Item1;
            item2 = Item2;
            item3 = Item3;
            item4 = Item4;
            item5 = Item5;
            item6 = Item6;
            item7 = Item7;
            rest = Rest;
        }

        public static implicit operator ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(DatabaseTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value)
            => new ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7, value.Rest);

        public static implicit operator DatabaseTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(ValueTuple<T1, T2, T3, T4, T5, T6, T7, TRest> value)
            => new DatabaseTuple<T1, T2, T3, T4, T5, T6, T7, TRest>(value.Item1, value.Item2, value.Item3, value.Item4, value.Item5, value.Item6, value.Item7, value.Rest);

        public override string ToString() => DatabaseTupleConvert.ToTupleString(this);
    }

    public static class DatabaseTupleConvert
    {
        public static TBacking ToBacking<TBacking>(object value)
            where TBacking : struct
        {
            if (value == null)
            {
                return default;
            }

            object backing = CreateDatabaseTuple(typeof(TBacking), FlattenValueTuple(value, value.GetType()), 0, out _);
            return (TBacking)backing;
        }

        public static TTuple ToValueTuple<TTuple>(object backing)
            where TTuple : struct
        {
            if (backing == null)
            {
                return default;
            }

            List<object> values = FlattenDatabaseTuple(backing, backing.GetType());
            object tuple = CreateValueTuple(typeof(TTuple), values, 0, out _);
            return (TTuple)tuple;
        }

        public static string ToTupleString(object value)
        {
            if (value == null)
            {
                return "()";
            }

            List<object> values = IsDatabaseTupleType(value.GetType())
                ? FlattenDatabaseTuple(value, value.GetType())
                : FlattenValueTuple(value, value.GetType());

            var sb = new StringBuilder();
            sb.Append("(");
            for (int i = 0; i < values.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(", ");
                }

                sb.Append(FormatValue(values[i]));
            }

            sb.Append(")");
            return sb.ToString();
        }

        internal static bool IsDatabaseTupleType(Type type)
        {
            return type != null
                   && type.IsValueType
                   && type.IsGenericType
                   && type.Namespace == "Database"
                   && type.Name.StartsWith("DatabaseTuple`", StringComparison.Ordinal);
        }

        internal static List<object> FlattenDatabaseTuple(object value, Type tupleType)
        {
            var values = new List<object>();
            AppendDatabaseTupleValues(value, tupleType, values);
            return values;
        }

        internal static object CreateDatabaseTuple(Type tupleType, IList<object> values, int startIndex, out int nextIndex)
        {
            Type[] genericArguments = tupleType.GetGenericArguments();
            object[] ctorArgs;

            if (genericArguments.Length <= 7)
            {
                ctorArgs = new object[genericArguments.Length];
                CopyValues(values, startIndex, ctorArgs, genericArguments.Length);
                nextIndex = startIndex + genericArguments.Length;
            }
            else
            {
                ctorArgs = new object[8];
                CopyValues(values, startIndex, ctorArgs, 7);
                ctorArgs[7] = CreateDatabaseTuple(genericArguments[7], values, startIndex + 7, out nextIndex);
            }

            return Activator.CreateInstance(tupleType, ctorArgs);
        }

        private static List<object> FlattenValueTuple(object value, Type tupleType)
        {
            var values = new List<object>();
            AppendValueTupleValues(value, tupleType, values);
            return values;
        }

        private static object CreateValueTuple(Type tupleType, IList<object> values, int startIndex, out int nextIndex)
        {
            Type[] genericArguments = tupleType.GetGenericArguments();
            object[] ctorArgs;

            if (genericArguments.Length <= 7)
            {
                ctorArgs = new object[genericArguments.Length];
                CopyValues(values, startIndex, ctorArgs, genericArguments.Length);
                nextIndex = startIndex + genericArguments.Length;
            }
            else
            {
                ctorArgs = new object[8];
                CopyValues(values, startIndex, ctorArgs, 7);
                ctorArgs[7] = CreateValueTuple(genericArguments[7], values, startIndex + 7, out nextIndex);
            }

            return Activator.CreateInstance(tupleType, ctorArgs);
        }

        private static void AppendValueTupleValues(object value, Type tupleType, List<object> values)
        {
            for (int i = 1; i <= 7; i++)
            {
                FieldInfo itemField = tupleType.GetField("Item" + i, BindingFlags.Public | BindingFlags.Instance);
                if (itemField == null)
                {
                    break;
                }

                values.Add(itemField.GetValue(value));
            }

            FieldInfo restField = tupleType.GetField("Rest", BindingFlags.Public | BindingFlags.Instance);
            if (restField != null)
            {
                object restValue = restField.GetValue(value);
                if (restValue != null)
                {
                    AppendValueTupleValues(restValue, restField.FieldType, values);
                }
            }
        }

        private static void AppendDatabaseTupleValues(object value, Type tupleType, List<object> values)
        {
            for (int i = 1; i <= 7; i++)
            {
                FieldInfo itemField = tupleType.GetField("Item" + i, BindingFlags.Public | BindingFlags.Instance);
                if (itemField == null)
                {
                    break;
                }

                values.Add(itemField.GetValue(value));
            }

            FieldInfo restField = tupleType.GetField("Rest", BindingFlags.Public | BindingFlags.Instance);
            if (restField != null)
            {
                object restValue = restField.GetValue(value);
                if (restValue != null)
                {
                    AppendDatabaseTupleValues(restValue, restField.FieldType, values);
                }
            }
        }

        private static string FormatValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string stringValue)
            {
                return stringValue;
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return value.ToString();
        }

        private static void CopyValues(IList<object> source, int sourceIndex, object[] destination, int count)
        {
            for (int i = 0; i < count; i++)
            {
                destination[i] = source[sourceIndex + i];
            }
        }
    }
}
