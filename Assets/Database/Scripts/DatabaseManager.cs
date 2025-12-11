using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Database.DataReader;
using Database.Generated;
using UnityEngine.Networking;
using Newtonsoft.Json; // Json.Net
using UnityEngine.UI;
using System.Security.Cryptography;
using System.Text;
using Cardevil.Scriptable.Cache;
using Cardevil.DataStructure;
using Cardevil.DataStructure.Serializables;
using Cardevil.Manager;
using Cysharp.Threading.Tasks;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace Database
{
    /// <summary>
    /// 플레이타임에 로드하는 클래스
    /// </summary>
    public class DatabaseManager : MonoBehaviour, IManager
    {
       

        [SerializeField] private McDatabase mcDatabase = new McDatabase();
        [SerializeField] private bool isInitialized = false;
        [Header("Path Settings")]
        [SerializeField] private string googleSheetUrl = "URL_HERE"; // 구글 시트 URL
        [SerializeField] private string localJsonPath = "Database/";
        [SerializeField] private string streamingAssetPath = "Json/";
        [Header("Settings")]
        [SerializeField, Tooltip("인터넷/로컬 유무와 상관없이 streaming으로 작동")] private bool forceUseStreamingAsset = false;
        [SerializeField] private int timeoutSeconds = 10;
        [Header("Sprite Cache Settings")] // 구분을 위해 헤더 추가
        [SerializeField] private SpriteCacheIndex spriteCacheIndex; // 여기에 MySpriteCacheIndex.asset 파일을 끌어다 놓으세요.

        // 스프라이트 캐시를 위한 Dictionary //
        /// <summary>
        /// 로드된 스프라이트를 관리하는 캐시. Key: 이미지 URL, Value: 로드된 Sprite
        /// </summary>
        [field: SerializeField] public SerializableDictionary<string, Sprite> SpriteCache { get; private set; } = new();


        public event Action OnInitialized;
        public string FinalLocalPath => Path.Combine(Application.persistentDataPath, localJsonPath);
        public string FinalStreamingAssetPath => Path.Combine(Application.streamingAssetsPath, streamingAssetPath);

        public McDatabase Database => mcDatabase;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            // DontDestroyOnLoad(gameObject);
        }
        
        public async UniTask InitializeAsync()
        {
            Initialize();
            await UniTask.WaitUntil(() => isInitialized);
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
            SpriteCache.Clear();



            yield return LoadDatabase();
            // yield return new WaitForSeconds(1f); // 테스트용 딜레이
            yield return LoadAllSpriteResources();



            Debug.Log("[DatabaseManager] 초기화 완료");
            isInitialized = true;
            OnInitialized?.Invoke();
        }

        private IEnumerator LoadDatabase()
        {
            if (forceUseStreamingAsset)
            {
                Debug.Log("[DatabaseManager] 강제 스트리밍 에셋 모드. 로컬 파일을 로드합니다.");
                yield return LoadLocalDf();
            }
            else if (String.IsNullOrEmpty(googleSheetUrl))
            {
                Debug.Log("[DatabaseManager] 구글 시트 URL이 비었습니다. 로컬 파일을 로드합니다.");
                yield return LoadLocalDf();
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
                    yield return LoadLocalDf();
                }
            }
            else
            {
                Debug.Log("[DatabaseManager] 인터넷 연결안됨. 로컬 파일을 로드합니다.");
                yield return LoadLocalDf();
            }
        }

        private IEnumerator LoadLocalDf()
        {
            string json = null;
            List<string> targetClassNames = mcDatabase.ClassNames;
            List<DataFrame> dataFrames = new List<DataFrame>();
            foreach (var className in targetClassNames)
            {
                string fileName = $"{className}.df";
                string localPath = Path.Combine(FinalLocalPath, fileName);
                string streamingPath = Path.Combine(FinalStreamingAssetPath, fileName);
                Debug.Log($"[DatabaseManager] 로컬 파일 로드 시도: {localPath}");
                yield return LoadSavedFile(localPath, result => json = result);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log($"[DatabaseManager] 스트리밍 에셋 로드 시도: {streamingPath}");
                    yield return LoadStreamingAsset(streamingPath, result => json = result);
                }
                if (!string.IsNullOrEmpty(json))
                {
                    DataFrame df = JsonConvert.DeserializeObject<DataFrame>(json);
                    if (df != null)
                    {
                        dataFrames.Add(df);
                    }
                    else
                    {
                        Debug.LogWarning($"[DatabaseManager] {className} 데이터 프레임 역직렬화 실패.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DatabaseManager] {className} 데이터 로드 실패. 파일이 존재하지 않음.");
                }

            }
            
            mcDatabase.InitializeAll(dataFrames);
            
        }
        private IEnumerator LoadLocalJson()
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


        #region Srpite 관련 코드
        private IEnumerator LoadAllSpriteResources()
        {
            List<Coroutine> loadingCoroutines = new List<Coroutine>();
            // 중복된 URL 로드를 방지하기 위해 HashSet을 사용합니다.
            var urlsToLoad = new HashSet<string>();

            // 어떤 URL 목록을 사용할지 상황에 맞게 결정합니다.
            // [에디터 모드]일 경우: DB에서 직접 URL을 가져와 다운로드하고 인덱스를 생성하는 것이 목적입니다.
            if (!Application.isPlaying)
            {
                Debug.Log("[DatabaseManager] 에디터 모드: DB에서 직접 URL을 수집합니다.");
                if (mcDatabase.MachineRewardList != null)
                {
                    foreach (var item in mcDatabase.MachineRewardList)
                    {
                        if (!string.IsNullOrEmpty(item.URL))
                            urlsToLoad.Add(item.URL.Trim()); // URL의 앞뒤 공백 제거
                    }
                }
                // 만약 mobList 등 다른 테이블에도 URL 필드가 있다면 여기에 같은 방식으로 추가합니다.
                // if (mcDatabase.mobList != null) { ... }
            }
            // [플레이 모드]일 경우: 이미 만들어진 인덱스를 사용해 빠르게 메모리에 로드하는 것이 목적입니다.
            else
            {
                Debug.Log("[DatabaseManager] 플레이 모드: SpriteCacheIndex를 사용합니다.");
                if (spriteCacheIndex != null)
                {
                    foreach (var url in spriteCacheIndex.cachedUrls)
                    {
                        urlsToLoad.Add(url);
                    }
                }
            }

            Debug.Log($"[DatabaseManager] 총 {urlsToLoad.Count}개의 고유 URL에 대한 이미지 로드를 시작합니다.");

            // 결정된 URL 목록을 기반으로 이미지 로딩 코루틴을 실행합니다.
            foreach (var url in urlsToLoad)
            {
                // 로딩 시작 전 SpriteCache에 이미 있는지 한번 더 확인하여 중복 실행 방지
                if (!SpriteCache.ContainsKey(url))
                {
                    loadingCoroutines.Add(StartCoroutine(LoadImageCoroutine(url, sprite =>
                    {
                        if (sprite != null)
                        {
                            SpriteCache[url] = sprite;
                        }
                    })));
                }
            }

            // 시작된 모든 코루틴이 끝날 때까지 기다립니다.
            foreach (var coroutine in loadingCoroutines)
            {
                yield return coroutine;
            }

            Debug.Log($"[DatabaseManager] 모든 스프라이트 리소스 로딩 완료. 캐시된 스프라이트 수: {SpriteCache.Count}");
        }
        // <summary>
        /// URL을 기반으로 이미지를 로드하고, 완료 시 콜백으로 Sprite를 반환하는 코루틴
        /// </summary>
        /// <param name="url">이미지 URL</param>
        /// <param name="onComplete">로드 완료 시 실행될 콜백 (결과 Sprite 전달)</param>
        private IEnumerator LoadImageCoroutine(string url, Action<Sprite> onComplete)
        {
            url = ConvertToGoogleDriveDownloadUrl(url);
            Debug.Log($"입력받은 url :{url} 과 cachePath :{GetCachePathForUrl(url)}");
            string cachePath = GetCachePathForUrl(url);
            Sprite resultSprite = null;
            if (File.Exists(cachePath))
            {
                // 캐시 파일이 존재하면 로컬에서 로드
                byte[] fileData = File.ReadAllBytes(cachePath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData)) // 이 함수가 이미지 데이터를 텍스처로 변환
                {
                    // 성공
                    resultSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
                else
                {
                    // 실패
                    Debug.LogError($"[DatabaseManager] Texture.LoadImage FAILED for URL: {url}. The image file might be in an unsupported format.");
                }
            }
            else
            {
                // 캐시 파일이 없으면 웹에서 다운로드
                using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        Texture2D texture = DownloadHandlerTexture.GetContent(www);
                        resultSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        File.WriteAllBytes(cachePath, www.downloadHandler.data);

#if UNITY_EDITOR
                        // 에디터 모드에서 실행했을 경우에만 인덱스에 기록
                        if (!Application.isPlaying && spriteCacheIndex != null)
                        {
                            if (!spriteCacheIndex.cachedUrls.Contains(url))
                            {
                                spriteCacheIndex.cachedUrls.Add(url);
                                EditorUtility.SetDirty(spriteCacheIndex); // 변경사항이 있음을 에디터에 알림
                            }
                        }
#endif

                    }
                    else
                    {
                        Debug.LogWarning($"스프라이트 다운로드 실패: {url} | Error: {www.error}");
                    }
                }
            }

            // 결과 Sprite를 콜백으로 전달
            onComplete?.Invoke(resultSprite);
        }

        /// <summary>
        /// 이미지를 저장할 캐시 디렉토리 경로를 반환. 폴더가 없으면 생성
        /// </summary>
        private string GetCacheDirectory()
        {
            string path = Path.Combine(Application.persistentDataPath, "ImageCache");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        /// <summary>
        /// URL을 기반으로 고유하고 안전한 로컬 파일 경로를 생성
        /// </summary>
        private string GetCachePathForUrl(string url)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(url);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return Path.Combine(GetCacheDirectory(), sb.ToString());
            }
        }

        /// <summary>
        /// URL을 사용해 캐시된 스프라이트를 가져옵니다.
        /// </summary>
        /// <param name="url">JSON 데이터에 있는 이미지 URL</param>
        /// <param name="sprite">찾은 스프라이트를 담을 변수</param>
        /// <returns>스프라이트를 찾았으면 true, 아니면 false</returns>
        public bool TryGetSprite(string url, out Sprite sprite)
        {
            sprite = null;

            // 초기화가 안됐거나 URL이 비어있으면 실패 처리
            if (!isInitialized)
            {
                Debug.LogWarning("[DatabaseManager] 아직 초기화되지 않아서 스프라이트를 가져올 수 없습니다.");
                return false;
            }
            if (string.IsNullOrEmpty(url))
            {
                // URL이 없는건 에러가 아니므로 조용히 실패 처리
                return false;
            }

            // SpriteCache에서 URL을 키로 사용해 스프라이트를 찾아보고, 결과를 bool로 반환
            return SpriteCache.TryGetValue(url, out sprite);
        }

        #endregion

        /// <summary>
        /// 일반 Google Drive 공유 링크를 직접 다운로드 가능한 URL로 변환합니다.
        /// </summary>
        /// <param name="anyUrl">검사 및 변환할 URL</param>
        /// <returns>변환된 URL. 변환이 필요 없거나 실패 시 원본 URL 반환.</returns>
        private string ConvertToGoogleDriveDownloadUrl(string anyUrl)
        {
            // URL이 유효한지, 그리고 우리가 변환하려는 공유 링크 형태인지 확인합니다.
            if (string.IsNullOrEmpty(anyUrl) || !anyUrl.Contains("/file/d/"))
            {
                return anyUrl; // 변환할 필요가 없는 URL(이미 uc?id 형식이거나 다른 URL)은 그대로 반환
            }

            try
            {
                // URL을 "/d/" 기준으로 잘라 파일 ID를 포함한 뒷부분을 얻습니다.
                string[] parts = anyUrl.Split(new string[] { "/d/" }, StringSplitOptions.None);
                // 뒷부분에서 다시 "/" 기준으로 잘라 순수한 파일 ID만 추출합니다.
                string fileId = parts[1].Split('/')[0];

                // 추출된 파일 ID를 직접 다운로드 URL 형식에 맞춰 조합합니다.
                return $"https://drive.google.com/uc?id={fileId}";
            }
            catch (Exception ex)
            {
                // URL 형식이 예상과 달라 오류가 발생할 경우를 대비합니다.
                Debug.LogWarning($"Google Drive URL 변환 실패: {anyUrl} | Error: {ex.Message}. 원본 URL을 사용합니다.");
                return anyUrl; // 실패 시 원본 URL 반환
            }
        }
    }
    
}