using UnityEngine;
using System.Collections.Generic;

namespace Cardevil.Scriptable.Cache
{
    [CreateAssetMenu(fileName = "SpriteCacheIndex", menuName = "Database/Sprite Cache Index")]
    public class SpriteCacheIndex : ScriptableObject
    {
        public List<string> cachedUrls = new List<string>();
    }
}
