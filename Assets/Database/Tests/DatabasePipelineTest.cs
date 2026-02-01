using Database;
using System;
using System.Collections.Generic;
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

    }
}
