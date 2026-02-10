using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.NewCard.InStage
{
    public class StageCardCoreView : MonoBehaviour
    {
        [Header("Buttons")] 
        [SerializeField] private Button useButton;
        [SerializeField] private Button discardButton;
        [SerializeField] private Button sortByNumberButton;
        [SerializeField] private Button sortByIconButton;

        [Header("Texts")] 
        [SerializeField] private TextMeshProUGUI deckCountText;
        [SerializeField] private TextMeshProUGUI discardRemainCountText;
    }
}