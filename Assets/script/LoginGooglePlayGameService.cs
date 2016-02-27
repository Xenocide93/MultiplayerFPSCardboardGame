using UnityEngine;
using System.Collections;

public class LoginGooglePlayGameService : MonoBehaviour {
	public void login(){
		Social.localUser.Authenticate((bool success) => {
			if(success){
				ConsoleLog.SLog("login Success");
			} else {
				ConsoleLog.SLog("login Fail");
			}
		});
	}
}
