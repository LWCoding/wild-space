using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    
    // Centralized typing state management
    private static bool _isTypingInProgress = false;
    private int _activeTypingCount = 0; // Track how many YarnTypeToFinish scripts are typing

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
        // Automatically wire up the history buttons if they're assigned
        SetupHistoryButtons();
        SetupPageNavigationButtons();
        SetInitialButtonText();
        UpdatePageNavigationButtons();
    }

    void OnEnable()
    {
        // Subscribe to our own events for UI updates
        OnDialogueAdded += OnDialogueAddedInternal;
        
        // Subscribe to YarnSpinner typewriter events
        LinePresenter.OnTypewriterFinished += OnYarnTypewriterFinished;
    }

    void OnDisable()
    {
        // Unsubscribe from our own events
        OnDialogueAdded -= OnDialogueAddedInternal;
        
        // Unsubscribe from YarnSpinner events
        LinePresenter.OnTypewriterFinished -= OnYarnTypewriterFinished;
    }

    void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (Instance == this)
        {
            LinePresenter.OnDialogueLineCompleted -= OnDialogueLineCompleted;
            LinePresenter.OnTypewriterFinished -= OnYarnTypewriterFinished;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            // Do not handle H if the player is typing in a TMP_InputField
            if (IsTMPInputFieldFocused())
            {
                return;
            }

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
    /// Returns true if a TMP_InputField currently has focus (user typing in an input field).
    /// </summary>
    private bool IsTMPInputFieldFocused()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        var selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null)
        {
            return false;
        }

        var tmpInput = selected.GetComponent<TMP_InputField>();
        return tmpInput != null && tmpInput.isFocused;
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
        
        // Force scroll to bottom when new dialogue is added
        StartCoroutine(ScrollToBottomOnNewDialogue());
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
            {
                StartCoroutine(ScrollToBottomNextFrame());
            }
            else
            {
                // Only set scroll position if content actually needs scrolling
                StartCoroutine(ResetScrollPositionIfNeeded());
            }
            
            // Force scroll bar refresh for page changes
            StartCoroutine(ForceScrollBarRefresh());
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

        // Don't allow history toggle if typing is currently in progress
        if (_isTypingInProgress)
        {
            Debug.Log("Cannot toggle history while typing is in progress");
            return;
        }

        // Check if history is currently open
        bool wasHistoryOpen = historyPanel.activeSelf;

        // Toggle the history panel
        historyPanel.SetActive(!wasHistoryOpen);

        // Determine new state after toggling
        bool isHistoryOpen = historyPanel.activeSelf;

        // Update the button text using the new state
        UpdateButtonTexts(isHistoryOpen);

        // If we just opened the panel, refresh to show the latest content
        if (isHistoryOpen)
        {
            int totalPages = GetTotalPages();
            if (totalPages > 0)
            {
                currentPageIndex = totalPages - 1;
                RefreshPage(scrollToBottom: true);
            }
            else
            {
                // Ensure scroll state is still sane even with no pages
                RefreshPage(scrollToBottom: false);
            }
        }
    }

    /// <summary>
    /// Public method to be called by Unity Button OnClick events
    /// This is the method you should connect to your history button
    /// </summary>
    public void OnHistoryButtonClicked()
    {
        // Don't allow history toggle if typing is currently in progress
        if (_isTypingInProgress)
        {
            Debug.Log("Cannot toggle history while typing is in progress");
            return;
        }
        
        ToggleHistory();
        
        // Remove focus from the button to prevent Space key from re-triggering it
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    /// <summary>
    /// Automatically sets up the history buttons to call ToggleHistory when clicked
    /// </summary>
    private void SetupHistoryButtons()
    {
        if (historyButton != null)
        {
            historyButton.onClick.RemoveAllListeners();
            historyButton.onClick.AddListener(OnHistoryButtonClicked);
        }
    }

    /// <summary>
    /// Public method to manually set up the first history button
    /// Call this if you want to wire up the button programmatically
    /// </summary>
    public void SetHistoryButton(Button button)
    {
        historyButton = button;
        SetupHistoryButtons();
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
        bool isHistoryOpen = historyPanel != null && historyPanel.activeSelf;
        UpdateButtonTexts(isHistoryOpen);
    }
    
    /// <summary>
    /// Updates the text for both history buttons
    /// </summary>
    private void UpdateButtonTexts(bool isHistoryOpen)
    {
        string buttonText = isHistoryOpen ? closeHistoryText : openHistoryText;
        if (historyButton != null)
        {
            TMP_Text buttonLabel = historyButton.GetComponentInChildren<TMP_Text>();
            if (buttonLabel != null)
            {
                buttonLabel.text = buttonText;
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
            // Update scroll bar visibility
            UpdateScrollBarVisibility();
        }
    }

    /// <summary>
    /// Waits a frame, then resets scroll position only if content needs scrolling.
    /// This prevents unnecessary scroll bar appearance on short content.
    /// </summary>
    private IEnumerator ResetScrollPositionIfNeeded()
    {
        yield return null; // wait one frame
        if (scrollRect != null)
        {
            // Force update canvases to ensure layout is complete
            Canvas.ForceUpdateCanvases();
            
            // Check if content actually needs scrolling
            RectTransform content = scrollRect.content;
            RectTransform viewport = scrollRect.viewport;
            
            if (content != null && viewport != null)
            {
                // Only reset scroll position if content height exceeds viewport height
                if (content.rect.height > viewport.rect.height)
                {
                    scrollRect.verticalNormalizedPosition = 0f; // 0 = bottom, 1 = top
                }
                else
                {
                    // Content fits in viewport, don't manipulate scroll position
                    // This prevents unnecessary scroll bar appearance
                    scrollRect.verticalNormalizedPosition = 1f; // Keep at top for short content
                }
            }
            
            // Force scroll bar update
            UpdateScrollBarVisibility();
        }
    }
    
    /// <summary>
    /// Updates the scroll bar visibility based on content size
    /// </summary>
    private void UpdateScrollBarVisibility()
    {
        if (scrollRect != null)
        {
            RectTransform content = scrollRect.content;
            RectTransform viewport = scrollRect.viewport;
            
            if (content != null && viewport != null)
            {
                // Always enable scrolling first, then check if we need to disable it
                scrollRect.vertical = true;
                
                // Force a layout update to get accurate measurements
                LayoutRebuilder.ForceRebuildLayoutImmediate(content);
                LayoutRebuilder.ForceRebuildLayoutImmediate(viewport);
                
                // Now check if content actually needs scrolling
                bool needsScrolling = content.rect.height > viewport.rect.height;
                
                // Only disable scrolling if content definitely doesn't need it
                if (!needsScrolling)
                {
                    scrollRect.vertical = false;
                }
            }
        }
    }
    
    /// <summary>
    /// Forces a complete scroll bar refresh after a delay to ensure proper state
    /// </summary>
    private IEnumerator ForceScrollBarRefresh()
    {
        yield return new WaitForEndOfFrame(); // Wait for layout to complete
        yield return null; // Wait one more frame for everything to settle
        
        if (scrollRect != null)
        {
            // Force canvas update
            Canvas.ForceUpdateCanvases();
            
            // Reset scroll bar state completely
            scrollRect.vertical = true;
            
            // Update visibility based on actual content
            UpdateScrollBarVisibility();
        }
    }
    
    /// <summary>
    /// Scrolls to bottom when new dialogue is added, with proper timing
    /// </summary>
    private IEnumerator ScrollToBottomOnNewDialogue()
    {
        // First refresh the page content
        RefreshPage();
        
        // Wait for content to be updated
        yield return new WaitForEndOfFrame();
        yield return null; // Wait one more frame for text to render
        
        if (scrollRect != null)
        {
            // Force canvas update to ensure layout is complete
            Canvas.ForceUpdateCanvases();
            
            // Ensure scrolling is enabled
            scrollRect.vertical = true;
            
            // Force scroll to the very bottom
            scrollRect.verticalNormalizedPosition = 0f;
            
            // Update scroll bar visibility
            UpdateScrollBarVisibility();
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
    
    /// <summary>
    /// Set typing in progress state (called by YarnTypeToFinish scripts)
    /// </summary>
    public void SetTypingInProgress(bool isTyping)
    {
        if (isTyping)
        {
            _activeTypingCount++;
        }
        else
        {
            _activeTypingCount = Mathf.Max(0, _activeTypingCount - 1);
        }
        
        _isTypingInProgress = _activeTypingCount > 0;
        
        Debug.Log($"Typing state updated: {_isTypingInProgress} (active count: {_activeTypingCount})");
    }
    
    /// <summary>
    /// Check if any typing is currently in progress
    /// </summary>
    public bool IsTypingInProgress()
    {
        return _isTypingInProgress;
    }
    
    /// <summary>
    /// Called when YarnSpinner typewriter finishes
    /// </summary>
    private void OnYarnTypewriterFinished()
    {
        // YarnSpinner typewriter finished, but we still need to check if YarnTypeToFinish is active
        // This is handled by the individual YarnTypeToFinish scripts calling SetTypingInProgress
        Debug.Log("YarnSpinner typewriter finished");
    }
    
}
