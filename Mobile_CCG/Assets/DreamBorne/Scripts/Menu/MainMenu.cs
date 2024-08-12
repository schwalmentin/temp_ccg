using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [Space]
    [SerializeField] private CardDeck[] cardDecks;

    [SerializeField] private Color unselectedColor = new Color(0.4f, 0.4f, 0.4f);
    private int currentDeck;
    private Image currentImage;

    private void Awake()
    {
        foreach (CardDeck cardDeck in this.cardDecks)
        {
            cardDeck.deckIcon.color = this.unselectedColor;
        }
    }

    /// <summary>
    /// Provides the function to join a match with the lobby manager to unity editor event.
    /// </summary>
    public void PlayGame()
    {
        LobbyManager.Instance.JoinMatch();
    }

    /// <summary>
    /// Provides the function to quit a game from the editor or a build.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
        
        Application.Quit();
    }

    public void SelectDeck(int deck)
    {
        // Check if deck is valid
        if (this.cardDecks.Length < deck || deck < 0) return;
        this.playButton.interactable = true;
        
        // Unselect current deck
        if (this.currentImage != null)
        {
            this.currentImage.color = this.unselectedColor;
        }
        
        // Select new deck
        this.currentImage = this.cardDecks[deck].deckIcon;
        this.currentImage.color = new Color(1, 1, 1);
        this.currentDeck = deck;
        
        // Submit deck
        LobbyManager.Instance.Deck = this.cardDecks[this.currentDeck].cards;
    }
}

[System.Serializable]
public struct CardDeck
{
    public Image deckIcon;
    public int[] cards;
}