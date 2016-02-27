using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class ButtonPanelController : MonoBehaviour {

	private GameObject[] loginPanel, roomPanel;

	// Use this for initialization
	void Start () {
		for(int i=0; i<4; i++){
			loginPanel [i] = transform.GetChild (i).GetChild (1).GetChild (0).gameObject;
			roomPanel[i] = transform.GetChild (i).GetChild (1).GetChild (1).gameObject;
		}

		if (PlayGamesPlatform.Instance.IsAuthenticated()) {
			foreach (GameObject panel in loginPanel) {
				panel.SetActive (false);
			}
			foreach (GameObject panel in roomPanel) {
				panel.SetActive (true);
			}
		}
	}

	public void login(){
		Social.localUser.Authenticate((bool success) => {
			if(success){
				ConsoleLog.SLog("login Success");

				foreach (GameObject panel in loginPanel) {
					panel.SetActive (false);
				}
				foreach (GameObject panel in roomPanel) {
					panel.SetActive (true);
				}
			} else {
				ConsoleLog.SLog("login Fail");
			}
		});
	}
}
