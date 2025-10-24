using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

/// <summary>
/// Extension script that adds hover sound functionality to OptionItem components.
/// This script extends the existing Yarnspinner OptionItem without modifying the original script.
/// </summary>
[RequireComponent(typeof(OptionItem))]
public class OptionItemHoverSoundExtension : MonoBehaviour
{
    private AudioManager audioManager;
    private OptionItem optionItem;
    private bool wasHighlighted = false;

    void Awake()
    {
        // Get reference to the AudioManager
        audioManager = AudioManager.Instance;
        
        // Get reference to the OptionItem component
        optionItem = GetComponent<OptionItem>();
        
        if (audioManager == null)
        {
            Debug.LogWarning("OptionItemHoverSoundExtension: AudioManager instance not found. Hover sounds will not play.");
        }
        
        if (optionItem == null)
        {
            Debug.LogWarning("OptionItemHoverSoundExtension: OptionItem component not found. This script requires an OptionItem component.");
        }
    }

    void Update()
    {
        // Check if the option is currently highlighted
        bool isCurrentlyHighlighted = optionItem != null && optionItem.IsHighlighted;
        
        // If the option just became highlighted (wasn't highlighted before but is now)
        if (isCurrentlyHighlighted && !wasHighlighted)
        {
            // Play the hover sound effect
            if (audioManager != null)
            {
                audioManager.PlayDialogueHoverSound();
            }
        }
        
        // Update the previous state
        wasHighlighted = isCurrentlyHighlighted;
    }
}
