using Cardevil.Attributes;
using UnityEngine;
using Image = UnityEngine.UI.Image;

namespace Cardevil.Dungeon.UI
{
    public class LineUI : MonoBehaviour
    {
        [SerializeField, VisibleOnly] private Vector3 startPos = Vector3.zero;
        [SerializeField, VisibleOnly] private Vector3 endPos = Vector3.up * 4;
        
        [SerializeField] private Color lineColor = Color.white;
        [SerializeField] private Image lineImage;
        public Vector3 StartPos
        {
            get => startPos;
            set
            {
                startPos = value;
                UpdateLine();
            }
        }
        public Vector3 EndPos
        {
            get => endPos;
            set
            {
                endPos = value;
                UpdateLine();
            }
        }
        
        private void UpdateLine()
        {
            Vector3 direction = endPos - startPos;
            float distance = direction.magnitude;
            transform.position = startPos + direction / 2;
            transform.up = direction.normalized;
            transform.localScale = new Vector3(transform.localScale.x, distance / 2, transform.localScale.z);
        }
    }
}