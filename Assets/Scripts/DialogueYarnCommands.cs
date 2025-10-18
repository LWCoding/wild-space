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

    /// <summary>
    /// Yarn: <<play_music "AudioClipName">> or <<play_music "AudioClipName" 0.8>> or <<play_music "AudioClipName" 0.8 3.0>>
    /// Smoothly transitions to a new audio clip using AudioManager.
    /// Optional parameters: volume (0-1), transition time (seconds).
    /// </summary>
    [YarnCommand("play_music")]
    public static void PlayMusic(string clipName, string volume = "", string transitionTime = "")
    {
        // Use the singleton AudioManager instance
        if (AudioManager.Instance == null)
        {
            Debug.LogError($"AudioManager singleton not initialized. Cannot play music '{clipName}'");
            return;
        }

        // Load the audio clip from Resources
        AudioClip clip = Resources.Load<AudioClip>($"Audio/{clipName}");
        if (clip == null)
        {
            Debug.LogError($"Audio clip '{clipName}' not found in Resources/Audio/ folder");
            return;
        }

        // Parse optional volume (default to 0.1)
        float vol = 0.1f;
        if (!string.IsNullOrEmpty(volume))
        {
            if (float.TryParse(volume, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float volValue))
            {
                vol = Mathf.Clamp01(volValue); // Clamp between 0 and 1
            }
            else
            {
                Debug.LogWarning($"Invalid volume '{volume}'. Using default volume of 0.1.");
            }
        }

        // Parse optional transition time (default to -1 to use AudioManager default)
        float transition = -1f;
        if (!string.IsNullOrEmpty(transitionTime))
        {
            if (float.TryParse(transitionTime, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float timeValue))
            {
                transition = timeValue;
            }
            else
            {
                Debug.LogWarning($"Invalid transition time '{transitionTime}'. Using AudioManager default.");
            }
        }

        // Call the AudioManager to change the clip
        AudioManager.Instance.ChangeAudioClip(clip, vol, transition);
        Debug.Log($"Playing music: {clipName} (volume: {vol}, transition: {transition}s)");
    }

    /// <summary>
    /// Yarn: <<stop_music>>
    /// Immediately stops all music without transition.
    /// </summary>
    [YarnCommand("stop_music")]
    public static void StopMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("AudioManager singleton not initialized. Cannot stop music.");
            return;
        }

        AudioManager.Instance.StopAudio();
        Debug.Log("Stopped all music");
    }
}


