using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class MultiplayerController : MonoBehaviour {

	public static MultiplayerController instance;

	// Use this for initialization
	void Awake () {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
		} else if (instance != this) {
			Destroy (gameObject);
		}
	}

	public void CreateRoomWithInvite(){
		ConsoleLog.SLog("CreateRoomWithInvite");

		const int MinOpponents = 1, MaxOpponents = 4;
		const int GameVariant = 0;
		PlayGamesPlatform.Instance.RealTime.CreateWithInvitationScreen(
			MinOpponents, MaxOpponents, GameVariant, MultiplayerListener.Instance
		);
	}

	public void ShowInvite(){
		ConsoleLog.SLog("ShowInvite");

		PlayGamesPlatform.Instance.RealTime.AcceptFromInbox(MultiplayerListener.Instance);
	}

	public void SendMessageToAll(string msg){
		BinaryFormatter bf = new BinaryFormatter();
		MemoryStream ms = new MemoryStream();
		bf.Serialize(ms, msg);
		byte[] myByteArray = ms.ToArray();
		byte[] message = ms.ToArray();
		bool reliable = true;
		PlayGamesPlatform.Instance.RealTime.SendMessageToAll(reliable, message);
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

			MultiplayerController.instance.SendMessageToAll ("HI ALL!");
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

			MultiplayerController.instance.SendMessageToAll ("WELCOME ALL!");

		}

		public void OnPeersDisconnected(string[] participantIds){
			ConsoleLog.SLog("OnPeersConnected: " + participantIds);
			
		}

		public void OnRealTimeMessageReceived(bool isReliable, string senderId, byte[] data){
			ConsoleLog.SLog("OnRealTimeMessageReceived");
			ConsoleLog.SLog("senderId: " + senderId);

			BinaryFormatter bf = new BinaryFormatter ();
			string msg = (string) bf.Deserialize (new MemoryStream (data));
			ConsoleLog.SLog ("Message: " + msg);
		}
	}
}
