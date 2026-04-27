using Database;
using Database.DataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NUnit.Framework;
using UnityEngine;

namespace Machamy.McDatabase.Tests
{
    public class DatabasePipelineTest
    {
        // 1. Parsing Robustness Test
        [Test]
        public void SplitToList_Robustness_Test()
        {
            // Case 1: Simple list
            string simple = "[1, 2, 3]";
            var resSimple = ClassInstanceFactory.SplitToList(simple);
            Assert.AreEqual(3, resSimple.Length);
            Assert.AreEqual("1", resSimple[0]);

            // Case 2: Quoted comma (The Fix)
            string quoted = "[\"a,b\", \"c\"]";
            var resQuoted = ClassInstanceFactory.SplitToList(quoted);
            Assert.AreEqual(2, resQuoted.Length);
            Assert.AreEqual("\"a,b\"", resQuoted[0]); // Quote remains for ConvertValue to handle
            Assert.AreEqual("\"c\"", resQuoted[1]);

            // Case 3: Nested List
            string nested = "[[1,2], [3,4]]";
            var resNested = ClassInstanceFactory.SplitToList(nested);
            Assert.AreEqual(2, resNested.Length);
            Assert.AreEqual("[1,2]", resNested[0]);
            Assert.AreEqual("[3,4]", resNested[1]);

            // Case 4: Nested Dictionary
            // Case 4: Nested Dictionary
            string dict = "a:1, b:2";
            var resDict = ClassInstanceFactory.SplitToList(dict);
            Assert.AreEqual(2, resDict.Length);
            Assert.IsTrue(resDict[0].Contains("a") && resDict[0].Contains("1"));

            // Case 5: List of tuples
            string tupleList = "[(1,2), (3,4)]";
            var resTupleList = ClassInstanceFactory.SplitToList(tupleList);
            Assert.AreEqual(2, resTupleList.Length);
            Assert.AreEqual("(1,2)", resTupleList[0]);
            Assert.AreEqual("(3,4)", resTupleList[1]);
        }

        [Test]
        public void SplitToKV_Test()
        {
            string simple = "key : value";
            var kv = ClassInstanceFactory.SplitToKV(simple);
            Assert.AreEqual("key", kv[0]);
            Assert.AreEqual("value", kv[1]);

            string complex = "key : {nested:value}";
            var kv2 = ClassInstanceFactory.SplitToKV(complex);
            Assert.AreEqual("key", kv2[0]);
            Assert.AreEqual("{nested:value}", kv2[1]);
        }

        // Helper to access private static method using Reflection
        // Helper methods removed as we are using internal access now

        // 2. Full Pipeline Test (Mocking)
        // Since we cannot easily generate C# classes at runtime in a test without compilation wait, 
        // we will test the 'ClassInstanceFactory.ConvertValue' logic directly for complex types.
        [Test]
        public void ConvertValue_Dictionary_Test()
        {
            string input = "{A:1, B:2}";
            var dict = (Dictionary<string, int>)ClassInstanceFactory.ConvertValue(typeof(Dictionary<string, int>), input);

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(1, dict["A"]);
            Assert.AreEqual(2, dict["B"]);
        }

        [Test]
        public void ConvertValue_Nested_Dictionary_Test()
        {
            // Dictionary<string, List<int>>
            // Input format: { Key1 : [1,2], Key2 : [3,4] }
            string input = "{ Key1 : [1,2], Key2 : [3, 4] }";
            var dict = (Dictionary<string, List<int>>)ClassInstanceFactory.ConvertValue(typeof(Dictionary<string, List<int>>), input);

            Assert.AreEqual(2, dict.Count);
            Assert.AreEqual(2, dict["Key1"].Count);
            Assert.AreEqual(1, dict["Key1"][0]);
        }

        [Test]
        public void ConvertValue_QuotedString_InList_Test()
        {
            // List<string> where element contains comma
            string input = "[\"Hello, World\", \"Bye\"]";
            var list = (List<string>)ClassInstanceFactory.ConvertValue(typeof(List<string>), input);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual("Hello, World", list[0]); // Quotes should be stripped by ConvertValue
            Assert.AreEqual("Bye", list[1]);
        }

        [Test]
        public void ConvertValue_ValueTuple_Test()
        {
            string input = "(1, 2)";
            var tuple = ((int, int))ClassInstanceFactory.ConvertValue(typeof((int, int)), input);

            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual(2, tuple.Item2);
        }

        [Test]
        public void ConvertValue_ValueTuple_ArraySyntax_Test()
        {
            string input = "[1, 2]";
            var tuple = ((int, int))ClassInstanceFactory.ConvertValue(typeof((int, int)), input);

            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual(2, tuple.Item2);
        }

        [Test]
        public void ConvertValue_ListOfValueTuple_Test()
        {
            string input = "[(1,2), (3,4)]";
            var list = (List<(int, int)>)ClassInstanceFactory.ConvertValue(typeof(List<(int, int)>), input);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0].Item1);
            Assert.AreEqual(2, list[0].Item2);
            Assert.AreEqual(3, list[1].Item1);
            Assert.AreEqual(4, list[1].Item2);
        }

        [Test]
        public void ConvertValue_ListOfValueTuple_WithoutBrackets_Test()
        {
            string input = "(1,2), (3,4)";
            var list = (List<(int, int)>)ClassInstanceFactory.ConvertValue(typeof(List<(int, int)>), input);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0].Item1);
            Assert.AreEqual(2, list[0].Item2);
            Assert.AreEqual(3, list[1].Item1);
            Assert.AreEqual(4, list[1].Item2);
        }

        [Test]
        public void ConvertValue_ValueTuple_WithEightItems_Test()
        {
            string input = "(1,2,3,4,5,6,7,8)";
            var tuple = (ValueTuple<int, int, int, int, int, int, int, ValueTuple<int>>)ClassInstanceFactory.ConvertValue(
                typeof((int, int, int, int, int, int, int, int)),
                input);

            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual(8, tuple.Rest.Item1);
        }

        [Test]
        public void DatabaseTupleJsonConverter_SerializesTupleDtoAsJsonArray()
        {
            var value = new DatabaseTuple<int, int, string>
            {
                Item1 = 10,
                Item2 = 20,
                Item3 = "SampleText"
            };

            string json = JsonConvert.SerializeObject(value);

            Assert.AreEqual("[10,20,\"SampleText\"]", json);
        }

        [Test]
        public void DatabaseTupleJsonConverter_DeserializesTupleDtoFromJsonArray()
        {
            var value = JsonConvert.DeserializeObject<DatabaseTuple<int, int, string>>("[10,20,\"SampleText\"]");

            Assert.AreEqual(10, value.Item1);
            Assert.AreEqual(20, value.Item2);
            Assert.AreEqual("SampleText", value.Item3);
        }

        [Test]
        public void DatabaseTupleJsonConverter_DeserializesRecursiveTupleFromJsonArray()
        {
            var value = JsonConvert.DeserializeObject<DatabaseTuple<int, int, int, int, int, int, int, DatabaseTuple<int, int>>>("[1,2,3,4,5,6,7,8,9]");

            Assert.AreEqual(1, value.Item1);
            Assert.AreEqual(7, value.Item7);
            Assert.AreEqual(8, value.Rest.Item1);
            Assert.AreEqual(9, value.Rest.Item2);
        }

        [Test]
        public void DatabaseTupleConvert_ToValueTuple_SupportsRecursiveTuple()
        {
            var backing = new DatabaseTuple<int, int, int, int, int, int, int, DatabaseTuple<int, int>>(
                1, 2, 3, 4, 5, 6, 7,
                new DatabaseTuple<int, int>(8, 9));

            var tuple = DatabaseTupleConvert.ToValueTuple<(int, int, int, int, int, int, int, int, int)>(backing);

            Assert.AreEqual(1, tuple.Item1);
            Assert.AreEqual(7, tuple.Item7);
            Assert.AreEqual(8, tuple.Rest.Item1);
            Assert.AreEqual(9, tuple.Rest.Item2);
        }

        [Test]
        public void DatabaseTupleConvert_ToBacking_SupportsSixteenItems()
        {
            var tuple = (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
            var backing = DatabaseTupleConvert.ToBacking<DatabaseTuple<int, int, int, int, int, int, int, DatabaseTuple<int, int, int, int, int, int, int, DatabaseTuple<int, int>>>>(tuple);

            Assert.AreEqual(1, backing.Item1);
            Assert.AreEqual(7, backing.Item7);
            Assert.AreEqual(8, backing.Rest.Item1);
            Assert.AreEqual(14, backing.Rest.Item7);
            Assert.AreEqual(15, backing.Rest.Rest.Item1);
            Assert.AreEqual(16, backing.Rest.Rest.Item2);
        }

        [Test]
        public void ConvertValue_TupleDtoList_FromJsonArrayList_Test()
        {
            string input = "[[1,2,\"A\"],[3,4,\"B\"]]";
            var list = (List<DatabaseTuple<int, int, string>>)ClassInstanceFactory.ConvertValue(
                typeof(List<DatabaseTuple<int, int, string>>),
                input);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0].Item1);
            Assert.AreEqual(2, list[0].Item2);
            Assert.AreEqual("A", list[0].Item3);
            Assert.AreEqual(3, list[1].Item1);
            Assert.AreEqual(4, list[1].Item2);
            Assert.AreEqual("B", list[1].Item3);
        }

        [Test]
        public void ReflectionUtil_DerivedTypeCache_IsCreated_AndStable()
        {
            ReflectionUtil.ClearCachesForTests();

            Assert.IsFalse(ReflectionUtil.HasDerivedTypeCache(typeof(IDBData)));

            var first = ReflectionUtil.FindDerivedTypesWithCache<IDBData>();
            var second = ReflectionUtil.FindDerivedTypesWithCache<IDBData>();

            Assert.IsTrue(ReflectionUtil.HasDerivedTypeCache(typeof(IDBData)));
            CollectionAssert.AreEquivalent(first, second);
            Assert.IsTrue(first.Exists(type => type.Name == "CustomClassTest"));
        }

        [Test]
        public void AutomaticWatcher_ChangeWatcherDir_RecreatesDisposedWatcher()
        {
            string firstDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            string secondDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(firstDir);
            Directory.CreateDirectory(secondDir);

            try
            {
                var watcher = new AutomaticWatcher(firstDir, new RawJsonReader(), ".json", "~$");
                watcher.DisposeWatcher();
                watcher.ChangeWatcherDir(secondDir);

                var watcherField = typeof(AutomaticWatcher).GetField("_watcher", BindingFlags.NonPublic | BindingFlags.Instance);
                var innerWatcher = watcherField?.GetValue(watcher);

                Assert.NotNull(innerWatcher);
                Assert.AreEqual(secondDir, watcher.Path);

                watcher.DisposeWatcher();
            }
            finally
            {
                Directory.Delete(firstDir, true);
                Directory.Delete(secondDir, true);
            }
        }

        [Test]
        public void DataFrame_ToString_HandlesEmptyRows()
        {
            var dataFrame = new DataFrame("Sample")
            {
                varNames = new[] { "value" },
                types = new[] { "string" },
                comments = new[] { string.Empty },
                data = new[] { Array.Empty<string>() }
            };

            Assert.DoesNotThrow(() => dataFrame.ToString());
        }

        [Test]
        public void GoogleSheetReader_Read_WithNonUrl_ReturnsEmptyList()
        {
            var reader = new GoogleSheetReader();
            var dataFrames = reader.Read("not-a-url");

            Assert.NotNull(dataFrames);
            Assert.AreEqual(0, dataFrames.Count);
        }

        [Test]
        public void GoogleSheetReader_FilterDataFrames_ReturnsOnlyRequestedSheet()
        {
            var frames = new List<DataFrame>
            {
                new DataFrame("Alpha"),
                new DataFrame("Beta"),
                new DataFrame("Gamma")
            };

            var method = typeof(GoogleSheetReader).GetMethod("FilterDataFrames", BindingFlags.NonPublic | BindingFlags.Static);
            var filtered = method?.Invoke(null, new object[] { frames, "beta" }) as List<DataFrame>;

            Assert.NotNull(filtered);
            Assert.AreEqual(1, filtered.Count);
            Assert.AreEqual("Beta", filtered[0].name);
        }

        [Test]
        public void ClassDefinitionFactory_GenerateClassDefinition_PreservesFullEnumAndClassTypes()
        {
            var df = new DataFrame("Sample")
            {
                varNames = new[] { "fullEnumList", "shortEnum", "shortNestedEnum", "customClass" },
                types = new[]
                {
                    "List<Enum<Cardevil.Core.Utils.Define.SlotRewardType>>",
                    "Enum<SlotRewardType>",
                    "Enum<RareType>",
                    "Class<DBSampleEntryClassJson>"
                },
                comments = new[] { string.Empty, string.Empty, string.Empty, string.Empty },
                data = Array.Empty<string[]>()
            };

            string generated = ClassDefinitionFactory.GenerateClassDefinition(df);

            StringAssert.Contains("public List<Cardevil.Core.Utils.Define.SlotRewardType> fullEnumList;", generated);
            StringAssert.Contains("public Cardevil.Core.Utils.Define.SlotRewardType shortEnum;", generated);
            StringAssert.Contains("public Cardevil.Core.Utils.Define.RareType shortNestedEnum;", generated);
            StringAssert.Contains("public Database.DBSampleEntryClassJson customClass;", generated);
        }

        [Test]
        public void ClassDefinitionFactory_GenerateClassDefinition_NormalizesLegacyEnumNamespace()
        {
            var df = new DataFrame("Sample")
            {
                varNames = new[] { "Rarity" },
                types = new[] { "Enum<Cardevil.Relics.RelicRarity>" },
                comments = new[] { string.Empty },
                data = Array.Empty<string[]>()
            };

            string generated = ClassDefinitionFactory.GenerateClassDefinition(df);

            StringAssert.Contains("public Cardevil.Gameplay.Items.RelicRarity Rarity;", generated);
        }

        [Test]
        public void ClassDefinitionFactory_GenerateClassDefinition_SupportsTupleTypes()
        {
            var df = new DataFrame("Sample")
            {
                varNames = new[] { "Pair", "PairList", "ValueTupleList", "LargeTuple", "HugeTuple" },
                types = new[]
                {
                    "(int, float)",
                    "List<(int, float)>",
                    "List<ValueTuple<int, Enum<SlotRewardType>>>",
                    "(int, int, int, int, int, int, int, int)",
                    "(int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int)"
                },
                comments = new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty },
                data = Array.Empty<string[]>()
            };

            string generated = ClassDefinitionFactory.GenerateClassDefinition(df);

            StringAssert.Contains("[JsonIgnore]", generated);
            StringAssert.Contains("public (int, float) Pair;", generated);
            StringAssert.Contains("private DatabaseTuple<int, float> PairSerialized;", generated);
            StringAssert.Contains("public partial class Sample : ISerializationCallbackReceiver", generated);
            StringAssert.Contains("private void SyncTupleFieldsToSerializedBacking()", generated);
            StringAssert.Contains("public List<(int, float)> PairList;", generated);
            StringAssert.Contains("private List<DatabaseTuple<int, float>> PairListSerialized = new List<DatabaseTuple<int, float>>();", generated);
            StringAssert.Contains("public List<(int, Cardevil.Core.Utils.Define.SlotRewardType)> ValueTupleList;", generated);
            StringAssert.Contains("private DatabaseTuple<int, int, int, int, int, int, int, DatabaseTuple<int>> LargeTupleSerialized;", generated);
            StringAssert.Contains("DatabaseTupleConvert.ToValueTuple<(int, int, int, int, int, int, int, int)>(LargeTupleSerialized)", generated);
            StringAssert.Contains("private DatabaseTuple<int, int, int, int, int, int, int, DatabaseTuple<int, int, int, int, int, int, int, DatabaseTuple<int, int>>>", generated);
        }

        [Test]
        public void RawJsonReader_ReadSheet_StoresStructuredValuesAsCompactJson()
        {
            var root = Newtonsoft.Json.Linq.JToken.Parse(
                "{\"type\":{\"CustomClass\":\"Class<DBSampleEntryClassJson>\",\"CustomClassList\":\"List<Class<DBSampleEntryClassJson>>\",\"ListList\":\"List<List<int>>\",\"tuple\":\"(int,int,string)\",\"tupleList\":\"List<(int,int,string)>\"},\"data\":[{\"CustomClass\":{\"a\":10,\"b\":20.0,\"c\":\"SampleText\"},\"CustomClassList\":[{\"a\":10,\"b\":20.0,\"c\":\"SampleText\"},{\"a\":222,\"b\":20.4444,\"c\":\"SampleText2\"}],\"ListList\":[[2,3,3],[2,3,1,2]],\"tuple\":[1,2,\"SampleText\"],\"tupleList\":[[1,2,\"A\"],[3,4,\"B\"]]}]}");

            var df = new RawJsonReader().ReadSheet("Sample", root);

            Assert.AreEqual("{\"a\":10,\"b\":20.0,\"c\":\"SampleText\"}", df.data[0][0]);
            Assert.AreEqual("[{\"a\":10,\"b\":20.0,\"c\":\"SampleText\"},{\"a\":222,\"b\":20.4444,\"c\":\"SampleText2\"}]", df.data[0][1]);
            Assert.AreEqual("[[2,3,3],[2,3,1,2]]", df.data[0][2]);
            Assert.AreEqual("[1,2,\"SampleText\"]", df.data[0][3]);
            Assert.AreEqual("[[1,2,\"A\"],[3,4,\"B\"]]", df.data[0][4]);
        }
    }
}
