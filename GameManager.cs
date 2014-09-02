using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {

	private static GameManager _instance;
	
	public static GameManager instance
	{
		get
		{
			if(_instance == null)
			{_instance = GameObject.FindObjectOfType<GameManager>();}
			return _instance;
		}
	}

	public AudioClip m_TrainPlacedSound;
	public AudioClip m_InvestmentSound;
	public AudioClip m_BGM;
	public AudioClip m_MyTurnSound;
	public AudioClip m_GameStartSound;
	public GameObject m_TrainPrefab;
	public GameObject m_PlayerPrefab;
	private List<Player> m_Players;
	private Banks m_Banks;
	private TrainColor m_PlayerPlacingColor;
	private Dictionary<int, Tile> m_Tiles;
	private int m_PlayerTurn;
	private int m_PlayersScored;
	private NetworkPlayer m_HostNetInfo;
	private Player m_LocalPlayer;

	[HideInInspector]
	[System.NonSerialized]
	public bool m_IsOnlineGame = true;//True until local is started
	[HideInInspector]
	public bool m_IsHost = true;

	// Use this for initialization
	void Start () {
		m_Tiles = new Dictionary<int, Tile>();
		m_Players = new List<Player>();
	}

	//playerCount is for local only, not necessary
	public void BeginGame(int players = 0)
	{
		GameObject.FindObjectOfType<Editor>().DeSerializeHexes();
		audio.PlayOneShot(m_GameStartSound);
		GameGUI.instance.PlayBGM();
		m_Banks = Banks.instance;

		if(!m_IsOnlineGame)
		{
			for(int i = 1; i <= players; ++i)
			{
				GameObject go = (GameObject)GameObject.Instantiate(m_PlayerPrefab);
				Player p = go.GetComponent<Player>();
				p.m_PlayerName = "Player[" + i.ToString() + "]";
				p.SetLocalPlayer();
				m_Players.Add(p);
			}
		}
		else
		{
			foreach(GameObject go in GameObject.FindGameObjectsWithTag("Player"))
			{
				m_Players.Add(go.GetComponent<Player>());
			}
			GameGUI.instance.WaitingForTurn();
		}

		Debug.Log ("Beginning game with " + m_Players.Count.ToString() + " players");
		//If I'm host, which should be true for local game
		if(m_IsHost)
		{
			int trainsToDistribute = 0;
			switch(m_Players.Count)
			{
			case 3 :
				trainsToDistribute = 10;
				break;
				
			case 4 :
				trainsToDistribute = 8;
				break;
				
			case 5 :
				trainsToDistribute = 6;
				break;
				
			case 6 :
				trainsToDistribute = 5;
				break;
				
			default :
				Debug.LogException(new System.Exception("Not enough players or too many " + m_Players.Count.ToString()));
				break;
			}
			for(int i = 0; i < m_Players.Count; ++i)
			{
				m_Banks.Distribute(m_Players[i], trainsToDistribute);
			}
			m_Banks.PlaceParisPieces();
			m_PlayerTurn = Random.Range(0,m_Players.Count);
			m_Players[m_PlayerTurn].BeginTurn();
		}
	}

	[RPC]
	void NetHostEndTurn(bool hitBlue)
	{
		EndTurn(hitBlue);
	}

	public void EndTurn(bool hitBlue)
	{
		if(m_IsOnlineGame && !m_IsHost)
		{
			networkView.RPC("NetHostEndTurn", m_HostNetInfo, hitBlue);
		}
		else
		{
			if(hitBlue || Banks.instance.GetColorsAvailable().Count == 1)
			{
				GameEnd();
				return;
			}

			if(m_PlayerTurn == m_Players.Count-1)
			{
				m_PlayerTurn = 0;
			}
			else
			{
				++m_PlayerTurn;
			}

			m_Players[m_PlayerTurn].BeginTurn();
		}
	}

	private void GameEnd()
	{
		if(m_IsHost)
		{
			foreach(Player p in m_Players)
			{
				Scoreboard.instance.SetPlayerScore(p, m_Players.Count);
			}
		}
		GameGUI.instance.GameEnd(m_Players);
		audio.PlayOneShot(m_GameStartSound);
	}

	//MP only
	public void PlayerReceivedScore(Player p)
	{
		++m_PlayersScored;
		if(m_PlayersScored == m_Players.Count)
		{
			GameEnd();
		}
	}

	[RPC]
	//Tiles dont have netviews, so we need the GM to handle this
	void NetTileOccupy(int color, int id)
	{
		m_Tiles[id].NetOccupy((TrainColor)color);
	}
	
	[RPC]
	void NetRemoveBoardPieces()
	{
		Banks.instance.RemoveBoardPieces();
	}

	public void AddTile(int id, Tile t)
	{
		m_Tiles[id] = t;
	}

	public Tile GetTile(int id)
	{
		return m_Tiles[id];
	}

	public void PlayerPlacingColor(TrainColor color)
	{
		m_PlayerPlacingColor = color;
	}

	public TrainColor GetPlayerColorChoice()
	{
		return m_PlayerPlacingColor; 
	}

	public GameObject GetTrainPrefab()
	{
		return m_TrainPrefab;
	}

	//MP only
	public void ServerInit(string playerName)
	{
		m_IsHost = true;
		GameObject go = (GameObject)Network.Instantiate(m_PlayerPrefab, Vector3.zero, Quaternion.identity, 0);
		go.GetComponentInChildren<Player>().SetPlayerInfo(playerName);
	}
	
	//MP only
	public void PlayerConnected(string playerName)
	{
		GameObject go = (GameObject)Network.Instantiate(m_PlayerPrefab, Vector3.zero, Quaternion.identity, 0);
		go.GetComponentInChildren<Player>().SetPlayerInfo(playerName);
	}

	//MP only
	public void SetLocalPlayer(Player p)
	{
		m_LocalPlayer = p;
	}

	public Player GetLocalPlayer()
	{
		return m_LocalPlayer;
	}

	//MP only
	//Called when non-host get the RPC to start game
	public void HostCalledBeginGame(NetworkMessageInfo host)
	{
		m_HostNetInfo = host.sender;
		m_IsHost = false;
		BeginGame();
	}

	// Update is called once per frame
	void Update () {
	
	}
}
