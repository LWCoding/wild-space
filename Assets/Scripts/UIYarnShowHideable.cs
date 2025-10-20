using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;

/// <summary>
/// Component that allows any GameObject to be shown or hidden via Yarn commands.
/// Attach this to any GameObject you want to control with <<show>> and <<hide>> commands.
/// </summary>
public class UIYarnShowHideable : MonoBehaviour
{

    [Header("Should Start Visible?")]
    [SerializeField] private bool _startVisible = true;

    private void Awake()
    {
        if (_startVisible)
        {
            ShowObject();
        }
        else
        {
            HideObject();
        }
    }

    /// <summary>
    /// Shows this GameObject.
    /// Usage in Yarn: <<show_ui ObjectName>>
    /// </summary>
    [YarnCommand("show_ui")]
    public void ShowObject()
    {
        TryGetComponent<Image>(out var img);
        if (img != null)
        {
            img.enabled = true;
        }
        TryGetComponent<TextMeshProUGUI>(out var text);
        if (text != null)
        {
            text.enabled = true;
        }

        // Also show any children under this GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);
        }
        Debug.Log($"Showed UI object: {gameObject.name}");
    }

    /// <summary>
    /// Hides this GameObject.
    /// Usage in Yarn: <<hide_ui ObjectName>>
    /// </summary>
    [YarnCommand("hide_ui")]
    public void HideObject()
    {
        TryGetComponent<Image>(out var img);
        if (img != null)
        {
            img.enabled = false;
        }
        TryGetComponent<TextMeshProUGUI>(out var text);
        if (text != null)
        {
            text.enabled = false;
        }

        // Also hide any children under this GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        Debug.Log($"Hid UI object: {gameObject.name}");
    }

}
