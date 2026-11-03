using UnityEngine;
using Yarn.Unity;

public class ClickAnywhereToStart : MonoBehaviour
{
	[SerializeField] private string _nodeName = "Start";
	[SerializeField] private DialogueRunner _dialogueRunner;

	[Header("Click Guarding")]
	[Tooltip("Clicks within this many seconds of the window gaining focus are ignored, so the click that focuses the web page doesn't start the game.")]
	[SerializeField] private float _ignoreClicksAfterFocusSeconds = 0.3f;
	[Tooltip("Clicks within this many seconds of the scene loading are ignored.")]
	[SerializeField] private float _ignoreClicksAfterLoadSeconds = 0.5f;

	private bool _hasStarted = false;
	private float _enabledTime;
	private float _focusGainedTime;

	private void Awake()
	{
		if (_dialogueRunner == null)
		{
			_dialogueRunner = FindObjectOfType<DialogueRunner>();
		}
	}

	private void OnEnable()
	{
		_enabledTime = Time.unscaledTime;
		// If we already have focus, treat it as gained at load time.
		_focusGainedTime = Application.isFocused ? Time.unscaledTime : float.PositiveInfinity;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		_focusGainedTime = hasFocus ? Time.unscaledTime : float.PositiveInfinity;
	}

	private bool CanAcceptClick()
	{
		float now = Time.unscaledTime;
		return Application.isFocused
			&& now - _enabledTime >= _ignoreClicksAfterLoadSeconds
			&& now - _focusGainedTime >= _ignoreClicksAfterFocusSeconds;
	}

	private void Update()
	{
		if (_hasStarted)
		{
			return;
		}

		if (Input.GetMouseButtonDown(0))
		{
			// Ignore the click that focuses the window (e.g. clicking into the WebGL canvas)
			if (!CanAcceptClick())
			{
				return;
			}

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
