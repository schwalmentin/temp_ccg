using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class CustomSceneManager : Singleton<CustomSceneManager> 
{
    #region Scene Manager Methods

        /// <summary>
        /// Either switch to (additive = false) or add (additive = true) a scene asynchronously based on the parameter sceneName.
        /// </summary>
        /// <param name="sceneName"></param>
        public Task SwitchSceneAsync(string sceneName, bool additive)
        {
            SceneManager.LoadSceneAsync(sceneName, additive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            return Task.CompletedTask;
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
        /// Switch the current scene asynchronous based on the parameter sceneName during a host session.
        /// </summary>
        /// <param name="sceneName"></param>
        public void SwitchNetworkScene(string sceneName)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

    #endregion
}
