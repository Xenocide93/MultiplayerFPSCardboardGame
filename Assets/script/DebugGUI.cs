using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Nearby;
using UnityEngine.SocialPlatforms;

public class DebugGUI : MonoBehaviour {
	public bool startWithGUI;

	public GazeInputModule gazeModule;
	static private Text debugText;
	private bool showMenu;

	// Use this for initialization
	void Start () {
		DebugGUI.debugText = transform.GetChild (1).GetComponent<Text> ();

		showMenu = !startWithGUI;
		ToggleMenu ();
	}

	public void ToggleMenu(){

		showMenu = !showMenu;
		gazeModule.enabled = !showMenu;
		transform.GetChild (2).gameObject.SetActive (showMenu);
	}
}
