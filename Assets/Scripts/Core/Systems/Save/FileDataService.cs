using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Cardevil.Save
{
    public class FileDataService : IDataService
    {
        private ISerializer _serializer;
        private string _saveDirectory;
        private string _fileExtension;

        public FileDataService(ISerializer serializer, string directory = "Saves")
        {
            _serializer = serializer;
            if (String.IsNullOrWhiteSpace(directory))
                _saveDirectory = Application.persistentDataPath;
            else
                _saveDirectory = Path.Combine(Application.persistentDataPath, directory);
            _fileExtension = ".json";

            if (!Directory.Exists(_saveDirectory))
            {
                Directory.CreateDirectory(_saveDirectory);
            }
        }
        
        private string GetFilePath(string name)
        {
            return Path.Combine(_saveDirectory, name + _fileExtension);
        }
        
        public void Save(GameSave data, bool overwrite = true)
        {
            string targetPath = GetFilePath(data.FileName);
            if (File.Exists(targetPath) && !overwrite)
            {
                throw new InvalidOperationException($"Save file '{data.FileName}' already exists and overwrite is set to false.");
            }
            File.WriteAllText(targetPath, _serializer.Serialize(data));
        }

        public GameSave Load(string name)
        {
            string targetPath = GetFilePath(name);
            if (!File.Exists(targetPath))
            {
                throw new FileNotFoundException($"Save file '{name}' not found.");
            }
            string rawData = File.ReadAllText(targetPath);
            return _serializer.Deserialize<GameSave>(rawData);
        }

        public bool Delete(string name)
        {
            string targetPath = GetFilePath(name);
            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
                return true;
            }
            return false;
        }

        public void DeleteAll()
        {
            foreach (string saveName in GetAllSaveNames())
            {
                Delete(saveName);
            }
        }

        public IEnumerable<string> GetAllSaveNames()
        {
            foreach (var file in Directory.GetFiles(_saveDirectory, "*" + _fileExtension))
            {
                yield return Path.GetFileNameWithoutExtension(file);
            }
        }

        public bool SaveExists(string name)
        {
            string targetPath = GetFilePath(name);
            return File.Exists(targetPath);
        }
    }
}