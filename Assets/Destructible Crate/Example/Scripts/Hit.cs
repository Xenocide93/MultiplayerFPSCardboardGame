using UnityEngine;
using System.Collections;

public class Hit : MonoBehaviour {

	public GameObject DestroyedObject;


	
	void OnCollisionEnter( Collision collision ) {
		if( collision.impactForceSum.magnitude > 25f) {
		DestroyIt();
		}
	}
	
		void DestroyIt(){
		if(DestroyedObject) {
			Instantiate(DestroyedObject, transform.position, transform.rotation);
		}
		Destroy(gameObject);

	}
}