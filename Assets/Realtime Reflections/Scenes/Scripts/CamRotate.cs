using UnityEngine;
using System.Collections;

public class CamRotate : MonoBehaviour {

	public Transform target;
	public float distance = 3.0f;
	public float speed = 0.5f;


	// Use this for initialization
	void Start () 
	{
	
	}
	
	void LateUpdate () 
	{
		transform.position = new Vector3(	target.position.x + Mathf.Sin(Time.time)*distance, 
											transform.position.y, 
											target.position.z + Mathf.Cos(Time.time)*distance
										);
		
		transform.LookAt(target.position);
	}

}
