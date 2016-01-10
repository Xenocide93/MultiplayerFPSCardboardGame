using UnityEngine;
using System.Collections;

public class VisorGismo : MonoBehaviour {

	public float gizmoSize = .75f;
	public Color gizmoColor = Color.cyan;
	public float offsetX = 0.0f;
	public float offsetY = 0.0f;
	public float offsetZ = 0.0f;


	void OnDrawGizmos() {
		Gizmos.color = gizmoColor;
		Gizmos.DrawWireSphere (transform.position + new Vector3 (offsetX, offsetY, offsetZ), gizmoSize);
	}
}
