using UnityEngine;
using System.Collections;

public class resetTextTarget : MonoBehaviour {
	public GameObject testShootPrefab;

	public void resetTextShootObject(){
		GameObject[] testShootTag = GameObject.FindGameObjectsWithTag ("TestShootTarget");
		foreach (GameObject o in testShootTag) {
			Destroy (o);
		}

		Instantiate (testShootPrefab, new Vector3(7.06f, 0.810892f, 0.57f), new Quaternion(0f, 0f, 0f, 0f));
	}
}
