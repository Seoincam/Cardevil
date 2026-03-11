namespace Cardevil.Core.Systems.Sounds
{
    public partial struct SoundReference
    {
        public static SoundReference Empty => new SoundReference(string.Empty);
        
        #region Music
        public static SoundReference Music_1Stage             => new SoundReference("Sounds/Music/1Stage");
        public static SoundReference Music_2Stage             => new SoundReference("Sounds/Music/2Stage");
        public static SoundReference Music_3Stage             => new SoundReference("Sounds/Music/3Stage");
        public static SoundReference Music_CelestialTheme     => new SoundReference("Sounds/Music/Celestial Theme(VolumeReference)");
        public static SoundReference Music_Dealing            => new SoundReference("Sounds/Music/Dealing");
        public static SoundReference Music_Funny              => new SoundReference("Sounds/Music/Funny");
        public static SoundReference Music_MainTheme          => new SoundReference("Sounds/Music/MainTheme");
        public static SoundReference Music_RandomEncounter    => new SoundReference("Sounds/Music/RandomEncounter");
 
        #endregion
        
        #region SFX
        public static SoundReference SFX_CardDiscard          => new SoundReference("Sounds/SFX/CardDiscard");
        public static SoundReference SFX_CardDrop             => new SoundReference("Sounds/SFX/CardDrop");
        public static SoundReference SFX_CardFocus1           => new SoundReference("Sounds/SFX/CardFocus1");
        public static SoundReference SFX_CardFocus2           => new SoundReference("Sounds/SFX/CardFocus2");
        public static SoundReference SFX_CardFocus3           => new SoundReference("Sounds/SFX/CardFocus3");
        public static SoundReference SFX_CardGoingIntoDeck1   => new SoundReference("Sounds/SFX/CardGoingIntoDeck1");
        public static SoundReference SFX_CardGoingIntoDeck2   => new SoundReference("Sounds/SFX/CardGoingIntoDeck2");
        public static SoundReference SFX_CardGoingIntoDeck3   => new SoundReference("Sounds/SFX/CardGoingIntoDeck3");
        public static SoundReference SFX_CardPick             => new SoundReference("Sounds/SFX/CardPick");
        public static SoundReference SFX_FlushBlack           => new SoundReference("Sounds/SFX/FlushBlack");
        public static SoundReference SFX_FlushGreen           => new SoundReference("Sounds/SFX/FlushGreen");
        public static SoundReference SFX_FlushRed             => new SoundReference("Sounds/SFX/FlushRed");
        public static SoundReference SFX_Move                 => new SoundReference("Sounds/SFX/Move");
        public static SoundReference SFX_PlayerFall           => new SoundReference("Sounds/SFX/PlayerFall");
        public static SoundReference SFX_PlayHand             => new SoundReference("Sounds/SFX/PlayHand");
        public static SoundReference SFX_PlayingCardTrigger   => new SoundReference("Sounds/SFX/PlayingCardTrigger");
        public static SoundReference SFX_Purchase             => new SoundReference("Sounds/SFX/Purchase");
        public static SoundReference SFX_RankSelect           => new SoundReference("Sounds/SFX/RankSelect");
        public static SoundReference SFX_ScoreIncrease        => new SoundReference("Sounds/SFX/ScoreIncrease");
        public static SoundReference SFX_Select               => new SoundReference("Sounds/SFX/Select");
        public static SoundReference SFX_SlotMachine          => new SoundReference("Sounds/SFX/SlotMachine");
        public static SoundReference SFX_SlotMachineStop      => new SoundReference("Sounds/SFX/SlotMachineStop");
        public static SoundReference SFX_UpgradeCard1         => new SoundReference("Sounds/SFX/UpgradeCard1");
        public static SoundReference SFX_UpgradeCard2         => new SoundReference("Sounds/SFX/UpgradeCard2");
        public static SoundReference SFX_UpgradeCard3         => new SoundReference("Sounds/SFX/UpgradeCard3");
        public static SoundReference SFX_UpgradeFall          => new SoundReference("Sounds/SFX/UpgradeFall");
        public static SoundReference SFX_UpgradeKeep          => new SoundReference("Sounds/SFX/UpgradeKeep");
        #endregion
        
        #region Monster SFX
        public static SoundReference SFX_MonsterCandleHit         => new SoundReference("Sounds/SFX/Monster/MonsterCandleHit");
        public static SoundReference SFX_MonsterCardHit           => new SoundReference("Sounds/SFX/Monster/MonsterCardHit");
        public static SoundReference SFX_MonsterDemonHit          => new SoundReference("Sounds/SFX/Monster/MonsterDemonHit");
        public static SoundReference SFX_MonsterDogHit            => new SoundReference("Sounds/SFX/Monster/MonsterDogHit");
        public static SoundReference SFX_MonsterFlameHit          => new SoundReference("Sounds/SFX/Monster/MonsterFlameHit");
        public static SoundReference SFX_MonsterGolemHit          => new SoundReference("Sounds/SFX/Monster/MonsterGolemHit");
        public static SoundReference SFX_MonsterGolemRockDestroy  => new SoundReference("Sounds/SFX/Monster/MonsterGolemRockDestroy");
        public static SoundReference SFX_MonsterGolemRockHit      => new SoundReference("Sounds/SFX/Monster/MonsterGolemRockHit");
        public static SoundReference SFX_MonsterGolemRockPlace    => new SoundReference("Sounds/SFX/Monster/MonsterGolemRockPlace");
        public static SoundReference SFX_MonsterMimicHit          => new SoundReference("Sounds/SFX/Monster/MonsterMimicHit");
        public static SoundReference SFX_MonsterMirrorHit         => new SoundReference("Sounds/SFX/Monster/MonsterMirrorHit");
        #endregion
        
        private string soundPath;
        
        public SoundReference(string soundPath)
        {
            this.soundPath = soundPath;
        }
        
        public string SoundPath => soundPath;
        
        
        public static implicit operator string(SoundReference soundReference)
        {
            return soundReference.soundPath;
        }
    }
}