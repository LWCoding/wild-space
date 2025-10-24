using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

/// <summary>
/// Singleton manager that logs all finished dialogue lines and provides them to other systems.
/// This system captures dialogue lines after they have been fully displayed to the user.
/// </summary>
public class DialogueHistoryManager : MonoBehaviour
{
    public static DialogueHistoryManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject historyPanel;      // The parent panel to show/hide
    [SerializeField] private Button historyButton;        // UI Button for toggling history
    [SerializeField] private Button previousPageButton;    // UI Button for previous page
    [SerializeField] private Button nextPageButton;       // UI Button for next page
    [SerializeField] private TMP_Text historyText;         // Text box under ScrollView > Content
    [SerializeField] private TMP_Text pageLabel;           // Page indicator (optional)
    [SerializeField] private ScrollRect scrollRect;        // ScrollRect to handle scrolling

    [Header("UI Settings")]
    [SerializeField] private int entriesPerPage = 20;     // Number of dialogue entries per page
    [SerializeField] private string openHistoryText = "Open History [H]";
    [SerializeField] private string closeHistoryText = "Close History [H]";

    // Events for other systems to subscribe to
    public static event Action<DialogueHistoryEntry> OnDialogueAdded;

    // Internal storage
    private List<DialogueHistoryEntry> dialogueHistory = new List<DialogueHistoryEntry>();
    private StringBuilder historyStringBuilder = new StringBuilder();
    private int currentPageIndex = 0;

    /// <summary>
    /// Represents a single entry in the dialogue history
    /// </summary>
    [System.Serializable]
    public class DialogueHistoryEntry
    {
        public string characterName;
        public string dialogueText;
        public DateTime timestamp;

        public DialogueHistoryEntry(string characterName, string dialogueText)
        {
            this.characterName = characterName;
            this.dialogueText = dialogueText;
            this.timestamp = DateTime.Now;
        }

        public string GetFormattedText()
        {
            StringBuilder sb = new StringBuilder();

            if (!string.IsNullOrEmpty(characterName))
            {
                sb.Append($"{characterName}: ");
            }

            sb.Append(dialogueText);
            return sb.ToString();
        }
    }

    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeHistorySystem();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Automatically wire up the history button if it's assigned
        SetupHistoryButton();
        SetupPageNavigationButtons();
        SetInitialButtonText();
        UpdatePageNavigationButtons();
    }

    void OnEnable()
    {
        // Subscribe to our own events for UI updates
        OnDialogueAdded += OnDialogueAddedInternal;
    }

    void OnDisable()
    {
        // Unsubscribe from our own events
        OnDialogueAdded -= OnDialogueAddedInternal;
    }

    void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (Instance == this)
        {
            LinePresenter.OnDialogueLineCompleted -= OnDialogueLineCompleted;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHistory();
        }
    }

    /// <summary>
    /// Initialize the history system by subscribing to YarnSpinner events
    /// </summary>
    private void InitializeHistorySystem()
    {
        // Subscribe to the LinePresenter's dialogue line completed event
        // This ensures we only log lines after they've been fully displayed
        LinePresenter.OnDialogueLineCompleted += OnDialogueLineCompleted;
        
        Debug.Log("DialogueHistoryManager initialized and subscribed to dialogue events");
    }

    /// <summary>
    /// Called when a dialogue line has been completed and displayed
    /// This is where we capture the completed dialogue line for history
    /// </summary>
    /// <param name="completedLine">The completed dialogue line</param>
    private void OnDialogueLineCompleted(LocalizedLine completedLine)
    {
        // Extract character name and dialogue text
        string characterName = completedLine.CharacterName ?? "";
        string dialogueText = completedLine.TextWithoutCharacterName.Text;
        
        // Add to history
        AddDialogueToHistory(characterName, dialogueText);
    }

    /// <summary>
    /// Add a dialogue line to the history (called by modified LinePresenter)
    /// </summary>
    /// <param name="characterName">Name of the character speaking</param>
    /// <param name="dialogueText">The dialogue text</param>
    public void AddDialogueToHistory(string characterName, string dialogueText)
    {
        // Create new history entry
        var entry = new DialogueHistoryEntry(characterName, dialogueText);
        
        // Add to history list
        dialogueHistory.Add(entry);
        
        // Notify subscribers with the new entry
        OnDialogueAdded?.Invoke(entry);
        
        Debug.Log($"Added to dialogue history: {entry.GetFormattedText()}");
    }

    /// <summary>
    /// Internal handler for when dialogue is added (for UI updates)
    /// </summary>
    private void OnDialogueAddedInternal(DialogueHistoryEntry entry)
    {
        // Calculate total pages
        int totalPages = GetTotalPages();

        // Always jump to the last page when new dialogue is added
        currentPageIndex = totalPages - 1;
        RefreshPage(scrollToBottom: true);
    }

    /// <summary>
    /// Refreshes the current page, optionally scrolling to the bottom.
    /// </summary>
    public void RefreshPage(bool scrollToBottom = false)
    {
        int totalPages = GetTotalPages();
        
        if (totalPages == 0)
        {
            if (historyText != null)
                historyText.text = "<i>No history yet...</i>";
            if (pageLabel != null) pageLabel.text = "";
            return;
        }

        currentPageIndex = Mathf.Clamp(currentPageIndex, 0, totalPages - 1);
        var pageEntries = GetPageEntries(currentPageIndex);

        if (historyText != null)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (var entry in pageEntries)
            {
                string speaker = string.IsNullOrEmpty(entry.characterName) ? "Narrator" : entry.characterName;
                string line = entry.dialogueText.Replace("\n", " ");

                bool isPlayerOrNarrator =
                    speaker.Equals("You", System.StringComparison.OrdinalIgnoreCase) ||
                    speaker.Equals("Narrator", System.StringComparison.OrdinalIgnoreCase);

                // Use italics for narrator/player, normal text for other characters
                string italicTag = isPlayerOrNarrator ? "<i>" : "";
                string italicCloseTag = isPlayerOrNarrator ? "</i>" : "";

                sb.AppendLine($"<b>{speaker}:</b> {italicTag}{line}{italicCloseTag}");
            }

            historyText.text = sb.ToString();
        }

        // Update label
        if (pageLabel != null)
            pageLabel.text = $"{currentPageIndex + 1}/{totalPages}";

        // Scroll logic
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            if (scrollToBottom)
                StartCoroutine(ScrollToBottomNextFrame());
            else
                scrollRect.verticalNormalizedPosition = 0f; // 0 = bottom, 1 = top
        }

        // Update page navigation button states
        UpdatePageNavigationButtons();
    }

    public void PreviousPage()
    {
        int totalPages = GetTotalPages();
        if (totalPages == 0) return;

        if (currentPageIndex > 0)
        {
            currentPageIndex--;
            RefreshPage();
            UpdatePageNavigationButtons();
        }
    }

    public void NextPage()
    {
        int totalPages = GetTotalPages();
        if (totalPages == 0) return;

        if (currentPageIndex < totalPages - 1)
        {
            currentPageIndex++;
            RefreshPage();
            UpdatePageNavigationButtons();
        }
    }

    public void ToggleHistory()
    {
        if (historyPanel == null)
        {
            Debug.LogWarning("No history panel assigned!");
            return;
        }

        // Check if history is currently open
        bool isHistoryOpen = historyPanel.activeSelf;

        // Toggle the history panel
        historyPanel.SetActive(!isHistoryOpen);

        // Update the button text if button is assigned
        if (historyButton != null)
        {
            TMP_Text buttonLabel = historyButton.GetComponentInChildren<TMP_Text>();
            if (buttonLabel != null)
            {
                buttonLabel.text = isHistoryOpen ? openHistoryText : closeHistoryText;
            }
        }

        // If we just opened the panel, refresh to show the latest content
        if (!isHistoryOpen)
        {
            int totalPages = GetTotalPages();
            if (totalPages > 0)
            {
                currentPageIndex = totalPages - 1;
                RefreshPage(scrollToBottom: true);
            }
        }
    }

    /// <summary>
    /// Public method to be called by Unity Button OnClick events
    /// This is the method you should connect to your history button
    /// </summary>
    public void OnHistoryButtonClicked()
    {
        ToggleHistory();
    }

    /// <summary>
    /// Automatically sets up the history button to call ToggleHistory when clicked
    /// </summary>
    private void SetupHistoryButton()
    {
        if (historyButton != null)
        {
            // Remove any existing listeners to avoid duplicates
            historyButton.onClick.RemoveAllListeners();
            // Add our toggle method as a listener
            historyButton.onClick.AddListener(OnHistoryButtonClicked);
        }
    }

    /// <summary>
    /// Public method to manually set up the history button
    /// Call this if you want to wire up the button programmatically
    /// </summary>
    public void SetHistoryButton(Button button)
    {
        historyButton = button;
        SetupHistoryButton();
    }

    /// <summary>
    /// Automatically sets up the page navigation buttons
    /// </summary>
    private void SetupPageNavigationButtons()
    {
        if (previousPageButton != null)
        {
            previousPageButton.onClick.RemoveAllListeners();
            previousPageButton.onClick.AddListener(PreviousPage);
        }

        if (nextPageButton != null)
        {
            nextPageButton.onClick.RemoveAllListeners();
            nextPageButton.onClick.AddListener(NextPage);
        }
    }

    /// <summary>
    /// Updates the enabled state of page navigation buttons
    /// </summary>
    private void UpdatePageNavigationButtons()
    {
        int totalPages = GetTotalPages();
        
        if (previousPageButton != null)
        {
            previousPageButton.interactable = currentPageIndex > 0;
        }
        
        if (nextPageButton != null)
        {
            nextPageButton.interactable = currentPageIndex < totalPages - 1;
        }
    }

    /// <summary>
    /// Sets the initial button text when the game starts
    /// </summary>
    private void SetInitialButtonText()
    {
        if (historyButton != null)
        {
            TMP_Text buttonLabel = historyButton.GetComponentInChildren<TMP_Text>();
            if (buttonLabel != null)
            {
                // Set initial text based on whether history panel is open or closed
                bool isHistoryOpen = historyPanel != null && historyPanel.activeSelf;
                buttonLabel.text = isHistoryOpen ? closeHistoryText : openHistoryText;
            }
        }
    }

    /// <summary>
    /// Waits a frame, then scrolls to the bottom.
    /// This prevents Unity's layout system from snapping back up.
    /// </summary>
    private IEnumerator ScrollToBottomNextFrame()
    {
        yield return null; // wait one frame
        if (scrollRect != null)
        {
            // Force update canvases to ensure layout is complete
            Canvas.ForceUpdateCanvases();
            // Set to 0 to scroll to the very bottom
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    /// <summary>
    /// Calculates the total number of pages based on entries per page
    /// </summary>
    private int GetTotalPages()
    {
        if (dialogueHistory.Count == 0) return 0;
        return Mathf.CeilToInt((float)dialogueHistory.Count / entriesPerPage);
    }

    /// <summary>
    /// Gets the entries for a specific page
    /// </summary>
    private List<DialogueHistoryEntry> GetPageEntries(int pageIndex)
    {
        int startIndex = pageIndex * entriesPerPage;
        int endIndex = Mathf.Min(startIndex + entriesPerPage, dialogueHistory.Count);
        
        var pageEntries = new List<DialogueHistoryEntry>();
        for (int i = startIndex; i < endIndex; i++)
        {
            pageEntries.Add(dialogueHistory[i]);
        }
        
        return pageEntries;
    }

    /// <summary>
    /// Get all dialogue history entries (for external access)
    /// </summary>
    public List<DialogueHistoryEntry> GetAllHistoryEntries()
    {
        return new List<DialogueHistoryEntry>(dialogueHistory);
    }

    /// <summary>
    /// Clear all dialogue history
    /// </summary>
    public void ClearHistory()
    {
        dialogueHistory.Clear();
        currentPageIndex = 0;
        RefreshPage();
    }

    /// <summary>
    /// Get the current history count
    /// </summary>
    public int GetHistoryCount()
    {
        return dialogueHistory.Count;
    }
    
}
