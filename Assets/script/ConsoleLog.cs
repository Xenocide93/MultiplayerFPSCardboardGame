using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConsoleLog : MonoBehaviour {
	public bool showMenu;

	public static ConsoleLog Instanst;
	private static Text console;
	private static GameObject ConsoleCanvas;
	private static bool isShowMenu;
	private static GazeInputModule pGazeModule;

	// Use this for initialization
	void Start () {
		ConsoleLog.isShowMenu = showMenu;
		ConsoleLog.pGazeModule = GameObject.FindObjectOfType<GazeInputModule>();

		ConsoleLog.Instanst = this;
		ConsoleLog.console = GameObject.FindGameObjectWithTag("ConsoleLog").GetComponent<Text> ();
		ConsoleLog.console.text = "";
		ConsoleLog.ConsoleCanvas = gameObject;
	}

	public static void SToggleMenu(){
		ConsoleLog.isShowMenu = !ConsoleLog.isShowMenu;
//		ConsoleLog.pGazeModule.enabled = !isShowMenu;
		ConsoleLog.ConsoleCanvas.transform.GetChild(0).gameObject.SetActive (isShowMenu);
		if (isShowMenu)
			ConsoleLog.SLog ("Show Log");
		else
			ConsoleLog.SLog ("Hide Log");
	}

	public void ToggleMenu(){
		ConsoleLog.SToggleMenu ();
	}
		
	public static void SLog(string log){
		Debug.Log (log);

		if (console == null) return;
		console.text = "--- " + log + "\n" + console.text;

		if (CountLine (console.text) > 500) {
			ConsoleLog.SClearLog ();
		}
	}

	public static void SLog(Exception log){
		Debug.LogWarning (log);

		if (console == null) return;
		console.text = "--- " + log + "\n" + console.text;
	}

	public static void SLogWarning(string log){
		Debug.LogWarning (log);

		if (console == null) return;
		console.text = "--- " + log + "\n" + console.text;
	}

	public static void SLogError(string log){
		Debug.LogError (log);

		if (console == null) return;
		console.text = "--- " + log + "\n" + console.text;
	}

	public static void SClearLog(){
		if (console == null) return;
		console.text = "";
	}

	public void Log(string log){
		Debug.Log (log);

		if (console == null) return;
		ConsoleLog.console.text = "--- " + log + "\n" + console.text;
	}

	public void ClearLog(){
		if (console == null) return;
		ConsoleLog.console.text = "";
	}

	private static int CountLine(string s)
	{
		int n = 0;
		foreach( var c in s )
		{
			if ( c == '\n' ) n++;
		}
		return n+1;
	}
}
