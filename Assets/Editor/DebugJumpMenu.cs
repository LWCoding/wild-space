using UnityEditor;
using UnityEngine;
using Yarn.Unity;
using Object = UnityEngine.Object;

/// <summary>
/// Editor-only Play Mode shortcuts for checking suspected bugs on this branch.
/// Menu: Wild Space > Bug Check > ...
///
/// Bug keys (all fixed except P1, which is intentional):
///   B1  [fixed] Day 4 started on top of the black-hole dream; StartDay4 now clears the screen and shows PCRoom.
///   B2  [fixed] Neutral End is now "ENDING 4/5" (Bad End stays 3/5).
///   L1  [fixed] Intro no longer skips on the click that focuses the web page (web build only, no jump - see below).
///   L2  [fixed] The "off" monitor is hidden when the monitor turns on (Days 2, 3, 4).
///   T1  [fixed] "My device receives a strange series of pings..."
///   T2  [fixed] "(close my eyes)"
///   P1  [intentional] Day 3 "...Dad?" scene shows only faint stars + the cat.
///   P2  [fixed] Day 4 good-path farewell no longer shows the sitting sprite (stars only).
///   D1  [fixed] Removed ReportedCatDay2_GameOver (jumped to a missing node). No jump.
///   D2  [fixed] Removed the unused MonitorDontReportDay3 scene object (targeted a missing node). No jump.
///
/// L1 can't be reproduced in the editor: make a web build, load it, and click once into the page.
/// The intro should NOT disappear on that first click.
/// </summary>
public static class DebugJumpMenu
{
	private const string Root = "Wild Space/Bug Check/";

	private const string B1 = Root + "[B1] Day 3 dream into Day 4 start";
	private const string B2a = Root + "[B2] Bad End (ENDING 3-5)";
	private const string B2b = Root + "[B2] Neutral End (ENDING 3-5)";
	private const string L2d2 = Root + "[L2] Day 2 wake up - click monitor";
	private const string L2d3 = Root + "[L2] Day 3 wake up - click monitor";
	private const string L2d4 = Root + "[L2] Day 4 wake up - click monitor";
	private const string T1 = Root + "[T1] Day 1 cat intro (line is at the end)";
	private const string T2 = Root + "[T2] Day 3 dream choices";
	private const string P1 = Root + "[P1] Day 3 cat's story into Dad scene";
	private const string P2 = Root + "[P2] Day 4 good-path farewell";

	// B1: watch the black hole dream end; when Day 4 starts the PC room should be visible,
	// not the black hole behind the monitor. Also covers T2 (pick any option).
	[MenuItem(B1, false, 1)]
	private static void JumpB1() => JumpTo("DreamDay3");

	// B2: read the ending header at the end of each.
	[MenuItem(B2a, false, 2)]
	private static void JumpB2a() => JumpTo("BadEnd");

	[MenuItem(B2b, false, 3)]
	private static void JumpB2b() => JumpTo("GoodEnd");

	// L2: click the monitor; the off monitor should disappear, not sit over the on monitor.
	[MenuItem(L2d2, false, 20)]
	private static void JumpL2Day2() => JumpTo("StartDay2", "PCRoom");

	[MenuItem(L2d3, false, 21)]
	private static void JumpL2Day3() => JumpTo("StartDay3");

	// PCRoom is set up here so this checks L2 on its own (without B1 in the way).
	[MenuItem(L2d4, false, 22)]
	private static void JumpL2Day4() => JumpTo("StartDay4", "PCRoom");

	// T1: the line comes right after the cat's transmission cuts off. Turn on the Skip
	// toggle (top of screen) to fast-forward through the conversation.
	[MenuItem(T1, false, 40)]
	private static void JumpT1() => JumpTo("CatIntro", "PCRoom", "MonitorSilhouette");

	// T2: the dream ends with a choice between "(reach toward it)" and "(close your eyes)".
	[MenuItem(T2, false, 41)]
	private static void JumpT2() => JumpTo("DreamDay3");

	// P1: the cat tells the story of the human; after it, "...Dad?" plays over the stars.
	[MenuItem(P1, false, 60)]
	private static void JumpP1() => JumpTo("CatConvoDay3_End", "PCRoom", "MonitorSilhouette");

	// P2: the farewell at the viewport; the cat talks over the stars/sitting scene.
	[MenuItem(P2, false, 61)]
	private static void JumpP2() => JumpTo("CatConvoDay4_GoodPath_Farewell");

	[MenuItem(B1, true)]
	[MenuItem(B2a, true)]
	[MenuItem(B2b, true)]
	[MenuItem(L2d2, true)]
	[MenuItem(L2d3, true)]
	[MenuItem(L2d4, true)]
	[MenuItem(T1, true)]
	[MenuItem(T2, true)]
	[MenuItem(P1, true)]
	[MenuItem(P2, true)]
	private static bool ValidatePlaying() => EditorApplication.isPlaying;

	private static async void JumpTo(string nodeName, params string[] objectsToShow)
	{
		var runner = Object.FindObjectOfType<DialogueRunner>();
		if (runner == null)
		{
			Debug.LogError("DebugJumpMenu: No DialogueRunner in the scene.");
			return;
		}

		// Stop the intro screen from starting the game on the next click.
		foreach (var starter in Object.FindObjectsOfType<ClickAnywhereToStart>())
		{
			starter.enabled = false;
		}

		if (runner.IsDialogueRunning)
		{
			await runner.Stop();
		}

		// Same UI state ConfirmStart sets up.
		DialogueYarnCommands.HideAllUIObjects();
		SetUI("IntroScreen", false);
		SetUI("HistoryButton", true);
		SetUI("SkipToggle", true);

		// The state the previous node would normally leave on screen.
		if (objectsToShow.Length > 0)
		{
			DialogueYarnCommands.HideAllObjects();
			foreach (var name in objectsToShow)
			{
				SetObject(name, true);
			}
		}

		Debug.Log($"DebugJumpMenu: Jumping to node '{nodeName}'.");
		await runner.StartDialogue(nodeName);
	}

	private static void SetUI(string objectName, bool visible)
	{
		foreach (var ui in Object.FindObjectsOfType<UIYarnShowHideable>(true))
		{
			if (ui.gameObject.name != objectName) continue;
			if (visible) ui.ShowObject(); else ui.HideObject();
		}
	}

	private static void SetObject(string objectName, bool visible)
	{
		foreach (var obj in Object.FindObjectsOfType<YarnShowHideable>(true))
		{
			if (obj.gameObject.name != objectName) continue;
			if (visible) obj.ShowObject(); else obj.HideObject();
		}
	}
}
