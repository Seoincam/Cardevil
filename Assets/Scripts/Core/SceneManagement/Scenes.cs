namespace Cardevil.Core.SceneManagement
{
    public enum Scenes
    {
        Bootstrap,
        Title,
        Stage,
        World,
        Shop
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