using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Adds additional Yarn command functionality for dialogue control.
/// Static methods here can be called directly from Yarn.
/// </summary>
public static class DialogueYarnCommands
{
    /// <summary>
    /// Yarn: <<hide_all_characters>>
    /// Hides all spawned characters using CharacterManager.
    /// </summary>
    [YarnCommand("hide_all_characters")]
    public static void HideAllCharacters()
    {
        CharacterManager.HideAllCharacters();
    }

    /// <summary>
    /// Yarn: <<hide_all_objects>>
    /// Hides all objects with YarnShowHideable components.
    /// </summary>
    [YarnCommand("hide_all_objects")]
    public static void HideAllObjects()
    {
        var showHideableObjects = Object.FindObjectsOfType<YarnShowHideable>();
        foreach (var obj in showHideableObjects)
        {
            if (obj != null)
            {
                obj.HideObject();
            }
        }
        Debug.Log($"Hid {showHideableObjects.Length} YarnShowHideable objects");
    }

    /// <summary>
    /// Yarn: <<hide_all_objects>>
    /// Hides all objects with YarnShowHideable components.
    /// </summary>
    [YarnCommand("hide_all_ui_objects")]
    public static void HideAllUIObjects()
    {
        var showHideableObjects = Object.FindObjectsOfType<UIYarnShowHideable>();
        foreach (var obj in showHideableObjects)
        {
            if (obj != null)
            {
                obj.HideObject();
            }
        }
        Debug.Log($"Hid {showHideableObjects.Length} UIYarnShowHideable objects");
    }

    /// <summary>
    /// Yarn: <<speed 2>> or <<speed 0.5>>
    /// Sets global dialogue typing speed multiplier.
    /// </summary>
    [YarnCommand("speed")]
    public static void SetGlobalSpeed(string multiplier)
    {
        if (double.TryParse(multiplier, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value))
        {
            if (value <= 0)
            {
                value = 0.0001; // clamp to tiny positive to avoid zero/negative
            }
            BasicTypewriter.GlobalSpeedMultiplier = value;
            Debug.Log($"Set global dialogue speed multiplier to {value}");
        }
        else
        {
            Debug.LogWarning($"Invalid speed value '{multiplier}'. Expected a number, e.g., 2 or 0.5");
        }
    }
}


