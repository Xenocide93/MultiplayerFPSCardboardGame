using UnityEngine;
using System.Collections;

public class MultiplayerControllerMapper : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public void LeaveRoom (){
		MultiplayerController.instance.LeaveRoom ();
	}
}
