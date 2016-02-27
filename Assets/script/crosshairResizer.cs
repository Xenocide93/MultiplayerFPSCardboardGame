using UnityEngine;
using System.Collections;

public class crosshairResizer : MonoBehaviour {

	[Range(0f, 100f)]
	public float size;
	public float multiplier;

	private Transform[] crosshairs = new Transform[4];
	private Transform parent;

	// Use this for initialization
	void Start () {
		parent = transform.parent;
		crosshairs[0] = transform.GetChild (0);
		crosshairs[1] = transform.GetChild (1);
		crosshairs[2] = transform.GetChild (2);
		crosshairs[3] = transform.GetChild (3);
	}
	
	// Update is called once per frame
	void Update () {
		expandCrosshair (size);
	}

	public void expandCrosshair(float size){
		for (int i = 0; i < 4; i++) {
			crosshairs [i].position = 
				parent.position - crosshairs [i].up * size * multiplier * parent.parent.localScale.x;
		}
	}
}
