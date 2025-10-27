using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Yarn.Unity;

/// <summary>
/// Retry button that resets all Yarn variables and restarts the game from the beginning.
/// </summary>
[RequireComponent(typeof(Button))]
public class GameRetryButton : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DialogueRunner dialogueRunner;

    private Button retryButton;

    private void Awake()
    {
        retryButton = GetComponent<Button>();
        retryButton.onClick.AddListener(OnRetryClicked);
        
        // Auto-find DialogueRunner if not assigned
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
            if (dialogueRunner == null)
            {
                Debug.LogError("GameRetryButton: No DialogueRunner found! Please assign one in the inspector.");
            }
        }
    }

    /// <summary>
    /// Called when the retry button is clicked. Resets all variables and restarts the game.
    /// </summary>
    public void OnRetryClicked()
    {
        if (dialogueRunner == null)
        {
            Debug.LogError("GameRetryButton: Cannot retry - DialogueRunner is null!");
            return;
        }

        Debug.Log("Retry button clicked - resetting game and reloading scene...");
        
        // Reset all Yarn variables to their default values
        ResetAllYarnVariables();
        
        // Clear dialogue history if it exists
        if (DialogueHistoryManager.Instance != null)
        {
            DialogueHistoryManager.Instance.ClearHistory();
            Debug.Log("Dialogue history cleared.");
        }
        
        // Reload the current scene to reset all game objects and components
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
        
        // Note: We don't call StartDialogue here because the scene reload will
        // trigger the DialogueRunner's auto-start (if configured) or you can
        // manually start it after the scene loads
    }

    /// <summary>
    /// Resets all Yarn variables to their initial values as defined in the Yarn project.
    /// Uses the program's InitialValues dictionary to automatically restore all declared variables.
    /// </summary>
    private void ResetAllYarnVariables()
    {
        var storage = dialogueRunner.VariableStorage;
        
        // Clear all variables first
        storage.Clear();
        
        // Check if we have access to the Yarn project's initial values
        if (dialogueRunner.YarnProject != null)
        {
            // Get all initial values from the Yarn program
            var initialValues = dialogueRunner.YarnProject.InitialValues;
            
            // Restore each variable to its initial value
            foreach (var kvp in initialValues)
            {
                var variableName = kvp.Key;
                var value = kvp.Value;
                
                // Set the value based on its type
                if (value is string stringValue)
                {
                    storage.SetValue(variableName, stringValue);
                }
                else if (value is bool boolValue)
                {
                    storage.SetValue(variableName, boolValue);
                }
                else if (value is float floatValue)
                {
                    storage.SetValue(variableName, floatValue);
                }
                else if (value is int intValue)
                {
                    // Convert int to float since Yarn stores numbers as floats
                    storage.SetValue(variableName, (float)intValue);
                }
                else
                {
                    Debug.LogWarning($"GameRetryButton: Unknown variable type for {variableName}: {value.GetType()}");
                }
            }
            
            Debug.Log($"All Yarn variables have been reset to their initial values ({initialValues.Count} variables).");
        }
        else
        {
            Debug.LogWarning("GameRetryButton: Could not access Yarn project initial values. Falling back to manual reset.");
            
            // Fallback: manually reset critical variables if we can't access the program
            ResetCriticalVariablesManually(storage);
        }
    }

    /// <summary>
    /// Fallback method that manually resets critical variables if we can't access the Yarn project.
    /// This is kept as a backup in case the automatic method fails.
    /// </summary>
    private void ResetCriticalVariablesManually(VariableStorageBehaviour storage)
    {
        // Character name variables
        storage.SetValue("$catThinksPlayerNameIs", "Human");
        storage.SetValue("$dogThinksPlayerNameIs", "Human");
        storage.SetValue("$opossumThinksPlayerNameIs", "Human");
        storage.SetValue("$birdThinksPlayerNameIs", "Human");
        storage.SetValue("$playerName", "Player");
        storage.SetValue("$catName", "Gnarp");
        storage.SetValue("$dogName", "Purble");
        storage.SetValue("$opossumName", "Joey");
        storage.SetValue("$birdName", "Alfie");
        
        // Boolean flags
        storage.SetValue("$dogKnowsPlayerName", false);
        storage.SetValue("$catSecured", false);
        storage.SetValue("$dogSecured", false);
        storage.SetValue("$opossumSecured", false);
        storage.SetValue("$birdSecured", false);
        storage.SetValue("$knowsAboutConditionalChoices", false);
        storage.SetValue("$talkedToDog", false);
        storage.SetValue("$talkedToOpossum", false);
        storage.SetValue("$talkedToBird", false);
        storage.SetValue("$askedEarthForHelp", false);
        storage.SetValue("$desperate", false);
        storage.SetValue("$talkedToPersonDay2", false);
        
        // Relationship values (likes/dislikes - starting at 4 for neutral)
        storage.SetValue("$catLikesYou", 4f);
        storage.SetValue("$catDislikesYou", 4f);
        storage.SetValue("$dogLikesYou", 4f);
        storage.SetValue("$dogDislikesYou", 4f);
        storage.SetValue("$opossumLikesYou", 4f);
        storage.SetValue("$opossumDislikesYou", 4f);
        storage.SetValue("$birdLikesYou", 4f);
        storage.SetValue("$birdDislikesYou", 4f);
        
        // Self-perception values
        storage.SetValue("$posSelfPerception", 0f);
        storage.SetValue("$negSelfPerception", 0f);
        
        // Day counter
        storage.SetValue("$daysPassed", 1f);
        
        Debug.Log("All Yarn variables have been reset to their initial values (manual method).");
    }
}
