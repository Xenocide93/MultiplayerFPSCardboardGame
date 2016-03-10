using UnityEngine;
using System.Collections;

public class SlimeBarrel : MonoBehaviour {
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

	void SetBending() {
		GetComponent<AudioSource>().Stop();
		GetComponent<AudioSource>().pitch = Random.Range(0.4f, 0.7f);
		GetComponent<AudioSource>().Play();
		if (hitCount >= 5) {
			Destroy (transform.parent.gameObject);	
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
