namespace Cardevil.Gameplay.Field
{
    // public class FieldWallFloor : MonoBehaviour
    // {
    //     [Header("Reference")] 
    //     [SerializeField] private GameObject[] _leftWalls;
    //     [SerializeField] private GameObject[] _rightWalls;
    //     [Header("Settings")]
    //     [SerializeField] private float _distance = 0.5f;
    //
    //     [SerializeField] private Vector3 _wallScale = new Vector3(2.8f, 0.4f, 1f);
    //
    //
    //     private void OnValidate()
    //     {
    //         for (int i = 0; i < _leftWalls.Length; i++)
    //         {
    //             if (_leftWalls[i] != null)
    //             {
    //                 _leftWalls[i].transform.localScale = _wallScale;
    //                 _leftWalls[i].transform.localPosition = new Vector3(-_distance, 0, i * _distance);
    //             }
    //
    //             if (_rightWalls[i] != null)
    //             {
    //                 _rightWalls[i].transform.localScale = _wallScale;
    //                 _rightWalls[i].transform.localPosition = new Vector3(_distance, 0, i * _distance);
    //             }
    //         }
    //     }
    // }
}