#pragma strict
var closedBarrels : MeshFilter[];
var meshTypes : Mesh[];
var slime : GameObject;
var greenLight : Light;
private var alternator : int;
var greenStrobo = true;

function Start () {
	alternator = 3;
}

function Update () {
	 var randomLightForce = Mathf.PingPong(Time.time/alternator, 0.25);
	 if(randomLightForce > 0.24) {
	 alternator = Random.Range(1,4);
	 }
	 if(greenStrobo == true) {
	 greenLight.intensity = randomLightForce;
	 } else {
	 greenLight.intensity = 0.23;
	 }
}

function OnGUI () {
	GUILayout.BeginArea (Rect (20,20,600,100));
	GUILayout.BeginHorizontal ("box");
		//GUILayout.Label("Scene info:\n4 pixel lights with shadows\n\n2 Shared Materials with Specular and Normal maps.\n");
		if(GUILayout.Button ("SET:\nNORMAL")) {
			SetNormal();
		}
		if(GUILayout.Button ("SET:\nOPENED")) {
			SetOpened();
		}
		if(GUILayout.Button ("SET:\nDEFORMED")) {
			SetBending();
		}
		if(GUILayout.Button ("SET:\nMASHED")) {
			SetFlat();
		}
		
		if(GUILayout.Button ("TOGGLE:\nSLIME")) {
			if(slime.activeSelf == true) {
			slime.SetActive(false);
			} else {
			slime.SetActive(true);
			}
		}
		if(GUILayout.Button ("TOGGLE:\nSTROBE")) {
			if(greenStrobo == true) {
			greenStrobo = false;
			} else {
			greenStrobo = true;
			}
		}
	GUILayout.EndHorizontal ();
	GUILayout.EndArea ();
}


function SetNormal() {
	for (var closedBarrel in closedBarrels) {
		closedBarrel.GetComponent.<AudioSource>().Stop();
		closedBarrel.GetComponent.<AudioSource>().pitch = Random.Range(0.4, 0.7);
		closedBarrel.GetComponent.<AudioSource>().Play();
		closedBarrel.mesh = meshTypes[0];
		//closedBarrel.transform.localEulerAngles.y = Random.Range(0,360);
		yield WaitForSeconds(0.01);
	}
}
function SetOpened() {
	for (var closedBarrel in closedBarrels) {
		closedBarrel.GetComponent.<AudioSource>().Stop();
		closedBarrel.GetComponent.<AudioSource>().pitch = Random.Range(0.4, 0.7);
		closedBarrel.GetComponent.<AudioSource>().Play();
		closedBarrel.mesh = meshTypes[5];
		//closedBarrel.transform.localEulerAngles.y = Random.Range(0,360);
		yield WaitForSeconds(0.01);
	}
}
function SetBending() {
	for (var closedBarrel in closedBarrels) {
		closedBarrel.GetComponent.<AudioSource>().Stop();
		closedBarrel.GetComponent.<AudioSource>().pitch = Random.Range(0.4, 0.7);
		//closedBarrel.audio.volume = Random.Range(0.5, 1.0);
		closedBarrel.GetComponent.<AudioSource>().Play();
		closedBarrel.mesh = meshTypes[Random.Range(1,4)];
		closedBarrel.transform.localEulerAngles.y = Random.Range(0,360);
		yield WaitForSeconds(Random.Range(0.01,0.1));
	}
}
function SetFlat() {
	for (var closedBarrel in closedBarrels) {
		closedBarrel.GetComponent.<AudioSource>().Stop();
		closedBarrel.GetComponent.<AudioSource>().pitch = Random.Range(0.4, 0.7);
		//closedBarrel.audio.volume = Random.Range(0.5, 1.0);
		closedBarrel.GetComponent.<AudioSource>().Play();
		closedBarrel.mesh = meshTypes[4];
		closedBarrel.transform.localEulerAngles.y = Random.Range(0,360);
		yield WaitForSeconds(Random.Range(0.01,0.1));
	}
}
