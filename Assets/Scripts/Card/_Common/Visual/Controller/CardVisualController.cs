using UnityEngine;

namespace Cardevil.Card.Visual.Controller
{
    public class CardVisualController : CardVisualControllerBase
    {
        [Header("Sprite Specific Prefabs")]
        [SerializeField] private GameObject trailPrefab;
        private TrailRenderer _currentTrail;

        public float TrailTime => _currentTrail?.time ?? 0f;

        public void SetTrail()
        {
            _currentTrail = Instantiate(trailPrefab, transform).GetComponent<TrailRenderer>();
        }

        public void ClearTrail()
        {
            if (_currentTrail)
            {
                Destroy(_currentTrail);
            }
        }
    }
}
