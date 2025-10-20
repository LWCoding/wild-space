using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Singleton manager that logs all finished dialogue lines and provides them to other systems.
/// This system captures dialogue lines after they have been fully displayed to the user.
/// </summary>
public class DialogueHistoryManager : MonoBehaviour
{
    public static DialogueHistoryManager Instance { get; private set; }

    [Header("History Settings")]
    [SerializeField] private int maxHistoryEntries = 100;
    [SerializeField] private bool includeTimestamps = false;
    [SerializeField] private string timestampFormat = "HH:mm:ss";

    // Events for other systems to subscribe to
    public static event Action<DialogueHistoryEntry> OnDialogueAdded;
    public static event Action OnHistoryCleared;

    // Internal storage
    private List<DialogueHistoryEntry> dialogueHistory = new List<DialogueHistoryEntry>();
    private StringBuilder historyStringBuilder = new StringBuilder();

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

        public string GetFormattedText(bool includeTimestamp = false, string timestampFormat = "HH:mm:ss")
        {
            StringBuilder sb = new StringBuilder();

            if (includeTimestamp)
            {
                sb.Append($"[{timestamp.ToString(timestampFormat)}] ");
            }

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

    void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (Instance == this)
        {
            LinePresenter.OnDialogueLineCompleted -= OnDialogueLineCompleted;
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
        
        // Maintain max history size
        if (dialogueHistory.Count > maxHistoryEntries)
        {
            dialogueHistory.RemoveAt(0);
        }
        
        // Notify subscribers with the new entry
        OnDialogueAdded?.Invoke(entry);
        
        Debug.Log($"Added to dialogue history: {entry.GetFormattedText(includeTimestamps, timestampFormat)}");
    }
    
}
