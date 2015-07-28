using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using JiverModel;
using UnityEngine.UI;

public class JiverUI : JiverResponder {
	public GameObject channelButtonPrefab;
	public GameObject uiThemePrefab;

	enum TAB_MODE {CHANNEL, CLAN};
	TAB_MODE tabMode;

	string selectedChannelUrl = "";
	float lastTextPositionY;
	bool autoScroll = true;

	ArrayList btnChannels = new ArrayList();



	JiverTheme uiTheme;

	GameObject mainPanel;

	GameObject uiPanel;

	Text txtTitle;
	Text txtContent;
	Scrollbar scrollbar;
	InputField inputMessage;
	Button btnSend;
	Button btnChannel;
	Button btnClan;
	Button btnMainClose;

	GameObject channelPanel;
	Button btnChannelClose;

	GameObject gridPannel;


	#region implemented abstract members of JiverResponder
	public override void OnConnect (JiverModel.Channel channel)
	{
		Debug.Log ("Connect to " + channel.GetName ());
		txtTitle.text = "#" +  channel.GetUrlWithoutAppPrefix();
		selectedChannelUrl = channel.GetUrl ();

	}
	public override void OnError (int errorCode)
	{
		Debug.Log ("JIVER Error: " + errorCode);
	}
	public override void OnMessageReceived (JiverModel.Message message)
	{
		TrimContent ();
		txtContent.text = txtContent.text + (MessageRichText(message) + "\n");
	}
	public override void OnSystemMessageReceived (JiverModel.SystemMessage message)
	{
		TrimContent ();
		txtContent.text = txtContent.text + (SystemMessageRichText(message.GetMessage()) + "\n");
	}

	public override void OnBroadcastMessageReceived (JiverModel.BroadcastMessage message)
	{
		TrimContent ();
		txtContent.text = txtContent.text + (SystemMessageRichText(message.GetMessage()) + "\n");
	}

	private void TrimContent() {

	}

	public override void OnQueryChannelList (List<Channel> channels)
	{
		foreach (Object btnChannel in btnChannels) {
			GameObject.Destroy(btnChannel);
		}
		btnChannels.Clear ();

		foreach (Channel channel in channels) {
			GameObject btnChannel = Instantiate (channelButtonPrefab) as GameObject;
			btnChannel.GetComponent<Image>().sprite = uiTheme.channelButtonOff;

			if(channel.GetUrl() == selectedChannelUrl) {
				btnChannel.GetComponent<Image>().overrideSprite = uiTheme.channelButtonOn;
				btnChannel.GetComponentInChildren<Text>().color = uiTheme.channelButtonOnColor;
			} else {
				btnChannel.GetComponent<Image>().overrideSprite = null;
				btnChannel.GetComponentInChildren<Text>().color = uiTheme.channelButtonOffColor;
			}

			Text text = btnChannel.GetComponentInChildren<Text> ();
			text.text = "#" + channel.GetUrlWithoutAppPrefix ();
			btnChannel.transform.SetParent(gridPannel.transform);
			btnChannel.transform.localScale = Vector3.one;
			btnChannels.Add (btnChannel);

			Channel channelFinal = channel;
			btnChannel.GetComponent<Button>().onClick.AddListener(() => {
				Connect(channelFinal.GetUrl());
				channelPanel.SetActive(false);
				SelectTab(TAB_MODE.CHANNEL);
			});
		}
	}
	#endregion

	void Awake() {
	}

	void FixedUpdate() {
		if (autoScroll) {
			ScrollToBottom ();
		}
	}

	void Start() {
		InitComponents ();
//		OpenChannelList ();
		Connect ("jia_test.Unity3d");
		SelectTab (TAB_MODE.CHANNEL);
	}


	void Connect (string channelUrl)
	{
		Debug.Log ("Connect to " + channelUrl);
		ResetContent ();
		Jiver.Join (channelUrl);
		Jiver.Connect ();
	}

	void ResetContent() {
		txtContent.text = "";
		lastTextPositionY = 0;
		autoScroll = true;
	}

	void OpenChannelList ()
	{
		channelPanel.SetActive (true);
		Jiver.QueryChannelList ();
	}

	void SelectTab(TAB_MODE tab)
	{
		tabMode = tab;
		if (tabMode == TAB_MODE.CHANNEL) {
			btnChannel.GetComponent<Image>().overrideSprite = uiTheme.chatChannelButtonOn;
			btnChannel.GetComponentInChildren<Text>().color = uiTheme.chatChannelButtonOnColor;

			btnClan.GetComponent<Image>().overrideSprite = null;
			btnClan.GetComponentInChildren<Text>().color = uiTheme.chatChannelButtonOffColor;
		} else {
			btnChannel.GetComponent<Image>().overrideSprite = null;
			btnChannel.GetComponentInChildren<Text>().color = uiTheme.chatChannelButtonOffColor;

			btnClan.GetComponent<Image>().overrideSprite = uiTheme.chatChannelButtonOn;
			btnClan.GetComponentInChildren<Text>().color = uiTheme.chatChannelButtonOnColor;
		}
	}




	void InitComponents ()
	{



		uiPanel = GameObject.Find ("JIVERUI/UIPanel");
		(Instantiate (uiThemePrefab) as GameObject).transform.parent = uiPanel.transform;

		uiTheme = GameObject.FindObjectOfType (typeof(JiverTheme)) as JiverTheme;
		mainPanel = GameObject.Find ("JIVERUI/UIPanel/MainPanel");
		mainPanel.GetComponent<Image> ().sprite = uiTheme.chatFrameBG;

		channelPanel = GameObject.Find ("JIVERUI/UIPanel/ChannelPanel");
		channelPanel.GetComponent<Image> ().sprite = uiTheme.channelListFrameBG;

		gridPannel = GameObject.Find ("JIVERUI/UIPanel/ChannelPanel/ScrollArea/GridPanel");
	
		txtContent = GameObject.Find("JIVERUI/UIPanel/MainPanel/ScrollArea/TxtContent").GetComponent<Text>();// (Text);
		txtContent.color = uiTheme.messageColor;

		txtTitle = GameObject.Find ("JIVERUI/UIPanel/MainPanel/TxtTitle").GetComponent<Text> ();
		txtTitle.color = uiTheme.titleColor;



		scrollbar = GameObject.Find ("JIVERUI/UIPanel/MainPanel/Scrollbar").GetComponent<Scrollbar>();
		ColorBlock cb = scrollbar.colors;
		cb.normalColor = uiTheme.scrollBarColor;
		cb.pressedColor = uiTheme.scrollBarColor;
		cb.highlightedColor = uiTheme.scrollBarColor;
		scrollbar.colors = cb;
		scrollbar.onValueChanged.AddListener ((float value) => {
			if(value <= 0) {
				autoScroll = true;
				lastTextPositionY = txtContent.transform.position.y;
				return;
			}

			if(lastTextPositionY - txtContent.transform.position.y >= 100) {
				autoScroll = false;
			}

			lastTextPositionY = txtContent.transform.position.y;
		});

		inputMessage = GameObject.Find ("JIVERUI/UIPanel/MainPanel/InputMessage").GetComponent<InputField> ();
		inputMessage.GetComponent<Image> ().sprite = uiTheme.inputTextBG;
		inputMessage.onEndEdit.AddListener ((string msg) => {
			Submit();
		});

		GameObject.Find ("JIVERUI/UIPanel/MainPanel/InputMessage/Placeholder").GetComponent<Text> ().color = uiTheme.inputTextPlaceholderColor;
		GameObject.Find ("JIVERUI/UIPanel/MainPanel/InputMessage/Text").GetComponent<Text> ().color = uiTheme.inputTextColor;

		btnSend = GameObject.Find ("JIVERUI/UIPanel/MainPanel/BtnSend").GetComponent<Button> ();
		btnSend.GetComponent<Image> ().sprite = uiTheme.sendButton;
		btnSend.GetComponentInChildren<Text> ().color = uiTheme.sendButtonColor;
		btnSend.onClick.AddListener (() => {
			Submit();
		});


		btnClan = GameObject.Find ("JIVERUI/UIPanel/MainPanel/BtnClan").GetComponent<Button> ();
		btnClan.GetComponent<Image> ().sprite = uiTheme.chatChannelButtonOff;
		btnClan.onClick.AddListener (() => {
			Connect ("jia_test.Clan");
			SelectTab(TAB_MODE.CLAN);
		});


		btnMainClose = GameObject.Find ("JIVERUI/UIPanel/MainPanel/BtnClose").GetComponent<Button> ();
		btnMainClose.GetComponent<Image> ().sprite = uiTheme.closeButton;
		btnMainClose.onClick.AddListener (() => {
			uiPanel.SetActive(false);
		});

		GameObject.Find ("JIVERUI/UIPanel/ChannelPanel/TxtTitle").GetComponent<Text> ().color = uiTheme.titleColor;
		
		Scrollbar channelScrollbar = GameObject.Find ("JIVERUI/UIPanel/ChannelPanel/Scrollbar").GetComponent<Scrollbar>();
		cb = channelScrollbar.colors;
		cb.normalColor = uiTheme.scrollBarColor;
		cb.pressedColor = uiTheme.scrollBarColor;
		cb.highlightedColor = uiTheme.scrollBarColor;
		channelScrollbar.colors = cb;

		btnChannel = GameObject.Find ("JIVERUI/UIPanel/MainPanel/BtnChannel").GetComponent<Button> ();
		btnChannel.GetComponent<Image> ().sprite = uiTheme.chatChannelButtonOff;
		btnChannel.onClick.AddListener (() => {
			OpenChannelList();
			
		});


		btnChannelClose = GameObject.Find ("JIVERUI/UIPanel/ChannelPanel/BtnChannelClose").GetComponent<Button> ();
		btnChannelClose.GetComponent<Image> ().sprite = uiTheme.closeButton;
		btnChannelClose.onClick.AddListener (() => {
			channelPanel.SetActive(false);
		});

		uiPanel.SetActive (true);
		mainPanel.SetActive (true);
		channelPanel.SetActive (false);
	}

	string MessageRichText(Message message)
	{
		return "<color=#" + JiverTheme.ToHex(uiTheme.senderColor) + ">" + message.GetSenderName() + ": </color>" + message.GetMessage();
	}

	string SystemMessageRichText(string message)
	{
		return "<color=#" + JiverTheme.ToHex(uiTheme.systemMessageColor) + ">" + message + "</color>";
	}

	void ScrollToBottom()
	{
		scrollbar.value = 0;
	}

	void Submit() {
		if (inputMessage.text.Length > 0) {
			Jiver.SendMessage(inputMessage.text);
			inputMessage.text = "";
			ScrollToBottom();
		}
	}
}
