using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var tupleFields = new List<TupleFieldDefinition>();

            StringBuilder usingSb = new StringBuilder();
            usingSb.AppendLine("using System.Text;")
                .AppendLine("using System;")
                .AppendLine("using System.Collections.Generic;")
                .AppendLine("using System.Runtime.Serialization;")
                .AppendLine("using Newtonsoft.Json;")
                .AppendLine("using UnityEngine;")
                .AppendLine();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("namespace Database.Generated")
                .AppendLine("{")
                .AppendLine();

            sb.AppendLine("    [UnityEngine.Scripting.Preserve]")
                .AppendLine("    [Serializable]")
                .Append("    public partial class ")
                .Append(df.name)
                .AppendLine(": IDBData")
                .AppendLine("    {")
                .AppendLine();

            for (int i = 0; i < df.MaxColumn; i++)
            {
                string type = df.types[i];
                if (string.IsNullOrEmpty(type))
                {
                    continue;
                }

                string tl = type.Trim().ToLowerInvariant();
                if (tl == "null" || tl == "comment")
                {
                    continue;
                }

                TupleFieldDefinition tupleField = TryCreateTupleFieldDefinition(df.name, df.varNames[i], type);
                if (tupleField != null)
                {
                    tupleFields.Add(tupleField);
                    sb.Append(GetTuplePublicFieldDefinition(tupleField, df.comments[i], 2));
                    continue;
                }

                sb.Append(GetVariableDefinition(df.varNames[i], type, df.comments[i], 2));
            }

            sb.AppendLine("    }")
                .AppendLine();

            if (tupleFields.Count > 0)
            {
                sb.Append(GetTupleSerializationPartial(df.name, tupleFields));
            }

            sb.AppendLine("}");
            return usingSb + sb.ToString();
        }

        private static StringBuilder GetVariableDefinition(string name, string type, string comment = null, int indent = 0)
        {
            StringBuilder indentSb = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                indentSb.Append("    ");
            }

            string finalType = ResolveType(type, TypeResolutionMode.Public, out bool isKnown);
            string finalComment = comment;
            if (!isKnown)
            {
                finalComment = $"(Reference:{type}) \n {indentSb}/// {comment ?? ""}";
            }

            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(finalComment))
            {
                sb.Append(indentSb)
                    .Append("/// <summary> ");
                var commetLines = finalComment.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                if(commetLines.Length == 1)
                {
                    sb.Append(finalComment.Trim())
                        .AppendLine(" </summary>");
                }
                else
                {
                    sb.AppendLine();
                    foreach (var line in commetLines)
                    {
                        sb.Append(indentSb).Append("/// ");
                        sb.Append("<br/>");
                        sb.AppendLine(line.Trim());
                    }
                    sb.Append(indentSb).Append("/// ");
                    sb.AppendLine(" </summary>");
                }
                
            }

            sb.Append(indentSb)
                .Append("public ")
                .Append(finalType)
                .Append(" ")
                .Append(name)
                .AppendLine(";");

            return sb;
        }

        private static StringBuilder GetTuplePublicFieldDefinition(TupleFieldDefinition tupleField, string comment, int indent)
        {
            StringBuilder indentSb = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                indentSb.Append("    ");
            }

            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(comment))
            {
                sb.Append(indentSb)
                    .Append("/// <summary> ")
                    .Append(comment)
                    .AppendLine(" </summary>");
            }

            sb.Append(indentSb).AppendLine("[NonSerialized]");
            sb.Append(indentSb).AppendLine("[JsonIgnore]");
            sb.Append(indentSb)
                .Append("public ")
                .Append(tupleField.PublicTypeName)
                .Append(" ")
                .Append(tupleField.FieldName)
                .AppendLine(";");
            return sb;
        }

        private static StringBuilder GetTupleSerializationPartial(string className, List<TupleFieldDefinition> tupleFields)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("    public partial class ")
                .Append(className)
                .AppendLine(" : ISerializationCallbackReceiver")
                .AppendLine("    {")
                .AppendLine();

            foreach (var tupleField in tupleFields)
            {
                sb.Append("        [SerializeField, JsonProperty(\"")
                    .Append(tupleField.FieldName)
                    .Append("\"), InspectorName(\"")
                    .Append(tupleField.FieldName)
                    .Append("\")]")
                    .AppendLine();
                sb.Append("        private ")
                    .Append(tupleField.BackingTypeName)
                    .Append(" ")
                    .Append(tupleField.BackingFieldName);

                if (tupleField.IsTupleList)
                {
                    sb.Append(" = new ")
                        .Append(tupleField.BackingTypeName)
                        .Append("();");
                }
                else
                {
                    sb.Append(";");
                }

                sb.AppendLine().AppendLine();
            }

            sb.AppendLine("        public void OnBeforeSerialize()");
            sb.AppendLine("        {");
            sb.AppendLine("            SyncTupleFieldsToSerializedBacking();");
            sb.AppendLine("        }")
                .AppendLine();
            sb.AppendLine("        public void OnAfterDeserialize()");
            sb.AppendLine("        {");
            sb.AppendLine("            SyncTupleFieldsFromSerializedBacking();");
            sb.AppendLine("        }")
                .AppendLine();
            sb.AppendLine("        [OnSerializing]");
            sb.AppendLine("        private void OnJsonSerializing(StreamingContext context)");
            sb.AppendLine("        {");
            sb.AppendLine("            SyncTupleFieldsToSerializedBacking();");
            sb.AppendLine("        }")
                .AppendLine();
            sb.AppendLine("        [OnDeserialized]");
            sb.AppendLine("        private void OnJsonDeserialized(StreamingContext context)");
            sb.AppendLine("        {");
            sb.AppendLine("            SyncTupleFieldsFromSerializedBacking();");
            sb.AppendLine("        }")
                .AppendLine();

            sb.AppendLine("        private void SyncTupleFieldsToSerializedBacking()");
            sb.AppendLine("        {");
            foreach (var tupleField in tupleFields)
            {
                if (tupleField.IsTupleList)
                {
                    sb.Append("            ")
                        .Append(tupleField.BackingFieldName)
                        .Append(" = ")
                        .Append(tupleField.FieldName)
                        .Append("?.ConvertAll(item => DatabaseTupleConvert.ToBacking<")
                        .Append(tupleField.BackingElementTypeName)
                        .Append(">(item)) ?? new ")
                        .Append(tupleField.BackingTypeName)
                        .Append("();")
                        .AppendLine();
                }
                else
                {
                    sb.Append("            ")
                        .Append(tupleField.BackingFieldName)
                        .Append(" = DatabaseTupleConvert.ToBacking<")
                        .Append(tupleField.BackingTypeName)
                        .Append(">(")
                        .Append(tupleField.FieldName)
                        .Append(");")
                        .AppendLine();
                }
            }
            sb.AppendLine("        }")
                .AppendLine();

            sb.AppendLine("        private void SyncTupleFieldsFromSerializedBacking()");
            sb.AppendLine("        {");
            foreach (var tupleField in tupleFields)
            {
                if (tupleField.IsTupleList)
                {
                    sb.Append("            ")
                        .Append(tupleField.FieldName)
                        .Append(" = ")
                        .Append(tupleField.BackingFieldName)
                        .Append("?.ConvertAll(item => DatabaseTupleConvert.ToValueTuple<")
                        .Append(tupleField.PublicElementTypeName)
                        .Append(">(item)) ?? new ")
                        .Append(tupleField.PublicTypeName)
                        .Append("();")
                        .AppendLine();
                }
                else
                {
                    sb.Append("            ")
                        .Append(tupleField.FieldName)
                        .Append(" = DatabaseTupleConvert.ToValueTuple<")
                        .Append(tupleField.PublicTypeName)
                        .Append(">(")
                        .Append(tupleField.BackingFieldName)
                        .Append(");")
                        .AppendLine();
                }
            }
            sb.AppendLine("        }");

            sb.AppendLine("    }")
                .AppendLine();
            return sb;
        }

        private static TupleFieldDefinition TryCreateTupleFieldDefinition(string ownerClassName, string fieldName, string originalType)
        {
            if (TryParseTupleType(originalType, out var tupleTypes))
            {
                string publicType = ResolveTupleType(tupleTypes, TypeResolutionMode.Public, out bool publicKnown);
                string backingType = ResolveTupleType(tupleTypes, TypeResolutionMode.Backing, out bool backingKnown);
                if (publicKnown && backingKnown)
                {
                    return TupleFieldDefinition.CreateSingle(fieldName, publicType, backingType);
                }

                return null;
            }

            if (TryGetListElementType(originalType, out string listElementType)
                && TryParseTupleType(listElementType, out var listTupleTypes))
            {
                string publicElementType = ResolveTupleType(listTupleTypes, TypeResolutionMode.Public, out bool publicKnown);
                string backingElementType = ResolveTupleType(listTupleTypes, TypeResolutionMode.Backing, out bool backingKnown);
                if (publicKnown && backingKnown)
                {
                    return TupleFieldDefinition.CreateList(
                        fieldName,
                        $"List<{publicElementType}>",
                        $"List<{backingElementType}>",
                        publicElementType,
                        backingElementType);
                }
            }

            return null;
        }

        private static string ResolveType(string original, TypeResolutionMode mode, out bool isKnown, int depth = 0)
        {
            isKnown = false;
            if (string.IsNullOrEmpty(original))
            {
                return "string";
            }

            string typeName = original.Trim();
            string typeNameLower = typeName.ToLowerInvariant();

            if (IsKnown(typeNameLower))
            {
                isKnown = true;
                return typeNameLower;
            }

            if (TryParseTupleType(typeName, out var tupleTypes))
            {
                string tupleType = ResolveTupleType(tupleTypes, mode, out bool tupleKnown);
                isKnown = tupleKnown;
                return tupleType;
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

                return "string";
            }

            if (TryGetListElementType(typeName, out string inner))
            {
                string resolvedInner = ResolveType(inner, mode, out bool innerKnown, depth + 1);
                if (innerKnown)
                {
                    isKnown = true;
                    return $"List<{resolvedInner}>";
                }

                return "string";
            }

            if (typeNameLower.StartsWith("dictionary<") && typeNameLower.EndsWith(">"))
            {
                string inner2 = ExtractGenericInner(typeName, "Dictionary");
                List<string> typeArguments = SplitTopLevel(inner2, ',');
                if (typeArguments.Count == 2)
                {
                    string keyType = ResolveType(typeArguments[0], mode, out bool keyKnown, depth + 1);
                    string valueType = ResolveType(typeArguments[1], mode, out bool valueKnown, depth + 1);

                    if (keyKnown && valueKnown)
                    {
                        isKnown = true;
                        return $"System.Collections.Generic.Dictionary<{keyType}, {valueType}>";
                    }
                }

                return "string";
            }

            return "string";
        }

        private static string ResolveTupleType(List<string> tupleTypes, TypeResolutionMode mode, out bool isKnown)
        {
            isKnown = false;
            if (tupleTypes == null || tupleTypes.Count < 2)
            {
                return "string";
            }

            var resolved = new List<string>(tupleTypes.Count);
            foreach (var tupleType in tupleTypes)
            {
                string resolvedType = ResolveType(tupleType, mode, out bool itemKnown);
                if (!itemKnown)
                {
                    return "string";
                }

                resolved.Add(resolvedType);
            }

            isKnown = true;
            if (mode == TypeResolutionMode.Public)
            {
                return $"({string.Join(", ", resolved)})";
            }

            return ResolveBackingTupleType(resolved, 0);
        }

        private static string ResolveBackingTupleType(List<string> resolvedTypes, int startIndex)
        {
            int remainingCount = resolvedTypes.Count - startIndex;
            if (remainingCount <= 7)
            {
                return $"DatabaseTuple<{string.Join(", ", resolvedTypes.Skip(startIndex))}>";
            }

            var currentTypes = resolvedTypes.Skip(startIndex).Take(7).ToList();
            currentTypes.Add(ResolveBackingTupleType(resolvedTypes, startIndex + 7));
            return $"DatabaseTuple<{string.Join(", ", currentTypes)}>";
        }

        private static bool TryGetListElementType(string typeName, out string listElementType)
        {
            listElementType = null;
            string trimmed = typeName?.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)
                || !trimmed.StartsWith("List<", StringComparison.OrdinalIgnoreCase)
                || !trimmed.EndsWith(">"))
            {
                return false;
            }

            listElementType = ExtractGenericInner(trimmed, "List");
            return true;
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

        private static bool TryParseTupleType(string typeName, out List<string> tupleTypeNames)
        {
            tupleTypeNames = null;

            if (string.IsNullOrWhiteSpace(typeName))
            {
                return false;
            }

            string trimmed = typeName.Trim();
            if (trimmed.StartsWith("(") && trimmed.EndsWith(")"))
            {
                string inner = trimmed.Substring(1, trimmed.Length - 2);
                List<string> tupleItems = SplitTopLevel(inner, ',');
                if (tupleItems.Count >= 2)
                {
                    tupleTypeNames = tupleItems;
                    return true;
                }
            }

            if (trimmed.StartsWith("ValueTuple<", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(">"))
            {
                string inner = ExtractGenericInner(trimmed, "ValueTuple");
                List<string> tupleItems = SplitTopLevel(inner, ',');
                if (tupleItems.Count >= 2)
                {
                    tupleTypeNames = tupleItems;
                    return true;
                }
            }

            return false;
        }

        private static string ExtractGenericInner(string typeName, string genericTypeName)
        {
            int prefixLength = genericTypeName.Length + 1;
            return typeName.Substring(prefixLength, typeName.Length - prefixLength - 1).Trim();
        }

        private static List<string> SplitTopLevel(string value, char separator)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(value))
            {
                return result;
            }

            int angleDepth = 0;
            int parenDepth = 0;
            int bracketDepth = 0;
            int braceDepth = 0;
            bool inQuote = false;
            int segmentStart = 0;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                if (c == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (inQuote)
                {
                    continue;
                }

                switch (c)
                {
                    case '<':
                        angleDepth++;
                        break;
                    case '>':
                        angleDepth--;
                        break;
                    case '(':
                        parenDepth++;
                        break;
                    case ')':
                        parenDepth--;
                        break;
                    case '[':
                        bracketDepth++;
                        break;
                    case ']':
                        bracketDepth--;
                        break;
                    case '{':
                        braceDepth++;
                        break;
                    case '}':
                        braceDepth--;
                        break;
                    default:
                        if (c == separator && angleDepth == 0 && parenDepth == 0 && bracketDepth == 0 && braceDepth == 0)
                        {
                            result.Add(value.Substring(segmentStart, i - segmentStart).Trim());
                            segmentStart = i + 1;
                        }
                        break;
                }
            }

            result.Add(value.Substring(segmentStart).Trim());
            return result;
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

        private enum TypeResolutionMode
        {
            Public,
            Backing
        }

        private sealed class TupleFieldDefinition
        {
            private TupleFieldDefinition(
                string fieldName,
                string publicTypeName,
                string backingTypeName,
                bool isTupleList,
                string publicElementTypeName,
                string backingElementTypeName)
            {
                FieldName = fieldName;
                PublicTypeName = publicTypeName;
                BackingTypeName = backingTypeName;
                IsTupleList = isTupleList;
                PublicElementTypeName = publicElementTypeName;
                BackingElementTypeName = backingElementTypeName;
                BackingFieldName = fieldName + "Serialized";
            }

            public string FieldName { get; }
            public string PublicTypeName { get; }
            public string BackingTypeName { get; }
            public bool IsTupleList { get; }
            public string PublicElementTypeName { get; }
            public string BackingElementTypeName { get; }
            public string BackingFieldName { get; }

            public static TupleFieldDefinition CreateSingle(string fieldName, string publicTypeName, string backingTypeName)
            {
                return new TupleFieldDefinition(fieldName, publicTypeName, backingTypeName, false, null, null);
            }

            public static TupleFieldDefinition CreateList(
                string fieldName,
                string publicTypeName,
                string backingTypeName,
                string publicElementTypeName,
                string backingElementTypeName)
            {
                return new TupleFieldDefinition(
                    fieldName,
                    publicTypeName,
                    backingTypeName,
                    true,
                    publicElementTypeName,
                    backingElementTypeName);
            }
        }
    }
}
