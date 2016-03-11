using UnityEngine;
using System.Collections;

public class SlimeBarrel : MonoBehaviour {

	public GameObject[] gunItems;
	public GameObject slime;
	public Mesh[] meshTypes;
	private int alternator;
	private MeshFilter closedBarrels;
	private int hitCount;

	// Use this for initialization
	void Start () {
		hitCount = 0;
		alternator = 3;
		closedBarrels = GetComponent<MeshFilter>();
	}

	// Update is called once per frame
	public void Hited () {
		hitCount++;
		SetBending ();
	}

	void SetSlime() {
		if(!slime.activeSelf) {
			slime.SetActive(true);
		} 
	}

	public void DestroyIt() {
		int rand = Random.Range(0,3);
		GameObject itemBoxesTemp = (GameObject)Instantiate(gunItems[rand], transform.position, Quaternion.identity);
		GetComponent<Rigidbody> ().isKinematic = false;
		Destroy (transform.parent.gameObject,1f);	
	}
		
	void SetBending() {
		GetComponent<AudioSource>().Stop();
		GetComponent<AudioSource>().pitch = Random.Range(0.4f, 0.7f);
		GetComponent<AudioSource>().Play();
		if (hitCount >= 5) {
			int rand = Random.Range(0,3);
			Vector3 newPosition = transform.position;
			newPosition.y += 0.8f;
			GameObject itemBoxesTemp = (GameObject)Instantiate(gunItems[rand], newPosition, Quaternion.identity);
			itemBoxesTemp.transform.Rotate (90,0,0);
			GetComponent<Rigidbody> ().isKinematic = false;
			Destroy (transform.parent.gameObject,1f);	
		} else if (hitCount == 1) {
			closedBarrels.mesh = meshTypes[1];
		} else if (hitCount == 4) {
			Quaternion target = Quaternion.Euler(0, 3f, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime*1f);
			SetSlime ();
		} else {
			Quaternion target = Quaternion.Euler(0, 3f, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime*1f);
		}
	}
}
