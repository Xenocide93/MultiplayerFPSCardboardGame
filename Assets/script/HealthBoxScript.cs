using UnityEngine;
using System.Collections;

public class HealthBoxScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other){
		if (other.GetComponent<PlayerGameManager> () != null) {
			other.GetComponent<PlayerGameManager> ().addHealth (30f);
			Destroy (gameObject);
		}
	}
}
