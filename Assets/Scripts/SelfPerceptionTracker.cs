using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Yarn.Unity;

/// <summary>
/// Tracks posSelfPerception and negSelfPerception variables from Yarn and controls URP vignette intensity.
/// Each point of negSelfPerception increases vignette intensity by 0.1.
/// Each point of posSelfPerception decreases vignette intensity by 0.1.
/// Intensity has a minimum value of 0.35 and is clamped between 0.35 and 1.
/// </summary>
public class SelfPerceptionTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Volume globalVolume;
    
    [Header("Settings")]
    [SerializeField] private float intensityPerPoint = 0.1f; // 10% per point
    [SerializeField] private float minimumIntensity = 0.35f; // Minimum vignette intensity
    [SerializeField] private float transitionDuration = 0.5f; // Duration for smooth vignette transitions
    
    // Variable names in Yarn
    private const string POS_SELF_PERCEPTION_VAR = "posSelfPerception";
    private const string NEG_SELF_PERCEPTION_VAR = "negSelfPerception";
    
    // Cached values to detect changes
    private int lastPosSelfPerception = 0;
    private int lastNegSelfPerception = 0;
    
    private DialogueRunner dialogueRunner;
    private Vignette vignette;
    
    // Smooth transition tracking
    private Coroutine currentTransition;
    private float targetIntensity;
    
    void Start()
    {
        // Get or find the DialogueRunner if not assigned
        if (dialogueRunner == null)
        {
            dialogueRunner = FindObjectOfType<DialogueRunner>();
            if (dialogueRunner == null)
            {
                Debug.LogError("SelfPerceptionTracker: No DialogueRunner found in scene!");
                return;
            }
        }
        
        // Get or find the Global Volume if not assigned
        if (globalVolume == null)
        {
            globalVolume = FindObjectOfType<Volume>();
            if (globalVolume == null)
            {
                Debug.LogError("SelfPerceptionTracker: No Volume component found in scene!");
                return;
            }
        }
        
        // Get the vignette component from the volume profile
        if (globalVolume.profile != null)
        {
            globalVolume.profile.TryGet<Vignette>(out vignette);
            if (vignette == null)
            {
                Debug.LogError("SelfPerceptionTracker: No Vignette component found in the volume profile!");
                return;
            }
        }
        else
        {
            Debug.LogError("SelfPerceptionTracker: Volume profile is null!");
            return;
        }
        
        // Load initial perception values from Yarn
        lastPosSelfPerception = GetYarnVariable(POS_SELF_PERCEPTION_VAR);
        lastNegSelfPerception = GetYarnVariable(NEG_SELF_PERCEPTION_VAR);
        
        // Initialize with current values (immediate, no transition)
        UpdateVignetteIntensity(immediate: true);
        
        // Subscribe to dialogue completion events instead of polling
        DialogueHistoryManager.OnDialogueAdded += OnDialogueLineCompleted;
        
        Debug.Log("SelfPerceptionTracker initialized successfully");
    }
    
    void OnDestroy()
    {
        // Unsubscribe from the dialogue completion event
        DialogueHistoryManager.OnDialogueAdded -= OnDialogueLineCompleted;
    }
    
    /// <summary>
    /// Called when a dialogue line is completed - updates vignette based on current perception values
    /// </summary>
    private void OnDialogueLineCompleted(DialogueHistoryManager.DialogueHistoryEntry entry)
    {
        if (dialogueRunner?.VariableStorage == null || vignette == null)
            return;
            
        // Get current values from Yarn variables
        int currentPosSelfPerception = GetYarnVariable(POS_SELF_PERCEPTION_VAR);
        int currentNegSelfPerception = GetYarnVariable(NEG_SELF_PERCEPTION_VAR);
        
        // Check if values have changed
        bool valuesChanged = (currentPosSelfPerception != lastPosSelfPerception || 
                             currentNegSelfPerception != lastNegSelfPerception);
        
        lastPosSelfPerception = currentPosSelfPerception;
        lastNegSelfPerception = currentNegSelfPerception;
        
        UpdateVignetteIntensity();
        
        if (valuesChanged)
        {
            Debug.Log($"Self-perception updated after dialogue: pos={currentPosSelfPerception}, neg={currentNegSelfPerception}, vignette intensity={vignette.intensity.value}");
        }
    }
    
    /// <summary>
    /// Gets a Yarn variable value as an integer
    /// </summary>
    /// <param name="variableName">Name of the variable (without $ prefix)</param>
    /// <returns>Variable value as integer, 0 if not found</returns>
    private int GetYarnVariable(string variableName)
    {
        if (dialogueRunner?.VariableStorage == null)
        {
            Debug.LogWarning($"DialogueRunner or VariableStorage is null when trying to get ${variableName}");
            return 0;
        }
            
        // Get as float (Yarn stores numbers as floats)
        if (dialogueRunner.VariableStorage.TryGetValue<float>($"${variableName}", out float floatValue))
        {
            return Mathf.RoundToInt(floatValue);
        }
        else
        {
            // Variable doesn't exist yet, return 0
            return 0;
        }
    }
    
    /// <summary>
    /// Updates the vignette intensity based on current self-perception values
    /// </summary>
    /// <param name="immediate">If true, sets intensity immediately without transition</param>
    private void UpdateVignetteIntensity(bool immediate = false)
    {
        if (vignette == null)
            return;
            
        // Calculate target intensity: negative perception increases, positive decreases
        float intensity = (lastNegSelfPerception - lastPosSelfPerception) * intensityPerPoint;
        
        // Add minimum intensity and clamp between minimum and 1
        targetIntensity = Mathf.Clamp(intensity + minimumIntensity, minimumIntensity, 1f);
        
        // If immediate, set the intensity directly without transition
        if (immediate)
        {
            vignette.intensity.Override(targetIntensity);
            return;
        }
        
        // Start smooth transition to target intensity
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        currentTransition = StartCoroutine(SmoothTransitionToIntensity(targetIntensity));
    }
    
    /// <summary>
    /// Smoothly transitions the vignette intensity over the specified duration
    /// </summary>
    /// <param name="targetIntensity">The target intensity to transition to</param>
    /// <returns>Coroutine enumerator</returns>
    private System.Collections.IEnumerator SmoothTransitionToIntensity(float targetIntensity)
    {
        float startIntensity = vignette.intensity.value;
        float elapsedTime = 0f;
        
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / transitionDuration;
            
            // Use smooth interpolation (ease-in-out)
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            float currentIntensity = Mathf.Lerp(startIntensity, targetIntensity, smoothProgress);
            
            vignette.intensity.Override(currentIntensity);
            
            yield return null;
        }
        
        // Ensure we end exactly at the target intensity
        vignette.intensity.Override(targetIntensity);
        currentTransition = null;
    }
    
    /// <summary>
    /// Manually sets the vignette intensity (for testing or external control)
    /// </summary>
    /// <param name="intensity">Intensity value (0-1)</param>
    public void SetVignetteIntensity(float intensity)
    {
        if (vignette == null)
            return;
            
        intensity = Mathf.Clamp(intensity, minimumIntensity, 1f);
        
        // Stop any current transition
        if (currentTransition != null)
        {
            StopCoroutine(currentTransition);
        }
        
        // Start smooth transition to the new intensity
        currentTransition = StartCoroutine(SmoothTransitionToIntensity(intensity));
        Debug.Log($"Manually transitioning vignette intensity to {intensity:F2}");
    }
    
    /// <summary>
    /// Gets the current vignette intensity
    /// </summary>
    /// <returns>Current vignette intensity (0-1)</returns>
    public float GetCurrentVignetteIntensity()
    {
        return vignette != null ? vignette.intensity.value : 0f;
    }
    
    /// <summary>
    /// Gets the current self-perception values
    /// </summary>
    /// <returns>Tuple of (posSelfPerception, negSelfPerception)</returns>
    public (int pos, int neg) GetCurrentSelfPerception()
    {
        return (lastPosSelfPerception, lastNegSelfPerception);
    }
    
    /// <summary>
    /// Forces an immediate update of the vignette based on current Yarn variables
    /// </summary>
    public void ForceUpdate()
    {
        OnDialogueLineCompleted(null);
    }
}
