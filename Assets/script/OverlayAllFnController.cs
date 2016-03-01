using UnityEngine;
using System.Collections;

public class OverlayAllFnController : MonoBehaviour {

	public bool show;
	public GazeInputModule gazeModule;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < transform.childCount; i++) {
			if (transform.GetChild (i).name.Equals ("BG")) continue;
			if (transform.GetChild (i).name.Equals ("GPG")) continue;
			transform.GetChild (i).gameObject.SetActive (show);
		}
		gazeModule.enabled = !show;
	}
}
