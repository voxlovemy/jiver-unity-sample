using UnityEngine;
using System.Collections;

public class JiverMain : MonoBehaviour {
	void Start () {
		string appId = "A7A2672C-AD11-11E4-8DAA-0A18B21C2D82";
		string userId = SystemInfo.deviceUniqueIdentifier;
		string userName = "Unity-" + userId.Substring (0, 5);
		string channelUrl = "jia_test.Unity3d";

		Jiver.Init (appId);
		Jiver.Login (userId, userName);
		Jiver.Join (channelUrl);
		Jiver.Connect ();
	}
	
	void Update () {
	
	}
}
