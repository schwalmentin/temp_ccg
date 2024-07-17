using System.Threading.Tasks;
using UnityEngine;

public class LobbyOrchestra : MonoBehaviour
{
    /// <summary>
    /// Provides the method to join a match with the lobby manager to unity editor event.
    /// </summary>
    public void PlayGame()
    {
        LobbyManager.Instance.JoinMatch();
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        
        Application.Quit();
    }
}
