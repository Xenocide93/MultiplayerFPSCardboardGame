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
	
	//Payload Tag
	public const string PLAYER_DATA = "playerData";
	public const string REQ_INIT = "reqInit";
	public const string RES_INIT = "resInit";
	public const string	REQ_LEAVE_ROOM = "reqLeaveRoom";
	//Character Type Tag
	public const int CHAR_TYPE_PISTOL = 1;
	public const int CHAR_TYPE_RIFLE = 2;
	public const int CHAR_TYPE_SHORTGUN = 3;
	public const int CHAR_TYPE_SNIPER = 4;

	public static MultiplayerController instance;


	public GameObject otherPlayerDummyPrefab;

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
	private GameObject[] characterGameObjects;
	private PlayerData[] latestPlayerDatas;
	public Vector3[] spawnPoints;
	[HideInInspector]
	public bool[] hasNewPlayerDatas;
	private bool[] updatedLastFrame;

	public float lerpTime;

	public float timeBetweenBroadcast = 1f;
	private float broadcastTimer = 0f;


	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	void Start(){
		localPlayer = GameObject.FindGameObjectWithTag ("Player");
		cardboardHead = GameObject.FindGameObjectWithTag ("PlayerHead");
		localGameManager = localPlayer.GetComponent<PlayerGameManager> ();
	}

	void Update(){
		if (PlayGamesPlatform.Instance.RealTime.IsRoomConnected() && localPlayerNumber != -1) {
			for (int i = 0; i < updatedLastFrame.Length; i++) { updatedLastFrame[i] = false; }
			BroadcastPlayerData ();
			UpdateOtherPlayerCharacter ();
		}
		PrintLastestPlayerData ();
	}

	public void InitializeRoomCapacity(int roomCapacity){
		ConsoleLog.SLog("InitializeRoomCapacity");

		MaxOpponents = (uint) roomCapacity - 1;
		characterGameObjects = new GameObject[MaxOpponents + 1];
		latestPlayerDatas = new PlayerData[MaxOpponents + 1];
		hasNewPlayerDatas = new bool[MaxOpponents + 1];
		updatedLastFrame = new bool[MaxOpponents + 1];
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

	private void BroadcastPlayerData(){
		if (broadcastTimer < timeBetweenBroadcast) {
			broadcastTimer += Time.deltaTime;
			return;
		} else {
			broadcastTimer = 0f;
		}

		PlayerData data = new PlayerData (
			localPlayerNumber,
			localGameManager.health,
			0, //TODO get character type from player game manager script
			localPlayer.transform.position,
			new Vector3( cardboardHead.transform.rotation.eulerAngles.x, localPlayer.transform.rotation.eulerAngles.y, cardboardHead.transform.rotation.eulerAngles.z)
		);

		bool reliable = false;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliable, PayloadWrapper.Build(PLAYER_DATA, data));
	}

	public GameObject GetCharacter(int otherPlayerNumber){
		if (characterGameObjects [otherPlayerNumber] == null) {

			//debug mega log
			for (int i = 0; i < 20; i++) {
				ConsoleLog.SLog ("---------------------- instantiate character player [" + otherPlayerNumber + "] ----------------------");
			}

			//TODO instantiate character prefab according to character type
			characterGameObjects[otherPlayerNumber] = Instantiate (
				otherPlayerDummyPrefab, 
				latestPlayerDatas[otherPlayerNumber].position,
				latestPlayerDatas[otherPlayerNumber].rotation
			) as GameObject;
		}

		return characterGameObjects [otherPlayerNumber];
	}

	public void UpdateOtherPlayerCharacter(){
		try {
			Transform otherChar;

			for (int i = 0; i < MaxOpponents+1; i++) {
				if (!hasNewPlayerDatas [i] || latestPlayerDatas[i] == null || i == localPlayerNumber) {
					continue;
				}

				otherChar = GetCharacter(i).transform;

				otherChar.position = Vector3.Lerp(otherChar.position, latestPlayerDatas[i].position, lerpTime);
				otherChar.rotation = Quaternion.Slerp(otherChar.rotation, latestPlayerDatas [i].rotation, lerpTime); //TODO set cardboardHead rotation
				hasNewPlayerDatas [i] = false;
				updatedLastFrame[i] = true;
			}
		} catch (Exception e){
			ConsoleLog.SLog ("Error in UpdateOtherPlayerCharacter");
			ConsoleLog.SLog (e.Message);
		}
	}

	private void PrintLastestPlayerData(){
		try {
			latestPlayerDataText.text = "localPlayerNumber: " + localPlayerNumber + "  MaxPlayer: " + (MaxOpponents + 1) + "\n";

			latestPlayerDataText.text += "updatedLastFrame: ";
			for (int i = 0; i < MaxOpponents + 1; i++){ latestPlayerDataText.text += (updatedLastFrame[i] ? "1 " : "0 ");}
			latestPlayerDataText.text += "\n";

			latestPlayerDataText.text += "characterGameObjects: ";
			for (int i = 0; i < MaxOpponents + 1; i++){ latestPlayerDataText.text += (characterGameObjects[i] == null ? "X " : "/ ");}
			latestPlayerDataText.text += "\n";

//			latestPlayerDataText.text += "\n + Payload Data +\n";
//			for (int i = 0; i < MaxOpponents + 1; i++) {
//				if (latestPlayerDatas [i] == null || i == localPlayerNumber) continue;
//
//				latestPlayerDataText.text += "[" + i + "] ";
//				latestPlayerDataText.text += "Pos: " + 
//					roundDown(latestPlayerDatas[i].position.x, 1) + ", " + 
//					roundDown(latestPlayerDatas[i].position.y, 1) + ", " + 
//					roundDown(latestPlayerDatas[i].position.z, 1) + " ";
//				latestPlayerDataText.text += "Rot: " +
//					roundDown(latestPlayerDatas [i].rotation.eulerAngles.x, 1) + ", " +
//					roundDown(latestPlayerDatas [i].rotation.eulerAngles.y, 1) + ", " +
//					roundDown(latestPlayerDatas [i].rotation.eulerAngles.z, 1) + "\n";
//			}

//			latestPlayerDataText.text += "\n + Local Data +\n";
//			for (int i = 0; i < MaxOpponents + 1; i++) {
//				if (latestPlayerDatas [i] == null || i == localPlayerNumber) continue;
//
//				latestPlayerDataText.text += "[" + i + "] ";
//				latestPlayerDataText.text += "Pos: " + 
//					roundDown(characterGameObjects[i].transform.position.x, 1) + ", " + 
//					roundDown(characterGameObjects[i].transform.position.y, 1) + ", " + 
//					roundDown(characterGameObjects[i].transform.position.z, 1) + " ";
//				latestPlayerDataText.text += "Rot: " +
//					roundDown(characterGameObjects[i].transform.rotation.eulerAngles.x, 1) + ", " +
//					roundDown(characterGameObjects[i].transform.rotation.eulerAngles.y, 1) + ", " +
//					roundDown(characterGameObjects[i].transform.rotation.eulerAngles.z, 1) + "\n";
//			}

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

			ConsoleLog.SLog("MessageReceived ID: " + senderId);

			BinaryFormatter bf = new BinaryFormatter ();
			PayloadWrapper payloadWrapper = (PayloadWrapper) bf.Deserialize (new MemoryStream (data));
			ConsoleLog.SLog("time: " + (int) Time.realtimeSinceStartup + " tag: " + payloadWrapper.tag);

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
					PlayerData otherPlayerData = (PlayerData)payloadWrapper.payload;
					MultiplayerController.instance.latestPlayerDatas [otherPlayerData.playerNumber] = otherPlayerData;
					MultiplayerController.instance.hasNewPlayerDatas [otherPlayerData.playerNumber] = true;

					ConsoleLog.SLog ("Received Player Number: " + otherPlayerData.playerNumber);
				} catch (Exception e) {
					ConsoleLog.SLog ("something wrong during recieving player data");
					ConsoleLog.SLog (e.Message);
				}

				break;

			case MultiplayerController.REQ_LEAVE_ROOM:
				MultiplayerController.instance.RemovePlayerFromGame ((int)payloadWrapper.payload);
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
		ConsoleLog.SLog ("Send Payload");
		ConsoleLog.SLog("time: " + (int) Time.realtimeSinceStartup + " tag: " + tag);
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
	private float[] positionArray = new float[3];
	private float[] rotationArray = new float[3];

	public PlayerData(int playerNumber, float health, int charType, Vector3 pos, Vector3 rot){
		this.playerNumber = playerNumber;
		this.health = health;
		this.characterType = charType;
		this.positionArray [0] = pos.x;
		this.positionArray [1] = pos.y;
		this.positionArray [2] = pos.z;
		this.rotationArray [0] = rot.x;
		this.rotationArray [1] = rot.y;
		this.rotationArray [2] = rot.z;
	}

	public Vector3 position {
		get { return new Vector3 (positionArray [0], positionArray [1], positionArray [2]); }
	}

	public Quaternion rotation {
		get { return Quaternion.Euler (new Vector3(rotationArray [0], rotationArray [1], rotationArray [2])); }
	}
}
