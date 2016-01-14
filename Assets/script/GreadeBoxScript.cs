using UnityEngine;
using System.Collections;

public class GreadeBoxScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		if (other.GetComponent<PlayerGameManager> () != null) {
			other.GetComponent<PlayerGameManager> ().addStoreGrenade (5);
			Destroy (gameObject);
		}
	}
}
