using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class RoomSetupUiController : MonoBehaviour {
	public static RoomSetupUiController instance;

	public const int STATE_LOGIN = 0;
	public const int STATE_INDEX = 1;
	public const int STATE_SELECT_MODE = 2;
	public const int STATE_SELECT_ROOM_SIZE = 3;
	public const int STATE_SELECT_TEAM = 4;
	public const int STATE_VR_READY = 5;
	public const int STATE_START_GAME = 6;

	public const int SIDE_NORTH = 0;
	public const int SIDE_SOUTH = 1;
	public const int SIDE_EAST = 2;
	public const int SIDE_WEST = 3;

	private int uiState = 0;

	private GameObject[] title = new GameObject[4];
	private GameObject[] backBtns = new GameObject[4];
	private GameObject[] ui0 = new GameObject[4];
	private GameObject[] ui1 = new GameObject[4];
	private GameObject[] ui2 = new GameObject[4];
	private GameObject[] ui3 = new GameObject[4];
	private GameObject[] ui4 = new GameObject[4];
	private GameObject[] ui5 = new GameObject[4];
	private GameObject[] ui6 = new GameObject[4];

	void Awake () {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	// Use this for initialization
	void Start () {
		StartGooglePlayGamesServices ();
		FindUiElements ();
		SetUiState (STATE_LOGIN);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetUiState (int state) {
		ResetUi ();

		switch (state) {
		case STATE_LOGIN:
			SetActiveGameObjects (ui0, true);
			SetActiveGameObjects (backBtns, false);
			SetActiveGameObjects (title, true);
			uiState = STATE_LOGIN;
			break;
		case STATE_INDEX:
			SetActiveGameObjects (ui1, true);
			SetActiveGameObjects (backBtns, true);
			SetActiveGameObjects (title, true);
			uiState = STATE_INDEX;
			break;
		case STATE_SELECT_MODE:
			SetActiveGameObjects (ui2, true);
			SetActiveGameObjects (backBtns, true);
			SetActiveGameObjects (title, true);
			uiState = STATE_SELECT_MODE;
			break;
		case STATE_SELECT_ROOM_SIZE:
			SetActiveGameObjects (ui3, true);
			SetActiveGameObjects (backBtns, true);
			SetActiveGameObjects (title, true);
			uiState = STATE_SELECT_ROOM_SIZE;
			break;
		case STATE_SELECT_TEAM:
			SetActiveGameObjects (ui4, true);
			SetActiveGameObjects (backBtns, true);
			SetActiveGameObjects (title, false);
			uiState = STATE_SELECT_TEAM;
			break;
		case STATE_VR_READY:
			SetActiveGameObjects (ui5, true);
			SetActiveGameObjects (backBtns, false);
			SetActiveGameObjects (title, false);
			uiState = STATE_VR_READY;
			break;
		case STATE_START_GAME:
			SetActiveGameObjects (ui6, true);
			SetActiveGameObjects (backBtns, false);
			SetActiveGameObjects (title, false);
			uiState = STATE_START_GAME;
			break;
		default:
			ConsoleLog.SLog ("Error: Not found ui state (" + state + ")");
			break;
		}
	}

	public void ResetUi () {
		SetActiveGameObjects (ui0, false);
		SetActiveGameObjects (ui1, false);
		SetActiveGameObjects (ui2, false);
		SetActiveGameObjects (ui3, false);
		SetActiveGameObjects (ui4, false);
		SetActiveGameObjects (ui5, false);
		SetActiveGameObjects (ui6, false);
		SetActiveGameObjects (backBtns, false);
		SetActiveGameObjects (title, false);
	}

	public void PreviousScreen () {
		switch (uiState) {
		case STATE_LOGIN:
			ConsoleLog.SLog ("something wrong ? (1)");
			SetUiState (STATE_LOGIN);
			break;
		case STATE_INDEX: 
			SetUiState (STATE_LOGIN); 
			break;
		case STATE_SELECT_MODE: 
			SetUiState (STATE_INDEX);
			break;
		case STATE_SELECT_ROOM_SIZE:
			SetUiState (STATE_SELECT_MODE);
			break;
		case STATE_SELECT_TEAM:
			SetUiState (STATE_INDEX);
			break;
		case STATE_VR_READY:
			ConsoleLog.SLog ("something wrong ? (2)");
			SetUiState (STATE_INDEX);
			break;
		case STATE_START_GAME:
			ConsoleLog.SLog ("something wrong ? (3)");
			SetUiState (STATE_INDEX);
			break;
		default:
			ConsoleLog.SLog ("Error: Not found ui state (" + uiState + ")");
			break;
		}
	}

	void FindUiElements () {
		for (int i = 0; i < 4; i++) {
			Transform panel = transform.GetChild (i);
			title [i] = panel.GetChild (0).gameObject;
			Transform canvas = panel.GetChild (1);
			ui0[i] = canvas.GetChild (0).gameObject;
			ui1[i] = canvas.GetChild (1).gameObject;
			ui2[i] = canvas.GetChild (2).gameObject;
			ui3[i] = canvas.GetChild (3).gameObject;
			ui4[i] = canvas.GetChild (4).gameObject;
			ui5[i] = canvas.GetChild (5).gameObject;
			ui6[i] = canvas.GetChild (6).gameObject;
			backBtns[i] = canvas.GetChild (7).gameObject;

			if (Application.isEditor) { break; }
		}
	}

	private void StartGooglePlayGamesServices () {
		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			// enables saving game progress.
			.EnableSavedGames()
			.Build();
		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
	}

	public void LoginGPGS () {
		if (Application.isEditor) {
			ConsoleLog.SLog("login bypass");
			SetUiState (STATE_INDEX);
			return;
		}

		Social.localUser.Authenticate((bool success) => {
			if(success){
				ConsoleLog.SLog("login Success");
				SetUiState (STATE_INDEX);
			} else {
				ConsoleLog.SLog("login Fail");
			}
		});
	}

	private void SetActiveGameObjects (GameObject[] gameObjects, bool active) {
		foreach (GameObject eachGameObject in gameObjects) {
			if (eachGameObject != null) eachGameObject.SetActive (active);
		}
	}
}
