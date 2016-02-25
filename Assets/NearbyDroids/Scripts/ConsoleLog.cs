using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ConsoleLog : MonoBehaviour {

	public static ConsoleLog Instanst;
	private static Text console;

	// Use this for initialization
	void Start () {
		ConsoleLog.Instanst = this;
		ConsoleLog.console = GetComponent<Text> ();
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
