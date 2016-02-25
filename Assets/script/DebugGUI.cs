using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Nearby;
using UnityEngine.SocialPlatforms;

public class DebugGUI : MonoBehaviour {
	public bool startWithGUI;

	public GazeInputModule gazeModule;
	static private Text debugText;
	private bool showMenu;

	// Use this for initialization
	void Start () {
		DebugGUI.debugText = transform.GetChild (1).GetComponent<Text> ();

		showMenu = !startWithGUI;
		ToggleMenu ();
	}

	public void ToggleMenu(){
		dualLog ("toggle menu");

		showMenu = !showMenu;
		gazeModule.enabled = !showMenu;
		transform.GetChild (2).gameObject.SetActive (showMenu);
	}

	public void Advertise(){
		dualLog ("Advertize");

		Text hostNameText = transform.GetChild (2).GetChild (2).GetChild (1).gameObject.GetComponent<Text>();

		List<string> appIdentifiers = new List<string>();
		appIdentifiers.Add(PlayGamesPlatform.Nearby.GetAppBundleId());
		PlayGamesPlatform.Nearby.StartAdvertising(
			"Host Name",  // User-friendly name
			appIdentifiers,  // App bundle Id for this game
			TimeSpan.FromSeconds(0),// 0 = advertise forever
			(AdvertisingResult result) =>
			{
				dualLog("OnAdvertisingResult: " + result);
			},
			(ConnectionRequest request) =>
			{
				dualLog("Received connection request: " +
					request.RemoteEndpoint.DeviceId + " " +
					request.RemoteEndpoint.EndpointId + " " +
					request.RemoteEndpoint.Name);
			}
		);
	}

	public void Discovery(){
		dualLog ("Discovery");

		NearbyListener listener = new NearbyListener();

		PlayGamesPlatform.Nearby.StartDiscovery(
			PlayGamesPlatform.Nearby.GetServiceId(),
			TimeSpan.FromSeconds(0),
			listener);
	}



	internal class NearbyListener : IDiscoveryListener {
		
		public void OnEndpointFound(EndpointDetails discoveredEndpoint)
		{
			DebugGUI.dualLog("Found Endpoint!");
//			NearbyRoom room = new NearbyRoom(
//				discoveredEndpoint.DeviceId,
//				discoveredEndpoint.EndpointId,
//				discoveredEndpoint.Name);
			
		}

		public void OnEndpointLost(string lostEndpointId)
		{
			DebugGUI.dualLog("Endpoint lost: " + lostEndpointId);
		}
	}

	static void dualLog(string log){
		ConsoleLog.SLog (log);
		debugText.text = log;
	}
}
