using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

using GooglePlayGames;
using GooglePlayGames.BasicApi.Nearby;

using NearbyDroids;

public class NearbyDebug : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}

	public void InitNearby(){
		PlayGamesPlatform.InitializeNearby((client) =>
			{
				ConsoleLog.SLog("Nearby connections initialized");
			});
	}

	public void Advertize(){
		ConsoleLog.SLog("Advertise");
		CreateRoom ("Host 111");
	}

	public void Discovery(){
		ConsoleLog.SLog("Discovery");

		NearbyRoom.FindRooms(OnRoomFound);
	}

	public void StopDiscovery(){
		ConsoleLog.SLog("Stop Discovery");
		NearbyRoom.StopRoomDiscovery ();
	}

	internal void CreateRoom(string roomName){
		ConsoleLog.SLog("Create Room");
		NearbyRoom room = NearbyRoom.CreateRoom(roomName);
		room.AutoJoin = false;
		room.AlwaysOpen = true;
		room.WaitForPlayers (OnPlayerFound);
		ConsoleLog.SLog("Waiting for players...");
	}

	internal void OnPlayerFound(NearbyPlayer player, byte[] data)
	{
		ConsoleLog.SLog ("Player found: " + player.Name);
	}

	internal void OnRoomFound(NearbyRoom room, bool available)
	{
		if (available){
			ConsoleLog.SLog("Room found: " + room.Name);
		} else {	
			ConsoleLog.SLog("Room gone: " + room.Name);
		}
	}
}
