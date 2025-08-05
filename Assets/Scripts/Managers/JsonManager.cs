using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class JsonManager
{
    public void Save<T>(T data, string name = null)
    {
        //안드로이드에서의 저장 위치를 다르게 해주어야 한다
        //Application.dataPath를 이용하면 어디로 가는지는 구글링 해보길 바란다
        //안드로이드의 경우에는 데이터조작을 막기위해 2진데이터로 변환을 해야한다
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name + ".json";
        }
        string savePath = Application.dataPath;
        string appender = $"/userData/{name}";
#if UNITY_EDITOR_WIN

#endif
#if UNITY_ANDROID
        savePath = Application.persistentDataPath;
 
#endif
        StringBuilder builder = new StringBuilder(savePath);
        builder.Append(appender);
        string jsonText = JsonUtility.ToJson(data, true);
        //이러면은 일단 데이터가 텍스트로 변환이 된다
        //jsonUtility를 이용하여 data인 WholeGameData를 json형식의 text로 바꾸어준다
        //파일스트림을 이렇게 지정해주고 저장해주면된당 끗
        FileStream fileStream = new FileStream(builder.ToString(), FileMode.Create);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonText);
        fileStream.Write(bytes, 0, bytes.Length);
        fileStream.Close();
    }
    public T Load<T>(string name = null) where T : new()
    {
        T data;
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name + ".json";
        }
        string loadPath = Application.dataPath;
        string directory = "/userData";
        string appender = $"/{name}";
#if UNITY_EDITOR_WIN

#endif

#if UNITY_ANDROID
        loadPath = Application.persistentDataPath;
#endif
        StringBuilder builder = new StringBuilder(loadPath);
        builder.Append(directory);
        //위까지는 세이브랑 똑같다
        //파일스트림을 만들어준다. 파일모드를 open으로 해서 열어준다. 다 구글링이다
        if (!Directory.Exists(builder.ToString()))
        {
            //디렉토리가 없는경우 만들어준다
            Directory.CreateDirectory(builder.ToString());
        }
        builder.Append(appender);

        if (File.Exists(builder.ToString()))
        {
            //세이브 파일이 있는경우
            FileStream stream = new FileStream(builder.ToString(), FileMode.Open);

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            stream.Close();
            string jsonData = Encoding.UTF8.GetString(bytes);

            //텍스트를 string으로 바꾼다음에 FromJson에 넣어주면은 우리가 쓸 수 있는 객체로 바꿀 수 있다
            data = JsonUtility.FromJson<T>(jsonData);
        }
        else
        {
            TextAsset textAsset = Resources.Load<TextAsset>($"Data/{typeof(T).Name}");

            if (textAsset == null) // Json파일찾기
            {
                Debug.LogError($"파일을 찾을 수 없습니다: Data/{typeof(T).Name}");
                return default;
            }

            Debug.Log(textAsset.text);
            data = JsonUtility.FromJson<T>(textAsset.text);
            Save<T>(data);
        }
        return data;
        //이 정보를 게임매니저나, 로딩으로 넘겨주는 것이당
    }

}
