using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public class ScoreData
    {
        public string player_ID;
        public int maxScore;
    }
    public class VolumeData
    {
        public float masterVolume;
        public float bgmVolume;
        public float sfxVolume;
        public VolumeData()
        {
            masterVolume = 0.7f;
            bgmVolume = 0.7f;
            sfxVolume = 0.7f;
        }
    }
    [System.Serializable]
    public class WholeGameData
    {
        public bool firstPlay;
        public int maxScore;

        public WholeGameData()
        {
            firstPlay = false;
            maxScore = 0;
        }
    }


    public enum UseType
    {
        Active,
        Passive
    }
    public enum WorldObject
    {
        Unknown,
        Player,
        Enemy,
    }
    public enum State
    {
        Idle,
        Walk,
        Crouch
    }
    public enum UIEvent
    {
        Click,
        BeginDrag,
        Drag,
        DragEnd,
        PointerDown,
        PointerUP
    }
    public enum MouseEvent
    {
        Press,
        Click,
        End
    }
    
    [Obsolete("Pls Use Scenes instead.")]
    public enum Scene
    {
        Unknown,
        BaseScene,
        TitleScene,
        MainGame,
    }
    public enum Sound
    {
        Master,
        BGM,
        SFX,
        MaxCount
    }
    
    public enum HighlightType
    {
        None,
        DefaultAttack
    }

    public enum SlotRewardType
    {
        Heal,
        FixedGold,
        RandomGold,
        DarkUpgrade,
        StartReroll,
        ExactUpgrade,
        Relic,
        Unknown

    }

    public enum RareType
    {
        Normal,
        Rare,
        Epic,
        Legend
    }
}