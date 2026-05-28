using UnityEngine;
using UnityEngine.EventSystems;

namespace Cardevil.UI
{
    public class EnsureSingleEventSystem : MonoBehaviour
    {
        private void Awake()
        {
            var eventSystems = FindObjectsByType<EventSystem>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None);

            foreach (var eventSystem in eventSystems)
            {
                if (eventSystem && eventSystem.gameObject != gameObject)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
    }
}
