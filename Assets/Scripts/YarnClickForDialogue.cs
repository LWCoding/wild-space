using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(Collider2D))]
public class YarnClickForDialogue : MonoBehaviour
{

    [Header("Object Assignments")]
    [SerializeField] private bool _onlyInteractableOnce = false;
    [SerializeField] private string _nodeName;
    
    [Header("Conditional Interaction")]
    [SerializeField] private List<string> _requiredVariables = new List<string>();  // Checks these booleans in yarnspinner variable storage; only interactable if all are true

    [Header("Pre-interaction Pulse")]
    [SerializeField] private bool _enablePulse = true;

    [Header("Hover Visuals")]
    [SerializeField] private Color _hoverColor = new Color(0.8f, 0.8f, 0.8f);

    [Header("Post-interaction Visuals")]
    [Range(0f, 1f)]
    [SerializeField] private float _interactedAlpha = 0.75f;

    private DialogueRunner _dialogueRunner;
    private int _timesRan = 0;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _pulseCoroutine;
    private Vector3 _initialScale;
    private Color _originalColor;

    private float _pulseScaleAmount = 0.01f;
    private float _pulseSpeed = 0.25f; // cycles per second

    private void Awake()
    {
        _dialogueRunner = FindObjectOfType<DialogueRunner>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _initialScale = transform.localScale;
        _originalColor = _spriteRenderer.color;
    }

    private void OnEnable()
    {
        // Hide object if required variables are not met
        if (!AreRequiredVariablesMet())
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Begin pulsing if not yet interacted and variables are met
        if (_enablePulse && _timesRan == 0)
        {
            _pulseCoroutine = StartCoroutine(PulseRoutine());
        }
    }

    private void OnDisable()
    {
        if (_pulseCoroutine != null)
        {
            StopCoroutine(_pulseCoroutine);
            _pulseCoroutine = null;
        }
        transform.localScale = _initialScale;
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseEnter()
    {
        // Tint on hover if not yet interacted
        if (_spriteRenderer != null && _timesRan == 0)
        {
            _spriteRenderer.color = _hoverColor;
        }
    }

    private void OnMouseExit()
    {
        // Restore original color and cursor
        if (_spriteRenderer != null && _timesRan == 0)
        {
            _spriteRenderer.color = _originalColor;
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void OnMouseDown()
    {
        // Don't render click if dialogue is running
        if (_dialogueRunner.IsDialogueRunning)
        {
            return;
        }
        
        HandleClick();
    }

    private async void HandleClick()
    {
        // Stop if we've already interacted and we only want one interaction
        if (_timesRan > 0 && _onlyInteractableOnce)
        {
            return;
        }
        _timesRan++;
        _dialogueRunner.VariableStorage.SetValue("$clickedTimes", _timesRan);  // Register how many times clicked

        // Stop pulsing and gray out after first interaction
        if (_timesRan == 1)
        {
            if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
            transform.localScale = _initialScale;
            if (_spriteRenderer != null)
            {
                var c = _originalColor;
                _spriteRenderer.color = new Color(c.r, c.g, c.b, _interactedAlpha);
            }
        }

        await _dialogueRunner.StartDialogue(_nodeName);
    }

    private IEnumerator PulseRoutine()
    {
        float t = 0f;
        while (_timesRan == 0)
        {
            t += Time.deltaTime * _pulseSpeed * Mathf.PI * 2f; // radians/sec
            float scaleOffset = Mathf.Sin(t) * _pulseScaleAmount;
            transform.localScale = _initialScale * (1f + scaleOffset);
            yield return null;
        }
        transform.localScale = _initialScale;
    }

    private bool AreRequiredVariablesMet()
    {
        // If no required variables are specified, object is always interactable
        if (_requiredVariables == null || _requiredVariables.Count == 0)
        {
            return true;
        }

        // Check if all required variables are true
        foreach (string variableName in _requiredVariables)
        {
            if (string.IsNullOrEmpty(variableName))
                continue;
                
            if (_dialogueRunner.VariableStorage.TryGetValue<bool>($"${variableName}", out bool variableValue))
            {
                if (!variableValue)
                {
                    return false;
                }
            }
            else
            {
                Debug.LogError($"Variable {variableName} (as a boolean) does not exist in yarnspinner variable storage");
                return false;
            }
        }

        return true;
    }

}
