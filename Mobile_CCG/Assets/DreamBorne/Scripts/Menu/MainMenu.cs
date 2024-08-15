using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Image playButtonImage;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [Space]
    [SerializeField] private CardDeck[] cardDecks;

    [SerializeField] private Color unselectedColorText = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private Color selectedColorText = new Color(0.6f, 0.6f, 0.6f);
    [Space]
    [SerializeField] private Color unselectedColorPictures = new Color(0.4f, 0.4f, 0.4f);
    [Space]
    [SerializeField] private Color unselectedColorButton = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private Color selectedColorButton = new Color(0.6f, 0.6f, 0.6f);

    private int currentDeck;
    private CardDeck currentCardDeck;

    private void Awake()
    {
        foreach (CardDeck cardDeck in this.cardDecks)
        {
            cardDeck.deckIcon.color = this.unselectedColorPictures;
            cardDeck.deckName.color = this.unselectedColorText;
        }

        this.playButtonImage.color = this.unselectedColorButton;
        this.playButtonText.color = this.unselectedColorButton;
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
        
        // Enable play button
        this.playButton.interactable = true;
        this.playButtonImage.color = this.selectedColorButton;
        this.playButtonText.color = this.selectedColorButton;

        
        // Unselect current deck
        if (this.currentCardDeck.deckIcon != null)
        {
            this.currentCardDeck.deckIcon.color = this.unselectedColorPictures;
            this.currentCardDeck.deckName.color = this.unselectedColorText;
        }
        
        // Select new deck
        this.currentCardDeck = this.cardDecks[deck];
        this.currentCardDeck.deckIcon.color = new Color(1, 1, 1);
        this.currentCardDeck.deckName.color = this.selectedColorText;
        this.currentDeck = deck;
        
        // Submit deck
        LobbyManager.Instance.Deck = this.cardDecks[this.currentDeck].cards;
    }
}

[System.Serializable]
public struct CardDeck
{
    public Image deckIcon;
    public TextMeshProUGUI deckName;
    public int[] cards;
}