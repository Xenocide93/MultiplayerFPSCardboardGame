using UnityEngine;
using System.Collections;

public class Force : MonoBehaviour {

	void OnMouseDown()
	{
		GetComponent<Rigidbody>().AddForce(-transform.forward * 600);
			GetComponent<Rigidbody>().useGravity = true;
	}
}