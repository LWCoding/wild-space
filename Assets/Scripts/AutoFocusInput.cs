using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
 

[DisallowMultipleComponent]
public class AutoFocusInput : MonoBehaviour
{
	[SerializeField]
	private TMP_InputField tmpInputField;

	private void Awake()
	{
		// Auto-wire if not assigned
		if (tmpInputField == null)
		{
			tmpInputField = GetComponent<TMP_InputField>();
		}
	}

	private void OnEnable()
	{
		// Wire input handlers
		if (tmpInputField != null)
		{
			tmpInputField.onValueChanged.AddListener(OnValueChanged);
			tmpInputField.onEndEdit.AddListener(OnEndEdit);
		}

		// Defer focus a frame to ensure EventSystem and layout are ready
		StartCoroutine(FocusNextFrame());
	}

	private void OnDisable()
	{
		if (tmpInputField != null)
		{
			tmpInputField.onValueChanged.RemoveListener(OnValueChanged);
			tmpInputField.onEndEdit.RemoveListener(OnEndEdit);
		}
	}

	private IEnumerator FocusNextFrame()
	{
		// Give Unity a frame (or two) to finish enabling UI
		yield return null;
		yield return new WaitForEndOfFrame();

		var eventSystem = EventSystem.current;
		if (eventSystem == null)
		{
			yield break;
		}

		// Prefer TMP if present
		if (tmpInputField != null && tmpInputField.isActiveAndEnabled && tmpInputField.gameObject.activeInHierarchy)
		{
			eventSystem.SetSelectedGameObject(tmpInputField.gameObject);
			// Simulate click to place caret, then activate
			tmpInputField.OnPointerClick(new PointerEventData(eventSystem));
			tmpInputField.Select();
			tmpInputField.ActivateInputField();
			yield break;
		}

		// If no TMP field found, do nothing
	}

	private void OnValueChanged(string currentValue)
	{
		if (tmpInputField == null)
		{
			return;
		}

		// Prevent first character from being a space
		if (currentValue.Length == 1 && currentValue[0] == ' ')
		{
			SetTextPreserveCaret(string.Empty, 0);
			return;
		}
	}

	private void OnEndEdit(string submittedValue)
	{
		if (tmpInputField == null)
		{
			return;
		}
	}

	private void SetTextPreserveCaret(string newText, int newCaretPosition)
	{
		// Update text while minimizing recursive onValueChanged churn
		if (tmpInputField.text == newText)
		{
			return;
		}

		// Set both string and caret positions for TMP
		tmpInputField.text = newText;
		tmpInputField.stringPosition = newCaretPosition;
		tmpInputField.caretPosition = newCaretPosition;
	}
}


