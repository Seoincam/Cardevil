using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Database.DataReader;
using Database.Generated;
using UnityEngine.Networking;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace Database
{
    /// <summary>
    /// 플레이타임에 로드하는 클래스
    /// </summary>
    public class DatabaseManager : MonoBehaviour
    {
        [SerializeField] private McDatabase mcDatabase = new McDatabase();
        [SerializeField] private bool isInitialized = false;
        [Header("Path Settings")]
        [SerializeField] private string googleSheetUrl = "URL_HERE"; // 구글 시트 URL
        [SerializeField] private string localJsonPath = "Database/";
        [SerializeField] private string streamingAssetPath = "Json/";
        [Header("Settings")]
        [SerializeField,Tooltip("인터넷/로컬 유무와 상관없이 streaming으로 작동")] private bool forceUseStreamingAsset = false;
        [SerializeField] private int timeoutSeconds = 10;
        
        
        public string FinalLocalPath => Path.Combine(Application.persistentDataPath, localJsonPath);
        public string FinalStreamingAssetPath => Path.Combine(Application.streamingAssetsPath, streamingAssetPath);

        public McDatabase Database => mcDatabase;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            isInitialized = false;
            DontDestroyOnLoad(gameObject);
        }

        [ContextMenu("Clear Database")]
        public void Clear()
        {
            mcDatabase.ClearAll();
            isInitialized = false;
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                Debug.LogWarning("[DatabaseManager] 이미 초기화되었습니다.");
                return;
            }
            StartCoroutine(InitializeRoutine());
        }
        
        [ContextMenu("Force Initialize In Play Mode")]
        public void ForceInitialize()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DatabaseManager] 에디터 모드에서는 이 기능을 사용할 수 없습니다.");
                return;
            }
            isInitialized = false;
            StartCoroutine(InitializeRoutine());
        }


        
        #if UNITY_EDITOR
        [ContextMenu("Initialize In Edit Mode")]
        public void InitializeInEditMode()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[DatabaseManager] 플레이 모드에서는 이 기능을 사용할 수 없습니다.");
                return;
            }
            isInitialized = false;
            EditorCoroutineUtility.StartCoroutine(InitializeRoutine(), this);
        }
        #endif
        
        private IEnumerator InitializeRoutine()
        {
            Debug.Log("[DatabaseManager] 초기화 시작");
            Debug.Log("[DatabaseManager] DB 데이터 삭제");
            mcDatabase.ClearAll();
            yield return LoadDatabase();
            // yield return new WaitForSeconds(1f); // 테스트용 딜레이
            
            Debug.Log("[DatabaseManager] 초기화 완료");
            isInitialized = true;
        }
        
        private IEnumerator LoadDatabase()
        {
            if (forceUseStreamingAsset)
            {
                Debug.Log("[DatabaseManager] 강제 스트리밍 에셋 모드. 로컬 파일을 로드합니다.");
                yield return LoadLocal();
            }
            else if (String.IsNullOrEmpty(googleSheetUrl))
            {
                Debug.Log("[DatabaseManager] 구글 시트 URL이 비었습니다. 로컬 파일을 로드합니다.");
                yield return LoadLocal();
            }
            else if (IsInternetAvailable())
            {
                Debug.Log("[DatabaseManager] 인터넷 연결됨. 구글 시트를 로드합니다.");
                bool isSuccess = false;
                GoogleSheetReader reader = new GoogleSheetReader();
                yield return reader.LoadDataFramesRoutine(googleSheetUrl, success => isSuccess = success, timeoutSeconds);
                if (isSuccess)
                {
                    mcDatabase.InitializeAll(reader.DataFrames);
                    Debug.Log($"[DatabaseManager] 구글 시트 데이터 로드 성공. 항목 수: {reader.DataFrames.Count}");
                }
                else
                {
                    Debug.LogWarning("[DatabaseManager] 구글 시트 데이터 로드 실패. 로컬 파일을 로드합니다.");
                    yield return LoadLocal();
                }
            }
            else
            {
                Debug.Log("[DatabaseManager] 인터넷 연결안됨. 로컬 파일을 로드합니다.");
                yield return LoadLocal();
            }
        }

        private IEnumerator LoadLocal()
        {
            string json = null;
            List<string> targetClassNames = mcDatabase.ClassNames;
            foreach (var className in targetClassNames)
            {
                string fileName = $"{className}.json";
                string localPath = Path.Combine(FinalLocalPath, fileName);
                string streamingPath = Path.Combine(FinalStreamingAssetPath, fileName);
                if (forceUseStreamingAsset)
                {
                    Debug.Log($"[DatabaseManager] 강제 스트리밍 에셋 모드. 스트리밍 에셋 로드 시도: {streamingPath}");
                    yield return LoadStreamingAsset(streamingPath, result => json = result);
                }
                else 
                {
                    Debug.Log($"[DatabaseManager] 로컬 파일 로드 시도: {localPath}");
                    yield return LoadSavedFile(localPath, result => json = result);
                    if (string.IsNullOrEmpty(json))
                    {
                        Debug.Log($"[DatabaseManager] 스트리밍 에셋 로드 시도: {streamingPath}");
                        yield return LoadStreamingAsset(streamingPath, result => json = result);
                    }
                }

                if (!string.IsNullOrEmpty(json))
                {
                    Type type = mcDatabase.GetTypeByName(className);
                    if (type == null)
                    {
                        Debug.LogWarning($"[DatabaseManager] {className}에 해당하는 타입을 찾을 수 없습니다.");
                        continue;
                    }
                    mcDatabase.AddInstancesFromJsonList(className, json);
                }
                else
                {
                    Debug.LogWarning($"[DatabaseManager] {className} 데이터 로드 실패. 파일이 존재하지 않음.");
                }
            }
            Debug.Log($"[DatabaseManager] 로컬 JSON 길이: {json?.Length ?? 0}");
            yield break;    
        }
        private IEnumerator LoadSavedFile(string path, Action<string> onComplete)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                onComplete?.Invoke(json);
            }
            else
            {
                onComplete?.Invoke(null);
            }
            yield break;
        }
        
        private IEnumerator LoadStreamingAsset(string path, Action<string> onComplete)
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.WebGLPlayer)
            {
                using UnityWebRequest www = UnityWebRequest.Get(path);
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[DatabaseManager] 스트리밍 에셋 로드 실패: {www.error}");
                    onComplete?.Invoke(null);
                }
                else
                {
                    onComplete?.Invoke(www.downloadHandler.text);
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    onComplete?.Invoke(json);
                }
                else
                {
                    onComplete?.Invoke(null);
                }
            }
            
        }
        
        
        private bool IsInternetAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }
        
        private IEnumerator Timer(float time, Action onTimeout)
        {
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            onTimeout?.Invoke();
        }
        
        
    }
}