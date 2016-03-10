using UnityEngine;
using System.Collections;

public class Hit : MonoBehaviour {

	public GameObject DestroyedObject;
	private int hitCount;

	void Start() {
		hitCount = 0;
	}
		
	public void Hited() {
		hitCount++;
		if (hitCount >= 3) {
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