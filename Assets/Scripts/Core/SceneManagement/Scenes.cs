namespace Cardevil.Core.SceneManagement
{
    public enum Scenes
    {
        Bootstrap = 0,
        Title = 1,
        Stage = 2,
        World = 3,
        Shop = 4,
        BlackMarket = 5,
        GoodPlace = 6,
        Heal = 7,
    }
    
    
    public static class ScenesHelper
    {
        public static string GetSceneName(this Scenes scene)
        {
            return SceneReference.Find(scene).SceneName;
        }
        
        public static SceneReference GetSceneReference(this Scenes scene)
        {
            return SceneReference.Find(scene);
        }
    }
}
