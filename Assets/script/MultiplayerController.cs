using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MultiplayerController : MonoBehaviour {
	const int MinOpponents = 1, MaxOpponents = 4;
	public const string PLAYER_DATA = "playerData";
	public const string REQ_PLAYER_NUMBER = "reqPlayerNum";
	public const string RES_PLAYER_NUMBER = "resPlayerNum";
	public const string INIT_PLAYER = "initPlayer";

	public static MultiplayerController instance;

	public GameObject otherPlayerDummyPrefab;

	[HideInInspector]
	public int playerCount = 0;
	[HideInInspector]
	public int localPlayerNumber = -1;
	[HideInInspector]
	public bool isHost = false;

	private GameObject localPlayer;
	private PlayerGameManager localGameManager;
	private GameObject[] characterGameObjects = new GameObject[MaxOpponents];
	private PlayerData[] lastestPlayerDatas = new PlayerData[MaxOpponents];
	[HideInInspector]
	public bool[] hasNewPlayerDatas = new bool[MaxOpponents];

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
		localGameManager = localPlayer.GetComponent<PlayerGameManager> ();
	}

	void Update(){
		if (localPlayerNumber >= 0) {
			BroadcastPlayerData ();
		}
		UpdateOtherPlayerCharacter ();
	}

	public void CreateRoomWithInvite(){
		ConsoleLog.SLog("CreateRoomWithInvite");

		localPlayerNumber = 0;
		isHost = true;
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

	public void SendInitPlayerData(){
		PlayerData data = new PlayerData (
			localPlayerNumber,
			localGameManager.health,
			0, //TODO get character type from player game manager script
			localPlayer.transform.position,
			localPlayer.transform.rotation //TODO get rotation from cardboardHead
		);

		bool reliable = true;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliable, PayloadWrapper.Build(INIT_PLAYER, data));
	}

	private void BroadcastPlayerData(){
		PlayerData data = new PlayerData (
			localPlayerNumber,
			localGameManager.health,
			0, //TODO get character type from player game manager script
			localPlayer.transform.position,
			localPlayer.transform.rotation //TODO get rotation from cardboardHead
		);

		bool reliable = true;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliable, PayloadWrapper.Build(PLAYER_DATA, data));
	}

	public GameObject GetCharacter(int otherPlayerNumber){
		if (characterGameObjects [otherPlayerNumber] == null) {
			//TODO instantiate character prefab according to character type
			characterGameObjects[otherPlayerNumber] = Instantiate(otherPlayerDummyPrefab);
		}

		return characterGameObjects [otherPlayerNumber];
	}

	public void UpdateOtherPlayerCharacter(){
		for (int i = 0; i < MaxOpponents; i++) {
			if (!hasNewPlayerDatas [i] || lastestPlayerDatas[i] == null) {
				continue;
			}

			GetCharacter(i).transform.position = lastestPlayerDatas [i].position;
			characterGameObjects [i].transform.rotation = lastestPlayerDatas [i].rotation; //TODO set cardboardHead rotation
			hasNewPlayerDatas [i] = false;
		}
	}

	private class MultiplayerListener : GooglePlayGames.BasicApi.Multiplayer.RealTimeMultiplayerListener {

		private static MultiplayerListener sInstance = new MultiplayerListener();

		public static MultiplayerListener Instance {
			get { return sInstance; }
		}

		public void OnRoomSetupProgress(float percent){
			ConsoleLog.SLog("OnRoomSetupProgress: " + percent);

		}

		public void OnRoomConnected(bool success){
			ConsoleLog.SLog("OnRoomConnected: " + success);

		}

		public void OnLeftRoom(){
			ConsoleLog.SLog("OnLeftRoom");
		
		}

		public void OnParticipantLeft( GooglePlayGames.BasicApi.Multiplayer.Participant leftParticipant){
			ConsoleLog.SLog("OnParticipantLeft: " + leftParticipant.DisplayName);
			
		}

		public void OnPeersConnected(string[] participantIds){
			ConsoleLog.SLog("OnRoomConnected\nID:");
			foreach (string id in participantIds) {
				ConsoleLog.SLog (id);
			}

			foreach (string id in participantIds) {
				if (MultiplayerController.instance.playerCount >= MultiplayerController.MaxOpponents) {
					//TODO notify room full
					return;
				}

				PlayGamesPlatform.Instance.RealTime.SendMessage (true, id, PayloadWrapper.Build (
					MultiplayerController.RES_PLAYER_NUMBER,
					MultiplayerController.instance.playerCount
				));
				MultiplayerController.instance.playerCount++;
			}

		}

		public void OnPeersDisconnected(string[] participantIds){
			ConsoleLog.SLog("OnPeersDisconnected: " + participantIds);
			
		}

		public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data){
			//deserialize data, get position and head's rotation of that sender, and set to it's charactor.

			ConsoleLog.SLog("MessageReceived ID: " + senderId);

			BinaryFormatter bf = new BinaryFormatter ();
			PayloadWrapper payloadWrapper = (PayloadWrapper) bf.Deserialize (new MemoryStream (data));
			ConsoleLog.SLog("time: " + (int) Time.realtimeSinceStartup + " tag: " + payloadWrapper.tag);

			switch (payloadWrapper.tag) {
			case MultiplayerController.REQ_PLAYER_NUMBER:

				break;

			case MultiplayerController.RES_PLAYER_NUMBER:
				MultiplayerController.instance.localPlayerNumber = (int) payloadWrapper.payload;
				break;

			case MultiplayerController.INIT_PLAYER:
				if (MultiplayerController.instance.playerCount >= MultiplayerController.MaxOpponents) {
					//TODO notify room full
					return;
				}

				PlayGamesPlatform.Instance.RealTime.SendMessage (true, senderId, PayloadWrapper.Build (
					MultiplayerController.RES_PLAYER_NUMBER,
					MultiplayerController.instance.playerCount
				));
				MultiplayerController.instance.playerCount++;
				break;

			case MultiplayerController.PLAYER_DATA:
				PlayerData otherPlayerData = (PlayerData)payloadWrapper.payload;
				MultiplayerController.instance.lastestPlayerDatas [otherPlayerData.playerNumber] = otherPlayerData;
				MultiplayerController.instance.hasNewPlayerDatas [otherPlayerData.playerNumber] = true;
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
public class PlayerData {
	public int playerNumber;
	public float health;
	public int characterType;
	public float[] positionArray = new float[3];
	public float[] rotationArray = new float[4];

	public PlayerData(int playerNumber, float health, int charType, Vector3 pos, Quaternion rot){
		this.playerNumber = playerNumber;
		this.health = health;
		this.characterType = charType;
		this.positionArray [0] = pos.x;
		this.positionArray [1] = pos.y;
		this.positionArray [2] = pos.z;
		this.rotationArray [0] = rot.x;
		this.rotationArray [1] = rot.y;
		this.rotationArray [2] = rot.z;
		this.rotationArray [3] = rot.w;
	}

	public Vector3 position {
		get { return new Vector3 (positionArray [0], positionArray [2], positionArray [2]); }
	}

	public Quaternion rotation {
		get { return new Quaternion (rotationArray [0], rotationArray [1], rotationArray [2], rotationArray [3]); }
	}
}
