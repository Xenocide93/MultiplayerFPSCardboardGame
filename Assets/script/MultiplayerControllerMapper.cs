using UnityEngine;
using System.Collections;

public class MultiplayerControllerMapper : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public void SelectMap(int map) {
		MultiplayerController.instance.SelectMap (map);
		RoomSetupUiController.instance.SetUiState (RoomSetupUiController.STATE_SELECT_MODE);
	}

	public void ShowInvite() {
		MultiplayerController.instance.ShowInvite ();
	}

	public void CreateRoomDeathmatch(int roomCapacity) {
		MultiplayerController.instance.CreateRoomWithInvite (roomCapacity, MultiplayerController.GAMEMODE_DEATHMATCH);
	}

	public void CreateRoomTeam(int roomCapacity) {
		MultiplayerController.instance.CreateRoomWithInvite (roomCapacity, MultiplayerController.GAMEMODE_TEAM);
	}

	public void SwitchTeam (){
		if (MultiplayerController.instance.playersTeamNumber[MultiplayerController.instance.localRoomPlayerNumber] == 1) {
			MultiplayerController.instance.SendSwtichTeamReq (2);
		} else if (MultiplayerController.instance.playersTeamNumber[MultiplayerController.instance.localRoomPlayerNumber] == 2) {
			MultiplayerController.instance.SendSwtichTeamReq (1);
		}
	}

	public void SendFinishSelectTeam () {
		MultiplayerController.instance.SendFinishSelectTeam ();
	}

	public void SendReady(){
		MultiplayerController.instance.SendReady ();
	}

	public void LeaveRoom (){
		MultiplayerController.instance.LeaveRoom ();
	}
}
