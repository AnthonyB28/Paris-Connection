 using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameGUI : MonoBehaviour {

	private static GameGUI _instance;
	
	public static GameGUI instance
	{
		get
		{
			if(_instance == null)
			{_instance = GameObject.FindObjectOfType<GameGUI>();}
			return _instance;
		}
	}

	public float m_ActionXPos = 10;
	public float m_ScoreXPos = 10;
	public float m_MessageBoxXPos = 10;

	private GUIScorePair[] m_ScorePair;
	private Player m_PlayerTakingTurn;
	private Tile m_HoveringTile;
	private List<Player> m_FinalScores;
	private HashSet<TrainColor> m_ColorsToInvest;
	private HashSet<TrainColor> m_ColorsToPlace;
	private GUIStyle m_EndScoreStyle;
	private Vector2 m_MessageBoxScrollPosV;
	private string m_MessageBox;

	private TrainColor m_ColorToInvest;
	private TrainColor m_ColorToPlace;

	private ushort m_PlacedTrains;
	private float m_LastClickTime;
	private float m_MessageBoxScrollPos = 40f;
	private float m_MessageBoxScrollMax = 200f;

	private bool m_ShowGUI = true;
	private bool m_MessageBoxIsDirty;
	private bool m_ShowControlGUI;
	private bool m_IsInvesting;
	private bool m_IsPlacing;
	private bool m_HasChosenColorToPlace;
	private bool m_HasChosenColorToInvest;
	private bool m_ShowInvestmentHover;
	private bool m_ShowGameEnd;
	private bool m_StopBGM;
	private bool m_Mute;
	private bool m_ShowPlayerSelect;
	private bool m_WaitingForTurn;
	private bool m_EndTurn; //Only when placing track less than 5

	// Use this for initialization
	void Start () {
		m_MessageBoxScrollPosV = Vector2.zero;
		m_MessageBox = "Hello! Welcome to Paris Connection!";
		m_ShowPlayerSelect = true;
		m_ScorePair = new GUIScorePair[6];
		GUIStyle blueStyle= new GUIStyle();
		blueStyle.normal.textColor = new Color32(34,138,255,100);
		m_ScorePair[0] = new GUIScorePair(blueStyle, TrainColor.Blue);
		GUIStyle brownStyle = new GUIStyle();
		brownStyle.normal.textColor = new Color32(235,171,139,100);
		m_ScorePair[1]= new GUIScorePair(brownStyle, TrainColor.Brown);
		GUIStyle blackStyle= new GUIStyle();
		blackStyle.normal.textColor = Color.black;
		m_ScorePair[2]= new GUIScorePair(blackStyle, TrainColor.Black);
		GUIStyle yellowStyle= new GUIStyle();
		yellowStyle.normal.textColor = Color.yellow;
		m_ScorePair[3]= new GUIScorePair(yellowStyle, TrainColor.Yellow);
		GUIStyle redStyle= new GUIStyle();
		redStyle.normal.textColor = Color.red;
		m_ScorePair[4]= new GUIScorePair(redStyle, TrainColor.Red);
		GUIStyle purpleStyle= new GUIStyle();
		purpleStyle.normal.textColor = new Color(128,0,128);
		m_ScorePair[5]= new GUIScorePair(purpleStyle, TrainColor.Purple);
		m_EndScoreStyle = new GUIStyle();
		m_EndScoreStyle.fontSize = 20;
		m_EndScoreStyle.fontStyle = FontStyle.Bold;
		m_EndScoreStyle.normal.textColor = Color.white;

		if(Application.loadedLevelName == "local")
		{
			m_ShowPlayerSelect = true;
			GameManager.instance.m_IsOnlineGame = false;
		}
	}

	public void BeginTurn(Player p)
	{
		audio.PlayOneShot(GameManager.instance.m_MyTurnSound);
		LogMessageBox(p.m_PlayerName +"'s turn!");
		m_WaitingForTurn = false;
		m_ShowPlayerSelect = false;
		m_PlayerTakingTurn = p;
		m_ColorsToInvest = p.GetColorsAvailableToInvest();
		m_ColorsToPlace = Banks.instance.GetColorsAvailable();
		m_IsInvesting = false;
		m_IsPlacing = false;
		m_HasChosenColorToPlace = false;
		m_HasChosenColorToInvest = false;
		m_PlacedTrains = 0;
		m_ShowControlGUI = true;
		m_ShowInvestmentHover = true;
	}

	//MP only
	//Host is telling us who's turn it is, and it isnt us locally
	public void SetPlayerTurn(Player p)
	{
		m_PlayerTakingTurn = p;
		WaitingForTurn();
		LogMessageBox(m_PlayerTakingTurn.m_PlayerName + "'s turn!");
	}

	//MP only
	//We're waiting for turn so don't show anything.
	public void WaitingForTurn()
	{
		m_WaitingForTurn = true;
		m_ShowPlayerSelect = false;
		m_ShowInvestmentHover = true;
	}

	public bool IsPlacingTrains()
	{
		return m_HasChosenColorToPlace; //important for valid check
	}

	public TrainColor GetColorChosenToPlace()
	{
		return m_ColorToPlace;
	}

	public void SetHoveringTile(Tile tile)
	{
		m_HoveringTile = tile;
	}

	public void UnsetHoveringTile()
	{
		m_HoveringTile = null;
	}

	public void StopShowingGUI()
	{
		m_ShowGUI = false;
	}

	public void GameEnd(List<Player> finalScores)
	{
		m_FinalScores = finalScores;
		m_ShowControlGUI = false;
		if(!GameManager.instance.m_IsOnlineGame)
		{
			m_ShowInvestmentHover = false;
		}
		m_ShowGameEnd = true;
	}

	public void LogMessageBox(string msg)
	{
		m_MessageBox += "\n" + msg;
		if(m_MessageBox.Length > 275)
		{
			m_MessageBoxIsDirty = true;
		}

	}

	public void PlayBGM()
	{
		audio.loop = true;
		audio.clip = GameManager.instance.m_BGM;
		audio.Play();
	}

	void Update()
	{
		if(Input.GetKey(KeyCode.Mouse0) && m_IsPlacing && m_HasChosenColorToPlace)
		{
			if(Time.time - m_LastClickTime > 0.3)
			{
				if(m_HoveringTile != null && !m_HoveringTile.IsFull())
				{
					if(m_HoveringTile.Occupy(m_ColorToPlace, m_PlayerTakingTurn))
					{
						++m_PlacedTrains;
					}
				}
				m_LastClickTime = Time.time;
			}
		}
	}

	// Update is called once per frame
	void OnGUI () {
		if(m_ShowGUI)
		{
			if(m_ShowPlayerSelect && !GameManager.instance.m_IsOnlineGame)
			{
				ShowPlayerSelectGUI();
			}

			if(!m_ShowPlayerSelect)
			{
				ShowSoundButtons();
				ShowScoreGUI();
				ShowMessageBox();
			}
			
			if(m_ShowInvestmentHover)
			{
				ShowPlayerInvestmentsGUI();
			}
			
			if(m_ShowControlGUI)
			{
				ShowControlGUI();
			}
			else if(m_ShowGameEnd)
			{
				ShowGameEndGUI();
			}
		}
	}

 	void ShowSoundButtons()
	{
		if(!m_Mute)
		{
			if(!m_StopBGM)
			{
				if (GUI.Button (new Rect (0,Screen.height-40,50,40), "Stop\nBGM")){
					m_StopBGM = true;
					audio.Stop();
				}
			}
			else
			{
				if (GUI.Button (new Rect (0,Screen.height-40,50,40), "Start\nBGM")){
					m_StopBGM = false;
					PlayBGM();
				}
			}

			if (GUI.Button (new Rect (60,Screen.height-40,50,40), "Mute")){
				m_Mute = true;
				AudioListener.pause = true;
			}
		}
		else
		{
			if (GUI.Button (new Rect (60,Screen.height-40,50,40), "Unmute")){
				m_Mute = false;
				AudioListener.pause = false;
			}
		}
	}

	void ShowMessageBox()
	{
		if(m_MessageBoxIsDirty)
		{
			m_MessageBoxScrollPos+=30;
			m_MessageBoxScrollMax+=30;
			m_MessageBoxScrollPosV = new Vector2(0, m_MessageBoxScrollPos);
			m_MessageBoxIsDirty = false;
		}

		m_MessageBoxScrollPosV = GUI.BeginScrollView(new Rect(m_MessageBoxXPos,Screen.height-230,230,200), m_MessageBoxScrollPosV, new Rect(m_MessageBoxXPos,0,300,m_MessageBoxScrollMax));
		GUI.Label (new Rect(m_MessageBoxXPos,0,300,m_MessageBoxScrollMax), m_MessageBox);
		GUI.EndScrollView();
	}

	void ShowPlayerSelectGUI()
	{
		int x = Screen.width/2-150;
		int y = Screen.height/2-200;
		GUI.Box(new Rect (x, y, 375, 150), "Select # Of Players:");
		y+=60;
		x+=70;
		if (GUI.Button (new Rect (x,y,50,50), "3")) {
			GameManager.instance.BeginGame(3);
		}
		x+=60;
		if (GUI.Button (new Rect (x,y,50,50), "4")) {
			GameManager.instance.BeginGame(4);
		}
		x+=60;
		if (GUI.Button (new Rect (x,y,50,50), "5")) {
			GameManager.instance.BeginGame(5);
		}
		x+=60;
		if (GUI.Button (new Rect (x,y,50,50), "6")) {
			GameManager.instance.BeginGame(6);
		}
	}

	void ShowGameEndGUI()
	{
		GUI.Box(new Rect (Screen.width/2-350, Screen.height/2-250, 700, 500), "GAME OVER \n\n SCORES:");
		int y = Screen.height/2 - 150;
		int x = Screen.width/2-200;
		for(int i = 0; i < m_FinalScores.Count; ++i)
		{
			if(i > 2)
			{
				y += 100;
				x = Screen.width/2-200;
			}
			GUI.Label (new Rect(x, y, 100, 50), new GUIContent(m_FinalScores[i].m_PlayerName + " :"),m_EndScoreStyle);
			GUI.Label (new Rect(x+45, y+25, 100, 20), new GUIContent(m_FinalScores[i].m_Score.ToString()), m_EndScoreStyle);
			x+= 150;
		}
		if (GUI.Button (new Rect (Screen.width/2,y+150,100,100), "Reload")) {
			Application.LoadLevel(0);
		}
	}

	void ShowScoreGUI()
	{
		int length = 450;
		if(!m_ShowInvestmentHover)
		{
			length = 300;
		}
		GUI.Box(new Rect (m_ScoreXPos, 0, 110, length), "");
		GUI.Label(new Rect(m_ScoreXPos+30, 0, 100, 20), "Score | Bank");
		int y = 20;
		for(int i = 0; i < 6; ++i)
		{
			GUI.Label(new Rect(m_ScoreXPos+10, y, 10, 20 ),TrainColors.GetName(m_ScorePair[i].m_Color), m_ScorePair[i].m_Style);
			GUI.Label(new Rect(m_ScoreXPos+50, y, 10, 20 ),Scoreboard.instance.GetTrainValue(m_ScorePair[i].m_Color).ToString(), m_ScorePair[i].m_Style);
			GUI.Label(new Rect(m_ScoreXPos+80, y, 10, 20 ),Banks.instance.GetNumOfColorAvailable(m_ScorePair[i].m_Color).ToString(), m_ScorePair[i].m_Style);
			y+=30;
		}
	}

	void ShowPlayerInvestmentsGUI()
	{
		// This line feeds "This is the tooltip" into GUI.tooltip
		GUI.Button (new Rect (m_ScoreXPos, 200, 100, 50), new GUIContent ("Hover For \nInvestments!", "T"));
		if(GUI.tooltip == "T")
		{
			int y = 260;
			for(int i = 0; i < 6; ++i)
			{
				GUI.Label(new Rect(m_ScoreXPos+10, y, 10, 20 ),TrainColors.GetName(m_ScorePair[i].m_Color), m_ScorePair[i].m_Style);
				if(GameManager.instance.m_IsOnlineGame)
				{
					GUI.Label(new Rect(m_ScoreXPos+50, y, 10, 20 ),GameManager.instance.GetLocalPlayer().GetInvestmentTotal(m_ScorePair[i].m_Color).ToString(), m_ScorePair[i].m_Style);
				}
				else
				{
					GUI.Label(new Rect(m_ScoreXPos+50, y, 10, 20 ),m_PlayerTakingTurn.GetInvestmentTotal(m_ScorePair[i].m_Color).ToString(), m_ScorePair[i].m_Style);
				}

				y+=30;
			}
		}
	}

	void ShowControlGUI()
	{
		GUI.Box (new Rect (m_ActionXPos,0,100,21), m_PlayerTakingTurn.m_PlayerName + " Turn!");
		
		//Player has not chosen either to start
		if(!m_IsInvesting && !m_IsPlacing && !m_WaitingForTurn)
		{
			GUI.Box (new Rect (m_ActionXPos,20,100,90), "Move Choices");
			
			if (GUI.Button (new Rect (m_ActionXPos+10,50,80,20), "Invest")) {
				m_IsInvesting = true;
			}
			
			// Make the second button.
			if (GUI.Button (new Rect (m_ActionXPos+10,80,80,20), "Place Track")) {
				m_IsPlacing = true;
			}
		}
		
		//Player chose to invest trains
		else if(m_IsInvesting && !m_HasChosenColorToInvest)
		{
			GUI.Box (new Rect (m_ActionXPos,20,100,250), "Choose Color" + "\nTo Trade In");
			
			int x = 60;
			foreach(TrainColor color in m_ColorsToInvest)
			{
				if (GUI.Button (new Rect (m_ActionXPos+10,x,80,20), TrainColors.GetName(color))) {
					m_ColorToInvest = color;
					m_HasChosenColorToInvest = true;
				}
				x += 30;
			}
			if (GUI.Button (new Rect (m_ActionXPos+10,x,80,20), "Return")){
				m_IsInvesting = false;
				m_HasChosenColorToInvest = false;
			}
		}
		
		//Player has chosen color to trade in, now choses color to get investments
		else if(m_IsInvesting && m_HasChosenColorToInvest)
		{
			GUI.Box (new Rect (m_ActionXPos,20,100,250), "Choose Color" + "\nTo Invest In");
			
			int x = 60;
			foreach(TrainColor color in m_ColorsToPlace)
			{
				if(color != m_ColorToInvest)
				{
					if (GUI.Button (new Rect (m_ActionXPos+10,x,80,20), TrainColors.GetName(color))) {
						m_ShowControlGUI = false;
						m_PlayerTakingTurn.Invest(m_ColorToInvest, color);
					}
					x += 30;
				}
				
			}
			if (GUI.Button (new Rect (m_ActionXPos+10,x,80,20), "Return")){
				m_IsInvesting = false;
				m_HasChosenColorToInvest = false;
			}
		}
		
		//Player chose to place trains
		else if(m_IsPlacing && !m_HasChosenColorToPlace)
		{
			GUI.Box (new Rect (m_ActionXPos,20,100,245), "Choose Color" + "\nTo Place Track");
			int y = 60;
			foreach(TrainColor color in m_ColorsToPlace)
			{
				if (GUI.Button (new Rect (m_ActionXPos+10,y,80,20), TrainColors.GetName(color))) {
					m_ColorToPlace = color;
					GameManager.instance.PlayerPlacingColor(color);
					m_HasChosenColorToPlace = true;
				}
				y += 30;
			}
			if (GUI.Button (new Rect (m_ActionXPos+10,y,80,20), "Return")){
				m_IsPlacing = false;
			}
		}
		
		//Player has chosen color to place, now needs to click hexes or end turn if > 1
		else if(m_IsPlacing && m_HasChosenColorToPlace)
		{
			GUI.Box (new Rect (m_ActionXPos,20,100,80), "Place tracks!");
			if(m_PlacedTrains == 5)
			{
				m_ShowControlGUI = false;
				m_HasChosenColorToPlace = false;
				m_IsPlacing = false;
				m_PlayerTakingTurn.EndTurn(false);
			}
			if(m_PlacedTrains == 0)
			{
				if (GUI.Button (new Rect (m_ActionXPos+10,40,80,20), "Return")){
					m_IsPlacing = false;
					m_HasChosenColorToPlace = false;
				}
			}
			if(m_PlacedTrains > 0)
			{
				if (GUI.Button (new Rect (m_ActionXPos+10,60,80,20), "End Turn")) {
					m_ShowControlGUI = false;
					m_HasChosenColorToPlace = false;
					m_IsPlacing = false;
					m_PlayerTakingTurn.EndTurn(false);
				}
			}
		}
		else if(m_WaitingForTurn)
		{
			GUI.Box (new Rect (m_ActionXPos,20,100,80), "Wait your turn! \n" + m_PlayerTakingTurn.m_PlayerName + " is going.");
		}
	}
}

public class GUIScorePair
{
	public GUIStyle m_Style;
	public TrainColor m_Color;

	public GUIScorePair(GUIStyle style, TrainColor color)
	{
		m_Style = style;
		m_Color = color;
	}
}
