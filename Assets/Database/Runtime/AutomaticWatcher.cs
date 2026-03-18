using System;
using System.Collections.Generic;
using System.IO;
using Database.DataReader;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Database
{
    public class AutomaticWatcher
    {
#if UNITY_EDITOR
        public string Path { get; private set; }
        public string Extension { get; }

        private readonly string _tempPrefix;
        private readonly IDataReader _reader;
        private readonly HashSet<string> _modifiedFiles = new HashSet<string>();

        private FileSystemWatcher _watcher;

        public Action<DataFrame> CreateCsharp;

        public AutomaticWatcher(string path, IDataReader reader, string extension, string tempPrefix = null)
        {
            Path = path;
            Extension = extension;
            _reader = reader;
            _tempPrefix = tempPrefix;

            _watcher = CreateWatcher();
        }

        private FileSystemWatcher CreateWatcher()
        {
            var watcher = new FileSystemWatcher(Path, $"*{Extension}");
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Changed += OnDataChanged;
            watcher.Created += OnDataCreated;
            watcher.Deleted += OnDataDeleted;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        public void ChangeWatcherDir(string newPath)
        {
            Path = newPath;
            if (_watcher != null)
            {
                _watcher.Path = Path;
                Debug.Log($"Watcher directory changed to: {newPath}");
            }
            else
            {
                _watcher = CreateWatcher();
            }
        }

        public void DisposeWatcher()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnDataCreated(object sender, FileSystemEventArgs e)
        {
            if (IsTempFileEvent(e.Name))
                return;

            Debug.Log($"File Created: {e.FullPath}");

            var dataFrames = _reader.Read(e.FullPath) ?? new List<DataFrame>();
            foreach (var dataFrame in dataFrames)
                CreateCsharp?.Invoke(dataFrame);

            DelayedAssetRefresh();
        }

        private void OnDataChanged(object sender, FileSystemEventArgs e)
        {
            if (string.IsNullOrEmpty(_tempPrefix))
            {
                var dataFrames = _reader.Read(e.FullPath) ?? new List<DataFrame>();
                foreach (var dataFrame in dataFrames)
                    CreateCsharp?.Invoke(dataFrame);

                DelayedAssetRefresh();
                return;
            }

            if (IsTempFileEvent(e.Name))
                return;

            Debug.Log($"File Changed: {e.FullPath}");
            _modifiedFiles.Add(e.FullPath);
        }

        private void OnDataDeleted(object sender, FileSystemEventArgs e)
        {
            if (IsTempFileEvent(e.Name))
            {
                OnTempFileRemoved(e.FullPath);
                return;
            }

            Debug.Log($"File Removed: {e.FullPath}");
        }

        private void OnTempFileRemoved(string path)
        {
            string realPath = path.Replace(_tempPrefix, string.Empty);

            if (_modifiedFiles.Contains(realPath))
            {
                EditorApplication.delayCall += () =>
                {
                    if (!_modifiedFiles.Contains(realPath))
                        return;

                    Debug.Log($"Creating {System.IO.Path.GetFileNameWithoutExtension(realPath)}.cs");

                    _modifiedFiles.Remove(realPath);
                    var dataFrames = _reader.Read(realPath) ?? new List<DataFrame>();
                    foreach (var dataFrame in dataFrames)
                        CreateCsharp?.Invoke(dataFrame);

                    if (_modifiedFiles.Count == 0)
                        DelayedAssetRefresh();
                };
            }
        }

        private bool IsTempFileEvent(string fileName)
        {
            return !string.IsNullOrEmpty(_tempPrefix)
                && !string.IsNullOrEmpty(fileName)
                && fileName.StartsWith(_tempPrefix, StringComparison.Ordinal);
        }

        private static void DelayedAssetRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                Debug.Log("Refreshing AssetDatabase...");
                AssetDatabase.Refresh();
            };
        }
#endif
    }
}
