using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;

using GooglePlayGames;
using GooglePlayGames.BasicApi.Nearby;

public class NearbyDebug2 : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	public void InitNearby(){
		PlayGamesPlatform.InitializeNearby((client) =>
			{
				ConsoleLog.SLog("Nearby connections initialized");
			});
	}

	public void Advertize(){
		ConsoleLog.SLog ("Advertize ...");

		List<string> appIdentifiers = new List<string>();
		appIdentifiers.Add(PlayGamesPlatform.Nearby.GetAppBundleId());
		PlayGamesPlatform.Nearby.StartAdvertising(
			"Host 222",  // User-friendly name
			appIdentifiers,  // App bundle Id for this game
			TimeSpan.FromSeconds(0),// 0 = advertise forever
			(AdvertisingResult result) =>
			{
				Debug.Log("OnAdvertisingResult: " + result);
			},
			(ConnectionRequest request) =>
			{
				Debug.Log("Received connection request: " +
					request.RemoteEndpoint.DeviceId + " " +
					request.RemoteEndpoint.EndpointId + " " +
					request.RemoteEndpoint.Name);
			}
		);
	}



	public void Discovery(){
		ConsoleLog.SLog ("Discovery ...");

		MyDiscoveryListener listener = new MyDiscoveryListener ();

		PlayGamesPlatform.Nearby.StartDiscovery(
			PlayGamesPlatform.Nearby.GetServiceId(),
			TimeSpan.FromSeconds(0),
			listener);
	}

	public void StopDiscovery(){
		ConsoleLog.SLog("Stop Discovery (??? not yet implement)");
	}
}
