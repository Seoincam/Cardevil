namespace Cardevil.Core.Systems.Save
{
    public interface ISerializer
    {
        public string Serialize<T>(T obj);
        public T Deserialize<T>(string raw);
    }
}