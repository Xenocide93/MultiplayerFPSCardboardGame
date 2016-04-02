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
	public const int STATE_SELECT_MAP = 7;

	public const int TEAM_INDEX_P0 = 0;
	public const int TEAM_INDEX_P1 = 1;
	public const int TEAM_INDEX_PX0 = 2;
	public const int TEAM_INDEX_PX1 = 3;
	public const int TEAM_INDEX_P2 = 4;
	public const int TEAM_INDEX_P3 = 5;
	public const int TEAM_INDEX_PX2 = 6;
	public const int TEAM_INDEX_PX3 = 7;

	public GameObject cardboardMain;
	private int uiState = 0;

	private GameObject title;
	private GameObject backBtns;
	private GameObject ui0, ui1, ui2, ui3, ui4, ui5, ui6, ui7;

	//select team UI element
	[HideInInspector] public GameObject p0, p1, p2, p3, px0, px1, px2, px3;
	[HideInInspector] public GameObject switchTeamBtn, startBtn;

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

	public void SetUiState (int state) {
		ResetUi ();

		switch (state) {
		case STATE_LOGIN:
			Show (ui0);
			Hide (backBtns);
			Show (title);
			uiState = STATE_LOGIN;
			break;
		case STATE_INDEX:
			Show (ui1);
			Show (backBtns);
			Show (title);
			uiState = STATE_INDEX;
			break;
		case STATE_SELECT_MODE:
			Show (ui2);
			Show (backBtns);
			Show (title);
			uiState = STATE_SELECT_MODE;
			break;
		case STATE_SELECT_ROOM_SIZE:
			Show (ui3);
			Show (backBtns);
			Show (title);
			uiState = STATE_SELECT_ROOM_SIZE;
			break;
		case STATE_SELECT_TEAM:
			Show (ui4);
			Show (backBtns);
			Hide (title);
			Hide (p0);
			Hide (p1);
			Hide (p2);
			Hide (p3);
			Hide (px0);
			Hide (px1);
			Hide (px2);
			Hide (px3);
			Hide (startBtn);
			uiState = STATE_SELECT_TEAM;
			break;
		case STATE_VR_READY:
			Show (ui5);
			Hide (backBtns);
			Hide (title);
			uiState = STATE_VR_READY;
			break;
		case STATE_START_GAME:
			Show (ui6);
			Hide (backBtns);
			Hide (title);
			uiState = STATE_START_GAME;
			break;
		case STATE_SELECT_MAP:
			Show (ui7);
			Show (backBtns);
			Show (title);
			uiState = STATE_SELECT_MAP;
			break;
		default:
			ConsoleLog.SLog ("Error: Not found ui state (" + state + ")");
			break;
		}
	}

	public void ResetUi () {
		Hide (ui0);
		Hide (ui1);
		Hide (ui2);
		Hide (ui3);
		Hide (ui4);
		Hide (ui5);
		Hide (ui6);
		Hide (ui7);
		Hide (backBtns);
		Hide (title);
	}

	public void PreviousScreen () {
		switch (uiState) {
		case STATE_LOGIN: 
			SetUiState (STATE_LOGIN); break;
		case STATE_INDEX: 
			SetUiState (STATE_LOGIN); break;
		case STATE_SELECT_MODE:
			SetUiState (STATE_SELECT_MAP); break;
		case STATE_SELECT_ROOM_SIZE: 
			SetUiState (STATE_SELECT_MODE); break;
		case STATE_SELECT_TEAM: 
			SetUiState (STATE_INDEX); break;
		case STATE_VR_READY: 
			SetUiState (STATE_INDEX); break;
		case STATE_START_GAME: 
			SetUiState (STATE_INDEX); break;
		case STATE_SELECT_MAP: 
			SetUiState (STATE_INDEX); break;
		default:
			ConsoleLog.SLog ("Error: Not found ui state (" + uiState + ")");
			break;
		}
	}

	void FindUiElements () {
		title = transform.GetChild (0).gameObject;
		Transform canvas = transform.GetChild (1);
		ui0 = canvas.GetChild (0).gameObject;
		ui1 = canvas.GetChild (1).gameObject;
		ui2 = canvas.GetChild (2).gameObject;
		ui3 = canvas.GetChild (3).gameObject;
		ui4 = canvas.GetChild (4).gameObject;
		ui5 = canvas.GetChild (5).gameObject;
		ui6 = canvas.GetChild (6).gameObject;
		ui7 = canvas.GetChild (7).gameObject;
		backBtns = canvas.GetChild (8).gameObject;

		//select team ui
		p0 = ui4.transform.GetChild (1).gameObject;
		p1 = ui4.transform.GetChild (2).gameObject;
		px0 = ui4.transform.GetChild (3).gameObject;
		px1 = ui4.transform.GetChild (4).gameObject;

		p2 = ui4.transform.GetChild (6).gameObject;
		p3 = ui4.transform.GetChild (7).gameObject;
		px2 = ui4.transform.GetChild (8).gameObject;
		px3 = ui4.transform.GetChild (9).gameObject;

		switchTeamBtn = ui4.transform.GetChild (10).gameObject;
		startBtn = ui4.transform.GetChild (11).gameObject;
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

	public void ResetCameraRotation (){
		ConsoleLog.SLog("ResetCameraRotation ()");
		Transform cardboardHead = cardboardMain.transform.GetChild (0).transform;
		Vector3 mainRot = cardboardMain.transform.rotation.eulerAngles;
		Vector3 headRot = cardboardHead.transform.localRotation.eulerAngles;

		cardboardMain.transform.rotation = Quaternion.Euler(new Vector3 (
			mainRot.x,
			-headRot.y,
			mainRot.z
		));
	}

	public void RefreshTeam (){
		//hide all player panel
		Hide (p0); Hide (p1); Hide (p2); Hide (p3);
		Hide (px0) ;Hide (px1); Hide (px2); Hide (px3);

		for (int i = 0; i < MultiplayerController.instance.tempGamePlayerNum.Length; i++) {
			SetNameAndAvatar (
				MultiplayerController.instance.tempGamePlayerNum [i],
				MultiplayerController.instance.playersName [i],
				MultiplayerController.instance.playersAvatar[i]
			);
			Show (GetNameAndAvatarPanel (MultiplayerController.instance.tempGamePlayerNum [i]));
		}
	}

	public void SetNameAndAvatar(int gamePlayerNum, string name, Texture2D avatar) {
		GameObject p = GetNameAndAvatarPanel (gamePlayerNum);
		Text nameText = p.transform.GetChild (1).GetComponent<Text> ();
		Image avatarImg = p.transform.GetChild (0).GetComponent<Image> ();

		if (nameText != null && avatarImg != null) {
			nameText.text = name;
			Sprite avatarSprite = Sprite.Create (
				avatar,
				new Rect (0, 0, avatar.width, avatar.height),
				new Vector2 (0f, 0f)
			);
			avatarImg.overrideSprite = avatarSprite;
		}
	}

	private GameObject GetNameAndAvatarPanel (int gamePlayerNum) {
		switch (gamePlayerNum) {
		case TEAM_INDEX_P0: return p0;
		case TEAM_INDEX_P1: return p1;
		case TEAM_INDEX_PX0: return px0;
		case TEAM_INDEX_PX1: return px1;
		case TEAM_INDEX_P2: return p2;
		case TEAM_INDEX_P3: return p3;
		case TEAM_INDEX_PX2: return px2;
		case TEAM_INDEX_PX3: return px3;
		default: return null;
		}
	}

	private void SetActive (GameObject[] gameObjects, bool active) {
		foreach (GameObject eachGameObject in gameObjects) {
			if (eachGameObject != null) eachGameObject.SetActive (active);
		}
	}

	public void Show (GameObject uiItem) {
		uiItem.SetActive (true);
	}

	public void Hide (GameObject uiItem) {
		uiItem.SetActive (false);
	}
}
