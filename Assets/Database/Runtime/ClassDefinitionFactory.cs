using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Database
{
    public static class ClassDefinitionFactory
    {
        private static readonly HashSet<string> KnownTypes = new HashSet<string>
        {
            "string", "bool", "byte", "sbyte",
            "short", "ushort", "int", "uint",
            "long", "ulong", "float", "double", "decimal"
        };

        private static readonly HashSet<string> KnownEnumTypes = new HashSet<string>
        {
            "Cardevil.Utils.Directions.Direction",
            "Define.RareType",
            "Define.SlotRewardType",
            "Cardevil.Cards.Evaluations.HandRanking",
            "Cardevil.Relics.EffectExcute",
            "Cardevil.Relics.EffectEvaluation"
        };

        public static string GenerateClassDefinition(DataFrame df)
        {
            StringBuilder usingSb = new StringBuilder();
            usingSb.AppendLine("using System.Text;")
                .AppendLine("using System;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace Database.Generated")
                .AppendLine("{")
                .AppendLine();
            sb.AppendLine("    [UnityEngine.Scripting.Preserve]")
                .AppendLine("    [Serializable]")
                .Append("    public partial class ")
                .Append(df.name)
                .AppendLine(": IDBData {")
                .AppendLine();

            for (int i = 0; i < df.MaxColumn; i++)
            {
                string type = df.types[i];
                if (string.IsNullOrEmpty(type))
                {
                    continue;
                }

                string tl = type.Trim().ToLower();
                if (tl == "null" || tl == "comment")
                {
                    continue;
                }

                sb.Append(GetVariableDefinition(df.varNames[i], type, df.comments[i], 2));
            }

            sb.AppendLine("    }")
                .AppendLine("}");

            return usingSb + sb.ToString();
        }

        private static StringBuilder GetVariableDefinition(string name, string type, string comment = null, int indent = 0)
        {
            StringBuilder indentSb = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                indentSb.Append("    ");
            }

            string finalType = DecideType(type, out bool isKnown);
            string finalComment = comment;
            if (!isKnown)
            {
                finalComment = $"(Reference:{type}) \n {indentSb}/// {comment ?? ""}";
            }

            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(finalComment))
            {
                sb.Append(indentSb)
                    .Append("/// <summary> ")
                    .Append(finalComment)
                    .AppendLine(" </summary>");
            }

            sb.Append(indentSb)
                .Append("public ")
                .Append(finalType)
                .Append(" ")
                .Append(name)
                .AppendLine(";");

            return sb;
        }

        private static string DecideType(string original, out bool isKnown, int depth = 0)
        {
            isKnown = false;
            if (string.IsNullOrEmpty(original))
            {
                return "string";
            }

            string typeName = original.Trim();
            string typeNameLower = typeName.ToLower();

            if (IsKnown(typeNameLower))
            {
                isKnown = true;
                return typeNameLower;
            }

            if (typeNameLower.StartsWith("enum<") && typeNameLower.EndsWith(">"))
            {
                string enumName = typeName.Substring(5, typeName.Length - 6).Trim();

                if (TryResolveTypeReference(enumName, static type => type.IsEnum, out var resolvedEnumName))
                {
                    isKnown = true;
                    return resolvedEnumName;
                }

                if (KnownEnumTypes.Contains(enumName) || LooksQualified(enumName))
                {
                    isKnown = true;
                    return NormalizeTypeName(enumName);
                }

                return "string";
            }

            if (typeNameLower.StartsWith("class<") && typeNameLower.EndsWith(">"))
            {
                string className = typeName.Substring(6, typeName.Length - 7).Trim();

                if (TryResolveTypeReference(className, CheckClass, out var resolvedClassName))
                {
                    isKnown = true;
                    return resolvedClassName;
                }

                if (LooksQualified(className))
                {
                    isKnown = true;
                    return NormalizeTypeName(className);
                }
            }

            if (typeNameLower.StartsWith("list<") && typeNameLower.EndsWith(">"))
            {
                string inner = typeName.Substring(5, typeName.Length - 6).Trim();
                string determinedInnerType = DecideType(inner, out bool innerIsKnown, depth + 1);

                if (innerIsKnown)
                {
                    isKnown = true;
                    return "List<" + determinedInnerType + ">";
                }

                return "string";
            }

            if (typeNameLower.StartsWith("dictionary<") && typeNameLower.EndsWith(">"))
            {
                string inner = typeName.Substring(11, typeName.Length - 12).Trim();

                int splitIndex = -1;
                int depthCount = 0;
                for (int i = 0; i < inner.Length; i++)
                {
                    if (inner[i] == '<')
                    {
                        depthCount++;
                    }
                    else if (inner[i] == '>')
                    {
                        depthCount--;
                    }
                    else if (inner[i] == ',' && depthCount == 0)
                    {
                        splitIndex = i;
                        break;
                    }
                }

                if (splitIndex != -1)
                {
                    string keyTypeRaw = inner.Substring(0, splitIndex).Trim();
                    string valueTypeRaw = inner.Substring(splitIndex + 1).Trim();

                    string keyType = DecideType(keyTypeRaw, out bool keyIsKnown, depth + 1);
                    string valueType = DecideType(valueTypeRaw, out bool valueIsKnown, depth + 1);

                    if (keyIsKnown && valueIsKnown)
                    {
                        isKnown = true;
                        return $"System.Collections.Generic.Dictionary<{keyType}, {valueType}>";
                    }
                }

                return "string";
            }

            return "string";
        }

        private static bool TryResolveTypeReference(string typeReference, Func<Type, bool> predicate, out string resolvedTypeName)
        {
            resolvedTypeName = null;
            if (string.IsNullOrWhiteSpace(typeReference))
            {
                return false;
            }

            var normalizedReference = NormalizeTypeName(typeReference);
            var candidates = new List<string> { normalizedReference };
            var leafName = GetLeafTypeName(normalizedReference);
            if (!string.Equals(leafName, normalizedReference, StringComparison.Ordinal))
            {
                candidates.Add(leafName);
            }

            foreach (var candidate in candidates)
            {
                var resolvedType = ReflectionUtil.FindTypeByFullName(candidate, false)
                    ?? ReflectionUtil.FindTypeByName(candidate, false)
                    ?? ReflectionUtil.FindTypeByFullName(candidate)
                    ?? ReflectionUtil.FindTypeByName(candidate);

                if (resolvedType == null || !predicate(resolvedType) || string.IsNullOrWhiteSpace(resolvedType.FullName))
                {
                    continue;
                }

                resolvedTypeName = NormalizeTypeName(resolvedType.FullName);
                return true;
            }

            return false;
        }

        private static bool CheckClass(Type type)
        {
            if (ReflectionUtil.GetAttribute<SerializableAttribute>(type) is SerializableAttribute)
            {
                return true;
            }

            if (ReflectionUtil.GetAttribute<JsonConverterAttribute>(type) is JsonConverterAttribute)
            {
                return true;
            }

            return false;
        }

        private static bool IsKnown(string tl)
        {
            return KnownTypes.Contains(tl);
        }

        private static bool LooksQualified(string typeName)
        {
            return !string.IsNullOrWhiteSpace(typeName) && typeName.Contains(".");
        }

        private static string NormalizeTypeName(string typeName)
        {
            return string.IsNullOrWhiteSpace(typeName) ? typeName : typeName.Trim().Replace("+", ".");
        }

        private static string GetLeafTypeName(string typeReference)
        {
            if (string.IsNullOrWhiteSpace(typeReference))
            {
                return typeReference;
            }

            int lastDotIndex = typeReference.LastIndexOf('.');
            return lastDotIndex >= 0 && lastDotIndex < typeReference.Length - 1
                ? typeReference.Substring(lastDotIndex + 1)
                : typeReference;
        }
    }
}
