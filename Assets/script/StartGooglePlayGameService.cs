using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class StartGooglePlayGameService : MonoBehaviour {

	// Use this for initialization
	void Start () {
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate ();

		Social.localUser.Authenticate((bool success) => {
			if(success){
				ConsoleLog.SLog("login Success");
			} else {
				ConsoleLog.SLog("login Fail");
			}
		});
	}

	public void initializeGPGS(){
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate ();
	}

	public void login(){
		Social.localUser.Authenticate((bool success) => {
			if(success){
				ConsoleLog.SLog("login Success");
			} else {
				ConsoleLog.SLog("login Fail");
			}
		});
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
