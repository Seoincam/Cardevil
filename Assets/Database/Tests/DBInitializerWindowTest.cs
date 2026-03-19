using System.Collections;
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
