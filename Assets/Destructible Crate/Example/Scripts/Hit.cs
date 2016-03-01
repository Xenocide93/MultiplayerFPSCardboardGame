using UnityEngine;
using System.Collections;

public class Hit : MonoBehaviour {

	public GameObject DestroyedObject;

	//test
	private float time;

	void Start() {
		time = 0f;
	}
		
	void Update() {
		time += Time.deltaTime;
		if (time >= 1f) {
			DestroyIt ();
		}
	}
	
	void OnCollisionEnter( Collision collision ) {
		if( collision.impactForceSum.magnitude > 25f) {
			DestroyIt();
		}
	}
	
	void DestroyIt(){
		if(DestroyedObject) {
			GameObject temp = (GameObject)Instantiate(DestroyedObject, transform.position, transform.rotation);
			Destroy (temp,2f);
		}
		Destroy (gameObject);
	}
}