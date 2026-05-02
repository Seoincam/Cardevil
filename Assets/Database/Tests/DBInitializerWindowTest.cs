using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Database.Tests
{
    public class DBInitializerWindowTest
    {
        private static readonly MethodInfo BuildJsonColumnsMethod =
            typeof(DBInitializerWindow).GetMethod("BuildJsonColumns", BindingFlags.NonPublic | BindingFlags.Static);

        private static readonly MethodInfo BuildJsonRowsMethod =
            typeof(DBInitializerWindow).GetMethod("BuildJsonRows", BindingFlags.NonPublic | BindingFlags.Static);

        [Test]
        public void BuildJsonColumns_UsesMetadataObject_WhenPresent()
        {
            var root = JToken.Parse(
                "{\"comment\":{\"Id\":\"identifier\",\"Name\":\"display\"},\"type\":{\"Id\":\"int\",\"Name\":\"string\"},\"data\":[{\"Id\":1,\"Name\":\"Sword\"}]}");

            var columns = InvokeList(BuildJsonColumnsMethod, root);

            Assert.That(columns.Count, Is.EqualTo(2));
            Assert.That(GetProperty(columns[0], "Name"), Is.EqualTo("Id"));
            Assert.That(GetProperty(columns[0], "Type"), Is.EqualTo("int"));
            Assert.That(GetProperty(columns[0], "Comment"), Is.EqualTo("identifier"));
            Assert.That(GetProperty(columns[1], "Name"), Is.EqualTo("Name"));
            Assert.That(GetProperty(columns[1], "Type"), Is.EqualTo("string"));
            Assert.That(GetProperty(columns[1], "Comment"), Is.EqualTo("display"));
        }

        [Test]
        public void BuildJsonRows_ExpandsObjectDataRows()
        {
            var root = JToken.Parse(
                "{\"comment\":{\"Id\":\"identifier\",\"Name\":\"display\"},\"type\":{\"Id\":\"int\",\"Name\":\"string\"},\"data\":[{\"Id\":1,\"Name\":\"Sword\"},{\"Id\":2,\"Name\":\"Shield\"}]}");

            var columns = InvokeList(BuildJsonColumnsMethod, root);
            var rows = InvokeList(BuildJsonRowsMethod, root, columns);

            Assert.That(rows.Count, Is.EqualTo(2));

            var firstCells = (IList)GetProperty(rows[0], "Cells");
            var secondCells = (IList)GetProperty(rows[1], "Cells");

            Assert.That(firstCells[0], Is.EqualTo("1"));
            Assert.That(firstCells[1], Is.EqualTo("Sword"));
            Assert.That(secondCells[0], Is.EqualTo("2"));
            Assert.That(secondCells[1], Is.EqualTo("Shield"));
        }

        [Test]
        public void BuildJsonRows_PreservesCompactTupleAndNestedArrayCells()
        {
            var root = JToken.Parse(
                "{\"type\":{\"tuple\":\"(int,int,string)\",\"tupleList\":\"List<(int,int,string)>\"},\"data\":[{\"tuple\":[1,2,\"SampleText\"],\"tupleList\":[[1,2,\"A\"],[3,4,\"B\"]]}]}");

            var columns = InvokeList(BuildJsonColumnsMethod, root);
            var rows = InvokeList(BuildJsonRowsMethod, root, columns);
            var firstCells = (IList)GetProperty(rows[0], "Cells");

            Assert.That(firstCells[0], Is.EqualTo("[1,2,\"SampleText\"]"));
            Assert.That(firstCells[1], Is.EqualTo("[[1,2,\"A\"],[3,4,\"B\"]]"));
        }

        [Test]
        public void DownloadStateStore_SaveAndLoadFromPath_RoundTrips()
        {
            string stateFilePath = Path.Combine(Path.GetTempPath(), $"dbinitializer-state-{Path.GetRandomFileName()}.json");
            var state = new DBInitializerDownloadState
            {
                InitializerGuid = "guid-123",
                AssetPath = "Assets/Database/DBInitializer.asset",
                LastFullDownloadUtc = "2026-03-19T12:34:56.0000000Z",
                SheetDownloadsUtc = new Dictionary<string, string>
                {
                    ["MachineReward"] = "2026-03-19T12:40:00.0000000Z"
                }
            };

            try
            {
                DBInitializerDownloadStateStore.SaveToPath(stateFilePath, state);
                var loaded = DBInitializerDownloadStateStore.LoadFromPath(stateFilePath);

                Assert.That(loaded, Is.Not.Null);
                Assert.That(loaded.InitializerGuid, Is.EqualTo(state.InitializerGuid));
                Assert.That(loaded.AssetPath, Is.EqualTo(state.AssetPath));
                Assert.That(loaded.LastFullDownloadUtc, Is.EqualTo(state.LastFullDownloadUtc));
                Assert.That(loaded.SheetDownloadsUtc["MachineReward"], Is.EqualTo(state.SheetDownloadsUtc["MachineReward"]));
            }
            finally
            {
                if (File.Exists(stateFilePath))
                {
                    File.Delete(stateFilePath);
                }
            }
        }

        private static IList InvokeList(MethodInfo method, params object[] args)
        {
            Assert.That(method, Is.Not.Null);
            return method.Invoke(null, args) as IList;
        }

        private static object GetProperty(object target, string propertyName)
        {
            return target.GetType().GetProperty(propertyName)?.GetValue(target);
        }
    }
}
