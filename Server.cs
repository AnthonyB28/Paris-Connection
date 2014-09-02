using UnityEngine;
using System.Collections;

public class Server : MonoBehaviour {

	private string m_ServerName = "MyServer";
	private string m_PlayerName = "Meeple";
	public string m_ServerIP = "127.0.0.1";
	public int m_ServerPort = 23466;
	public int m_NATPort = 50005;
	public int m_GamePort = 25005;
	private string m_Chat = "";
	private bool m_ServerInit; //Waiting for players
	private bool m_Connected; //Turn off lobby pick GUI
	private bool m_GameHasBegun;
	private string m_TextBox = "";
	private int m_PlayerCount;
	private bool m_NeedToRefresh;
	
	private bool m_LoadingList = false;
	private Vector2 m_ScrollViewBarPos = Vector2.zero;

	public AudioClip m_ChatNotification;

	void Awake()
	{
		MasterServer.ipAddress = m_ServerIP;
		MasterServer.port = m_ServerPort;
		Network.natFacilitatorIP = m_ServerIP;
		Network.natFacilitatorPort = m_NATPort;
	}

	// Use this for initialization
	void Start () {
		RefreshHostList();
	}

	void RefreshHostList()
	{
		m_LoadingList = true;
		MasterServer.ClearHostList();
		MasterServer.RequestHostList("ParisConnection");
	}

	void OnMasterServerEvent(MasterServerEvent msevent)
	{
		if(msevent == MasterServerEvent.HostListReceived)
		{
			m_LoadingList = false;
		}
	}

	void OnGUI()
	{
		if(!m_Connected)
		{
			if(!m_GameHasBegun)
			{
				if( GUILayout.Button ("Local Game"))
				{
					Application.LoadLevel (1);
				}
				if( GUILayout.Button ("Refresh"))
				{
					RefreshHostList();
				}
				
				GUILayout.Label ("Your Player Name:");
				m_PlayerName = GUILayout.TextField (m_PlayerName, GUILayout.Width (200f));
				
				if(m_LoadingList)
				{
					GUILayout.Label ("Loading...");
				}
				else
				{
					GUILayout.Label ("Available Servers:");
					m_ScrollViewBarPos = GUILayout.BeginScrollView( m_ScrollViewBarPos, GUILayout.Width (200f), GUILayout.Height (200f));
					HostData[] hosts = MasterServer.PollHostList();
					for(int i =0; i < hosts.Length; ++i)
					{
						if(hosts[i].comment != "full")
						{
							if(GUILayout.Button (hosts[i].gameName, GUILayout.ExpandWidth(true)))
							{
								Network.Connect(hosts[i]);
							}
						}
					}
					
					if( hosts.Length ==0)
					{
						GUILayout.Label ("No servers running!");
					}
					
					GUILayout.EndScrollView();
				}
				
				GUILayout.Label ("Server Name");
				m_ServerName = GUILayout.TextField (m_ServerName, GUILayout.Width (200f));
				if (GUILayout.Button ("Host", GUILayout.Width (100f))) {
					Network.InitializeServer (5, m_GamePort, true);
				}
			}
			else
			{
				//We've lost connection mid game for X reason
				GUILayout.Label ("The server has disconnected or a player has left mid game.\n" +
				           "Please refresh the web page to reload the game.");	
			}
		}
		else
		{
			if(m_GameHasBegun)
			{
				GUILayout.BeginArea(new Rect(GameGUI.instance.m_MessageBoxXPos-220, Screen.height-230, Screen.width, Screen.height));
			}
			m_ScrollViewBarPos = GUILayout.BeginScrollView( m_ScrollViewBarPos, GUILayout.Width (200f), GUILayout.Height (100f));
			GUILayout.Label(m_Chat, GUILayout.ExpandWidth(true));
			GUILayout.EndScrollView();
			GUILayout.Label ("Players connected: " + m_PlayerCount.ToString());
			GUILayout.Label ("Chat:");
			m_TextBox = GUILayout.TextField (m_TextBox, GUILayout.Width (200f));
			if (GUILayout.Button ("Send", GUILayout.Width (200f))) {
				string msg = m_PlayerName + ": " + m_TextBox;
				networkView.RPC("NetChat", RPCMode.AllBuffered, msg);
				m_TextBox = "";
			}

			if(m_GameHasBegun)
			{
				GUILayout.EndArea();
			}
			else
			{
				if(m_ServerInit)
				{
					if(m_PlayerCount > 2)
					{
						if (GUILayout.Button ("Start Game", GUILayout.Width (100f))) {
							HostStartGame();
						}
					}
					else
					{
						GUILayout.Label ("Waiting for 2 or more additional players...");
					}
				}
			}
		}
	}

	[RPC]
	void NetChat(string message)
	{
		audio.PlayOneShot(m_ChatNotification);
		m_Chat += "\n"+message;
	}

	void HostStartGame()
	{
		Network.maxConnections = -1;
		MasterServer.RegisterHost("ParisConnection", m_ServerName, "full");
		m_GameHasBegun = true;
		networkView.RPC("NetBeginGame", RPCMode.Others);
		GameManager.instance.BeginGame();
	}

	[RPC]
	void NetBeginGame(NetworkMessageInfo info)
	{
		m_GameHasBegun = true;
		GameManager.instance.HostCalledBeginGame(info);
	}

	void OnConnectedToServer()
	{
		m_Connected = true;
		networkView.RPC("NetChat", RPCMode.AllBuffered, m_PlayerName + " connected");
		++m_PlayerCount;
		GameManager.instance.PlayerConnected(m_PlayerName);
	}

	void OnPlayerConnected( NetworkPlayer player )
	{

	}
	
	public void IncreasePlayerCount()
	{
		++m_PlayerCount;
	}

	[RPC]
	void NetDecreasePlayerCount()
	{
		--m_PlayerCount;
	}

	void OnServerInitialized()
	{
		Debug.Log ("Server init");
		m_Connected = true;
		m_ServerInit = true;
		MasterServer.RegisterHost("ParisConnection", m_ServerName);
		GameManager.instance.ServerInit(m_PlayerName);
		++m_PlayerCount;
	}

	void OnPlayerDisconnected( NetworkPlayer player )
	{
		m_Chat += "\n A player disconnected";
		if(!m_GameHasBegun)
		{
			networkView.RPC ("NetDecreasePlayerCount", RPCMode.All);
			Network.RemoveRPCs(player);
			Network.DestroyPlayerObjects(player);
		}
		else
		{
			//Game started, player leaves, quit.
			networkView.RPC ("GameQuit", RPCMode.All);
		}
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection cause)
	{
		m_Connected = false;
		if(!m_GameHasBegun)
		{
			m_PlayerCount = 0; 
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
			foreach(GameObject go in players)
			{
				Destroy(go);
			}
		}
		else
		{
			GameGUI.instance.StopShowingGUI();
		}
	}
	
	[RPC]
	void GameQuit()
	{
		m_Connected = false;
		GameGUI.instance.StopShowingGUI();
	}
}
