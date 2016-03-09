using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MultiplayerController : MonoBehaviour {
	
	public static MultiplayerController instance;
	
	//Payload Tag
	public const string PLAYER_DATA = "playerData";
	public const string REQ_INIT = "reqInit";
	public const string RES_INIT = "resInit";
	public const string	REQ_LEAVE_ROOM = "reqLeaveRoom";
	public const string INFLICT_DAMAGE = "inflictDamage";

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

	//animation name hash from local animator
	private static int idleState = Animator.StringToHash("Base Layer.pistol idle normal");
	private static int aimState = Animator.StringToHash("Base Layer.pistol idle aim");
	private static int walkForwardState = Animator.StringToHash("Base Layer.pistol walk forward");
	private static int walkLeftState = Animator.StringToHash("Base Layer.pistol walk left");
	private static int walkRightState = Animator.StringToHash("Base Layer.pistol walk right");
	private static int aimWalkForwardState = Animator.StringToHash("Base Layer.pistol walk forward aim");
	private static int aimWalkLeftState = Animator.StringToHash("Base Layer.pistol walk left aim");
	private static int aimWalkRightState = Animator.StringToHash("Base Layer.pistol walk right aim");
	private static int jumpState = Animator.StringToHash("Base Layer.Jump");
	private static int walkBackwardState = Animator.StringToHash("Base Layer.pistol walk forward 0");
	private static int aimWalkBackwardState = Animator.StringToHash("Base Layer.pistol walk forward aim 0");

	private Animator localAnimator;
	public int localAnimationState = ANIM_IDLE;

	[HideInInspector]
	public int charType = 1;

	//for debug
	public Text latestPlayerDataText;

	//Game Setting
	const int MinOpponents = 1;
	public uint MaxOpponents = 3;
	[HideInInspector]
	public int playerCount = 0;
	[HideInInspector]
	public int localPlayerNumber = -1;

	private GameObject localPlayer, cardboardHead;
	private PlayerGameManager localGameManager;
	[HideInInspector]
	public GameObject[] characterGameObjects;
	[HideInInspector]
	public PlayerData[] latestPlayerDatas;
	public Vector3[] spawnPoints;
	[HideInInspector]
	public bool[] hasNewPlayerDatas;
	[HideInInspector]
	public bool[] updatedLastFrame;
	[HideInInspector]
	public string[] clientId;

	public float timeBetweenBroadcast = 0.5f;
	private float broadcastTimer = 0f;

	private bool isBroadcast = true;

	public GameObject OverlayLog, OverLayAllFn;

	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void Start() {
		localPlayer = GameObject.FindGameObjectWithTag ("Player");
		cardboardHead = GameObject.FindGameObjectWithTag ("PlayerHead");
		localGameManager = localPlayer.GetComponent<PlayerGameManager> ();
		localAnimator = localPlayer.GetComponent<Animator> ();
	}

	void Update() {
		
	}

	void LateUpdate() {
		if (PlayGamesPlatform.Instance.RealTime.IsRoomConnected() && localPlayerNumber != -1) {
			ResetNewPlayerDataFlag ();
			BroadcastPlayerData ();
		}
	}

	void OnGUI(){
		PrintPlayerData ();
	}

	public void InitializeRoomCapacity(int roomCapacity){
		ConsoleLog.SLog("InitializeRoomCapacity");

		MaxOpponents = (uint) roomCapacity - 1;
		characterGameObjects = new GameObject[MaxOpponents + 1];
		latestPlayerDatas = new PlayerData[MaxOpponents + 1];
		hasNewPlayerDatas = new bool[MaxOpponents + 1];
		updatedLastFrame = new bool[MaxOpponents + 1];
		clientId = new string[MaxOpponents + 1];
	}

	public void SelectCharacterType(int charType){
		this.charType = charType;
	}

	public void CreateRoomWithInvite(){
		ConsoleLog.SLog("CreateRoomWithInvite");

		localPlayerNumber = 0;
		playerCount = 1;

		const int GameVariant = 0; //TODO spacify game mode (game logic) when create room
		PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen (
			MinOpponents, MaxOpponents, GameVariant, MultiplayerListener.Instance
		);
	}

	public void ShowInvite(){
		ConsoleLog.SLog("ShowInvite");
		PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(MultiplayerListener.Instance);
	}

	public void LeaveRoom(){
		//tell everyboay I'm leaving
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
			REQ_LEAVE_ROOM, localPlayerNumber
		));
		PlayGamesPlatform.Instance.RealTime.LeaveRoom ();

		//clear leftover
		for (int i = 0; i < characterGameObjects.Length; i++) {
			if (characterGameObjects [i] == null) continue;
			Destroy (characterGameObjects [i]);
		}
		localPlayerNumber = -1;
		playerCount = 0;
		hasNewPlayerDatas = new bool[MaxOpponents + 1];
		latestPlayerDatas = new PlayerData[MaxOpponents + 1];
		characterGameObjects = new GameObject[MaxOpponents + 1];

		//TODO load main screen
	}

	public void RemovePlayerFromGame(int otherPlayerNumber){
		//He's gone. Forget him.
		Destroy (characterGameObjects [otherPlayerNumber]);
		characterGameObjects [otherPlayerNumber] = null;
		latestPlayerDatas [otherPlayerNumber] = null;
	}

	public void SetBroadcast (bool b){
		isBroadcast = b;
	}

	public void SetLocalAnimationState(int state){
		localAnimationState = state;
	}

	public void SendDamage (int remotePlayerNum, float damage){
		PlayGamesPlatform.Instance.RealTime.SendMessage (true, clientId [remotePlayerNum], PayloadWrapper.Build (
			INFLICT_DAMAGE,
			damage
		));
	}

	private void BroadcastPlayerData(){
		if (!isBroadcast) return;

		if (broadcastTimer < timeBetweenBroadcast) {
			broadcastTimer += Time.deltaTime;
			return;
		} else {
			broadcastTimer = 0f;
		}

		//TODO pack animation data

		PlayerData data = new PlayerData (
			localPlayerNumber,
			localGameManager.health,
			charType, //TODO get character type from player game manager script
			localPlayer.transform.position,
			new Vector3(
				cardboardHead.transform.localRotation.eulerAngles.x,
//				localPlayer.transform.rotation.eulerAngles.y,
				cardboardHead.transform.localRotation.eulerAngles.y,
				cardboardHead.transform.localRotation.eulerAngles.z),
			localAnimationState
		);

		bool reliable = false;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliable, PayloadWrapper.Build(PLAYER_DATA, data));
	}

	public GameObject GetCharacter(int otherPlayerNumber){
		CheckInitRemoteCharacter (otherPlayerNumber);
		return characterGameObjects [otherPlayerNumber];
	}

	public void CheckInitRemoteCharacter(int playerNum){
		if (characterGameObjects [playerNum] == null) {
			ConsoleLog.SLog ("---------------------- instantiate character player [" + playerNum + "] ----------------------");

			characterGameObjects[playerNum] = Instantiate (
				GetCharPrefab(latestPlayerDatas[playerNum].characterType),
				latestPlayerDatas[playerNum].position,
				latestPlayerDatas[playerNum].rotation
			) as GameObject;

			RemoteCharacterController remoteController = characterGameObjects [playerNum].GetComponent<RemoteCharacterController> ();
			remoteController.playerNum = playerNum;
		}
	}

	public GameObject GetCharPrefab (int charType){
		switch (charType) {
		case CHAR_TYPE_PISTOL: return UnityChanWithPisolPrefab; 
		case CHAR_TYPE_RIFLE: return UnityChanWithRiflePrefab;
		case CHAR_TYPE_SHORTGUN: return UnityChanWithShotgunPrefab;
		case CHAR_TYPE_SNIPER: return UnityChanWithSniperPrefab;
		default: return otherPlayerDummyPrefab;
		}
	}

//	public int GetLocalAnimStateNumber () {
//		
//		if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol idle normal")) {
//			return ANIM_IDLE;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol idle aim")) {
//			return ANIM_AIM;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk forward")) {
//			return ANIM_WALK_FORWARD;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk left")) {
//			return ANIM_WALK_LEFT;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk right")) {
//			return ANIM_WALK_RIGHT;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk forward aim")) {
//			return ANIM_AIM_WALK_FORWARD;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk left aim")) {
//			return ANIM_AIM_WALK_LEFT;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk right aim")) {
//			return ANIM_AIM_WALK_RIGHT;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.Jump")) {
//			return ANIM_JUMP;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk forward 0")) {
//			return ANIM_WALK_BACKWARD;
//		} else if (localAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.pistol walk forward aim 0")) {
//			return ANIM_AIM_WALK_BACKWARD;
//		} else { // default
//			ConsoleLog.SLog("Get Anim: Default");
//			return ANIM_IDLE;
//		}

//		ConsoleLog.SLog ("current: " + animStateFullPathHash);
//		ConsoleLog.SLog ("walk forward: " + walkForwardState);
//
//		if (animStateFullPathHash == idleState) return ANIM_IDLE;
//		else if (animStateFullPathHash == aimState) return ANIM_AIM;
//		else if (animStateFullPathHash == walkForwardState) return ANIM_WALK_FORWARD;
//		else if (animStateFullPathHash == walkLeftState) return ANIM_WALK_LEFT;
//		else if (animStateFullPathHash == walkRightState) return ANIM_WALK_RIGHT;
//		else if (animStateFullPathHash == aimWalkForwardState) return ANIM_AIM_WALK_FORWARD;
//		else if (animStateFullPathHash == aimWalkLeftState) return ANIM_AIM_WALK_LEFT;
//		else if (animStateFullPathHash == aimWalkRightState) return ANIM_AIM_WALK_RIGHT;
//		else if (animStateFullPathHash == jumpState) return ANIM_JUMP;
//		else if (animStateFullPathHash == walkBackwardState) return ANIM_WALK_BACKWARD;
//		else if (animStateFullPathHash == aimWalkBackwardState) return ANIM_AIM_WALK_BACKWARD;
//		else return ANIM_IDLE;
//	}

	private void ResetNewPlayerDataFlag(){
		for (int i = 0; i < hasNewPlayerDatas.Length; i++) {
			updatedLastFrame[i] = false;
		}
	}

	private void PrintPlayerData(){

		try {
			latestPlayerDataText.text = "localPlayerNumber: " + localPlayerNumber + "  MaxPlayer: " + (MaxOpponents + 1) + "\n";

			latestPlayerDataText.text += "updatedLastFrame: ";
			if (updatedLastFrame != null) 
				for (int i = 0; i < updatedLastFrame.Length; i++){ latestPlayerDataText.text += (updatedLastFrame[i] ? "1 " : "0 ");}
			latestPlayerDataText.text += "\n";

			latestPlayerDataText.text += "characterGameObjects: ";
			if (characterGameObjects != null) 
				for (int i = 0; i < characterGameObjects.Length; i++){ latestPlayerDataText.text += (characterGameObjects[i] == null ? "X " : "/ ");}
			latestPlayerDataText.text += "\n";

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

			latestPlayerDataText.text += "\n + Local Data +\n";
			if (characterGameObjects != null) {
				for (int i = 0; i < characterGameObjects.Length; i++) {
					if (latestPlayerDatas [i] == null || i == localPlayerNumber) continue;

					latestPlayerDataText.text += "[" + i + "] ";
					latestPlayerDataText.text += "Pos: " + 
						roundDown(characterGameObjects[i].transform.position.x, 1) + ", " + 
						roundDown(characterGameObjects[i].transform.position.y, 1) + ", " + 
						roundDown(characterGameObjects[i].transform.position.z, 1) + " ";
					latestPlayerDataText.text += "Rot: " +
						roundDown(characterGameObjects[i].transform.rotation.eulerAngles.x, 1) + ", " +
						roundDown(characterGameObjects[i].transform.rotation.eulerAngles.y, 1) + ", " +
						roundDown(characterGameObjects[i].transform.rotation.eulerAngles.z, 1) + "\n";
				}
			}

			latestPlayerDataText.text += "\n + Local Character\n";
			latestPlayerDataText.text += "pos: " + 
				roundDown(localPlayer.transform.position.x, 1) + ", " + 
				roundDown(localPlayer.transform.position.y, 1) + ", " + 
				roundDown(localPlayer.transform.position.z, 1) + "\n";
			latestPlayerDataText.text += "rot: " + 
				roundDown(localPlayer.transform.rotation.eulerAngles.x, 1) + ", " + 
				roundDown(localPlayer.transform.rotation.eulerAngles.y, 1) + ", " + 
				roundDown(localPlayer.transform.rotation.eulerAngles.z, 1) + "\n";
		} catch (Exception e){
			ConsoleLog.SLog ("error in PrintLatestPlayerData");
			ConsoleLog.SLog (e.Message);
		}
	}

	public void HideLogAndControllPanel () {
		OverlayLog.SetActive (false);
		OverLayAllFn.SetActive (false);
	}

	private float roundDown(float number, int precision){
		return (float) (((int)(number * Mathf.Pow (10, precision))) / Mathf.Pow (10, precision));
	}

	private class MultiplayerListener : GooglePlayGames.BasicApi.Multiplayer.RealTimeMultiplayerListener {

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

			if (MultiplayerController.instance.localPlayerNumber == -1) {
				PlayGamesPlatform.Instance.RealTime.SendMessageToAll (true, PayloadWrapper.Build (
					MultiplayerController.REQ_INIT,
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

			MultiplayerController.instance.HideLogAndControllPanel ();

//			foreach (string id in participantIds) {
//				ConsoleLog.SLog (id);
//			}
//
//			foreach (string id in participantIds) {
//				if (MultiplayerController.instance.playerCount >= MultiplayerController.instance.MaxOpponents) {
//					//TODO notify room full
//					return;
//				}
//
//				//send client player number
//				PlayGamesPlatform.Instance.RealTime.SendMessage (true, id, PayloadWrapper.Build (
//					MultiplayerController.RES_INIT,
//					MultiplayerController.instance.playerCount
//				));
//				MultiplayerController.instance.playerCount++;
//			}
		}

		public void OnPeersDisconnected(string[] participantIds){
			ConsoleLog.SLog("OnPeersDisconnected: " + participantIds);
			
		}

		public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data){
			//deserialize data, get position and head's rotation of that sender, and set to it's character.

//			ConsoleLog.SLog("MessageReceived ID: " + senderId);

			BinaryFormatter bf = new BinaryFormatter ();
			PayloadWrapper payloadWrapper = (PayloadWrapper) bf.Deserialize (new MemoryStream (data));
//			ConsoleLog.SLog("time: " + (int) Time.realtimeSinceStartup + " tag: " + payloadWrapper.tag);

			switch (payloadWrapper.tag) {
			case MultiplayerController.REQ_INIT:
				//if this is host
				if (MultiplayerController.instance.localPlayerNumber == 0) {
					
					//send client player number, room capacity for init room, and spawn point
					InitData initData = new InitData (
											MultiplayerController.instance.MaxOpponents + 1,
						                    MultiplayerController.instance.playerCount,
						                    MultiplayerController.instance.spawnPoints [MultiplayerController.instance.playerCount]
					                    );

					PlayGamesPlatform.Instance.RealTime.SendMessage (true, senderId, PayloadWrapper.Build (
						MultiplayerController.RES_INIT,
						initData
					));

					MultiplayerController.instance.playerCount++;
				}

				break;

			case MultiplayerController.RES_INIT:
				InitData resInitData = (InitData) payloadWrapper.payload;

				//init room
				MultiplayerController.instance.InitializeRoomCapacity ((int) resInitData.roomCapacity);

				//set spawn point
				MultiplayerController.instance.localPlayer.transform.position = resInitData.spawnPoint;

				//save assigned player number
				MultiplayerController.instance.localPlayerNumber = resInitData.playerNum;

				break;

			case MultiplayerController.PLAYER_DATA:
				try {
					//if someone who connected to room early broadcast player data before we initialize, ignore it.
					if(MultiplayerController.instance.localPlayerNumber == -1) return;

					//save other player's data and trigger update flag
					PlayerData otherPlayerData = (PlayerData) payloadWrapper.payload;
					MultiplayerController.instance.latestPlayerDatas [otherPlayerData.playerNumber] = otherPlayerData;
					MultiplayerController.instance.hasNewPlayerDatas [otherPlayerData.playerNumber] = true;

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

			case MultiplayerController.REQ_LEAVE_ROOM:
				MultiplayerController.instance.RemovePlayerFromGame ((int)payloadWrapper.payload);
				break;

			case MultiplayerController.INFLICT_DAMAGE:
				MultiplayerController.instance.localGameManager.takeDamage ((float)payloadWrapper.payload);
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
public class InitData {
	public int playerNum;
	public uint roomCapacity;
	private float[] vectorArray = new float[3];

	public InitData (uint roomCapacity, int playerNum, Vector3 spawnPoint){
		this.playerNum = playerNum;
		this.roomCapacity = roomCapacity;
		this.vectorArray[0] = spawnPoint.x;
		this.vectorArray[1] = spawnPoint.y;
		this.vectorArray[2] = spawnPoint.z;
	}

	public Vector3 spawnPoint {
		get { return new Vector3 (this.vectorArray[0], this.vectorArray[1], this.vectorArray[2]); }
	}
}

[Serializable]
public class PlayerData {
	public int playerNumber;
	public float health;
	public int characterType;

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

	public PlayerData(int playerNumber, float health, int charType, Vector3 pos, Vector3 rot, int animState){
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
	}
}
