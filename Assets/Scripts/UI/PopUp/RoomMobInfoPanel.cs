using Cardevil.Core.Utils;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cardevil.UI.PopUp
{
    // 일단 지금은 DTO로
    public struct MobInfo
    {
        public string Name;
        public string Description;
        public string Rank;
        public string AdditionalInfo;
        
        public MobInfo(string name, string description, string rank, string additionalInfo)
        {
            Name = name;
            Description = description;
            Rank = rank;
            AdditionalInfo = additionalInfo;
        }

        public static MobInfo Empty => new MobInfo(string.Empty, string.Empty, string.Empty, string.Empty);
        public static MobInfo Test => new MobInfo("위선의 패", "HP가 66.6% 미만이 되면, 그 이후의 공격부터 한단계 상위 족보를 뽑기 시작합니다.", "하이카드, 원 페어", "<sprite name=\"GoldCoin\"> 6");
    }
    
    public class RoomMobInfoPanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI rankLabel;
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI additionalInfoLabel;
        [SerializeField] private TextMeshProUGUI additionalInfoText;
        
        public CanvasGroup CanvasGroup => canvasGroup;
        
        private void Reset()
        {
            canvasGroup = gameObject.GetOrAddComponent<CanvasGroup>();
        }

        private void Awake()
        {
        }

        public void ClearMobInfo()
        {
            
        }
        
        public void SetMobInfo(MobInfo mobInfo)
        {
            SetMobInfo(mobInfo.Name, mobInfo.Description, mobInfo.Rank.ToString(), mobInfo.AdditionalInfo);
        }
        
        public void SetMobInfo(string title, string description, string rank, string additionalInfo)
        {
            titleText.text = title;
            descriptionText.text = description;
            rankText.text = rank;
            additionalInfoText.text = additionalInfo;
        }
        

        


    }
}