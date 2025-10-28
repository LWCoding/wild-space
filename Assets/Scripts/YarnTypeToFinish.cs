using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(SpriteRenderer))]
public class YarnTypeToFinish : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private TextMeshPro _textToChange;
    [SerializeField] private string _nodeName;

    [TextArea(10, 20)]
    [SerializeField] private string _messageToShow;

    private SpriteRenderer _spriteRenderer;  // To check to see if this object is visible
    private DialogueRunner _dialogueRunner;
    private int _charactersShown = 0;
    private bool _loadedNode = false;
    private AudioManager _audioManager;
    private bool _isCurrentlyTyping = false;
    private bool _wasSpriteEnabledLastFrame = false;

    private void Awake()
    {
        _dialogueRunner = FindObjectOfType<DialogueRunner>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioManager = AudioManager.Instance;
        _textToChange.text = "";  // Hide text initially
        _wasSpriteEnabledLastFrame = _spriteRenderer.enabled;  // Initialize to current state
    }
    

    private void Update()
    {
        // Check if sprite renderer just became enabled
        if (_spriteRenderer.enabled && !_wasSpriteEnabledLastFrame)
        {
            // Object just became visible, start blocking history
            _isCurrentlyTyping = true;
            DialogueHistoryManager.Instance?.SetTypingInProgress(true);
        }
        else if (!_spriteRenderer.enabled && _wasSpriteEnabledLastFrame)
        {
            // Object just became invisible, stop blocking history
            _isCurrentlyTyping = false;
            DialogueHistoryManager.Instance?.SetTypingInProgress(false);
        }
        
        // Update the flag for next frame
        _wasSpriteEnabledLastFrame = _spriteRenderer.enabled;
        
        if (_spriteRenderer.enabled)
        {
            // We want to check for keyboard input, but not mouse clicks
            if (Input.anyKeyDown && !(Input.GetMouseButtonDown(0)
            || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
            {
                // Only respond to input if this object is currently typing or if no other object is typing
                bool canRespond = _isCurrentlyTyping || (DialogueHistoryManager.Instance != null && !DialogueHistoryManager.Instance.IsTypingInProgress());
                
                if (canRespond && _charactersShown < _messageToShow.Length)
                {
                    // Only play typing sound if there are still characters to reveal
                    if (_audioManager == null) {
                        _audioManager = AudioManager.Instance;
                    }
                    if (_audioManager != null)
                    {
                        _audioManager.PlayTypingSound();
                    }
                    
                    // Process the input
                    _charactersShown = Mathf.Min(_charactersShown + 3, _messageToShow.Length);
                    
                    // Skip spaces.
                    while (_charactersShown < _messageToShow.Length && _messageToShow[_charactersShown] == ' ')
                    {
                        _charactersShown++;
                    }

                    _textToChange.text = _messageToShow[.._charactersShown];
                    
                    // Update typing state based on progress
                    bool isNowTyping = _charactersShown < _messageToShow.Length;
                    if (_isCurrentlyTyping != isNowTyping)
                    {
                        _isCurrentlyTyping = isNowTyping;
                        DialogueHistoryManager.Instance?.SetTypingInProgress(isNowTyping);
                    }
                    
                    if (!_loadedNode && _charactersShown == _messageToShow.Length)
                    {
                        _loadedNode = true;
                        StartCoroutine(StartYarnScriptAfterDelayCoroutine(1));
                    }
                }
            }
        }
    }

    private IEnumerator StartYarnScriptAfterDelayCoroutine(int delay)
    {
        yield return new WaitForSeconds(delay);
        _dialogueRunner.StartDialogue(_nodeName);
    }
    

}
