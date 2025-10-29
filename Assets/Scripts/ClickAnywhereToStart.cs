using UnityEngine;
using Yarn.Unity;

public class ClickAnywhereToStart : MonoBehaviour
{
	[SerializeField] private string _nodeName = "Start";
	[SerializeField] private DialogueRunner _dialogueRunner;

	private bool _hasStarted = false;

	private void Awake()
	{
		if (_dialogueRunner == null)
		{
			_dialogueRunner = FindObjectOfType<DialogueRunner>();
		}
	}

	private void Update()
	{
		if (_hasStarted)
		{
			return;
		}

		if (Input.GetMouseButtonDown(0))
		{
			if (_dialogueRunner == null)
			{
				Debug.LogWarning("ClickAnywhereToStart: No DialogueRunner found in scene.", this);
				enabled = false;
				return;
			}

			if (_dialogueRunner.IsDialogueRunning)
			{
				// Dialogue already running; don't trigger again
				_hasStarted = true;
				enabled = false;
				return;
			}

			_hasStarted = true;
			_dialogueRunner.StartDialogue(_nodeName);
			enabled = false; // ensure it only runs once
		}
	}
}


