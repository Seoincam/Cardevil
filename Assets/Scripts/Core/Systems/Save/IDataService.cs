using System.Collections.Generic;

namespace Cardevil.Save
{
    public interface IDataService
    {
        public void Save(GameSave data, bool overwrite = true);
        public GameSave Load(string name);
        public bool Delete(string name);
        public void DeleteAll();
        public IEnumerable<string> GetAllSaveNames();
        public bool SaveExists(string name);
    }
}