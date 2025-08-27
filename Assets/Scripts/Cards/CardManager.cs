using Cardevil.Systems;
using UnityEngine;

namespace Cardevil.Cards
{
    public class CardManager
    {
        public ICardHandBar handBar;
        public ITurnPlayerInput playerInput;

        public void Init()
        {
            var handBarObj = GameObject.Find("CardHandBar");
            if (handBarObj == null) Debug.LogError("CardHandBar이 씬 내 존재하지 않습니다.");
            handBar = handBarObj.GetComponent<ICardHandBar>();
            playerInput = handBarObj.GetComponent<ITurnPlayerInput>();

            handBar.Init();
        }
    }
}

