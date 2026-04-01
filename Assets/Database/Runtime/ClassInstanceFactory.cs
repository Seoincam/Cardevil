using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Database.Generated;
using Newtonsoft.Json;
using UnityEngine;

namespace Database
{
    public class ClassInstanceFactory
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldCache =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static object[] CreateInstance(DataFrame df)
        {
            var type = ReflectionUtil.FindTypeByFullName("Database.Generated." + df.name);
            if (type == null)
            {
                Debug.LogError($"[ClassInstanceFactory] ??낆쓣 李얠쓣 ???놁뒿?덈떎: Database.Generated.{df.name}");
                return Array.Empty<object>();
            }

            var fieldMap = GetFieldMap(type);
            int maxRow = df.data?.Length ?? 0;
            object[] instances = new object[maxRow];

            for (int i = 0; i < maxRow; i++)
            {
                var instance = Activator.CreateInstance(type);
                var row = df.data[i];

                for (int j = 0; j < row.Length; j++)
                {
                    var value = row[j];
                    var varName = df.GetVarName(j);

                    if (!fieldMap.TryGetValue(varName, out var field))
                        continue;

                    Type varType = field.FieldType;
                    object convertedValue = ConvertValue(varType, value);
                    field.SetValue(instance, convertedValue);
                }

                instances[i] = instance;
            }

            return instances;
        }

        private static Dictionary<string, FieldInfo> GetFieldMap(Type type)
        {
            if (FieldCache.TryGetValue(type, out var fieldMap))
                return fieldMap;

            fieldMap = type
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .ToDictionary(field => field.Name, StringComparer.Ordinal);
            FieldCache[type] = fieldMap;
            return fieldMap;
        }

        public static object ConvertValue(Type targetType, string value, int depth = 0)
        {
            value = value?.Trim() ?? string.Empty;

            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType()!;
                string[] elements = SplitToList(value);
                Array arrayInstance = Array.CreateInstance(elementType, elements.Length);

                for (int i = 0; i < elements.Length; i++)
                {
                    try
                    {
                        object convertedValue = ConvertValue(elementType, elements[i], depth + 1);
                        arrayInstance.SetValue(convertedValue, i);
                    }
                    catch (Exception e)
                    {
                        if (i == elements.Length - 1 && string.IsNullOrEmpty(elements[i]))
                        {
                            Array newArrayInstance = Array.CreateInstance(elementType, elements.Length - 1);
                            Array.Copy(arrayInstance, newArrayInstance, elements.Length - 1);
                            return newArrayInstance;
                        }
                        else
                        {
                            Debug.LogError($"[ClassInstanceFactory] 諛곗뿴 ?붿냼 蹂???ㅽ뙣: {targetType.Name}, 媛? '{elements[i]}', ?덉쇅: {e}");
                            throw;
                        }
                    }
                }

                return arrayInstance;
            }

            if (targetType.IsGenericType)
            {
                if (targetType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    Type elementType = targetType.GetGenericArguments()[0];
                    string[] elements = SplitToList(value);
                    var listInstance = Activator.CreateInstance(targetType);
                    var addMethod = targetType.GetMethod("Add");
                    foreach (string element in elements)
                    {
                        try
                        {
                            object convertedValue;
                            if (elementType == typeof(string) && element.StartsWith("\"") && element.EndsWith("\"") && element.Length >= 2)
                            {
                                convertedValue = ConvertValue(elementType, element.Substring(1, element.Length - 2), depth + 1);
                            }
                            else
                            {
                                convertedValue = ConvertValue(elementType, element, depth + 1);
                            }

                            addMethod!.Invoke(listInstance, new[] { convertedValue });
                        }
                        catch (Exception e)
                        {
                            if (element == elements[elements.Length - 1] && string.IsNullOrEmpty(element))
                            {
                                continue;
                            }
                            else
                            {
                                Debug.LogError($"[ClassInstanceFactory] 由ъ뒪???붿냼 蹂???ㅽ뙣: {targetType.Name}, 媛? '{element}', ?덉쇅: {e}");
                                throw;
                            }
                        }
                    }

                    return listInstance!;
                }

                if (targetType.GetGenericTypeDefinition() == typeof(Database.ListWrapper<>))
                {
                    Type elementType = targetType.GetGenericArguments()[0];
                    string[] elements = SplitToList(value);
                    var listWrapperInstance = Activator.CreateInstance(targetType);
                    var addMethod = targetType.GetMethod("Add");
                    foreach (var element in elements)
                    {
                        try
                        {
                            object convertedValue = ConvertValue(elementType, element, depth + 1);
                            addMethod!.Invoke(listWrapperInstance, new[] { convertedValue });
                        }
                        catch (Exception e)
                        {
                            if (element == elements[elements.Length - 1] && string.IsNullOrEmpty(element))
                            {
                                continue;
                            }
                            else
                            {
                                Debug.LogError($"[ClassInstanceFactory] 由ъ뒪?몃옒???붿냼 蹂???ㅽ뙣: {targetType.Name}, 媛? '{element}', ?덉쇅: {e}");
                                throw;
                            }
                        }
                    }

                    return listWrapperInstance!;
                }
            }

            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                Type keyType = targetType.GetGenericArguments()[0];
                Type valueType = targetType.GetGenericArguments()[1];

                var dictInstance = Activator.CreateInstance(targetType);
                var addMethod = targetType.GetMethod("Add");

                value = value.Trim();
                if (value.StartsWith("{") && value.EndsWith("}"))
                    value = value.Substring(1, value.Length - 2);

                string[] entries = SplitToList(value);

                foreach (string entry in entries)
                {
                    if (string.IsNullOrWhiteSpace(entry))
                        continue;

                    string[] kv = SplitToKV(entry);
                    if (kv.Length != 2)
                    {
                        Debug.LogWarning($"[ClassInstanceFactory] Dictionary parse error: invalid entry '{entry}'");
                        continue;
                    }

                    try
                    {
                        object keyObj = ConvertValue(keyType, kv[0], depth + 1);
                        object valObj = ConvertValue(valueType, kv[1], depth + 1);
                        addMethod!.Invoke(dictInstance, new[] { keyObj, valObj });
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ClassInstanceFactory] Dictionary Add Failed: {entry} / {e}");
                    }
                }

                return dictInstance!;
            }

            if (targetType.IsEnum)
            {
                if (string.IsNullOrEmpty(value))
                    return targetType.GetEnumValues().GetValue(0)!;
                if (int.TryParse(value, out int enumInt))
                    return Enum.ToObject(targetType, enumInt);
                if (Enum.TryParse(targetType, value, true, out object enumValue))
                    return enumValue;
                Debug.LogWarning($"[ClassInstanceFactory] Enum parse failed: {targetType.Name}, '{value}'");
                return targetType.GetEnumValues().GetValue(0)!;
            }

            if (targetType == typeof(string))
                return value;

            if (targetType == typeof(bool))
            {
                if (value == "0")
                    return false;
                if (value == "1")
                    return true;
                if (bool.TryParse(value, out bool booleanValue))
                    return booleanValue;
                return false;
            }

            if (string.IsNullOrEmpty(value))
                value = "0";
            else if (value == "null")
                value = "0";

            var ci = CultureInfo.InvariantCulture;

            if (targetType == typeof(int))
                return int.Parse(value, ci);
            if (targetType == typeof(float))
                return float.Parse(value, ci);
            if (targetType == typeof(double))
                return double.Parse(value, ci);
            if (targetType == typeof(long))
                return long.Parse(value, ci);
            if (targetType == typeof(uint))
                return uint.Parse(value, ci);
            if (targetType == typeof(ulong))
                return ulong.Parse(value, ci);
            if (targetType == typeof(short))
                return short.Parse(value, ci);
            if (targetType == typeof(ushort))
                return ushort.Parse(value, ci);
            if (targetType == typeof(byte))
                return byte.Parse(value, ci);
            if (targetType == typeof(sbyte))
                return sbyte.Parse(value, ci);
            if (targetType == typeof(decimal))
                return decimal.Parse(value, ci);

            try
            {
                Type jsonUtilType = typeof(IJsonUtilitySupport);
                if (jsonUtilType.IsAssignableFrom(targetType))
                {
                    var instance = JsonConvert.DeserializeObject(value, targetType);
                    return instance;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClassInstanceFactory] IJsonUtilitySupport 蹂???ㅽ뙣: {targetType.Name}, {e}");
            }

            try
            {
                Type loadableType = typeof(ILoadFromDatabaseString);
                if (loadableType.IsAssignableFrom(targetType))
                {
                    var instance = Activator.CreateInstance(targetType) as ILoadFromDatabaseString;
                    instance!.LoadFromDatabaseString(value);
                    return instance;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[ClassInstanceFactory] ILoadFromDatabaseString 蹂???ㅽ뙣: {targetType.Name}, {e}");
            }

            if (targetType == typeof(DateTime))
            {
                if (DateTime.TryParse(value, ci, DateTimeStyles.None, out DateTime dateTime))
                    return dateTime;
                return DateTime.MinValue;
            }

            if (targetType == typeof(TimeSpan))
            {
                if (TimeSpan.TryParse(value, ci, out TimeSpan timeSpan))
                    return timeSpan;
                return TimeSpan.Zero;
            }

            if (targetType.IsClass)
            {
                if (Attribute.IsDefined(targetType, typeof(SerializableAttribute)))
                {
                    try
                    {
                        var instance = JsonConvert.DeserializeObject(value, targetType);
                        return instance;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ClassInstanceFactory] Serializable ?대옒??蹂???ㅽ뙣: {targetType.Name}, {e}");
                    }
                }

                if (Attribute.IsDefined(targetType, typeof(JsonConverterAttribute)))
                {
                    try
                    {
                        var instance = JsonConvert.DeserializeObject(value, targetType);
                        return instance!;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ClassInstanceFactory] JsonConverter ?대옒??蹂???ㅽ뙣: {targetType.Name}, {e}");
                    }
                }
            }

            Debug.LogWarning($"[ClassInstanceFactory] ?????녿뒗 ???蹂???쒕룄: {targetType.Name}, 媛? '{value}'");
            return Convert.ChangeType(value, targetType, ci);
        }

        internal static string[] SplitToList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();

            value = value.Trim();

            if (value.StartsWith("[") && value.EndsWith("]"))
                value = value.Substring(1, value.Length - 2);

            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();

            if (value.EndsWith(","))
                value = value.Substring(0, value.Length - 1);

            var elements = new List<string>();
            int bracketCount = 0;
            int braceCount = 0;
            bool inQuote = false;
            int lastSplitIndex = 0;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (c == '"')
                    inQuote = !inQuote;

                if (!inQuote)
                {
                    if (c == '[')
                        bracketCount++;
                    else if (c == ']')
                        bracketCount--;
                    else if (c == '{')
                        braceCount++;
                    else if (c == '}')
                        braceCount--;

                    if (c == ',' && bracketCount == 0 && braceCount == 0)
                    {
                        elements.Add(value.Substring(lastSplitIndex, i - lastSplitIndex).Trim());
                        lastSplitIndex = i + 1;
                    }
                }
            }

            if (lastSplitIndex <= value.Length)
                elements.Add(value.Substring(lastSplitIndex).Trim());

            return elements.ToArray();
        }

        internal static string[] SplitToKV(string entry)
        {
            int bracketCount = 0;
            int braceCount = 0;
            bool inQuote = false;

            for (int i = 0; i < entry.Length; i++)
            {
                char c = entry[i];
                if (c == '"')
                    inQuote = !inQuote;

                if (!inQuote)
                {
                    if (c == '[')
                        bracketCount++;
                    else if (c == ']')
                        bracketCount--;
                    else if (c == '{')
                        braceCount++;
                    else if (c == '}')
                        braceCount--;

                    if (c == ':' && bracketCount == 0 && braceCount == 0)
                    {
                        return new[]
                        {
                            entry.Substring(0, i).Trim(),
                            entry.Substring(i + 1).Trim()
                        };
                    }
                }
            }

            return Array.Empty<string>();
        }
    }
}
