using UnityEngine;
using System.Collections;

public class MilitaryBarrel : MonoBehaviour {

	public GameObject[] itemBoxes;
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

	void SetBending() {
		GetComponent<AudioSource>().Stop();
		GetComponent<AudioSource>().pitch = Random.Range(0.4f, 0.7f);
		GetComponent<AudioSource>().Play();
		if (hitCount >= 5) {
			//create random item boxes
			int rand = Random.Range(0,3);
			GameObject itemBoxesTemp = (GameObject)Instantiate(itemBoxes[rand], transform.position, Quaternion.identity);
			if (rand != 2) itemBoxesTemp.transform.Rotate (270,0,0);
			GetComponent<Rigidbody> ().isKinematic = false;
			Destroy (transform.parent.gameObject,1f);	
		} else if (hitCount == 1) {
			closedBarrels.mesh = meshTypes[1];
		} else {
			Quaternion target = Quaternion.Euler(0, 3f, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime*1f);
		}
	}

	public void DestroyIt() {
		int rand = Random.Range(0,3);
		GameObject itemBoxesTemp = (GameObject)Instantiate(itemBoxes[rand], transform.position, Quaternion.identity);
		if (rand != 2) itemBoxesTemp.transform.Rotate (270,0,0);
		GetComponent<Rigidbody> ().isKinematic = false;
		Destroy (transform.parent.gameObject,1f);	
	}
}
