using UnityEngine;
using Yarn.Unity;

/// <summary>
/// Component that allows any GameObject to be shown or hidden via Yarn commands.
/// Attach this to any GameObject you want to control with <<show>> and <<hide>> commands.
/// </summary>
public class YarnShowHideable : MonoBehaviour
{
    
    [Header("Should Start Visible?")]
    [SerializeField] private bool _startVisible = true;
    
    private void Awake() {
        GetComponent<SpriteRenderer>().enabled = _startVisible;
    }

    /// <summary>
    /// Shows this GameObject.
    /// Usage in Yarn: <<show ObjectName>>
    /// </summary>
    [YarnCommand("show")]
    public void ShowObject()
    {
        GetComponent<SpriteRenderer>().enabled = true;
        Debug.Log($"Showed object: {gameObject.name}");
    }
    
    /// <summary>
    /// Hides this GameObject.
    /// Usage in Yarn: <<hide ObjectName>>
    /// </summary>
    [YarnCommand("hide")]
    public void HideObject()
    {
        GetComponent<SpriteRenderer>().enabled = false;
        Debug.Log($"Hid object: {gameObject.name}");
    }

}
