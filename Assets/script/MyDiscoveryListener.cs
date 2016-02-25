using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi.Nearby;

public class MyDiscoveryListener : IDiscoveryListener {

	public void OnEndpointFound(EndpointDetails discoveredEndpoint)
	{
		ConsoleLog.SLog("Found Endpoint: " +
			discoveredEndpoint.DeviceId + " " +
			discoveredEndpoint.EndpointId + " " + 
			discoveredEndpoint.Name);
	}

	public void OnEndpointLost(string lostEndpointId)
	{
		ConsoleLog.SLog("Endpoint lost: " + lostEndpointId);
	}
}
