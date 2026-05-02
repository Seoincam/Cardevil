using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Database.Generated;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
                Debug.LogError($"[ClassInstanceFactory] 타입을 찾을 수 없습니다: Database.Generated.{df.name}");
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

            if (IsTupleDtoType(targetType))
            {
                if (string.IsNullOrEmpty(value) || value == "null")
                {
                    return Activator.CreateInstance(targetType)!;
                }

                return JsonConvert.DeserializeObject(value, targetType)!;
            }

            if (IsValueTupleType(targetType))
            {
                return ConvertTupleValue(targetType, value, depth);
            }

            if (targetType.IsArray)
            {
                Type elementType = targetType.GetElementType()!;
                string[] elements = GetCollectionElements(value);
                elements = NormalizeTupleCollectionElements(elementType, elements);
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
                            Debug.LogError($"[ClassInstanceFactory] 배열 요소 변환 실패: {targetType.Name}, 값 '{elements[i]}', 예외: {e}");
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
                    string[] elements = GetCollectionElements(value);
                    elements = NormalizeTupleCollectionElements(elementType, elements);
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
                                Debug.LogError($"[ClassInstanceFactory] 리스트 요소 변환 실패: {targetType.Name}, 값 '{element}', 예외: {e}");
                                throw;
                            }
                        }
                    }

                    return listInstance!;
                }

                if (targetType.GetGenericTypeDefinition() == typeof(Database.ListWrapper<>))
                {
                    Type elementType = targetType.GetGenericArguments()[0];
                    string[] elements = GetCollectionElements(value);
                    elements = NormalizeTupleCollectionElements(elementType, elements);
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
                                Debug.LogError($"[ClassInstanceFactory] ListWrapper 요소 변환 실패: {targetType.Name}, 값 '{element}', 예외: {e}");
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
                Debug.LogError($"[ClassInstanceFactory] IJsonUtilitySupport 변환 실패: {targetType.Name}, {e}");
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
                Debug.LogError($"[ClassInstanceFactory] ILoadFromDatabaseString 변환 실패: {targetType.Name}, {e}");
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

            if (Attribute.IsDefined(targetType, typeof(JsonConverterAttribute)))
            {
                try
                {
                    var instance = JsonConvert.DeserializeObject(value, targetType);
                    return instance!;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ClassInstanceFactory] JsonConverter 변환 실패: {targetType.Name}, {e}");
                }
            }

            if (targetType.IsClass || targetType.IsValueType)
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
                        Debug.LogError($"[ClassInstanceFactory] Serializable 타입 변환 실패: {targetType.Name}, {e}");
                    }
                }

            }

            Debug.LogWarning($"[ClassInstanceFactory] 지원되지 않는 타입 변환 시도: {targetType.Name}, 값 '{value}'");
            return Convert.ChangeType(value, targetType, ci);
        }

        internal static string[] SplitToList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();

            value = value.Trim();

            if (IsWrappedBy(value, '[', ']'))
                value = value.Substring(1, value.Length - 2);

            if (string.IsNullOrWhiteSpace(value))
                return Array.Empty<string>();

            if (value.EndsWith(","))
                value = value.Substring(0, value.Length - 1);

            var elements = new List<string>();
            int bracketCount = 0;
            int braceCount = 0;
            int parenCount = 0;
            bool inQuote = false;
            int lastSplitIndex = 0;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (IsQuoteToggle(value, i))
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
                    else if (c == '(')
                        parenCount++;
                    else if (c == ')')
                        parenCount--;

                    if (c == ',' && bracketCount == 0 && braceCount == 0 && parenCount == 0)
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

        private static string[] GetCollectionElements(string value)
        {
            if (TrySplitJsonArray(value, out string[] jsonElements))
            {
                return jsonElements;
            }

            return SplitToList(value);
        }

        private static string[] NormalizeTupleCollectionElements(Type elementType, string[] elements)
        {
            if (!IsValueTupleType(elementType) || elements == null || elements.Length == 0)
            {
                return elements ?? Array.Empty<string>();
            }

            // ValueTuple 조각이 잘린 입력을 괄호/인용부호 균형 기준으로 재병합한다.
            var merged = new List<string>();
            var buffer = new System.Text.StringBuilder();
            int bracketCount = 0;
            int braceCount = 0;
            int parenCount = 0;
            bool inQuote = false;

            for (int i = 0; i < elements.Length; i++)
            {
                string token = (elements[i] ?? string.Empty).Trim();
                if (buffer.Length > 0)
                {
                    buffer.Append(',');
                }

                buffer.Append(token);
                UpdateDepthState(token, ref bracketCount, ref braceCount, ref parenCount, ref inQuote);

                if (!inQuote && bracketCount == 0 && braceCount == 0 && parenCount == 0)
                {
                    merged.Add(buffer.ToString().Trim());
                    buffer.Clear();
                }
            }

            if (buffer.Length > 0)
            {
                merged.Add(buffer.ToString().Trim());
            }

            return merged.ToArray();
        }

        private static bool TrySplitJsonArray(string value, out string[] elements)
        {
            elements = null;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string trimmed = value.Trim();
            if (!IsWrappedBy(trimmed, '[', ']'))
            {
                return false;
            }

            try
            {
                JArray array = JArray.Parse(trimmed);
                elements = array
                    .Select(ToCollectionElementString)
                    .ToArray();
                return true;
            }
            catch (JsonReaderException)
            {
                return false;
            }
        }

        private static string ToCollectionElementString(JToken token)
        {
            if (token == null || token.Type == JTokenType.Null)
            {
                return string.Empty;
            }

            return token.Type == JTokenType.String
                ? token.Value<string>()
                : token.ToString(Formatting.None);
        }

        internal static string[] SplitToKV(string entry)
        {
            int bracketCount = 0;
            int braceCount = 0;
            int parenCount = 0;
            bool inQuote = false;

            for (int i = 0; i < entry.Length; i++)
            {
                char c = entry[i];
                if (IsQuoteToggle(entry, i))
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
                    else if (c == '(')
                        parenCount++;
                    else if (c == ')')
                        parenCount--;

                    if (c == ':' && bracketCount == 0 && braceCount == 0 && parenCount == 0)
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

        private static bool IsValueTupleType(Type targetType)
        {
            return targetType.IsValueType
                   && targetType.IsGenericType
                   && targetType.FullName != null
                   && targetType.FullName.StartsWith("System.ValueTuple`", StringComparison.Ordinal);
        }

        private static bool IsTupleDtoType(Type targetType)
        {
            if (!Attribute.IsDefined(targetType, typeof(JsonConverterAttribute)))
            {
                return false;
            }

            int tupleFieldCount = targetType
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Count(field => field.Name.StartsWith("Item", StringComparison.Ordinal));

            return tupleFieldCount >= 1;
        }

        private static object ConvertTupleValue(Type targetType, string value, int depth)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "null")
            {
                return Activator.CreateInstance(targetType)!;
            }

            string tuplePayload = NormalizeTupleLiteral(value);
            if (string.IsNullOrWhiteSpace(tuplePayload))
            {
                return Activator.CreateInstance(targetType)!;
            }

            string[] elements = SplitToList(tuplePayload);
            List<Type> tupleTypes = GetTupleElementTypes(targetType);

            if (elements.Length != tupleTypes.Count)
            {
                throw new FormatException(
                    $"Tuple element count mismatch for {targetType}: expected {tupleTypes.Count}, got {elements.Length}. Value='{value}'");
            }

            object[] converted = new object[tupleTypes.Count];
            for (int i = 0; i < tupleTypes.Count; i++)
            {
                converted[i] = ConvertValue(tupleTypes[i], elements[i], depth + 1);
            }

            return BuildTupleInstance(targetType, converted, 0);
        }

        private static string NormalizeTupleLiteral(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            string trimmed = UnwrapQuotedValue(value.Trim());
            if (IsWrappedBy(trimmed, '(', ')'))
            {
                return trimmed.Substring(1, trimmed.Length - 2);
            }

            if (IsWrappedBy(trimmed, '[', ']'))
            {
                return trimmed.Substring(1, trimmed.Length - 2);
            }

            return trimmed;
        }

        private static string UnwrapQuotedValue(string value)
        {
            string trimmed = value;
            while (trimmed.Length >= 2
                   && trimmed[0] == '"'
                   && trimmed[trimmed.Length - 1] == '"'
                   && !IsEscaped(trimmed, trimmed.Length - 1))
            {
                trimmed = trimmed.Substring(1, trimmed.Length - 2).Trim();
            }

            return trimmed;
        }

        private static bool IsWrappedBy(string value, char start, char end)
        {
            return value.Length >= 2 && value[0] == start && value[^1] == end;
        }

        private static bool IsEscaped(string value, int index)
        {
            int backslashCount = 0;
            for (int i = index - 1; i >= 0 && value[i] == '\\'; i--)
            {
                backslashCount++;
            }

            return (backslashCount % 2) == 1;
        }

        private static bool IsQuoteToggle(string value, int index)
        {
            return value[index] == '"' && !IsEscaped(value, index);
        }

        private static void UpdateDepthState(
            string value,
            ref int bracketCount,
            ref int braceCount,
            ref int parenCount,
            ref bool inQuote)
        {
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (IsQuoteToggle(value, i))
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (inQuote)
                {
                    continue;
                }

                if (c == '[')
                    bracketCount++;
                else if (c == ']')
                    bracketCount--;
                else if (c == '{')
                    braceCount++;
                else if (c == '}')
                    braceCount--;
                else if (c == '(')
                    parenCount++;
                else if (c == ')')
                    parenCount--;
            }
        }

        private static List<Type> GetTupleElementTypes(Type tupleType)
        {
            var elementTypes = new List<Type>();
            AppendTupleElementTypes(tupleType, elementTypes);
            return elementTypes;
        }

        private static void AppendTupleElementTypes(Type tupleType, List<Type> elementTypes)
        {
            Type[] genericArguments = tupleType.GetGenericArguments();
            if (genericArguments.Length <= 7)
            {
                elementTypes.AddRange(genericArguments);
                return;
            }

            for (int i = 0; i < 7; i++)
            {
                elementTypes.Add(genericArguments[i]);
            }

            AppendTupleElementTypes(genericArguments[7], elementTypes);
        }

        private static object BuildTupleInstance(Type tupleType, object[] values, int startIndex)
        {
            Type[] genericArguments = tupleType.GetGenericArguments();
            object[] ctorArgs;

            if (genericArguments.Length <= 7)
            {
                ctorArgs = new object[genericArguments.Length];
                Array.Copy(values, startIndex, ctorArgs, 0, genericArguments.Length);
            }
            else
            {
                ctorArgs = new object[8];
                Array.Copy(values, startIndex, ctorArgs, 0, 7);
                ctorArgs[7] = BuildTupleInstance(genericArguments[7], values, startIndex + 7);
            }

            return Activator.CreateInstance(tupleType, ctorArgs)!;
        }
    }
}
