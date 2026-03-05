namespace Cardevil.Sounds
{
    public partial struct SoundReference
    {
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