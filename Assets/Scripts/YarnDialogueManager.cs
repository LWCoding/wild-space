using UnityEngine;
using Yarn.Unity;

public class YarnDialogueManager : MonoBehaviour
{
    [SerializeField] private YarnProject mainYarnProject;
    [SerializeField] private DialogueRunner dialogueRunner;

    async void Start()
    {
        // Load the entire project (all connected .yarn files)
        dialogueRunner.SetProject(mainYarnProject);

        // Start with a specific node
        await dialogueRunner.StartDialogue("Start");
    }
}