using Cardevil.Core.Utils;
using System;
using UnityEngine;

namespace Cardevil
{
    public class DungeonCanvas : MonoBehaviour
    {
        private static DungeonCanvas _instance;

        public static DungeonCanvas Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DungeonCanvas>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                LogEx.LogWarning("Multiple instances detected. Destroying duplicate.");
                _instance = this;
            }
        }

        void Start()
        {
        
        }

        void Update()
        {
        
        }
    }
}
