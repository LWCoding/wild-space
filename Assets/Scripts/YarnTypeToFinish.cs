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

    private void Awake()
    {
        _dialogueRunner = FindObjectOfType<DialogueRunner>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _textToChange.text = "";  // Hide text initially
    }

    private void Update()
    {
        if (_spriteRenderer.enabled)
        {
            // We want to check for keyboard clicks, but not mouse clicks
            if (Input.anyKeyDown && !(Input.GetMouseButtonDown(0)
            || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
            {
                _charactersShown = Mathf.Min(_charactersShown + 2, _messageToShow.Length);
                // Skip spaces.
                while (_charactersShown < _messageToShow.Length && _messageToShow[_charactersShown] == ' ')
                {
                    _charactersShown++;
                }

                _textToChange.text = _messageToShow[.._charactersShown];
                if (!_loadedNode && _charactersShown == _messageToShow.Length)
                {
                    _loadedNode = true;
                    StartCoroutine(StartYarnScriptAfterDelayCoroutine(1));
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
