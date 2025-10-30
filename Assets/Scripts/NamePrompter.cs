using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class NamePrompter : MonoBehaviour
{

    [SerializeField] private DialogueRunner _dialogueRunner;

    [Header("UI Assignments")]
    [SerializeField] private GameObject _uiPromptContainer;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _confirmButton;

    private void Awake()
    {
        _uiPromptContainer.SetActive(false);
        _confirmButton.onClick.AddListener(EnterName);
        _inputField.onValidateInput += OnValidateInput;
        _inputField.onSubmit.AddListener(OnInputSubmit);
    }

    // Allow only English letters and spaces, max of 12 chars while typing
    private char OnValidateInput(string text, int charIndex, char addedChar)
    {
        if (text.Length >= 12)
        {
            return '\0'; // reject if over max
        }
        if ((addedChar >= 'A' && addedChar <= 'Z') || (addedChar >= 'a' && addedChar <= 'z') || addedChar == ' ')
        {
            return addedChar;
        }
        return '\0';
    }

    // Trigger confirm button on Enter
    private void OnInputSubmit(string submittedValue)
    {
        if (_confirmButton != null && _confirmButton.interactable)
        {
            _confirmButton.onClick.Invoke();
        }
    }

    [YarnCommand("prompt_name")]
    public void PromptName()
    {
        _uiPromptContainer.SetActive(true);
    }

    /// <summary>
    /// Should be called by the confirm button. Sets variable to store name.
    /// </summary>
    public async void EnterName()
    {
        string name = _inputField.text; // Don't trim spaces during typing, do it only here
        name = name.TrimEnd(); // Only trim on submission
        if (name == "") {
            name = "Dipper";
        }
        // Only first 12 characters, trimmed spaces, only letters and spaces remain
        name = name.Length > 12 ? name.Substring(0, 12) : name;
        name = System.Text.RegularExpressions.Regex.Replace(name, "[^A-Za-z ]", "");
        _dialogueRunner.VariableStorage.SetValue("$playerName", name);
        _uiPromptContainer.SetActive(false);
        await _dialogueRunner.StartDialogue("AfterName");
    }

}
