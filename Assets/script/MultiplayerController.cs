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
	
	//Payload Tag
	public const string PLAYER_DATA = "playerData";
	public const string REQ_INIT_ROOM = "reqInitRoom";
	public const string RES_INIT_ROOM = "resInitRoom";
	public const string REQ_INIT_GAME = "reqInitGame";
	public const string REQ_SWITCH_TEAM = "reqSwitchTeam";
	public const string RES_SWITCH_TEAM = "resSwitchTeam";
	public const string FINISH_SELECT_TEAM = "finishSelectTeam";
	public const string READY = "ready";
	public const string START_GAME = "startGame";
	public const string RES_INIT_GAME = "resInitGame";
	public const string	REQ_LEAVE_ROOM = "reqLeaveRoom";
	public const string INFLICT_DAMAGE = "inflictDamage";
	public const string FIRE_RAY = "fireRay";
	public const string HAND_GRENADE = "handGrenade";
	public const string DESTROY_ITEM = "destroyItem";

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

	private Animator localAnimator;
	[HideInInspector] public int localAnimationState = ANIM_IDLE;

	//for debug
	public Text latestPlayerDataText;

	//Game Room Setting
	const int MinOpponents = 1;
	public uint MaxOpponents = 3;
	[HideInInspector] public uint gameMode = GAMEMODE_DEATHMATCH;
	[HideInInspector] public int playerCount = 0;
	[HideInInspector] public int localRoomPlayerNumber = -1;
	[HideInInspector] public bool isReady = false;
	[HideInInspector] public bool[] otherPlayerReady;

	//Team mode setting
	[HideInInspector] public int localTeamNumber = -1;
	[HideInInspector] public int[] otherPlayerTeamNumber;
	[HideInInspector] public bool isFinishSelectTeam = false;

	//Game Play Setting
	public bool isGameStart = false;
	[HideInInspector] public int localPlayerNumber = -1;

	private GameObject localPlayer, cardboardHead;
	private PlayerGameManager localGameManager;
	private UnityChanControlScriptWithRgidBody localUnityChanControlScript;
	[HideInInspector] public GameObject[] remoteCharacterGameObjects;
	[HideInInspector] public PlayerData[] latestPlayerDatas;
	[HideInInspector] public Vector3[] spawnPoints;
	[HideInInspector] public bool[] hasNewPlayerDatas;
	[HideInInspector] public bool[] updatedLastFrame;
	[HideInInspector] public string[] clientId;

	public float broadcastDataPerSec;
	[HideInInspector] public float timeBetweenBroadcast;
	private float broadcastTimer = 0f;

	private bool isBroadcast = true;

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

	void FixedUpdate () {
		CheckNullComponents ();
	}

	void Update() {
		
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

	private void CheckNullComponents() {
		if (!isGameStart) return;

		//In case of changing character mid game, It might be null game object.
		if (
			localPlayer == null ||
			cardboardHead == null ||
			localGameManager == null ||
			localUnityChanControlScript == null ||
			localAnimator == null ||
			spawnPoints == null
		) {
			FindComponents ();
		}
	}

	private void FindComponents () {
		ConsoleLog.SLog ("FindComponents()");

		try {
			ConsoleLog.SLog ("FindComponents() 1");
			localPlayer = GameObject.FindGameObjectWithTag ("Player");
			ConsoleLog.SLog ("FindComponents() 2");
			cardboardHead = GameObject.FindGameObjectWithTag ("PlayerHead");
			ConsoleLog.SLog ("FindComponents() 3");
			localGameManager = localPlayer.GetComponent<PlayerGameManager> ();
			ConsoleLog.SLog ("FindComponents() 4");
			localUnityChanControlScript = localPlayer.GetComponent<UnityChanControlScriptWithRgidBody> ();
			ConsoleLog.SLog ("FindComponents() 5");
			localAnimator = localPlayer.GetComponent<Animator> ();
			ConsoleLog.SLog ("FindComponents() 6");

			GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag ("SpawnPoint");
			ConsoleLog.SLog ("FindComponents() 7");
			spawnPoints = new Vector3[spawnPointObjects.Length];
			ConsoleLog.SLog ("FindComponents() 8");
			for (int i = 0; i < spawnPoints.Length; i++) {
				ConsoleLog.SLog ("FindComponents() 9("+i+")");
				spawnPoints [i] = spawnPointObjects [i].transform.position;
				ConsoleLog.SLog ("FindComponents() 10("+i+")");
			}
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in FindComponents()\n" + e.Message);
		}

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
			return clientId [playerNum];
		}

		ConsoleLog.SLog ("Error: Client Id not found (" + playerNum + ")");
		return "";
	}


	// ============== Game Room Function ============== //

	public void CreateRoomWithInvite(int roomCapacity, uint mode) {
		ConsoleLog.SLog ("CreateRoomWithInvite");

		InitializeRoomCapacity (roomCapacity);

		localRoomPlayerNumber = 0; //player number 0 is host
		playerCount = 1;
		gameMode = mode;
	
		PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen (
			MinOpponents, MaxOpponents, gameMode, MultiplayerListener.Instance
		);
	}

	public void CreateRoomDeathmatch(int roomCapacity) {
		CreateRoomWithInvite (roomCapacity, GAMEMODE_DEATHMATCH);
	}

	public void JoinRoom(int roomCapacity, uint mode, int roomPlayerNum) {
		ConsoleLog.SLog ("JoinRoom");

		InitializeRoomCapacity (roomCapacity);

		localRoomPlayerNumber = roomPlayerNum;
		gameMode = mode;
	}

	public void InitializeRoomCapacity(int roomCapacity){
		ConsoleLog.SLog("InitializeRoomCapacity");

		MaxOpponents = (uint) roomCapacity - 1;
		remoteCharacterGameObjects = new GameObject[MaxOpponents + 1];
		latestPlayerDatas = new PlayerData[MaxOpponents + 1];
		hasNewPlayerDatas = new bool[MaxOpponents + 1];
		updatedLastFrame = new bool[MaxOpponents + 1];
		clientId = new string[MaxOpponents + 1];
		otherPlayerReady = new bool[MaxOpponents + 1];
		otherPlayerTeamNumber = new int[MaxOpponents + 1];
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
			break;
		default:
			ConsoleLog.SLog ("Error SetRoomUiByGameMode (" + gameMode + "): unmatched gameMode");
			break;
		}
	}

	public void SendReady (){
		if (localRoomPlayerNumber == -1) return;

		ConsoleLog.SLog("SendReady ()");

		isReady = true;
		otherPlayerReady [localRoomPlayerNumber] = true;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true, 
			PayloadWrapper.Build (
				READY,
				localRoomPlayerNumber
			)
		);
		if (localRoomPlayerNumber == 0 && isAllReady()) {
			SendInitGame ();
		}
	}

	public void SendSwtichTeamReq (int team){
		ConsoleLog.SLog ("SendSwitchTeamReq (" + team + ")");

		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true,
			PayloadWrapper.Build (
				REQ_SWITCH_TEAM,
				team
			)
		);
	}
		
	public void SetTeam (int roomPlayerNum, int team){
		if (gameMode == GAMEMODE_TEAM) {
			if(localRoomPlayerNumber == roomPlayerNum) localTeamNumber = team;
			otherPlayerTeamNumber [roomPlayerNum] = team;

			//TODO switch team in UI
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

		foreach (int teamNum in otherPlayerTeamNumber) {
			if (teamNum == 1) team1Count++;
			else if (teamNum == 2) team2Count++;
		}
		if (team1Count == team2Count) return true;

		return false;
	}

	public bool isAllReady (){
		if (localRoomPlayerNumber == -1) { return false; }

		for (int i = 0; i < MaxOpponents + 1; i++) {
			if (!otherPlayerReady [i]) {
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

		try {
			ConsoleLog.SLog ("SendInitGame ()");

			if (gameMode == GAMEMODE_DEATHMATCH) {
				ConsoleLog.SLog ("1");

				clientId [0] = PlayGamesPlatform.Instance.RealTime.GetSelf().ParticipantId;
				ConsoleLog.SLog ("2");
				Shuffle (clientId);
				ConsoleLog.SLog ("3");

				for (int i = 0; i < clientId.Length; i++) {
					ConsoleLog.SLog ("4("+i+")");
					InitGameData data = new InitGameData (i, Vector3.zero);

					if (clientId [i] == PlayGamesPlatform.Instance.RealTime.GetSelf ().ParticipantId) {
						ConsoleLog.SLog ("5.1("+i+")");
						localPlayerNumber = i;
					} else {
						ConsoleLog.SLog ("5.2("+i+")");
						PlayGamesPlatform.Instance.RealTime.SendMessage (
							true,
							clientId[i],
							PayloadWrapper.Build(
								RES_INIT_GAME,
								data
							)
						);
					}
					ConsoleLog.SLog ("6("+i+")");
				}

				InitGameData data2 = new InitGameData (localPlayerNumber, Vector3.zero);
				InitGame(data2);
				ConsoleLog.SLog ("7");

			} else if (gameMode == GAMEMODE_TEAM) {

			}
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in SendInitGame ()\n" + e.Message);
		}
	}

	public void InitGame (InitGameData data){
		ConsoleLog.SLog ("InitGame (data.playerNum=" + data.playerNum + ")");

		switch (gameMode) {
		case GAMEMODE_DEATHMATCH:
			ConsoleLog.SLog ("InitGame1");
			localPlayerNumber = data.playerNum;
			ConsoleLog.SLog ("InitGame 2");
			SceneManager.LoadScene (1);
			ConsoleLog.SLog ("InitGame 3");
			FindComponents ();
			ConsoleLog.SLog ("InitGame 4");
			isGameStart = true;
			break;
		case GAMEMODE_TEAM:
			break;
		default:
			ConsoleLog.SLog ("Error: SendInitGame with unmatch gameMode (" + gameMode + ")");
			break;
		}
		ConsoleLog.SLog ("InitGame 5");
	}

	public void LeaveRoom (){
		//tell everyboay I'm leaving
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
			REQ_LEAVE_ROOM, localPlayerNumber
		));
		PlayGamesPlatform.Instance.RealTime.LeaveRoom ();

		//clear leftover
		for (int i = 0; i < remoteCharacterGameObjects.Length; i++) {
			if (remoteCharacterGameObjects [i] == null) continue;
			Destroy (remoteCharacterGameObjects [i]);
		}

		isGameStart = false;
		localPlayerNumber = -1;
		localRoomPlayerNumber = -1;
		playerCount = 0;
		hasNewPlayerDatas = new bool[MaxOpponents + 1];
		latestPlayerDatas = new PlayerData[MaxOpponents + 1];
		remoteCharacterGameObjects = new GameObject[MaxOpponents + 1];

		SceneManager.LoadScene (0);
		RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_INDEX);
	}

	public void RemovePlayerFromGame(int otherPlayerNumber){
		//He's gone. Forget him.
		Destroy (remoteCharacterGameObjects [otherPlayerNumber]);
		remoteCharacterGameObjects [otherPlayerNumber] = null;
		latestPlayerDatas [otherPlayerNumber] = null;
	}


	// ============== Game Play Communication Function ============== //

	public void SendDamage (int remotePlayerNum, float damage){
		PlayGamesPlatform.Instance.RealTime.SendMessage (true, GetClientId (remotePlayerNum), PayloadWrapper.Build (
			INFLICT_DAMAGE,
			damage
		));
	}

	public void SendFireRay (Ray fireRay) {
		if (!isGameStart) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			false,
			PayloadWrapper.Build (
				FIRE_RAY,
				new FireRayData (fireRay, localPlayerNumber)
			)
		);
	}

	public void SendHandGrenade (Vector3 position, Vector3 rotation, Vector3 force) {
		if (!isGameStart) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true,
			PayloadWrapper.Build (
				HAND_GRENADE,
				new GrenadeData (localPlayerNumber, position, rotation, force)
			)
		);
	}

	public void SendDestroyItem(int itemId, System.Object something = null){
		ConsoleLog.SLog ("SendDestroyItem("+itemId+")");
		if (!isGameStart) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true,
			PayloadWrapper.Build (
				DESTROY_ITEM,
				new DestroyItemData(itemId, something)
			)
		);
	}


	// ============== Broadcast Function ============== //

	private void BroadcastPlayerData(){
		if (!isBroadcast) return;

		if (broadcastTimer < timeBetweenBroadcast) {
			broadcastTimer += Time.deltaTime;
			return;
		} else {
			broadcastTimer = 0f;
		}

		PlayerData data = new PlayerData (
			localPlayerNumber,
			localGameManager.health,
			localUnityChanControlScript.characterType, //TODO get character type from player game manager script
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


	// ============== Remote Character Function ============== //

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


	// ============== Debug Function ============== //

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
			latestPlayerDataText.text = "localPlayerNumber: " + localPlayerNumber + "  MaxPlayer: " + (MaxOpponents + 1) + "\n";
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
					if (latestPlayerDatas [i] == null || i == localPlayerNumber) continue;

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
					if (latestPlayerDatas [i] == null || i == localPlayerNumber) continue;

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

	public class MultiplayerListener : GooglePlayGames.BasicApi.Multiplayer.RealTimeMultiplayerListener {

		private static MultiplayerListener sInstance = new MultiplayerListener();

		public static MultiplayerListener Instance {
			get { return sInstance; }
		}

		public void OnRoomSetupProgress(float percent){
			ConsoleLog.SLog("OnRoomSetupProgress: " + percent);

			PlayGamesPlatform.Instance.RealTime.ShowWaitingRoomUI();

			MultiplayerController.instance.SetRoomUiByGameMode (MultiplayerController.instance.gameMode);
		}

		public void OnRoomConnected(bool success){
			ConsoleLog.SLog("OnRoomConnected: " + success);

			if (!success) return;

			// if doesn't have room player number yet, request one
			if (MultiplayerController.instance.localRoomPlayerNumber == -1) {
				ConsoleLog.SLog("Send REQ_INIT_ROOM");
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
					MultiplayerController.REQ_INIT_ROOM,
					0
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
			
		}

		public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data) {
			//deserialize data, get position and head's rotation of that sender, and set to it's character.

//			ConsoleLog.SLog("MessageReceived ID: " + senderId);

			BinaryFormatter bf = new BinaryFormatter ();
			PayloadWrapper payloadWrapper = (PayloadWrapper) bf.Deserialize (new MemoryStream (data));
//			ConsoleLog.SLog("time: " + (int) Time.realtimeSinceStartup + " tag: " + payloadWrapper.tag);

			switch (payloadWrapper.tag) {
			case REQ_INIT_ROOM:
				//if this is host
				if (MultiplayerController.instance.localRoomPlayerNumber == 0) {
					
					//send client player number, room capacity for init room, and spawn point
					InitRoomData initRoomData = new InitRoomData (
						MultiplayerController.instance.MaxOpponents + 1,
						MultiplayerController.instance.playerCount,
						(int) MultiplayerController.instance.gameMode
					);

					PlayGamesPlatform.Instance.RealTime.SendMessage (true, senderId, PayloadWrapper.Build (
						RES_INIT_ROOM,
						initRoomData
					));

					MultiplayerController.instance.playerCount++;
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

			case REQ_SWITCH_TEAM:
				//if host
				if (MultiplayerController.instance.localRoomPlayerNumber == 0) {
					
					if (!MultiplayerController.instance.isFinishSelectTeam) {
						
						TeamData teamData = (TeamData) payloadWrapper.payload;
						MultiplayerController.instance.SetTeam (teamData.roomPlayerNum, teamData.teamNum);
						PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
							true,
							PayloadWrapper.Build (
								RES_SWITCH_TEAM,
								teamData
							)
						);
					}
				}
				break;

			case RES_SWITCH_TEAM:
				TeamData teamData2 = (TeamData) payloadWrapper.payload;
				MultiplayerController.instance.SetTeam (teamData2.roomPlayerNum, teamData2.teamNum);
				break;

			case FINISH_SELECT_TEAM:
				RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_VR_READY);
				break;

			case READY:
				MultiplayerController.instance.otherPlayerReady [(int)payloadWrapper.payload] = true;
				MultiplayerController.instance.clientId [(int)payloadWrapper.payload] = senderId;

				//TODO set UI to show that player is ready

				//if host
				if (MultiplayerController.instance.localRoomPlayerNumber == 0) {
					//if everyone is ready, start the game
					if (MultiplayerController.instance.isAllReady ()) {
						MultiplayerController.instance.SendInitGame ();
					}
				}

				break;

			case RES_INIT_GAME:
				InitGameData initGameData = (InitGameData) payloadWrapper.payload;

				MultiplayerController.instance.InitGame (initGameData);
				InitGameData resInitGameData = (InitGameData) payloadWrapper.payload;
				
//				//init room
//				MultiplayerController.instance.InitializeRoomCapacity ((int) resInitGameData.roomCapacity);
//				
//				//set spawn point
//				MultiplayerController.instance.localPlayer.transform.position = resInitGameData.spawnPoint;
//				
//				//save assigned player number
//				MultiplayerController.instance.localPlayerNumber = resInitGameData.playerNum;

				break;

			case PLAYER_DATA:
				try {
					//if someone who connected to room early and broadcast player data before we initialize, ignore it.
					if(!MultiplayerController.instance.isGameStart) return;

					PlayerData otherPlayerData = (PlayerData) payloadWrapper.payload;

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

					//save senderId for easy access in the future
					if(MultiplayerController.instance.clientId[otherPlayerData.playerNumber] == null) {
						MultiplayerController.instance.clientId[otherPlayerData.playerNumber] = senderId;
					}

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
				MultiplayerController.instance.localGameManager.takeDamage ((float)payloadWrapper.payload);
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
	public int playerNum;
	private float[] vectorArray = new float[3];

	public InitGameData (int playerNum, Vector3 spawnPoint){
		this.playerNum = playerNum;
		this.vectorArray[0] = spawnPoint.x;
		this.vectorArray[1] = spawnPoint.y;
		this.vectorArray[2] = spawnPoint.z;
	}

	public Vector3 spawnPoint {
		get { return new Vector3 (this.vectorArray[0], this.vectorArray[1], this.vectorArray[2]); }
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
public class PlayerData {
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

	public PlayerData(int playerNumber, float health, int charType, Vector3 pos, Vector3 rot, int animState, bool isAim, float time){
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
