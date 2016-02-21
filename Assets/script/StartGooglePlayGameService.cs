using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class StartGooglePlayGameService : MonoBehaviour {

	public Text debugText;

	// Use this for initialization
	void Start () {
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate ();

		Social.localUser.Authenticate((bool success) => {
			if(success){
				Debug.Log("login Success");
				debugText.text = "login Success";
			} else {
				Debug.Log("login Fail");
				debugText.text = "login Fail";
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
				Debug.Log("login Success");
				debugText.text = "login Success";
			} else {
				Debug.Log("login Fail");
				debugText.text = "login Fail";
			}
		});
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
