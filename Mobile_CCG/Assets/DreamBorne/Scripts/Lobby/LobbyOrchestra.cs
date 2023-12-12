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

    /// <summary>
    /// Provides the method to close a match with the lobby manager to unity editor event.
    /// </summary>
    public void CancelGame()
    {
        LobbyManager.Instance.CloseMatch();
    }
}
