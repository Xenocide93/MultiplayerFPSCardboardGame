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
	public const string FIRE_RAY = "fireRay";
	public const string HAND_GRENADE = "handGrenade";

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

	private Animator localAnimator;
	[HideInInspector]
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
		if (localPlayerNumber != -1) {
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
		ConsoleLog.SLog ("Character Type: " + charType);
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

	public void SetBroadcast (bool b){
		isBroadcast = b;
	}

	public void SetLocalAnimationState(int state){
		localAnimationState = state;
	}

	public void SendDamage (int remotePlayerNum, float damage){
		PlayGamesPlatform.Instance.RealTime.SendMessage (true, GetClientId (remotePlayerNum), PayloadWrapper.Build (
			INFLICT_DAMAGE,
			damage
		));
	}

	public void SendFireRay (Ray fireRay) {
		if (localPlayerNumber == -1) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			false,
			PayloadWrapper.Build (
				FIRE_RAY,
				new FireRayData (fireRay, localPlayerNumber)
			)
		);
	}

	public void SendHandGrenade (Vector3 position, Vector3 rotation, Vector3 force) {
		if (localPlayerNumber == -1) return;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll (
			true,
			PayloadWrapper.Build (
				HAND_GRENADE,
				new GrenadeData (localPlayerNumber, position, rotation, force)
			)
		);
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
			localAnimationState,
			localGameManager.isInAimMode
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
				ConsoleLog.SLog("Send REQ_INIT");
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

			case MultiplayerController.FIRE_RAY:
				FireRayData rayData = (FireRayData) payloadWrapper.payload;
				MultiplayerController.instance.characterGameObjects [rayData.playerNum]
					.GetComponent<RemoteCharacterController> ().FireGun (rayData.fireRay);
				break;

			case MultiplayerController.HAND_GRENADE:
				GrenadeData grenadeData = (GrenadeData)payloadWrapper.payload;
				MultiplayerController.instance.characterGameObjects [grenadeData.playerNum]
					.GetComponent<RemoteCharacterController> ().ThrowGrenade ( grenadeData.position, grenadeData.rotation, grenadeData.force);
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

	public PlayerData(int playerNumber, float health, int charType, Vector3 pos, Vector3 rot, int animState, bool isAim){
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
	}
}
