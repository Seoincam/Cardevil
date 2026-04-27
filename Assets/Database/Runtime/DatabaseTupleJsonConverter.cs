using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Database
{
    public class DatabaseTupleJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return IsDatabaseTupleType(objectType);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var values = DatabaseTupleConvert.FlattenDatabaseTuple(value, value.GetType());
            writer.WriteStartArray();
            for (int i = 0; i < values.Count; i++)
            {
                serializer.Serialize(writer, values[i]);
            }
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            object instance = Activator.CreateInstance(objectType);
            if (reader.TokenType == JsonToken.Null)
            {
                return instance;
            }

            JToken token = JToken.Load(reader);

            if (token is JArray array)
            {
                object[] flatValues = new object[array.Count];
                List<Type> elementTypes = GetTupleElementTypes(objectType);
                for (int i = 0; i < array.Count && i < elementTypes.Count; i++)
                {
                    flatValues[i] = array[i].ToObject(elementTypes[i], serializer);
                }

                return DatabaseTupleConvert.CreateDatabaseTuple(objectType, flatValues, 0, out _);
            }

            if (token is JObject obj)
            {
                PopulateTupleFromObject(obj, objectType, ref instance, serializer);
                return instance;
            }

            throw new JsonSerializationException($"Unsupported token '{token.Type}' for {objectType}.");
        }

        private static bool IsDatabaseTupleType(Type objectType)
        {
            return DatabaseTupleConvert.IsDatabaseTupleType(objectType);
        }

        private static void PopulateTupleFromObject(JObject obj, Type objectType, ref object instance, JsonSerializer serializer)
        {
            foreach (FieldInfo field in objectType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.Name == "Rest" && IsDatabaseTupleType(field.FieldType))
                {
                    object restInstance = Activator.CreateInstance(field.FieldType);
                    if (obj.TryGetValue("Rest", StringComparison.Ordinal, out JToken restToken) && restToken is JObject restObject)
                    {
                        PopulateTupleFromObject(restObject, field.FieldType, ref restInstance, serializer);
                    }

                    field.SetValue(instance, restInstance);
                    continue;
                }

                if (obj.TryGetValue(field.Name, StringComparison.Ordinal, out JToken fieldToken))
                {
                    field.SetValue(instance, fieldToken.ToObject(field.FieldType, serializer));
                }
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
            foreach (FieldInfo field in GetOrderedTupleFields(tupleType))
            {
                if (field.Name == "Rest" && IsDatabaseTupleType(field.FieldType))
                {
                    AppendTupleElementTypes(field.FieldType, elementTypes);
                    continue;
                }

                if (field.Name.StartsWith("Item", StringComparison.Ordinal))
                {
                    elementTypes.Add(field.FieldType);
                }
            }
        }

        private static IEnumerable<FieldInfo> GetOrderedTupleFields(Type tupleType)
        {
            return tupleType
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .OrderBy(field => field.Name == "Rest" ? 999 : GetItemIndex(field.Name));
        }

        private static int GetItemIndex(string fieldName)
        {
            return fieldName.StartsWith("Item", StringComparison.Ordinal)
                && int.TryParse(fieldName.Substring(4), out int index)
                ? index
                : int.MaxValue;
        }
    }
}
