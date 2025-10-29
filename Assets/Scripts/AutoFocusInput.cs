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
		// Defer focus a frame to ensure EventSystem and layout are ready
		StartCoroutine(FocusNextFrame());
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
}


