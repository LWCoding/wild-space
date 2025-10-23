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
        string name = _inputField.text;
        if (name == "") {
            name = "Dipper";
        }
        _dialogueRunner.VariableStorage.SetValue("$playerName", name);
        _uiPromptContainer.SetActive(false);
        await _dialogueRunner.StartDialogue("AfterName");
    }

}
