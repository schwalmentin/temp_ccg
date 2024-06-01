using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CustomSceneManager : Singleton<CustomSceneManager> 
{
    #region Scene Manager Methods

        /// <summary>
        /// Switch the current scene asyncronous based on the parameter sceneName.
        /// </summary>
        /// <param name="sceneName"></param>
        public void SwitchSceneAsync(string sceneName, bool additive)
        {
            SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        }

        /// <summary>
        /// Switch the current scene immediately based on the parameter sceneName.
        /// </summary>
        /// <param name="sceneName"></param>
        public void SwitchScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        /// <summary>
        /// Switch the current scene asyncronous based on the parameter sceneName during a host session.
        /// </summary>
        /// <param name="sceneName"></param>
        public void SwitchNetworkScene(string sceneName)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

    #endregion
}
