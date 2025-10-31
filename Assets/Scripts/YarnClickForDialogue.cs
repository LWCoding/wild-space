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
    [SerializeField] private float _interactedGrayness = 0.6f;

    private DialogueRunner _dialogueRunner;
    private int _timesRan = 0;

    private SpriteRenderer _spriteRenderer;
    private Coroutine _pulseCoroutine;
    private Vector3 _initialScale;
    private Color _originalColor;
    private bool _isHovered = false;

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
        _isHovered = true;

        // Don't tint on hover if dialogue is running
        if (_dialogueRunner.IsDialogueRunning)
        {
            return;
        }

        // Tint on hover if not yet interacted
        if (_spriteRenderer != null && _timesRan == 0)
        {
            _spriteRenderer.color = _hoverColor;
        }
    }

    private void OnMouseExit()
    {
        _isHovered = false;

        // Don't change color if dialogue is running
        if (_dialogueRunner.IsDialogueRunning)
        {
            return;
        }

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
                _spriteRenderer.color = new Color(_interactedGrayness, _interactedGrayness, _interactedGrayness, 1f);
            }
        }

                // Stop if we've already interacted and we only want one interaction
        if (_timesRan > 0 && _onlyInteractableOnce)
        {
            return;
        }
        _timesRan++;

        await _dialogueRunner.StartDialogue(_nodeName);

        // After dialogue ends, check if mouse is hovering and apply correct visual state
        UpdateHoverVisual();
    }

    private void UpdateHoverVisual()
    {
        if (_spriteRenderer == null)
        {
            return;
        }

        // Determine the base color based on interaction state
        Color baseColor = _originalColor;
        if (_timesRan > 0)
        {
            baseColor = new Color(_interactedGrayness, _interactedGrayness, _interactedGrayness, 1f);
        }

        // Apply hover color if currently hovered, otherwise use base color
        if (_isHovered && _timesRan == 0)
        {
            _spriteRenderer.color = _hoverColor;
        }
        else
        {
            _spriteRenderer.color = baseColor;
        }
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
