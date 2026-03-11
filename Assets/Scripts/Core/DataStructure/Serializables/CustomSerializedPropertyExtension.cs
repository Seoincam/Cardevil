#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace Cardevil.Core.DataStructure.Serializables
{
    public static class CustomSerializedPropertyExtension
    {
        public static IEnumerable<SerializedProperty> GetChildProperties (this SerializedProperty parent, int depth = 1)
        {
            parent = parent.Copy();

            int depthOfParent = parent.depth;
            var enumerator = parent.GetEnumerator();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current is not SerializedProperty childProperty)
                {
                    continue;
                }
                if (childProperty.depth > (depthOfParent + depth))
                {
                    continue;
                }
                yield return childProperty.Copy();
            }
        }
    }
}
#endif