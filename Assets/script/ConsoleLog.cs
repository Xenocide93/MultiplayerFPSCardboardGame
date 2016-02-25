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
		ConsoleLog.console = transform.GetChild(0).GetChild(1).GetComponent<Text> ();
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
		console.text = "--- " + log + "\n" + console.text;
	}

	public static void SLog(Exception log){
		Debug.LogWarning (log);
		console.text = "--- " + log + "\n" + console.text;
	}

	public static void SLogWarning(string log){
		Debug.LogWarning (log);
		console.text = "--- " + log + "\n" + console.text;
	}

	public static void SLogError(string log){
		Debug.LogError (log);
		console.text = "--- " + log + "\n" + console.text;
	}

	public static void SClearLog(){
		console.text = "";
	}

	public void Log(string log){
		Debug.Log (log);
		ConsoleLog.console.text = "--- " + log + "\n" + console.text;
	}

	public void ClearLog(){
		ConsoleLog.console.text = "";
	}
}
