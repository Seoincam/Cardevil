using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Database
{
    public static class DBInitializerDownloadStateStore
    {
        public static DBInitializerDownloadState Load(DBInitializerSO initializer)
        {
            if (!TryGetStateFilePath(initializer, out var stateFilePath))
            {
                return DBInitializerDownloadState.CreateEmpty();
            }

            var loaded = LoadFromPath(stateFilePath);
            if (loaded != null)
            {
                return loaded;
            }

            return CreateDefaultState(initializer);
        }

        public static bool RecordFullDownload(DBInitializerSO initializer, DateTime utcNow)
        {
            if (!TryGetStateFilePath(initializer, out var stateFilePath))
            {
                return false;
            }

            var state = LoadFromPath(stateFilePath) ?? CreateDefaultState(initializer);
            state.LastFullDownloadUtc = utcNow.ToString("O", CultureInfo.InvariantCulture);
            SaveToPath(stateFilePath, state);
            return true;
        }

        public static bool RecordSheetDownload(DBInitializerSO initializer, string sheetName, DateTime utcNow)
        {
            if (!TryGetStateFilePath(initializer, out var stateFilePath) || string.IsNullOrWhiteSpace(sheetName))
            {
                return false;
            }

            var state = LoadFromPath(stateFilePath) ?? CreateDefaultState(initializer);
            state.SheetDownloadsUtc[sheetName] = utcNow.ToString("O", CultureInfo.InvariantCulture);
            SaveToPath(stateFilePath, state);
            return true;
        }

        public static bool TryGetStateFilePath(DBInitializerSO initializer, out string stateFilePath)
        {
            stateFilePath = null;
            if (initializer == null)
            {
                return false;
            }

            string assetPath = AssetDatabase.GetAssetPath(initializer);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return false;
            }

            string guid = AssetDatabase.AssetPathToGUID(assetPath);
            if (string.IsNullOrWhiteSpace(guid))
            {
                return false;
            }

            string directoryPath = initializer.DownloadStateDirPath;
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                return false;
            }

            string initializerName = Path.GetFileNameWithoutExtension(assetPath);
            stateFilePath = Path.Combine(directoryPath, $"{initializerName}.{guid}.download-state.json");
            return true;
        }

        public static DBInitializerDownloadState LoadFromPath(string stateFilePath)
        {
            if (string.IsNullOrWhiteSpace(stateFilePath) || !File.Exists(stateFilePath))
            {
                return null;
            }

            try
            {
                string json = File.ReadAllText(stateFilePath);
                var state = JsonConvert.DeserializeObject<DBInitializerDownloadState>(json);
                return state ?? DBInitializerDownloadState.CreateEmpty();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DBInitializerDownloadStateStore] Failed to read state file: {stateFilePath}\n{ex.Message}");
                return null;
            }
        }

        public static void SaveToPath(string stateFilePath, DBInitializerDownloadState state)
        {
            if (string.IsNullOrWhiteSpace(stateFilePath) || state == null)
            {
                return;
            }

            string directoryPath = Path.GetDirectoryName(stateFilePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonConvert.SerializeObject(state, Formatting.Indented);
            File.WriteAllText(stateFilePath, json);
        }

        private static DBInitializerDownloadState CreateDefaultState(DBInitializerSO initializer)
        {
            if (initializer == null)
            {
                return DBInitializerDownloadState.CreateEmpty();
            }

            string assetPath = AssetDatabase.GetAssetPath(initializer);
            string guid = string.IsNullOrWhiteSpace(assetPath) ? string.Empty : AssetDatabase.AssetPathToGUID(assetPath);

            return new DBInitializerDownloadState
            {
                InitializerGuid = guid ?? string.Empty,
                AssetPath = assetPath ?? string.Empty,
                SheetDownloadsUtc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
        }
    }

    [Serializable]
    public sealed class DBInitializerDownloadState
    {
        public string InitializerGuid;
        public string AssetPath;
        public string LastFullDownloadUtc;
        public Dictionary<string, string> SheetDownloadsUtc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static DBInitializerDownloadState CreateEmpty()
        {
            return new DBInitializerDownloadState
            {
                InitializerGuid = string.Empty,
                AssetPath = string.Empty,
                LastFullDownloadUtc = string.Empty,
                SheetDownloadsUtc = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            };
        }
    }
}
