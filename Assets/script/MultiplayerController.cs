using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
using UnityEngine.SceneManagement;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MultiplayerController : MonoBehaviour {
	
	public static MultiplayerController instance;

	#region Class Variable
	
	//Payload Tag
	public const string PLAYER_DATA = "playerData";
	public const string REQ_INIT_ROOM = "reqInitRoom";
	public const string RES_INIT_ROOM = "resInitRoom";
	public const string ROOM_INTRODUCTION = "roomIntroduction";
	public const string REQ_SWITCH_TEAM = "reqSwitchTeam";
	public const string RES_SWITCH_TEAM = "resSwitchTeam";
	public const string FINISH_SELECT_TEAM = "finishSelectTeam";
	public const string READY = "ready";
	public const string RES_INIT_GAME = "resInitGame";
	public const string	REQ_LEAVE_ROOM = "reqLeaveRoom";
	public const string INFLICT_DAMAGE = "inflictDamage";
	public const string FIRE_RAY = "fireRay";
	public const string HAND_GRENADE = "handGrenade";
	public const string DESTROY_ITEM = "destroyItem";
	public const string FINISH_LOAD_SCENE = "finishLoadScene";
	public const string START_ROUND = "startRound";
	public const string INIT_NEXT_ROUND = "initNextRound";
	public const string END_ROUND = "endRound";
//
	//Character Type Tag
	public const int CHAR_TYPE_PISTOL = 1;
	public const int CHAR_TYPE_RIFLE = 2;
	public const int CHAR_TYPE_SHORTGUN = 3;
	public const int CHAR_TYPE_SNIPER = 4;

	//prefab for each character type
	public GameObject otherPlayerDummyPrefab;
	public GameObject UnityChanWithPisolPrefab;
	public GameObject UnityChanWithRiflePrefab;
	public GameObject UnityChanWithShotgunPrefab;
	public GameObject UnityChanWithSniperPrefab;

	//animation state tag
	public const int ANIM_IDLE = 0;
	public const int ANIM_AIM = 1;
	public const int ANIM_WALK_FORWARD = 2;
	public const int ANIM_WALK_LEFT = 3;
	public const int ANIM_WALK_RIGHT = 4;
	public const int ANIM_AIM_WALK_FORWARD = 5;
	public const int ANIM_AIM_WALK_LEFT = 6;
	public const int ANIM_AIM_WALK_RIGHT = 7;
	public const int ANIM_JUMP = 8;
	public const int ANIM_WALK_BACKWARD = 9;
	public const int ANIM_AIM_WALK_BACKWARD = 10;

	//game mode tag
	public const uint GAMEMODE_DEATHMATCH = 0;
	public const uint GAMEMODE_TEAM = 1;

	//map tag (Scene Build Index)
	public const int MAP_PAINTBALL = 1;
	public const int MAP_VILLAGE = 2;

	private Animator localAnimator;
	[HideInInspector] public int localAnimationState = ANIM_IDLE;

	//for debug
	public Text latestPlayerDataText;

	//Game Room Setting
	const int MinOpponents = 1;
	public uint MaxOpponents = 3;
	[HideInInspector] public int localRoomPlayerNumber = -1;
	[HideInInspector] public int map = MAP_PAINTBALL;
	[HideInInspector] public uint gameMode = GAMEMODE_DEATHMATCH;
	[HideInInspector] public int playerCount = 0;
	[HideInInspector] public bool[] playersReady;
	[HideInInspector] public string[] playersAvatarUrl;
	[HideInInspector] public Texture2D[] playersAvatar;
	[HideInInspector] public String[] playersName;
	[HideInInspector] public bool[] isFinishLoadScene;
	private bool haveSentEndRound = false;

	//Team mode setting
	[HideInInspector] public int round = 0;
	[HideInInspector] public int team1Score = 0;
	[HideInInspector] public int team2Score = 0;
	[HideInInspector] public int[] tempGamePlayerNum;
	[HideInInspector] public int[] playersTeamNumber;
	[HideInInspector] public bool isFinishSelectTeam = false;

	//Game Play Setting
	[HideInInspector] public int localGamePlayerNumber = -1;
	public bool isGameStart = false;
	[HideInInspector] public bool hadMovedToSpawnPoint = false;
	private bool firstTimeFoundAllComponent = true;

	private GameObject localPlayer, cardboardHead;
	private PlayerGameManager localGameManager;
	private UnityChanControlScriptWithRgidBody localUnityChanControlScript;
	[HideInInspector] public GameObject[] remoteCharacterGameObjects;
	[HideInInspector] public PlayerGameData[] latestPlayerDatas;
	[HideInInspector] public Vector3[] spawnPoints;
	[HideInInspector] public bool[] hasNewPlayerDatas;
	[HideInInspector] public bool[] updatedLastFrame;
	[HideInInspector] public string[] clientId;

	public float broadcastDataPerSec;
	[HideInInspector] public float timeBetweenBroadcast;
	private float broadcastTimer = 0f;

	private bool isBroadcast = true;

	#endregion

	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void Start() {
		timeBetweenBroadcast = 1 / broadcastDataPerSec;
	}

	void OnLevelWasLoaded (int levelIndex){
		ConsoleLog.SLog ("OnLevelWasLoaded (" + levelIndex + ")");
		if(levelIndex == 1 || levelIndex == 2) SendFinishLoadScene ();
	}

	void FixedUpdate () {
		CheckNullComponents ();
	}

	void LateUpdate() {
		if (isGameStart) {
			ResetNewPlayerDataFlag ();
			BroadcastPlayerData ();
		}
	}

	void OnGUI(){
//		PrintPlayerData ();
	}

	private IEnumerator  KeepFindingComponentsUntilFound(){
		int count = 0;

		while (IsComponentsNull()) {
			count++;
			ConsoleLog.SLog ("FindComponents (): " + count + " times");
			FindComponents ();
			yield return null;
		}
		ConsoleLog.SLog ("FindComponents (): Found it !");
		if (firstTimeFoundAllComponent) {
			firstTimeFoundAllComponent = false;
			SetupGameplayData ();
		}
	}

	private void CheckNullComponents() {
		if (!isGameStart) return;

		//In case of changing character mid game, It might be null game object.
		if (IsComponentsNull()) {
			FindComponents ();
		}
	}

	private bool IsComponentsNull() {
		return 
			localPlayer == null ||
			cardboardHead == null ||
			localGameManager == null ||
			localUnityChanControlScript == null ||
			localAnimator == null ||
			spawnPoints == null;
	}

	private void FindComponents () {
		ConsoleLog.SLog ("FindComponents()");

		try {
			localPlayer = GameObject.FindGameObjectWithTag ("Player");
			cardboardHead = GameObject.FindGameObjectWithTag ("PlayerHead");
			localGameManager = localPlayer.GetComponent<PlayerGameManager> ();
			localUnityChanControlScript = localPlayer.GetComponent<UnityChanControlScriptWithRgidBody> ();
			localAnimator = localPlayer.GetComponent<Animator> ();
			FindSpawnPoints ();

			// if found everything for the first time, setup those component
			if ( !IsComponentsNull() && firstTimeFoundAllComponent) {
				firstTimeFoundAllComponent = false;
				SetupGameplayData();
			}

		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in FindComponents()\n" + e.Message);
		}

	}

	//when game scene is loaded, setup
	private void SetupGameplayData () {
		ConsoleLog.SLog ("SetupGameplayData ()");

		localGameManager.team = playersTeamNumber[localGamePlayerNumber];
		localGameManager.health = 100f;
		localGameManager.HideDeadText ();
		localGameManager.HideRoundEndText ();
		localGameManager.HideGameEndText ();

		if (!hadMovedToSpawnPoint && spawnPoints [localGamePlayerNumber] != null) {
			MoveToSpawnPoint (localGamePlayerNumber);
		} else if (spawnPoints [localGamePlayerNumber] == null) {
			ConsoleLog.SLog ("spawnPoints [" + spawnPoints [localGamePlayerNumber] + "] = null");
		} else if (hadMovedToSpawnPoint) {
			ConsoleLog.SLog ("hadMovedToSpawnPoint = true");
		}
	}

	private void FindSpawnPoints () {
		GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag ("SpawnPoint");
		spawnPoints = new Vector3[spawnPointObjects.Length];
		for (int i = 0; i < spawnPoints.Length; i++) {
			spawnPoints [i] = spawnPointObjects [i].transform.position;
		}
	}

	private void MoveToSpawnPoint (int spawnPointNumber){
		ConsoleLog.SLog (
			"MoveToSpawnPoint"+spawnPointNumber+" ("+ 
			spawnPoints [spawnPointNumber][0] + ", " + 
			spawnPoints [spawnPointNumber][1] + ", " + 
			spawnPoints [spawnPointNumber][2] + ")"
		);

		localPlayer.transform.position = spawnPoints [spawnPointNumber];
		hadMovedToSpawnPoint = true;
	}

	public int GetPlayerNumber(string playerId) {
		for (int i=0; i<clientId.Length; i++) {
			if (clientId[i] != null && clientId [i].Equals (playerId)) {
				return i;
			}
		}

		ConsoleLog.SLog ("Error: player Id not match (" + playerId + ")");
		return -1;
	}

	public string GetClientId(int playerNum) {
		if (clientId [playerNum] != null) {
			ConsoleLog.SLog ("Player: " + playerNum + " Name: "+playersName[playerNum]+" ID: " + clientId [playerNum]);
			return clientId [playerNum];
		}

		ConsoleLog.SLog ("Error: Client Id not found (" + playerNum + ")");
		return "";
	}

	private IEnumerator LoadAvatarImage(int playerNum, string url) {
		ConsoleLog.SLog ("Start LoadAvatarImage(" + playerNum + ")");

		// Start a download of the given URL
		WWW www = new WWW(url);

		// Wait for download to complete
		yield return www;

		ConsoleLog.SLog ("Finish LoadAvatarImage(" + playerNum + ")");
		// save texture
		playersAvatar[playerNum] = www.texture;
	}

	private IEnumerator LoadAvatarImageAllThenChangeUi() {

		while (localRoomPlayerNumber == -1) {
			ConsoleLog.SLog ("LoadAvatarImageAllThenChangeUi() before JoinRoom() >>> wait");
			yield return null;
		}

		for (int i = 0; i < MaxOpponents + 1; i++) {

			if (playersAvatar [i] != null) continue;

			ConsoleLog.SLog ("Start LoadAvatarImage(" + i + ")");

			// Start a download of the given URL
			WWW www = new WWW(playersAvatarUrl [i]);

			// Wait for download to complete
			yield return www;

			ConsoleLog.SLog ("Finish LoadAvatarImage(" + i + ")");
			// save texture
			playersAvatar[i] = www.texture;
		}

		for (int i = 0; i < MaxOpponents + 1; i++) {
			if (playersAvatar [i] == null) {
				ConsoleLog.SLog ("playersAvatar ["+i+"] == null)");
			}
		}

		//after finish load all avatar, show select team panel or vr ready panel
		MultiplayerController.instance.SetRoomUiByGameMode (MultiplayerController.instance.gameMode);
	}

	// ============== Game Room Function ============== //
	#region Game Room Function

	public void CreateRoomWithInvite(int roomCapacity, uint mode) {
		ConsoleLog.SLog ("CreateRoomWithInvite");

		InitializeRoomCapacity (roomCapacity);

		gameMode = mode;
		localRoomPlayerNumber = 0; //player number 0 is host
		playersAvatarUrl[0] = PlayGamesPlatform.Instance.GetUserImageUrl();
		playersName [0] = PlayGamesPlatform.Instance.GetUserDisplayName();
		if (gameMode == GAMEMODE_DEATHMATCH) {
			for (int i = 0; i < playersTeamNumber.Length; i++) {playersTeamNumber[i] = -1;}
		} else if (gameMode == GAMEMODE_TEAM) {
			for (int i = 0; i < playersTeamNumber.Length; i++) {playersTeamNumber[i] = 1;}
		}
		playerCount = 1;
	
		PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen (
			MinOpponents, MaxOpponents, gameMode, MultiplayerListener.Instance
		);
	}

	public void JoinRoom(int roomCapacity, uint mode, int roomPlayerNum) {
		ConsoleLog.SLog ("JoinRoom");

		InitializeRoomCapacity (roomCapacity);

		gameMode = mode;
		localRoomPlayerNumber = roomPlayerNum;
		playersAvatarUrl[localRoomPlayerNumber] = PlayGamesPlatform.Instance.GetUserImageUrl();
		playersName [localRoomPlayerNumber] = PlayGamesPlatform.Instance.GetUserDisplayName();
		if (gameMode == GAMEMODE_DEATHMATCH) {
			for (int i = 0; i < playersTeamNumber.Length; i++) {playersTeamNumber[i] = -1;}
		} else if (gameMode == GAMEMODE_TEAM) {
			for (int i = 0; i < playersTeamNumber.Length; i++) {playersTeamNumber[i] = 1;}
		}
	}

	public void InitializeRoomCapacity(int roomCapacity){
		ConsoleLog.SLog("InitializeRoomCapacity("+roomCapacity+")");

		MaxOpponents = (uint) roomCapacity - 1;
		ConsoleLog.SLog("MaxOpponents = " + MaxOpponents);

		InitArrayVariable ();
	}

	private void InitArrayVariable (){

		ConsoleLog.SLog("MaxOpponents = " + MaxOpponents);
		remoteCharacterGameObjects = new GameObject[MaxOpponents + 1];
		latestPlayerDatas = new PlayerGameData[MaxOpponents + 1];
		hasNewPlayerDatas = new bool[MaxOpponents + 1];
		updatedLastFrame = new bool[MaxOpponents + 1];
		clientId = new string[MaxOpponents + 1];
		playersReady = new bool[MaxOpponents + 1];
		playersTeamNumber = new int[MaxOpponents + 1];
		playersAvatar = new Texture2D[MaxOpponents + 1];
		playersAvatarUrl = new string[MaxOpponents + 1];
		playersName = new string[MaxOpponents + 1];
		tempGamePlayerNum = new int[MaxOpponents + 1];
		isFinishLoadScene = new bool[MaxOpponents + 1];

		for (int i = 0; i < MaxOpponents + 1; i++){
			tempGamePlayerNum[i] = i;
		}
	}

	public void ShowInvite(){
		ConsoleLog.SLog("ShowInvite");
		PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(MultiplayerListener.Instance);
	}

	public void SetRoomUiByGameMode (uint mode) {
		switch (mode) {
		case GAMEMODE_DEATHMATCH:
			RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_VR_READY);
			break;
		case GAMEMODE_TEAM:
			RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_SELECT_TEAM);
			RoomSetupUiController.instance.RefreshTeam ();
			break;
		default:
			ConsoleLog.SLog ("Error SetRoomUiByGameMode (" + gameMode + "): unmatched gameMode");
			break;
		}
	}

	public void SelectMap (int map) {
		this.map = map;
	}

	public void SendReady (){
		if (localRoomPlayerNumber == -1) return;

		ConsoleLog.SLog("SendReady ()");

		playersReady [localRoomPlayerNumber] = true;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true, 
			PayloadWrapper.Build (
				READY,
				localRoomPlayerNumber
			)
		);
		if (localRoomPlayerNumber == 0 && IsAllReady()) {
			SendInitGame ();
		}
	}

	public void SendSwtichTeamReq (int team){
		ConsoleLog.SLog ("SendSwitchTeamReq (" + team + ")");
		ConsoleLog.SLog ("localRoomPlayerNumber =" + localRoomPlayerNumber);


		TeamData teamData = new TeamData (localRoomPlayerNumber, team);

		//if host, just change it yourself and tell everyone
		//if not host, send request to change from host
		if (localRoomPlayerNumber == 0) {
			SendSwitchTeamRes (teamData);
		} else {
			PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
				true,
				PayloadWrapper.Build (
					REQ_SWITCH_TEAM,
					teamData
				)
			);
		}
	}

	public void SendSwitchTeamRes (TeamData teamData){
		if (localRoomPlayerNumber == 0) {
			if (!isFinishSelectTeam) {
				SetTeam (teamData.roomPlayerNum, teamData.teamNum);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll ( true, PayloadWrapper.Build (
					RES_SWITCH_TEAM,
					teamData
				));

				if (IsTeamBalance ()) {
					RoomSetupUiController.instance.Show (RoomSetupUiController.instance.startBtn);
				} else {
					RoomSetupUiController.instance.Hide (RoomSetupUiController.instance.startBtn);
				}
			}
		}
	}

	//Only call this fn when receive respond from host
	public void SetTeam (int roomPlayerNum, int team){
		if (gameMode == GAMEMODE_TEAM) {
			//set team
			playersTeamNumber [roomPlayerNum] = team;

			//generate teamPlayerGameNumber for UI to draw select team panel
			//number 0 1 are team1
			//number 2 3 are leftover team1, causing unbalance team
			//number 4 5 are team2
			//number 6 7 are leftover team2, causing unbalance team

			int team1Count = 0;
			int team2Count = 4;

			for (int i = 0; i < playersTeamNumber.Length; i++) {
				if (playersTeamNumber [i] == 1) {
					tempGamePlayerNum [i] = team1Count;
					team1Count++;
				} else if (playersTeamNumber [i] == 2) {
					tempGamePlayerNum [i] = team2Count;
					team2Count++;
				}
			}
			RoomSetupUiController.instance.RefreshTeam ();
		}
	}

	public void SendFinishSelectTeam (){
		if (localRoomPlayerNumber != 0)
			return;

		ConsoleLog.SLog ("SendFinishSelectTeam ()");

		if (!IsTeamBalance ()) {
			ConsoleLog.SLog ("Team not balance");
			return;
		}

		isFinishSelectTeam = true;

		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true,
			PayloadWrapper.Build(
				FINISH_SELECT_TEAM,
				0
			)
		);

		RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_VR_READY);
	}

	public bool IsTeamBalance (){
		int team1Count = 0; int team2Count = 0;

		foreach (int teamNum in playersTeamNumber) {
			if (teamNum == 1) team1Count++;
			else if (teamNum == 2) team2Count++;
		}
		if (team1Count == team2Count) return true;

		return false;
	}

	public bool IsAllReady (){
		if (localRoomPlayerNumber == -1) { return false; }

		for (int i = 0; i < MaxOpponents + 1; i++) {
			if (!playersReady [i]) {
				return false;
			}
		}
		return true;
	}

	public void SendInitGame (){
		//Assign player number to each room member.
		//random assign for deathmatch
		//player 1 2 for team1
		//player 3 4 for team2

		ConsoleLog.SLog("==================== SendInitGame() from PlayGamesPlatform");
		System.Collections.Generic.List<GooglePlayGames.BasicApi.Multiplayer.Participant> members = PlayGamesPlatform.Instance.RealTime.GetConnectedParticipants ();
		foreach (GooglePlayGames.BasicApi.Multiplayer.Participant member in members) {
			ConsoleLog.SLog ("Name: " + member.DisplayName + " ID: " + member.ParticipantId);
		}

		try {

			clientId [0] = PlayGamesPlatform.Instance.RealTime.GetSelf().ParticipantId;

			if (gameMode == GAMEMODE_DEATHMATCH) {
				ConsoleLog.SLog ("gameMode == GAMEMODE_DEATHMATCH");

				ConsoleLog.SLog("==================== SendInitGame() before shuffle");
				for (int i=0; i<clientId.Length; i++){
					ConsoleLog.SLog(i+" Name: "+playersName[i]+" ID: "+clientId[i]);
				}

				int[] clientTeamNum = new int[MaxOpponents + 1];
				for (int i=0; i<clientTeamNum.Length; i++) {clientTeamNum[i] = -1;}

				string[] shuffleClientId = clientId.Clone() as string[];
				int[] gamePlayersIdbyRoomPlayersId = new int[shuffleClientId.Length];

				Shuffle (shuffleClientId);

				ConsoleLog.SLog("==================== SendInitGame() shuffleClientId after shuffle");
				for (int i=0; i<shuffleClientId.Length; i++){
					ConsoleLog.SLog(i+" ID: "+shuffleClientId[i]);
				}

				ConsoleLog.SLog("==================== SendInitGame() clientId after shuffle");
				for (int i=0; i<clientId.Length; i++){
					ConsoleLog.SLog(i+" ID: "+clientId[i]);
				}

				ConsoleLog.SLog("==================== SendInitGame() map from room to game");
				for(int i=0; i<clientId.Length; i++){
					for(int j=0; j<shuffleClientId.Length; j++){
						if(clientId[i] == shuffleClientId[j]) {
							gamePlayersIdbyRoomPlayersId[i] = j;
							ConsoleLog.SLog("roomNum "+i+" = gameNum "+j);
						}
					}
				}

				for (int i = 0; i < clientId.Length; i++) {
					if (shuffleClientId [i] == PlayGamesPlatform.Instance.RealTime.GetSelf ().ParticipantId) {
						localGamePlayerNumber = i;
					} else {
						InitGameData data = new InitGameData (clientTeamNum, gamePlayersIdbyRoomPlayersId, i, map, Vector3.zero);
						PlayGamesPlatform.Instance.RealTime.SendMessage ( true, shuffleClientId[i], PayloadWrapper.Build (
							RES_INIT_GAME,
							data
						));
					}
				}

				InitGameData data2 = new InitGameData (clientTeamNum, gamePlayersIdbyRoomPlayersId, localGamePlayerNumber, map, Vector3.zero);
				InitGame(data2);

			} else if (gameMode == GAMEMODE_TEAM) {
				ConsoleLog.SLog ("gameMode == GAMEMODE_TEAM");

				// int[second][first]
				// first level: index 0 for teamNum. index 1 for gamePlayerNum
				// second level index for select player order by roomPlayerNum;
				int[,] clientTeamNumAndGamePlayerNum = new int[MaxOpponents + 1,2];

				for (int i = 0; i < clientId.Length; i++) {

					//for team test
					//TODO remove this
					if (MaxOpponents == 1){
						switch (tempGamePlayerNum[i]){
						case 0: clientTeamNumAndGamePlayerNum[i,1] = 0; clientTeamNumAndGamePlayerNum[i,0] = 1; break;
						case 4: clientTeamNumAndGamePlayerNum[i,1] = 1; clientTeamNumAndGamePlayerNum[i,0] = 2; break;
						default:
							ConsoleLog.SLog("Error: unmatch tempGamePlayerNum. Is team balance ?");
							return;
						}
					} else {
						switch (tempGamePlayerNum[i]){
						case 0: clientTeamNumAndGamePlayerNum[i,1] = 0; clientTeamNumAndGamePlayerNum[i,0] = 1; break;
						case 1: clientTeamNumAndGamePlayerNum[i,1] = 1; clientTeamNumAndGamePlayerNum[i,0] = 1; break;
						case 4: clientTeamNumAndGamePlayerNum[i,1] = 2; clientTeamNumAndGamePlayerNum[i,0] = 2; break;
						case 5: clientTeamNumAndGamePlayerNum[i,1] = 3; clientTeamNumAndGamePlayerNum[i,0] = 2; break;
						default:
							ConsoleLog.SLog("Error: unmatch tempGamePlayerNum. Is team balance ?");
							return;
						}
					}
					
					ConsoleLog.SLog (
						"i="+i+
						" tempGamePlayerNum="+tempGamePlayerNum[i]+
						" team="+clientTeamNumAndGamePlayerNum[i,0]+
						" gamePlayerNum="+clientTeamNumAndGamePlayerNum[i,1]
					);
				}

				InitGameData initGameDataForHost = null;
				for (int i = 0; i < clientId.Length; i++){
					
					//check taem number
					if(playersTeamNumber[i] != clientTeamNumAndGamePlayerNum[i,0]) ConsoleLog.SLog("Error: team not match.");

					if (clientId [i] == PlayGamesPlatform.Instance.RealTime.GetSelf ().ParticipantId) {
						initGameDataForHost = new InitGameData (
							GetArrayAtIndex(clientTeamNumAndGamePlayerNum, 0),
							GetArrayAtIndex(clientTeamNumAndGamePlayerNum, 1),
							clientTeamNumAndGamePlayerNum[i,1],
							map,
							Vector3.zero);
					} else {
						InitGameData data = new InitGameData (
							GetArrayAtIndex(clientTeamNumAndGamePlayerNum, 0),
							GetArrayAtIndex(clientTeamNumAndGamePlayerNum, 1),
							clientTeamNumAndGamePlayerNum[i,1],
							map,
							Vector3.zero);
						PlayGamesPlatform.Instance.RealTime.SendMessage ( true, clientId[i], PayloadWrapper.Build (
							RES_INIT_GAME,
							data
						));
					}
				}

				InitGame(initGameDataForHost);

			}
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in SendInitGame ()\n" + e.Message);
		}
	}

	private void OrderByGamePlayerNum(int[] gamePlayerNumByRoomPlayerNum){
		Texture2D[] tempAvaters = new Texture2D[playersAvatar.Length];
		string[] tempNames = new string[playersName.Length];
		string[] tempClientId = new string[clientId.Length];

		ConsoleLog.SLog("==================== SendInitGame() swaping");
		for (int i = 0; i < gamePlayerNumByRoomPlayerNum.Length; i++) {
			tempAvaters [gamePlayerNumByRoomPlayerNum [i]] = playersAvatar [i];
			tempNames [gamePlayerNumByRoomPlayerNum [i]] = playersName [i];
			tempClientId [gamePlayerNumByRoomPlayerNum [i]] = clientId [i];
			ConsoleLog.SLog(
				"gamePlayerNum: "+gamePlayerNumByRoomPlayerNum [i]+
				" Name: "+tempNames[gamePlayerNumByRoomPlayerNum [i]]+
				" ID: "+clientId[gamePlayerNumByRoomPlayerNum [i]]
			);
		}

		playersAvatar = tempAvaters;
		playersName = tempNames;
		clientId = tempClientId;
	}

	public void LeaveRoom (){
		//tell everyboay I'm leaving
		if (localGamePlayerNumber == -1) {
			PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
				REQ_LEAVE_ROOM, localRoomPlayerNumber
			));
		} else {
			PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
				REQ_LEAVE_ROOM, localGamePlayerNumber
			));
		}
		PlayGamesPlatform.Instance.RealTime.LeaveRoom ();

		//clear leftover
		for (int i = 0; i < remoteCharacterGameObjects.Length; i++) {
			if (remoteCharacterGameObjects [i] != null) {
				Destroy (remoteCharacterGameObjects [i]);
			}
		}

		localGamePlayerNumber = -1;
		localRoomPlayerNumber = -1;
		playerCount = 0;
		round = 0;
		team1Score = 0;
		team2Score = 0;
		map = 0;
		isFinishSelectTeam = false;
		isGameStart = false;
		haveSentEndRound = false;
		hadMovedToSpawnPoint = false;
		firstTimeFoundAllComponent = true;

		InitArrayVariable ();

		SceneManager.LoadScene (0);
		RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_INDEX);
	}

	public void RemovePlayerFromGame(int otherPlayerNumber){
		try {
			//He's gone. Forget him.
			Destroy (remoteCharacterGameObjects [otherPlayerNumber]);
			remoteCharacterGameObjects [otherPlayerNumber] = null;
			latestPlayerDatas [otherPlayerNumber] = null;
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in RemovePlayerFromGame(" + otherPlayerNumber + ")\n" + e.Message);
		}

	}
		
	#endregion

	// ============== Round Control Function ============== //
	#region Round Control Function

	public void InitGame (InitGameData data){
		ConsoleLog.SLog ("InitGame (data.gamePlayerNum=" + data.gamePlayerNum + ")");

		this.map = data.map;

		ConsoleLog.SLog("==================== InitGame() before swap");
		for (int i=0; i<clientId.Length; i++){
			ConsoleLog.SLog(i+" Name: "+playersName[i]+" ID: "+clientId[i]);
		}

		//from now on, access everything by GamePlayerNum
		OrderByGamePlayerNum (data.gamePlayersNumByRoomPlayersNum);

		ConsoleLog.SLog("==================== SendInitGame() after swap");
		for (int i=0; i<clientId.Length; i++){
			ConsoleLog.SLog(i+" Name: "+playersName[i]+" ID: "+clientId[i]);
		}

		for (int i = 0; i < playersTeamNumber.Length; i++) {
			if (playersTeamNumber [i] != data.teamNums [i]) {
				ConsoleLog.SLog ("team num mismatch. " + playersTeamNumber [i] + "!=" + data.teamNums [i]);
			}
		}
		playersTeamNumber = data.teamNums;

		for (int i = 0; i < playersName.Length; i++) {
			ConsoleLog.SLog ("gamePlayerNum: "+i+" name: " + playersName[i] + " ID: " + clientId[i]);
		}

		localGamePlayerNumber = data.gamePlayerNum;

		InitRound (new RoundData (
			1, 0, 0
		));
	}

	public void InitRound(RoundData data){
		ConsoleLog.SLog ("InitRound () round " + data.round);

		isGameStart = false;
		firstTimeFoundAllComponent = true;
		hadMovedToSpawnPoint = false;

		round = data.round;
		team1Score = data.team1Score; //in deathmatch, team both team score will be 0
		team2Score = data.team2Score; //in deathmatch, team both team score will be 0

		//when finish load scene, in Start() will call SendFinishLoadScene() to notify host.
		SceneManager.LoadScene (map);

//		StartCoroutine (KeepFindingComponentsUntilFound ());
	}

	public void SendFinishLoadScene(){
		ConsoleLog.SLog ("I (player "+localGamePlayerNumber+") finish load scene");

		if (localGamePlayerNumber == -1) {return;} 

		ConsoleLog.SLog ("SendFinishLoadScene() 1");

		if (localGamePlayerNumber == 0) {
			ConsoleLog.SLog ("SendFinishLoadScene() 2");
			isFinishLoadScene [0] = true;
			if (IsAllFinishLoadScene()) SendStartRound();
			ConsoleLog.SLog ("SendFinishLoadScene() 3");
		} else {
			ConsoleLog.SLog ("SendFinishLoadScene() 4");
			PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build(
				FINISH_LOAD_SCENE,
				localGamePlayerNumber
			));
			ConsoleLog.SLog ("SendFinishLoadScene() 5");
		}
	}

	public bool IsAllFinishLoadScene(){
		if (localGamePlayerNumber != 0) { return false;	}
		foreach (bool finish in isFinishLoadScene) { if (!finish) return false;}

		return true;
	}

	private void SendStartRound(){
		if (localGamePlayerNumber != 0) { return; }

		ConsoleLog.SLog ("SendStartRound()");

		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
			START_ROUND,
			round
		));
		StartRound (round);
	}

	public void StartRound (int round){
		ConsoleLog.SLog ("StartRound(" + round + ")");

		if (this.round != round) {
			ConsoleLog.SLog ("round number mismatch (this.round=" + this.round + " round=" + round + ")");
			return;
		}

		try {
			MoveToSpawnPoint (localGamePlayerNumber);
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in MoveToSpawnPoint(" + localGamePlayerNumber + ")\n" + e.Message);
		}

		isGameStart = true;
		haveSentEndRound = false;
	}

	public IEnumerator SendInitNextRoundDelay (float delay){
		for (int i = 0; i < isFinishLoadScene.Length; i++) {
			isFinishLoadScene [i] = false;
		}
		round++;
		RoundData data = new RoundData (round, team1Score, team2Score); 
		ConsoleLog.SLog ("SendInitNextRoundDelay() round " + round + " Wait " + delay + " sec ...");

		yield return new WaitForSeconds (delay);

		ConsoleLog.SLog ("SendInitNextRoundDelay() round " + round + " Sending ...");
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
			INIT_NEXT_ROUND,
			data
		));
		InitRound (data);
	}
//
//	public void RestartRound(RoundData data){
//		SceneManager.LoadScene (map);
//	}

	//only host call this function at OnRealTimeMessageRecieved() tag PLAYER_DATA
	public void CheckEndRound(){
		if (haveSentEndRound) return;

		if (gameMode == GAMEMODE_TEAM) {
			
			int team1AliveCount = 0;
			int team2AliveCount = 0;
			for (int i=0; i<latestPlayerDatas.Length; i++){
				if (IsPlayerAlive (i)) {
					if(playersTeamNumber[i] == 1) team1AliveCount++;
					if(playersTeamNumber[i] == 2) team2AliveCount++;
				}
			}

			if (team1AliveCount > 0 && team2AliveCount == 0) {
				ConsoleLog.SLog ("Detect TEAM1 win");
				team1Score++;
				EndRoundData data = new EndRoundData (round, team1Score, team2Score, 1);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
					END_ROUND,
					data
				));
				haveSentEndRound = true;
				EndRound (data);
			} else if (team2AliveCount > 0 && team1AliveCount == 0) {
				ConsoleLog.SLog ("Detect TEAM2 win");
				team2Score++;
				EndRoundData data = new EndRoundData (round, team1Score, team2Score, 2);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
					END_ROUND,
					data
				));
				haveSentEndRound = true;
				EndRound (data);
			}

		} else if (gameMode == GAMEMODE_DEATHMATCH) {
			
			int aliveCount = 0;
			int aliveGamePlayerNum = -1;
			for (int i=0; i<latestPlayerDatas.Length; i++){
//				ConsoleLog.SLog (
//					"player " + i + 
//					" Name: " + playersName [i] + 
//					" HP: " + latestPlayerDatas [i].health + 
//					" IsAlive: "+IsPlayerAlive(i)
//				);
				if (IsPlayerAlive (i)) {
					aliveCount++;
					aliveGamePlayerNum = i;
				}
			}
//			ConsoleLog.SLog ("------------------------");
			if (aliveCount == 1) {
				ConsoleLog.SLog ("Detect player "+aliveGamePlayerNum+" win");
				EndRoundData data = new EndRoundData (aliveGamePlayerNum);
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
					END_ROUND,
					data
				));
				haveSentEndRound = true;
				EndRound (data);
			}
		}
	}

	public void EndRound (EndRoundData data){
		isGameStart = false;

		team1Score = data.team1Score;
		team2Score = data.team2Score;

		if (gameMode == GAMEMODE_DEATHMATCH) {
			// game end, show winner, load room setup scene
			localGameManager.HideDeadText();
			localGameManager.ShowGameEndText(playersName[data.winner]);
			StartCoroutine (LeaveRoomWithDelay (5f));

		} else if (gameMode == GAMEMODE_TEAM) {
			if (round < 3) {
				// round end, show round winner, wait for delay, start new round
				localGameManager.ShowRoundEndText(
					data.round,
					data.winner,
					data.team1Score,
					data.team2Score
				);

				if (localGamePlayerNumber == 0) {
					StartCoroutine(SendInitNextRoundDelay (5f));
				}
			} else if (round == 3) {
				// game end, show winner, load room setup scene
				if (data.team1Score > data.team2Score) {
					localGameManager.ShowGameEndText("TEAM1");
				} else {
					localGameManager.ShowGameEndText("TEAM2");
				}
				StartCoroutine (LeaveRoomWithDelay (5f));
			}
		}
	}

	private IEnumerator LeaveRoomWithDelay(float delay){
		yield return new WaitForSeconds (delay);
		LeaveRoom ();
//		PlayGamesPlatform.Instance.RealTime.LeaveRoom ();
	}

	#endregion

	// ============== Game Play Communication Function ============== //
	#region Game Play Communication Function

	public void SendDamage (int remotePlayerNum, float damage){
		GetClientId (remotePlayerNum); //TODO for debug, remove this.
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
			INFLICT_DAMAGE,
			new DamageData ( remotePlayerNum, damage)
		));
	}

	public void SendFireRay (Ray fireRay) {
		if (!isGameStart) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			false,
			PayloadWrapper.Build (
				FIRE_RAY,
				new FireRayData (fireRay, localGamePlayerNumber)
			)
		);
	}

	public void SendHandGrenade (Vector3 position, Vector3 rotation, Vector3 force) {
		if (!isGameStart) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true,
			PayloadWrapper.Build (
				HAND_GRENADE,
				new GrenadeData (localGamePlayerNumber, position, rotation, force)
			)
		);
	}

	public void SendDestroyItem(int itemId, System.Object something = null){
//		ConsoleLog.SLog ("SendDestroyItem("+itemId+")");
		if (!isGameStart) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true,
			PayloadWrapper.Build (
				DESTROY_ITEM,
				new DestroyItemData(itemId, something)
			)
		);
	}

	public bool IsPlayerAlive (int gamePlayerNum) {
		float hp = 0;
		if (gamePlayerNum == localGamePlayerNumber)
			return !localGameManager.isDead;
		else
			hp = latestPlayerDatas [gamePlayerNum].health;

		if (hp <= 0f)
			return false;
		else
			return true;
	}

	#endregion

	// ============== Broadcast Function ============== //
	#region Broadcast Function

	private void BroadcastPlayerData(){
		if (!isBroadcast) return;

		if (broadcastTimer < timeBetweenBroadcast) {
			broadcastTimer += Time.deltaTime;
			return;
		} else {
			broadcastTimer = 0f;
		}

		PlayerGameData data = new PlayerGameData (
			localGamePlayerNumber,
			localGameManager.health,
			localUnityChanControlScript.characterType,
			localPlayer.transform.position,
			new Vector3(
				cardboardHead.transform.localRotation.eulerAngles.x,
				cardboardHead.transform.localRotation.eulerAngles.y,
				cardboardHead.transform.localRotation.eulerAngles.z),
			localAnimationState,
			localGameManager.isInAimMode,
			Time.realtimeSinceStartup
		);

		bool reliable = false;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliable, PayloadWrapper.Build(PLAYER_DATA, data));
	}

	public void SetBroadcast (bool b){
		isBroadcast = b;
	}

	public void SetLocalAnimationState(int state){
		localAnimationState = state;
	}

	#endregion

	// ============== Remote Character Function ============== //
	#region Remote Character Function

	public GameObject GetCharacter(int otherPlayerNumber){
		CheckInitRemoteCharacter (otherPlayerNumber);
		return remoteCharacterGameObjects [otherPlayerNumber];
	}

	public void CheckInitRemoteCharacter(int playerNum){
		if (remoteCharacterGameObjects [playerNum] == null) {
			ConsoleLog.SLog ("--------- instantiate character player [" + playerNum + "] ---------");

			remoteCharacterGameObjects[playerNum] = Instantiate (
				GetCharPrefab(latestPlayerDatas[playerNum].characterType),
				latestPlayerDatas[playerNum].position,
				latestPlayerDatas[playerNum].rotation
			) as GameObject;

			RemoteCharacterController remoteController = remoteCharacterGameObjects [playerNum].GetComponent<RemoteCharacterController> ();
			remoteController.playerNum = playerNum;
		}
	}

	public GameObject GetCharPrefab (int charType) {
		switch (charType) {
		case CHAR_TYPE_PISTOL: return UnityChanWithPisolPrefab; 
		case CHAR_TYPE_RIFLE: return UnityChanWithRiflePrefab;
		case CHAR_TYPE_SHORTGUN: return UnityChanWithShotgunPrefab;
		case CHAR_TYPE_SNIPER: return UnityChanWithSniperPrefab;
		default: return otherPlayerDummyPrefab;
		}
	}

	public void ChangeRemoteCharacterType(int otherPlayerNumber, int SelectCharacterType){
		Destroy (remoteCharacterGameObjects [otherPlayerNumber]);
		CheckInitRemoteCharacter (otherPlayerNumber);
	}

	#endregion

	// ============== Debug Function ============== //
	#region Debug Function

	private void ResetNewPlayerDataFlag(){
		for (int i = 0; i < hasNewPlayerDatas.Length; i++) {
			updatedLastFrame[i] = false;
		}
	}

	private void PrintPlayerData(){
		ConsoleLog.SLog("PrintPlayerData() 1");

		if (latestPlayerDataText == null) { latestPlayerDataText = GameObject.FindGameObjectWithTag ("DebugText").GetComponent<Text> (); }
		if (latestPlayerDataText == null) { latestPlayerDataText = GameObject.FindGameObjectWithTag ("DebugText").GetComponent<Text> (); }
		if (latestPlayerDataText == null) { latestPlayerDataText = GameObject.FindGameObjectWithTag ("DebugText").GetComponent<Text> (); }
		if (latestPlayerDataText == null) { return; }

		try {
			ConsoleLog.SLog("2");
			latestPlayerDataText.text = "localPlayerNumber: " + localGamePlayerNumber + "  MaxPlayer: " + (MaxOpponents + 1) + "\n";
			ConsoleLog.SLog("3");

			latestPlayerDataText.text += "updatedLastFrame: ";
			if (updatedLastFrame != null) 
				for (int i = 0; i < updatedLastFrame.Length; i++){ latestPlayerDataText.text += (updatedLastFrame[i] ? "1 " : "0 ");}
			latestPlayerDataText.text += "\n";
			ConsoleLog.SLog("4");

			latestPlayerDataText.text += "characterGameObjects: ";
			if (remoteCharacterGameObjects != null) 
				for (int i = 0; i < remoteCharacterGameObjects.Length; i++){ latestPlayerDataText.text += (remoteCharacterGameObjects[i] == null ? "X " : "/ ");}
			latestPlayerDataText.text += "\n";
			ConsoleLog.SLog("5");

			latestPlayerDataText.text += "\n + Payload Data +\n";
			if (latestPlayerDatas != null) {
				for (int i = 0; i < latestPlayerDatas.Length; i++) {
					if (latestPlayerDatas [i] == null || i == localGamePlayerNumber) continue;

					latestPlayerDataText.text += "[" + i + "] ";
					latestPlayerDataText.text += "anim: " + latestPlayerDatas [i].animState + " ";
					latestPlayerDataText.text += "P: " + 
						roundDown(latestPlayerDatas[i].position.x, 1) + ", " + 
						roundDown(latestPlayerDatas[i].position.y, 1) + ", " + 
						roundDown(latestPlayerDatas[i].position.z, 1) + " ";
					latestPlayerDataText.text += "R: " +
						roundDown(latestPlayerDatas [i].rotation.eulerAngles.x, 1) + ", " +
						roundDown(latestPlayerDatas [i].rotation.eulerAngles.y, 1) + ", " +
						roundDown(latestPlayerDatas [i].rotation.eulerAngles.z, 1) + "\n";
				}
			}
			ConsoleLog.SLog("6");

			latestPlayerDataText.text += "\n + Local Data +\n";
			if (remoteCharacterGameObjects != null) {
				for (int i = 0; i < remoteCharacterGameObjects.Length; i++) {
					if (latestPlayerDatas [i] == null || i == localGamePlayerNumber) continue;

					latestPlayerDataText.text += "[" + i + "] ";
					latestPlayerDataText.text += "Pos: " + 
						roundDown(remoteCharacterGameObjects[i].transform.position.x, 1) + ", " + 
						roundDown(remoteCharacterGameObjects[i].transform.position.y, 1) + ", " + 
						roundDown(remoteCharacterGameObjects[i].transform.position.z, 1) + " ";
					latestPlayerDataText.text += "Rot: " +
						roundDown(remoteCharacterGameObjects[i].transform.rotation.eulerAngles.x, 1) + ", " +
						roundDown(remoteCharacterGameObjects[i].transform.rotation.eulerAngles.y, 1) + ", " +
						roundDown(remoteCharacterGameObjects[i].transform.rotation.eulerAngles.z, 1) + "\n";
				}
			}
			ConsoleLog.SLog("7");

			if (localPlayer == null) ConsoleLog.SLog("localPlayer null");

			latestPlayerDataText.text += "\n + Local Character\n";
			latestPlayerDataText.text += "pos: " + 
				roundDown(localPlayer.transform.position.x, 1) + ", " + 
				roundDown(localPlayer.transform.position.y, 1) + ", " + 
				roundDown(localPlayer.transform.position.z, 1) + "\n";
			latestPlayerDataText.text += "rot: " + 
				roundDown(localPlayer.transform.rotation.eulerAngles.x, 1) + ", " + 
				roundDown(localPlayer.transform.rotation.eulerAngles.y, 1) + ", " + 
				roundDown(localPlayer.transform.rotation.eulerAngles.z, 1) + "\n";
			ConsoleLog.SLog("8");

		} catch (Exception e){
			ConsoleLog.SLog ("error in PrintLatestPlayerData");
			ConsoleLog.SLog (e.Message);
		}
	}

	private float roundDown(float number, int precision){
		return (float) (((int)(number * Mathf.Pow (10, precision))) / Mathf.Pow (10, precision));
	}

	private T[] GetArrayAtIndex<T> (T[,] array, int outterArrayIndex) {
		T[] result = new T[array.GetLength(0)];
		for (int i = 0; i < result.Length; i++) {
			result [i] = array [i, outterArrayIndex];
		}
		return result;
	}

	public void Shuffle<T> (T[] array){
		System.Random rng = new System.Random ();

		int n = array.Length;
		while (n > 1) {
			int k = rng.Next(n--);
			T temp = array[n];
			array[n] = array[k];
			array[k] = temp;
		}
	}

	#endregion

	public class MultiplayerListener : GooglePlayGames.BasicApi.Multiplayer.RealTimeMultiplayerListener {

		private static MultiplayerListener sInstance = new MultiplayerListener();

		public static MultiplayerListener Instance {
			get { return sInstance; }
		}

		public void OnRoomSetupProgress(float percent){
			ConsoleLog.SLog("OnRoomSetupProgress: " + percent);

			PlayGamesPlatform.Instance.RealTime.ShowWaitingRoomUI();
		}

		public void OnRoomConnected(bool success){
			ConsoleLog.SLog("OnRoomConnected: " + success);

			if (!success) return;

			// if doesn't have room player number yet, introduce yourself to host and request room player number
			if (MultiplayerController.instance.localRoomPlayerNumber == -1) {
				ConsoleLog.SLog("Send REQ_INIT_ROOM");
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
					MultiplayerController.REQ_INIT_ROOM,
					new ReqInitRoomData (
						PlayGamesPlatform.Instance.GetUserDisplayName(),
						PlayGamesPlatform.Instance.GetUserImageUrl()
					)
				));
			}
		}

		public void OnLeftRoom(){
			ConsoleLog.SLog("OnLeftRoom");
		
		}

		public void OnParticipantLeft( GooglePlayGames.BasicApi.Multiplayer.Participant leftParticipant){
			ConsoleLog.SLog("OnParticipantLeft: " + leftParticipant.DisplayName);
			
		}

		public void OnPeersConnected(string[] participantIds){
			ConsoleLog.SLog("OnRoomConnected\nID:");
		}

		public void OnPeersDisconnected(string[] participantIds){
			ConsoleLog.SLog("OnPeersDisconnected: " + participantIds);

			if (!MultiplayerController.instance.isGameStart) return;

			foreach (string id in participantIds) {
				MultiplayerController.instance.RemovePlayerFromGame (MultiplayerController.instance.GetPlayerNumber (id));
			}
		}

		public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data) {
			//deserialize data, get position and head's rotation of that sender, and set to it's character.

//			ConsoleLog.SLog("MessageReceived ID: " + senderId);

			BinaryFormatter bf = new BinaryFormatter ();
			PayloadWrapper payloadWrapper = (PayloadWrapper) bf.Deserialize (new MemoryStream (data));
			if (payloadWrapper.tag != PLAYER_DATA && payloadWrapper.tag != DESTROY_ITEM ) ConsoleLog.SLog("tag: " + payloadWrapper.tag);

			switch (payloadWrapper.tag) {
			case REQ_INIT_ROOM:
				//if this is host
				if (MultiplayerController.instance.localRoomPlayerNumber == 0) {

					ReqInitRoomData reqInitRoomData = (ReqInitRoomData)payloadWrapper.payload;
					
					//send client player number, room capacity for init room, and gameMode
					InitRoomData initRoomData = new InitRoomData (
						MultiplayerController.instance.MaxOpponents + 1,
						MultiplayerController.instance.playerCount,
						(int) MultiplayerController.instance.gameMode
					);
					PlayGamesPlatform.Instance.RealTime.SendMessage (true, senderId, PayloadWrapper.Build (
						RES_INIT_ROOM,
						initRoomData
					));

					//save player avatar
					MultiplayerController.instance.playersName [MultiplayerController.instance.playerCount] = reqInitRoomData.playerName;
					MultiplayerController.instance.playersAvatarUrl [MultiplayerController.instance.playerCount] = reqInitRoomData.avatarURL;
					MultiplayerController.instance.clientId [MultiplayerController.instance.playerCount] = senderId;

					//move playerCount up, so the next request get a proper playerNum
					MultiplayerController.instance.playerCount++;

					//if all 4 member connected, boardcast everyone name, playerNum, avatar picture url
					if (MultiplayerController.instance.playerCount == (MultiplayerController.instance.MaxOpponents + 1)) {
						MultiplayerController.instance.StartCoroutine (MultiplayerController.instance.LoadAvatarImageAllThenChangeUi ());
						PlayGamesPlatform.Instance.RealTime.SendMessageToAll ( true, PayloadWrapper.Build (
							ROOM_INTRODUCTION,
							new RoomMemberIntroductionData (
								MultiplayerController.instance.playersName,
								MultiplayerController.instance.playersAvatarUrl,
								MultiplayerController.instance.clientId
							)
						));
					}
				}

				break;

			case RES_INIT_ROOM:
				InitRoomData resInitRoomData = (InitRoomData) payloadWrapper.payload;

				MultiplayerController.instance.JoinRoom (
					(int) resInitRoomData.roomCapacity,
					(uint) resInitRoomData.gameMode,
					resInitRoomData.roomPlayerNum
				);
				break;

			case ROOM_INTRODUCTION:
				RoomMemberIntroductionData roomIntroData = (RoomMemberIntroductionData)payloadWrapper.payload;
				MultiplayerController.instance.clientId = roomIntroData.memberId;
				MultiplayerController.instance.playersName = roomIntroData.memberNames;
				MultiplayerController.instance.playersAvatarUrl = roomIntroData.memberAvatars;
				MultiplayerController.instance.StartCoroutine (MultiplayerController.instance.LoadAvatarImageAllThenChangeUi ());

				break;

			case REQ_SWITCH_TEAM:
				MultiplayerController.instance.SendSwitchTeamRes((TeamData) payloadWrapper.payload);
				break;

			case RES_SWITCH_TEAM:
				TeamData teamData2 = (TeamData) payloadWrapper.payload;
				MultiplayerController.instance.SetTeam (teamData2.roomPlayerNum, teamData2.teamNum);
				break;

			case FINISH_SELECT_TEAM:
				RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_VR_READY);
				break;

			case READY:
				MultiplayerController.instance.playersReady [(int)payloadWrapper.payload] = true;
				MultiplayerController.instance.clientId [(int)payloadWrapper.payload] = senderId;
				if (MultiplayerController.instance.clientId [(int)payloadWrapper.payload] != senderId) {
					ConsoleLog.SLog ("Error: Introduced ClientId and Ready senderId mismatch!");
				}

				//TODO set UI to show that player is ready

				//if host
				if (MultiplayerController.instance.localRoomPlayerNumber == 0) {
					//if everyone is ready, start the game
					if (MultiplayerController.instance.IsAllReady ()) {
						MultiplayerController.instance.SendInitGame ();
					}
				}

				break;

			case RES_INIT_GAME:
				MultiplayerController.instance.InitGame ((InitGameData) payloadWrapper.payload);
				break;

			case FINISH_LOAD_SCENE:
				ConsoleLog.SLog ("player " + (int)payloadWrapper.payload + " finish load scene");

				if (MultiplayerController.instance.localGamePlayerNumber == 0) {
					MultiplayerController.instance.isFinishLoadScene [(int)payloadWrapper.payload] = true;
					if (MultiplayerController.instance.IsAllFinishLoadScene ()) {
						MultiplayerController.instance.SendStartRound ();
					}
				}
				break;

			case START_ROUND:
				MultiplayerController.instance.StartRound ((int) payloadWrapper.payload);
				break;

			case END_ROUND:
				MultiplayerController.instance.EndRound ((EndRoundData)payloadWrapper.payload);
				break;

			case INIT_NEXT_ROUND:
				MultiplayerController.instance.InitRound ((RoundData) payloadWrapper.payload);
				break;

			case PLAYER_DATA:
				try {
					//if someone who connected to room early and broadcast player data before we initialize, ignore it.
					if(!MultiplayerController.instance.isGameStart) return;

					PlayerGameData otherPlayerData = (PlayerGameData) payloadWrapper.payload;

					//if the data we had is newer than the one received, ignore it
					if(MultiplayerController.instance.latestPlayerDatas [otherPlayerData.playerNumber] != null &&
						MultiplayerController.instance.latestPlayerDatas [otherPlayerData.playerNumber].time > otherPlayerData.time) {
						ConsoleLog.SLog("Receive player data out of order");
						return;
					}

					//Check if the player change character type since last time. If so, instantiate new remote character
					if(MultiplayerController.instance.latestPlayerDatas [otherPlayerData.playerNumber] != null &&
						MultiplayerController.instance.latestPlayerDatas [otherPlayerData.playerNumber].characterType != otherPlayerData.characterType) {
						MultiplayerController.instance.ChangeRemoteCharacterType(otherPlayerData.playerNumber, otherPlayerData.characterType);
					}

					//After all condition above is pass, save other player's data and trigger update flag
					MultiplayerController.instance.latestPlayerDatas [otherPlayerData.playerNumber] = otherPlayerData;
					MultiplayerController.instance.hasNewPlayerDatas [otherPlayerData.playerNumber] = true;

					for (int i=0; i<MultiplayerController.instance.latestPlayerDatas.Length; i++){
						try {
							ConsoleLog.SLog (
								"player " + i + 
								" Name: " + MultiplayerController.instance.playersName [i] + 
								" HP: " + MultiplayerController.instance.latestPlayerDatas [i].health + 
								" IsAlive: "+MultiplayerController.instance.IsPlayerAlive(i)
							);
						} catch (System.Exception e) {
							ConsoleLog.SLog("Error in print latestPlayerData\n"+e.Message);
						}
					}

					// just 1 player (in this case player 0) check game end criteria
					if(MultiplayerController.instance.localGamePlayerNumber == 0) {
						MultiplayerController.instance.CheckEndRound();
					}

//					//save senderId for easy access in the future
//					if(MultiplayerController.instance.clientId[otherPlayerData.playerNumber] == null) {
//						MultiplayerController.instance.clientId[otherPlayerData.playerNumber] = senderId;
//					}

					//check remote character instant, initiate if needed
					MultiplayerController.instance.CheckInitRemoteCharacter (otherPlayerData.playerNumber);

				} catch (Exception e) {
					ConsoleLog.SLog ("something wrong during recieving player data");
					ConsoleLog.SLog (e.Message);
				}

				break;

			case REQ_LEAVE_ROOM:
				MultiplayerController.instance.RemovePlayerFromGame ((int)payloadWrapper.payload);
				break;

			case INFLICT_DAMAGE:
				DamageData damageData = (DamageData)payloadWrapper.payload;

				if (MultiplayerController.instance.localGamePlayerNumber == damageData.gamePlayerNum) {
					MultiplayerController.instance.localGameManager.TakeDamage (damageData.damage);
				}
				break;

			case FIRE_RAY:
				FireRayData rayData = (FireRayData) payloadWrapper.payload;
				MultiplayerController.instance.remoteCharacterGameObjects [rayData.playerNum]
					.GetComponent<RemoteCharacterController> ().FireGun (rayData.fireRay);
				break;

			case HAND_GRENADE:
				GrenadeData grenadeData = (GrenadeData)payloadWrapper.payload;
				MultiplayerController.instance.remoteCharacterGameObjects [grenadeData.playerNum]
					.GetComponent<RemoteCharacterController> ().ThrowGrenade ( grenadeData.position, grenadeData.rotation, grenadeData.force);
				break;

			case DESTROY_ITEM:
				try {
					DestroyItemData destroyItemData = (DestroyItemData) payloadWrapper.payload;
					if (destroyItemData.smallPayload == null) {
						ItemIdGenerator.instance.DestroyItemByRemote (destroyItemData.destroyItemId);
					} else {
						ItemIdGenerator.instance.DestroyItemByRemote (destroyItemData.destroyItemId, destroyItemData.smallPayload);
					}
				} catch (System.Exception e) {
					ConsoleLog.SLog ("Error in tag DESTROY_ITEM\n" + e.Message);
				}
				break;

			default:
				ConsoleLog.SLog ("ERROR: Invalid PayloadWrapper tag. Can't parse payload.");
				break;
			}
		}
	}
}

[Serializable]
public class PayloadWrapper {
	private static BinaryFormatter bf = new BinaryFormatter();

	public string tag;
	public System.Object payload;

	public PayloadWrapper (string tag, System.Object payload) {
		this.tag = tag;
		this.payload = payload;
	}

	public static byte[] Build(string tag, System.Object payload){
//		ConsoleLog.SLog ("Send Payload");
//		ConsoleLog.SLog("time: " + (int) Time.realtimeSinceStartup + " tag: " + tag);
		MemoryStream ms = new MemoryStream();
		bf.Serialize(ms, new PayloadWrapper(tag, payload));
		return ms.ToArray();
	}
}

[Serializable]
public class SerializeVector3 {
	public float[] vectorArray = new float[3];

	public SerializeVector3 (Vector3 vector){
		this.vectorArray[0] = vector.x;
		this.vectorArray[1] = vector.y;
		this.vectorArray[2] = vector.z;
	}

	public Vector3 vector3 {
		get { return new Vector3 (this.vectorArray[0], this.vectorArray[1], this.vectorArray[2]); }
	}
}

[Serializable]
public class ReqInitRoomData {
	public string playerName;
	public string avatarURL;

	public ReqInitRoomData ( string playerName, string avatarURL){
		this.playerName = playerName;
		this.avatarURL = avatarURL;
	}
}

[Serializable]
public class InitRoomData {
	public int roomPlayerNum;
	public uint roomCapacity;
	public int gameMode;

	public InitRoomData (uint roomCapacity, int roomPlayerNum, int gameMode){
		this.roomPlayerNum = roomPlayerNum;
		this.roomCapacity = roomCapacity;
		this.gameMode = gameMode;
	}
}

[Serializable]
public class RoomMemberIntroductionData {
	public string[] memberNames;
	public string[] memberAvatars;
	public string[] memberId;

	public RoomMemberIntroductionData (string[] memberNames, string[] memberAvatars, string[] memberId){
		this.memberNames = memberNames.Clone () as string[];
		this.memberAvatars = memberAvatars.Clone () as string[];
		this.memberId = memberId.Clone () as string[];
	}
}


[Serializable]
public class TeamData {
	public int roomPlayerNum;
	public int teamNum;

	public TeamData (int roomPlayerNum, int teamNum){
		this.roomPlayerNum = roomPlayerNum;
		this.teamNum = teamNum;
	}
}

[Serializable]
public class InitGameData {
	public int[] gamePlayersNumByRoomPlayersNum;
	public int[] teamNums;
	public int gamePlayerNum;
	public int map;
	private float[] vectorArray = new float[3];

	public InitGameData (int[] teamNums, int[] gamePlayersNumByRoomPlayersNum, int gamePlayerNum, int map, Vector3 spawnPoint){
		this.teamNums = teamNums.Clone() as int[];
		this.gamePlayersNumByRoomPlayersNum = gamePlayersNumByRoomPlayersNum.Clone() as int[];
		this.gamePlayerNum = gamePlayerNum;
		this.map = map;
		this.vectorArray[0] = spawnPoint.x;
		this.vectorArray[1] = spawnPoint.y;
		this.vectorArray[2] = spawnPoint.z;
	}

	public Vector3 spawnPoint {
		get { return new Vector3 (this.vectorArray[0], this.vectorArray[1], this.vectorArray[2]); }
	}
}

[Serializable]
public class RoundData {
	public int round, team1Score, team2Score;

	public RoundData (int round, int team1Score, int team2Score){
		this.round = round;
		this.team1Score = team1Score;
		this.team2Score = team2Score;
	}
}

[Serializable]
public class EndRoundData {
	public int round, team1Score, team2Score, winner;

	//team mode: winner = team number
	// if round == 3 means EndGame
	public EndRoundData (int round, int team1Score, int team2Score, int winnerTeamNumber){
		this.round = round;
		this.team1Score = team1Score;
		this.team2Score = team2Score;
		this.winner = winnerTeamNumber;
	}
		
	//deathmatch mode: winner = gamePlayerNum
	//receive this also means EndGame
	public EndRoundData (int winnerGamePlayerNum){
		this.winner = winnerGamePlayerNum;
	}
}

[Serializable]
public class DestroyItemData {
	public int destroyItemId;
	public System.Object smallPayload;

	public DestroyItemData (int itemNum, System.Object smallPayload = null) {
		this.destroyItemId = itemNum;
		this.smallPayload = smallPayload;
	}
}

[Serializable]
public class FireRayData {
	public int playerNum;
	private float[] startArray = new float[3];
	private float[] directionArray = new float[3];

	public Vector3 start {
		get { return new Vector3 (this.startArray[0], this.startArray[1], this.startArray[2]); }
	}

	public Vector3 direction {
		get { return new Vector3 (this.directionArray[0], this.directionArray[1], this.directionArray[2]); }
	}

	public Ray fireRay {
		get { return new Ray (this.start, this.direction); }
	}

	public FireRayData (Ray ray, int playerNum) {
		this.startArray [0] = ray.origin.x;
		this.startArray[1] = ray.origin.y;
		this.startArray[2] = ray.origin.z;
		this.directionArray [0] = ray.direction.x;
		this.directionArray[1] = ray.direction.y;
		this.directionArray[2] = ray.direction.z;
		this.playerNum = playerNum;
	}
}

[Serializable]
public class DamageData {
	public float damage;
	public int gamePlayerNum;

	public DamageData (int gamePlayerNum, float damage){
		this.gamePlayerNum = gamePlayerNum;
		this.damage = damage;
	}
}

[Serializable]
public class GrenadeData {
	public int playerNum;
	private float[] posArray = new float[3];
	private float[] rotArray = new float[3];
	private float[] forceArray = new float[3];

	public Vector3 position {
		get { return new Vector3 (this.posArray[0], this.posArray[1], this.posArray[2]); }
	}

	public Vector3 rotation {
		get { return new Vector3 (this.rotArray[0], this.rotArray[1], this.rotArray[2]); }
	}
		
	public Vector3 force {
		get { return new Vector3 (this.forceArray[0], this.forceArray[1], this.forceArray[2]); }
	}

	public GrenadeData (int playerNumber, Vector3 position, Vector3 rotation, Vector3 force) {
		this.posArray [0] = position.x;
		this.posArray[1] = position.y;
		this.posArray[2] = position.z;

		this.rotArray [0] = rotation.x;
		this.rotArray[1] = rotation.y;
		this.rotArray[2] = rotation.z;

		this.forceArray [0] = force.x;
		this.forceArray[1] = force.y;
		this.forceArray[2] = force.z;

		this.playerNum = playerNumber;
	}
}

[Serializable]
public class PlayerGameData {
	public int playerNumber;
	public float health;
	public int characterType;
	public bool isAim;
	public float time;

	//transform stuff
	private float[] positionArray = new float[3];
	private float[] rotationArray = new float[3];

	public Vector3 position {
		get { return new Vector3 (positionArray [0], positionArray [1], positionArray [2]); }
	}

	public Quaternion rotation {
		get { return Quaternion.Euler (new Vector3(rotationArray [0], rotationArray [1], rotationArray [2])); }
	}

	//animation stuff
	public int animState;

	public PlayerGameData(int playerNumber, float health, int charType, Vector3 pos, Vector3 rot, int animState, bool isAim, float time){
		this.playerNumber = playerNumber;
		this.health = health;
		this.characterType = charType;
		this.positionArray [0] = pos.x;
		this.positionArray [1] = pos.y;
		this.positionArray [2] = pos.z;
		this.rotationArray [0] = rot.x;
		this.rotationArray [1] = rot.y;
		this.rotationArray [2] = rot.z;
		this.animState = animState;
		this.isAim = isAim;
		this.time = time;
	}
}