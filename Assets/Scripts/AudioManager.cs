using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource1;
    [SerializeField] private AudioSource audioSource2;
    
    [Header("Initial Music")]
    [SerializeField] private AudioClip initialMusicClip;
    [SerializeField] private float initialMusicVolume;
    
    [Header("Transition Settings")]
    [SerializeField] private float defaultTransitionTime;
    
    private AudioSource currentSource;
    private AudioSource fadeSource;
    private bool isTransitioning = false;
    
    void Awake()
    {
        // Singleton pattern implementation
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Ensure we have audio sources assigned
        if (audioSource1 == null)
        {
            Debug.LogError("AudioManager: AudioSource1 is not assigned!");
            return;
        }
        
        if (audioSource2 == null)
        {
            Debug.LogError("AudioManager: AudioSource2 is not assigned!");
            return;
        }
        
        // Set initial current source
        currentSource = audioSource1;
        fadeSource = audioSource2;
        
        // Play initial music if assigned
        if (initialMusicClip != null)
        {
            PlayInitialMusic();
        }
    }
    
    /// <summary>
    /// Plays the initial music clip on loop
    /// </summary>
    private void PlayInitialMusic()
    {
        if (initialMusicClip != null && currentSource != null)
        {
            currentSource.clip = initialMusicClip;
            currentSource.volume = initialMusicVolume;
            currentSource.loop = true;
            currentSource.Play();
            Debug.Log($"Playing initial music: {initialMusicClip.name} (volume: {initialMusicVolume})");
        }
    }
    
    /// <summary>
    /// Changes the audio clip with a smooth transition
    /// </summary>
    /// <param name="newClip">The new audio clip to play</param>
    /// <param name="volume">Volume for the new clip (optional)</param>
    /// <param name="transitionTime">Time for the crossfade transition (optional)</param>
    public void ChangeAudioClip(AudioClip newClip, float volume = 0.2f, float transitionTime = -1)
    {
        if (newClip == null)
        {
            Debug.LogWarning("AudioManager: Cannot change to null audio clip");
            return;
        }
        
        if (isTransitioning)
        {
            Debug.LogWarning("AudioManager: Already transitioning, ignoring request");
            return;
        }
        
        // Use default transition time if not specified
        if (transitionTime <= 0)
            transitionTime = defaultTransitionTime;
        
        StartCoroutine(CrossfadeToNewClip(newClip, volume, transitionTime));
    }
    
    /// <summary>
    /// Stops the current audio immediately without transition
    /// </summary>
    public void StopAudio()
    {
        if (currentSource != null)
            currentSource.Stop();
        
        if (fadeSource != null)
            fadeSource.Stop();
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Plays an audio clip immediately without transition
    /// </summary>
    /// <param name="clip">The audio clip to play</param>
    /// <param name="volume">Volume for the clip (optional)</param>
    public void PlayAudioImmediate(AudioClip clip, float volume = 0.2f)
    {
        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Cannot play null audio clip");
            return;
        }
        
        StopAudio();
        currentSource.clip = clip;
        currentSource.volume = volume;
        currentSource.Play();
    }
    
    /// <summary>
    /// Gets the currently playing audio clip
    /// </summary>
    /// <returns>The currently playing audio clip, or null if none</returns>
    public AudioClip GetCurrentClip()
    {
        if (currentSource != null && currentSource.isPlaying)
            return currentSource.clip;
        
        return null;
    }
    
    /// <summary>
    /// Checks if audio is currently transitioning
    /// </summary>
    /// <returns>True if transitioning, false otherwise</returns>
    public bool IsTransitioning()
    {
        return isTransitioning;
    }
    
    /// <summary>
    /// Plays a start clip followed by a loopable clip that seamlessly loops
    /// </summary>
    /// <param name="startClip">The initial clip to play once</param>
    /// <param name="loopClip">The clip to loop after the start clip finishes</param>
    /// <param name="volume">Volume for both clips (optional)</param>
    /// <param name="fadeTime">Fade time between start and loop clips (optional, defaults to 0.5 seconds)</param>
    public void PlayStartThenLoop(AudioClip startClip, AudioClip loopClip, float volume = 0.2f, float fadeTime = 0.5f)
    {
        if (startClip == null || loopClip == null)
        {
            Debug.LogWarning("AudioManager: Cannot play null audio clips");
            return;
        }
        
        StartCoroutine(PlayStartThenLoopCoroutine(startClip, loopClip, volume, fadeTime));
    }
    
    private IEnumerator PlayStartThenLoopCoroutine(AudioClip startClip, AudioClip loopClip, float volume, float fadeTime)
    {
        // Stop any current audio
        StopAudio();
        
        // Play the start clip
        currentSource.clip = startClip;
        currentSource.volume = volume;
        currentSource.loop = false;
        currentSource.Play();
        
        // Wait for the start clip to finish
        yield return new WaitForSeconds(startClip.length);
        
        // If there's a fade time, do a smooth transition to the loop clip
        if (fadeTime > 0)
        {
            // Set up the loop clip on the fade source
            fadeSource.clip = loopClip;
            fadeSource.volume = 0f;
            fadeSource.loop = true;
            fadeSource.Play();
            
            float elapsedTime = 0f;
            
            // Crossfade from start clip to loop clip
            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeTime;
                
                // Fade out current source (start clip)
                if (currentSource != null && currentSource.isPlaying)
                {
                    currentSource.volume = Mathf.Lerp(volume, 0f, t);
                }
                
                // Fade in fade source (loop clip)
                if (fadeSource != null)
                {
                    fadeSource.volume = Mathf.Lerp(0f, volume, t);
                }
                
                yield return null;
            }
            
            // Ensure final volumes are set correctly
            if (currentSource != null)
            {
                currentSource.volume = 0f;
                currentSource.Stop();
            }
            
            if (fadeSource != null)
            {
                fadeSource.volume = volume;
            }
            
            // Swap the sources so the loop clip is now the current source
            AudioSource temp = currentSource;
            currentSource = fadeSource;
            fadeSource = temp;
        }
        else
        {
            // No fade time, just switch directly to the loop clip
            currentSource.clip = loopClip;
            currentSource.volume = volume;
            currentSource.loop = true;
            currentSource.Play();
        }
        
        Debug.Log($"Now looping: {loopClip.name}");
    }
    
    private IEnumerator CrossfadeToNewClip(AudioClip newClip, float volume, float transitionTime)
    {
        isTransitioning = true;
        
        // Set up the fade source with the new clip
        fadeSource.clip = newClip;
        fadeSource.volume = 0f;
        fadeSource.Play();
        
        float elapsedTime = 0f;
        
        // Crossfade over the specified time
        while (elapsedTime < transitionTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionTime;
            
            // Fade out current source
            if (currentSource != null && currentSource.isPlaying)
            {
                currentSource.volume = Mathf.Lerp(currentSource.volume, 0f, Time.deltaTime * (1f / transitionTime));
            }
            
            // Fade in new source
            if (fadeSource != null)
            {
                fadeSource.volume = Mathf.Lerp(0f, volume, t);
            }
            
            yield return null;
        }
        
        // Ensure final volumes are set correctly
        if (currentSource != null)
        {
            currentSource.volume = 0f;
            currentSource.Stop();
        }
        
        if (fadeSource != null)
        {
            fadeSource.volume = volume;
        }
        
        // Swap the sources
        AudioSource temp = currentSource;
        currentSource = fadeSource;
        fadeSource = temp;
        
        isTransitioning = false;
    }
}
