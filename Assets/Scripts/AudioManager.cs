using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource1;
    [SerializeField] private AudioSource audioSource2;
    [SerializeField] private AudioSource sfxAudioSource;
    
    [Header("SFX Audio")]
    [SerializeField] private AudioClip typingSoundClip;
    [SerializeField] private float typingSoundVolume = 0.3f;
    [SerializeField] private float pitchVariation = 0.2f; // How much the pitch can vary
    
    [Header("Initial Music")]
    [SerializeField] private AudioClip initialMusicClip;
    [SerializeField] private float initialMusicVolume;
    
    [Header("Transition Settings")]
    [SerializeField] private float defaultTransitionTime;
    
    private AudioSource currentSource;
    private AudioSource fadeSource;
    private bool isTransitioning = false;
    private Coroutine activeStartThenLoopCoroutine;
    private int startThenLoopToken = 0;
    
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
        
        if (sfxAudioSource == null)
        {
            Debug.LogError("AudioManager: SFX AudioSource is not assigned!");
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
        // Cancel any pending start-then-loop so it can't override this new music
        CancelStartThenLoop();

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
        // Ensure any start-then-loop coroutine is cancelled as well
        CancelStartThenLoop();

        if (currentSource != null)
            currentSource.Stop();
        
        if (fadeSource != null)
            fadeSource.Stop();
        
        isTransitioning = false;
    }
    
    /// <summary>
    /// Stops the current audio without cancelling start-then-loop operations
    /// </summary>
    private void StopAudioWithoutCancelling()
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
        // Cancel any pending start-then-loop so it can't override this immediate play
        CancelStartThenLoop();

        if (clip == null)
        {
            Debug.LogWarning("AudioManager: Cannot play null audio clip");
            return;
        }
        
        StopAudio();
        currentSource.clip = clip;
        currentSource.volume = volume;
        currentSource.pitch = 1.0f; // Ensure normal pitch
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

        // Cancel any existing start-then-loop and start a new guarded coroutine
        CancelStartThenLoop();
        int token = ++startThenLoopToken;
        activeStartThenLoopCoroutine = StartCoroutine(PlayStartThenLoopCoroutine(startClip, loopClip, volume, fadeTime, token));
    }
    
    private IEnumerator PlayStartThenLoopCoroutine(AudioClip startClip, AudioClip loopClip, float volume, float fadeTime, int token)
    {
        // Stop any current audio without cancelling this coroutine
        StopAudioWithoutCancelling();
        
        // Play the start clip
        currentSource.clip = startClip;
        currentSource.volume = volume;
        currentSource.pitch = 1.0f; // Ensure normal pitch
        currentSource.loop = false;
        currentSource.Play();
        
        // Wait for the start clip to finish
        yield return new WaitForSeconds(startClip.length);

        // If another music request came in, abort
        if (token != startThenLoopToken)
        {
            yield break;
        }
        
        // If there's a fade time, do a smooth transition to the loop clip
        if (fadeTime > 0)
        {
            // Set up the loop clip on the fade source
            fadeSource.clip = loopClip;
            fadeSource.volume = 0f;
            fadeSource.pitch = 1.0f; // Ensure normal pitch
            fadeSource.loop = true;
            fadeSource.Play();
            
            float elapsedTime = 0f;
            
            // Crossfade from start clip to loop clip
            while (elapsedTime < fadeTime)
            {
                if (token != startThenLoopToken)
                {
                    yield break;
                }
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
            if (token != startThenLoopToken)
            {
                yield break;
            }
            currentSource.clip = loopClip;
            currentSource.volume = volume;
            currentSource.pitch = 1.0f; // Ensure normal pitch
            currentSource.loop = true;
            currentSource.Play();
        }

        // Clear reference if this coroutine is still the active one
        if (activeStartThenLoopCoroutine != null && token == startThenLoopToken)
        {
            activeStartThenLoopCoroutine = null;
        }
    }
    
    private IEnumerator CrossfadeToNewClip(AudioClip newClip, float volume, float transitionTime)
    {
        isTransitioning = true;
        
        // Set up the fade source with the new clip
        fadeSource.clip = newClip;
        fadeSource.volume = 0f;
        fadeSource.pitch = 1.0f; // Ensure normal pitch
		fadeSource.loop = true;
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

    private void CancelStartThenLoop()
    {
        if (activeStartThenLoopCoroutine != null)
        {
            StopCoroutine(activeStartThenLoopCoroutine);
            activeStartThenLoopCoroutine = null;
        }
        // Bump token so any in-flight coroutine will self-abort on next check
        startThenLoopToken++;
    }

	/// <summary>
	/// Plays a sound effect one-shot over the current audio without interrupting it
	/// </summary>
	/// <param name="clip">The audio clip to play once</param>
	/// <param name="volume">Volume for the one-shot (0-1, default 1)</param>
	public void PlaySFXOneShot(AudioClip clip, float volume = 1f)
	{
		if (clip == null)
		{
			Debug.LogWarning("AudioManager: Cannot PlayOneShot with null clip");
			return;
		}

		if (sfxAudioSource == null)
		{
			Debug.LogWarning("AudioManager: No SFX AudioSource available for PlayOneShot");
			return;
		}

		// Use dedicated SFX AudioSource to avoid interfering with music
		sfxAudioSource.pitch = 1.0f;
		sfxAudioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
	}

	/// <summary>
	/// Plays the typing sound with random pitch variation
	/// </summary>
	public void PlayTypingSound()
	{
		if (typingSoundClip == null)
		{
			Debug.LogWarning("AudioManager: Typing sound clip is not assigned");
			return;
		}

		if (sfxAudioSource == null)
		{
			Debug.LogWarning("AudioManager: No SFX AudioSource available for typing sound");
			return;
		}

		// Apply random pitch variation
		float pitchVariationAmount = Random.Range(-pitchVariation, pitchVariation);
		sfxAudioSource.pitch = 1f + pitchVariationAmount;
		
		// Play the typing sound
		sfxAudioSource.PlayOneShot(typingSoundClip, typingSoundVolume);
		
		// Always restore pitch to 1.0 after a short delay
		StartCoroutine(RestoreSFXPitchAfterDelay(1.0f, 0.1f));
	}

	private IEnumerator RestorePitchAfterDelay(float targetPitch, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (currentSource != null)
		{
			currentSource.pitch = targetPitch;
		}
	}

	private IEnumerator RestoreSFXPitchAfterDelay(float targetPitch, float delay)
	{
		yield return new WaitForSeconds(delay);
		if (sfxAudioSource != null)
		{
			sfxAudioSource.pitch = targetPitch;
		}
	}
}
