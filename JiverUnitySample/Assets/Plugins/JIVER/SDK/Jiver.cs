// Jiver Unity SDK 0.9.3

using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections;
using JiverModel;
using SimpleJSON;
using System;

#region JIVER Responder Class
/*
 * Any custom UI classes must extend this responder class to receive events from JIVER.
 */
public abstract class JiverResponder : MonoBehaviour {
	public abstract void OnConnect (Channel channel);

	public abstract void OnError (int errorCode);

	public abstract void OnMessageReceived (Message message);

	public abstract void OnSystemMessageReceived (SystemMessage message);

	public abstract void OnBroadcastMessageReceived (BroadcastMessage message);

	public abstract void OnQueryChannelList (List<Channel> channels);

}
#endregion

public class Jiver : MonoBehaviour {
	private static JiverAdapter instance;
	private static JiverResponder jiverResponder;
	private static GameObject jiverGameObject;
	private static List<Channel> cachedChannelList;

	private static bool connected;

	private static float dummyMessageTimer = -1;
	private static bool dummyChannelListFlag = false;

	public GameObject responderGameObject;

	#region JIVER public methods
	interface JiverAdapter {
		void Init (string appId, string responder);
		void Login (string uuid, string nickname);
		void Join (string channelUrl);
		void Connect (int prevMessageLimit);
		void Disconnect ();
		void QueryChannelList();
		void SendMessage(string message);
		void SendMessageWithData(string message, string data);
	}
	#endregion


	#region JIVER Native Callbacks 
	void _OnConnect(string arg) {
		Debug.Log ("OnConnect: " + arg);
		if (jiverResponder != null) {
			Channel channel = new Channel(arg);
			jiverResponder.OnConnect(channel);
		}
	}
	
	void _OnError(string arg) {
		Debug.Log ("OnError: " + arg);
		if (jiverResponder != null) {
			int errorCode = int.Parse(arg);
			jiverResponder.OnError(errorCode);
		}
	}
	
	void _OnMessageReceived(string arg) {
//		Debug.Log ("OnMessageReceived: " + arg);
		if (jiverResponder != null) {
			Message message = new Message(arg);
			jiverResponder.OnMessageReceived(message);
		}
	}
	
	void _OnSystemMessageReceived(string arg) {
		Debug.Log ("OnSystemMessageReceived: " + arg);
		if (jiverResponder != null) {
			SystemMessage message = new SystemMessage(arg);
			jiverResponder.OnSystemMessageReceived(message);
		}
	}

	void _OnBroadcastMessageReceived(string arg) {
		Debug.Log ("OnBroadcastMessageReceived: " + arg);
		if (jiverResponder != null) {
			BroadcastMessage message = new BroadcastMessage(arg);
			jiverResponder.OnBroadcastMessageReceived(message);
		}
	}
	
	void _OnFileReceived(string arg) {
		// Not Yet Implemented.
		/*
		Debug.Log ("OnFileReceived: " + arg);
		if (jiverResponder != null) {
			FileLink fileLink = new FileLink(arg);
			jiverResponder.OnFileReceived(fileLink);
		}
		*/
	}

	void _OnMessagingStarted(string arg) {
		// Not Yet Implemented.
	}


	void _OnMessagingEnded(string arg) {
		// Not Yet Implemented.
	}

	void _OnReadReceived(string arg) {
		// Not Yet Implemented.
	}

	void _OnTypeStartReceived(string arg) {
		// Not Yet Implemented.
	}

	void _OnTypeEndReceived(string arg) {
		// Not Yet Implemented.
	}

	void _OnMessagesLoaded(string arg) {
		// Not Yet Implemented.
	}

	void _OnQueryChannelList(string arg) {
		Debug.Log ("OnQueryChannelList: " + arg);
		if (jiverResponder != null) {
			List<Channel> channelList = new List<Channel>();
			JSONArray jsonList = JSON.Parse(arg).AsArray;
			for(int i = 0; i < jsonList.Count; i++) {
				channelList.Add(new Channel(jsonList[i]));
			}

			cachedChannelList = channelList;

			jiverResponder.OnQueryChannelList(channelList);
		}
	}
	#endregion
		
	private static JiverAdapter GetInstance() {

		if (instance == null) {
#if UNITY_EDITOR
			Debug.Log("JIVER on Editor.");
			instance = new JiverDummyImpl(); 
			dummyMessageTimer = 0;
#elif UNITY_IOS
			Debug.Log("JIVER on iOS.");
			instance = new JiveriOSImpl();
#elif UNITY_ANDROID
			Debug.Log("JIVER on Android.");
			instance = new JiverAndroidImpl();
#else
			Debug.Log("JIVER on unknown platform.");
			instance = new JiverDummyImpl();
			dummyValue = 0;
#endif
		}

		return instance;
	}

	void Update() {
		if (dummyMessageTimer >= 0) {
			if(Mathf.Abs(dummyMessageTimer - Time.time) >= 0.3f) {
				string msgJson = "MESG{\"channel_id\": \"0\", \"message\": \"Dummy Text on Editor Mode - " + Time.time + "\", \"user\": {\"image\": \"http://url\", \"name\": \"Sender\"}, \"ts\": 1418979273365, \"scrap_id\": \"\"}";
				_OnMessageReceived(msgJson);
				dummyMessageTimer = Time.time;
			}
		}

		if (dummyChannelListFlag) {
			dummyChannelListFlag = false;
			JSONArray channels = new JSONArray();
			JSONClass channel = new JSONClass();

			channel.Add ("id", new JSONData(1));
			channel.Add ("channel_url", new JSONData("app_prefix.channel_url"));
			channel.Add ("name", new JSONData("Sample"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(2));
			channel.Add ("channel_url", new JSONData("app_prefix.Unity3d"));
			channel.Add ("name", new JSONData("Unity3d"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(3));
			channel.Add ("channel_url", new JSONData("app_prefix.Lobby"));
			channel.Add ("name", new JSONData("Lobby"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(4));
			channel.Add ("channel_url", new JSONData("app_prefix.Cocos2d"));
			channel.Add ("name", new JSONData("Cocos2d"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(5));
			channel.Add ("channel_url", new JSONData("app_prefix.GameInsight"));
			channel.Add ("name", new JSONData("GameInsight"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(6));
			channel.Add ("channel_url", new JSONData("app_prefix.iOS"));
			channel.Add ("name", new JSONData("iOS"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(7));
			channel.Add ("channel_url", new JSONData("app_prefix.Android"));
			channel.Add ("name", new JSONData("Android"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(8));
			channel.Add ("channel_url", new JSONData("app_prefix.News"));
			channel.Add ("name", new JSONData("News"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(9));
			channel.Add ("channel_url", new JSONData("app_prefix.Lobby"));
			channel.Add ("name", new JSONData("Lobby"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());

			channel.Add ("id", new JSONData(10));
			channel.Add ("channel_url", new JSONData("app_prefix.iPad"));
			channel.Add ("name", new JSONData("iPad"));
			channel.Add ("cover_img_url", new JSONData("http://localhost/image.jpg"));
			channel.Add ("member_count", new JSONData(999));
			channels.Add(channel.ToString());



			_OnQueryChannelList(channels.ToString());
		}
	}

	void OnEnable() {
		if (connected) {
			Connect (0);
		}

	}

	void OnDisable() {
		if (connected) {
			Disconnect ();
		}
	}

	void Start() {
	}

	void Awake() {
		Debug.Log ("JIVER Awake");
#if UNITY_ANDROID
//		AndroidJNIHelper.debug = true;
#endif
		jiverGameObject = gameObject;

		if (responderGameObject != null) {
			jiverResponder = responderGameObject.GetComponent<JiverResponder>();
		}
	}

	#region Jiver static methods
	public static void Init(string appId) {
		GetInstance().Init (appId, jiverGameObject.name);
	}

	public static void Login(string uuid, string nickname) {
		GetInstance().Login (uuid, nickname);
	}

	public static void Join(string channelUrl) {
		GetInstance().Join (channelUrl);
	}

	private static char[] CHARS_TO_TRIM = {'\n', '\r', };
	public static new void SendMessage(string message) {
		GetInstance ().SendMessage (message.TrimEnd(CHARS_TO_TRIM));
	}

	public static void Connect(int prevMessageLimit) {
		connected = true;
		GetInstance().Connect (prevMessageLimit);
	}
	
	public static void Disconnect() {
		connected = false;
		GetInstance().Disconnect ();
	}

	public static void QueryChannelList(bool usingCache = true) {
		if (usingCache && cachedChannelList != null) {
			if (jiverResponder != null) {
				jiverResponder.OnQueryChannelList(cachedChannelList);
				return;
			}
		}

		GetInstance().QueryChannelList ();
	}
	#endregion




	#region Jiver Adatper Dummy Impl.
	class JiverDummyImpl : JiverAdapter {


		public void Init (string appId, string responder) {
			Debug.Log ("JIVER disabled.");
			Debug.Log ("JIVER runs on Test mode.");
		}
		public void Login (string uuid, string nickname) {
			Debug.Log ("Login: " + uuid + ", " + nickname);
			Debug.Log ("JIVER runs on Test mode.");
		}
		public void Join (string channelUrl) {
			Debug.Log ("Join: " + channelUrl);
			Debug.Log ("JIVER runs on Test mode.");
		}
		public void Connect (int prevMessageLimit) {
			Debug.Log ("Connect...");
			Debug.Log ("JIVER runs on Test mode.");
		}
		public void Disconnect () {
			Debug.Log ("Disconnect...");
			Debug.Log ("JIVER runs on Test mode.");
		}

		public void QueryChannelList ()
		{
			Debug.Log ("QueryChannelList...");
			dummyChannelListFlag = true;

		}

		public void SendMessage (string message) {
			Debug.Log ("SendMessage: " + message);
		}

		public void SendMessageWithData (string message, string data) {
			Debug.Log ("SendMessage: " + message + " with data: " + data);
		}

	}
	#endregion

#if UNITY_ANDROID
	#region Jiver Adapter Android Impl.
	class JiverAndroidImpl : JiverAdapter {
		private AndroidJavaClass jiverClass;
		public void Init(string appId, string responder) {
			jiverClass = new AndroidJavaClass("com.smilefam.jia.Jiver");
			jiverClass.CallStatic ("init", appId);
			jiverClass.CallStatic ("setUnityResponder", responder);
		}

		public void Login(string uuid, string nickname) {
			jiverClass.CallStatic ("login", uuid, nickname);
		}

		public void Join(string channelUrl) {
			jiverClass.CallStatic ("join", channelUrl);
		}

		public void Connect(int prevMessageLimit) {
			jiverClass.CallStatic ("connectForUnity", prevMessageLimit);
		}

		public void Disconnect() {
			jiverClass.CallStatic ("disconnect");
		}

		public void QueryChannelList ()
		{
			jiverClass.CallStatic ("queryChannelListForUnity");
		}

		public void SendMessage (string message) {
			jiverClass.CallStatic ("send", message);	
		}

		public void SendMessageWithData (string message, string data) {
			jiverClass.CallStatic ("sendWithData", message, data);	
		}
	}
	#endregion
#endif


#if UNITY_IOS
	#region Jiver Adapter iOS Impl.
	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_Init (string appId, string responder);

	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_Login (string uuid, string nickname);

	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_Join (string channelUrl);

	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_Connect (int prevMessageLimit);

	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_Disconnect ();

	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_QueryChannelListForUnity ();

	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_Send (string message);

	[DllImport ("__Internal")]
	private static extern void _Jiver_iOS_SendWithData (string message, string data);

	class JiveriOSImpl : JiverAdapter {
		public void Init(string appId, string responder) {
			_Jiver_iOS_Init (appId, responder);
		}
		
		public void Login(string uuid, string nickname) {
			_Jiver_iOS_Login (uuid, nickname);
		}
		
		public void Join(string channelUrl) {
			_Jiver_iOS_Join (channelUrl);
		}
		
		public void Connect(int prevMessageLimit) {
			_Jiver_iOS_Connect (prevMessageLimit);
		}
		
		public void Disconnect() {
			_Jiver_iOS_Disconnect ();
		}
		
		public void QueryChannelList () {
			_Jiver_iOS_QueryChannelListForUnity();
		}
		
		public void SendMessage (string message) {
			_Jiver_iOS_Send (message);
		}

		public void SendMessageWithData (string message, string data) {
			_Jiver_iOS_SendWithData (message, data);
		}
	}
	#endregion
#endif
}

namespace JiverModel {

	public class FileLink {
		private JSONNode node;

		public FileLink(string json) {
			this.node = JSON.Parse (json);
		}

		public string GetSenderName() {
			return node ["user"] ["name"];
		}

		public string GetUrl() {
			return node ["url"];
		}

		public string GetName() {
			return node ["name"];
		}

		public int GetSize() {
			return node ["size"].AsInt;
		}

		public new string GetType() {
			return node ["type"];
		}

		public string GetCustomField() {
			return node ["custom"];
		}
	}

	public class SystemMessage {
		private JSONNode node;

		public SystemMessage(string json) {
			this.node = JSON.Parse (json);
		}

		public string GetMessage() {
			return node ["message"];
		}

		public long GetTimestamp() {
			return (long)node ["ts"].AsFloat;
		}
	}

	public class BroadcastMessage {
		private JSONNode node;
		
		public BroadcastMessage(string json) {
			this.node = JSON.Parse (json);
		}
		
		public string GetMessage() {
			return node ["message"];
		}
		
		public long GetTimestamp() {
			return (long)node ["ts"].AsFloat;
		}
	}


	public class Channel {
		private JSONNode node;

		public Channel(string json) {
			this.node = JSON.Parse (json);
		}

		public long GetId() {
			return (long)node["id"].AsFloat;
		}

		public int GetMemberCount() {
			return node["member_count"].AsInt;
		}

		public string GetUrl() {
			return node["channel_url"];
		}


		public string GetUrlWithoutAppPrefix() {
			string url = GetUrl ();
			string[] tokens = url.Split ('.');
			if (tokens.Length > 1) {
				return tokens[1];
			}

			return url;
		}

		public string GetName() {
			return node["name"];
		}

		public string GetCoverUrl() {
			return node["cover_img_url"];
		}
	}

	public class Message {
		private JSONNode node;
		public Message(string json) {
			this.node = JSON.Parse (json);
		}

		public string GetMessage() {
			return node ["message"];
		}

		public string GetData() {
			return node ["data"];
		}

		public bool IsOpMessage() {
			return node ["is_op_msg"].AsBool;
		}

		public bool IsGuestMessage() {
			return node["is_guest_msg"].AsBool;
		}

		public string GetSenderName() {
			return node ["user"]["name"];
		}

		public long GetTimestamp() {
			return (long)node ["ts"].AsFloat;
		}
	}
}