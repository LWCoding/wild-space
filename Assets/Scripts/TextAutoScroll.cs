using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(SpriteRenderer))]
public class TextAutoScroll : MonoBehaviour
{
    [Header("Object Assignments")]
    [SerializeField] private TextMeshPro _textToChange;
    [SerializeField] private string _nodeName;

    [TextArea(10, 20)]
    [SerializeField] private string _messageToShow;

    [Header("Auto-scroll Settings")]
    [SerializeField] private float _characterDelay = 0.05f; // Delay between each character
    [SerializeField] private float _periodDelay = 0.5f; // Delay after periods for natural reading pause

    private SpriteRenderer _spriteRenderer;
    private DialogueRunner _dialogueRunner;
    private bool _isTyping = false;
    private bool _hasStarted = false;

    private void Awake()
    {
        _dialogueRunner = FindObjectOfType<DialogueRunner>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _textToChange.text = "";  // Hide text initially
    }

    private void Update()
    {
        // Check if the object is visible and we haven't started typing yet
        if (_spriteRenderer.enabled && !_hasStarted)
        {
            _hasStarted = true;
            StartCoroutine(TypeTextCoroutine());
        }
    }

    private IEnumerator TypeTextCoroutine()
    {
        _isTyping = true;
        _textToChange.text = "";

        for (int i = 0; i <= _messageToShow.Length; i++)
        {
            _textToChange.text = _messageToShow[..i];
            
            // Check if the current character is a period and add extra delay
            if (i > 0 && i < _messageToShow.Length && _messageToShow[i - 1] == '.')
            {
                yield return new WaitForSeconds(_periodDelay);
            }
            else
            {
                yield return new WaitForSeconds(_characterDelay);
            }
        }

        _isTyping = false;

        // Wait a moment after typing is complete, then start the Yarn script
        yield return new WaitForSeconds(1f);
        _dialogueRunner.StartDialogue(_nodeName);
    }

    // Public method to manually start typing if needed
    public void StartTyping()
    {
        if (!_isTyping && !_hasStarted)
        {
            _hasStarted = true;
            StartCoroutine(TypeTextCoroutine());
        }
    }
}
