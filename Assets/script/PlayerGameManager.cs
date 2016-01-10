using UnityEngine;
using System.Collections;

public class PlayerGameManager : MonoBehaviour {

	//variable
	public int bulletLoadMax = 30;
	public int bulletStoreMax = 210;
	public float health = 100;

	private int bulletLoadCurrent = 30;
	private int bulletStoreCurrent = 210;

	//UI component
	public Transform healthBar;
	public TextMesh bulletText;


	// Use this for initialization
	void Start () {
		bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void fireGun(){
		if (bulletLoadCurrent <= 0) {
			reloadGun ();
		} else {
			bulletLoadCurrent--;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
		}
	}

	void reloadGun(){
		
	}
}
