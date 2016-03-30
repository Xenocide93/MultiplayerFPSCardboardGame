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
		MultiplayerController.instance.CreateRoomDeathmatch (roomCapacity);
	}

	public void SendReady(){
		MultiplayerController.instance.SendReady ();
	}

	public void LeaveRoom (){
		MultiplayerController.instance.LeaveRoom ();
	}
}
