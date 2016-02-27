using UnityEngine;
using System.Collections;

public class GunProperties : MonoBehaviour {

	public bool isAutomatic = false;
	public bool isTwoHanded = false;
	public float rateOfFire = 0.5f;
	public float reloadTime = 3f;
	public float gunRange = 100f;
	public float firePower = 100f;

	//Accuracy
	public float idleAcc = 10;
	public float idleAimAcc = 3;
	public float walkAcc = 50;
	public float walkAimAcc = 20;
}
