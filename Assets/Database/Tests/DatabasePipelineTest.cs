using Database;
using Database.DataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
    }
}
