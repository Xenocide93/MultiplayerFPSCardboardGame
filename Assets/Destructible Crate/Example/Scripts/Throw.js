#pragma strict

var ball : Rigidbody;
var throwPower : float = 10;
 
function Update () {
 
if (Input.GetButtonDown("Fire1")) {
 
var clone : Rigidbody;
 
clone = Instantiate(ball, transform.position, transform.rotation);
 
clone.velocity = transform.TransformDirection(Vector3.forward * throwPower);
 
}
 
}