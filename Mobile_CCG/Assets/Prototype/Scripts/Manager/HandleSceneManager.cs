using UnityEngine.SceneManagement;

public class HandleSceneManager : Singleton<HandleSceneManager> 
{
    /// <summary>
    /// Switch the current scene asyncronous based on the parameter sceneName
    /// </summary>
    /// <param name="sceneName"></param>
    public void SwitchSceneAsync(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
