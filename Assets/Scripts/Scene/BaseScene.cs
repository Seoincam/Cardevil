using Cardevil.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

[Obsolete()]
public abstract class BaseScene : MonoBehaviour
{

    public Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;
    void Awake()
    {
        Init();
    }

    protected virtual void Init()
    {
        Object obj = GameObject.FindObjectOfType(typeof(EventSystem));
        if (obj == null)
        {
            AssetUtil.Instantiate("UI/EventSystem").name = "@EventSystem";
        }
    }
    public abstract void Clear();
}