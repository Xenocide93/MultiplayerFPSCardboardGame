using UnityEngine;
using System.Collections;

public class Spin : MonoBehaviour {
	public float spinX = 1f;
	public float spinY = 1f;
	public float spinZ = 1f;

	void Awake(){
		StartCoroutine (RotateObject ());
	}

	private IEnumerator RotateObject(){
		while (true) {
			transform.Rotate (new Vector3 (spinX, spinY, spinZ) * Time.deltaTime);

			yield return null;
		}
	}
}
