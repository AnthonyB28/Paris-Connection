using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour{
	
	private List<TrainColor> m_Investments;
	private bool m_IsLocalPlayer; //In online mode, are we the player thats local?

	public int m_Score;
	public string m_PlayerName;

    void Awake()
	{
		m_Investments = new List<TrainColor>();
		m_Score = 0;
	}
	
	//MP only
	public void SetPlayerInfo(string name)
	{
		m_PlayerName = name;
		m_IsLocalPlayer = true;
		GameManager.instance.SetLocalPlayer(this);
		networkView.RPC("NetSetPlayerInfo", RPCMode.OthersBuffered, name);
	}

	[RPC]
	void NetSetPlayerInfo(string name, NetworkMessageInfo netplayer)
	{
		m_PlayerName = name;
		Debug.Log ("New player connected " + m_PlayerName);
		GameManager.instance.gameObject.GetComponent<Server>().IncreasePlayerCount();
	}

	//If MP, will send RPC to inform other players of this score
	public void SetScore(int score)
	{
		m_Score = score;
		if(GameManager.instance.m_IsOnlineGame)
		{
			networkView.RPC("NetSetPlayerScore", RPCMode.Others, score);
		}
		GameManager.instance.PlayerReceivedScore(this);
	}

	[RPC]
	void NetSetPlayerScore(int score)
	{
		m_Score = score;
		GameManager.instance.PlayerReceivedScore(this);
	}

	public void SetLocalPlayer()
	{
		m_IsLocalPlayer = true;
	}
	
	public void BeginTurn()
	{
		//Player can now either lay down track or invest
		if(m_IsLocalPlayer)
		{
			GameGUI.instance.BeginTurn(this);
		}
		else
		{
			networkView.RPC("NetInformBeginTurn", RPCMode.All);
		}
	}

	[RPC]
	//Host tells players who is currently making a move.
	private void NetInformBeginTurn()
	{
		if(m_IsLocalPlayer)
		{
			GameGUI.instance.BeginTurn(this);
		}
		else
		{
			GameGUI.instance.SetPlayerTurn(this);
		}
	}

	//If local game, receive 1 train
	//If MP tell everyone about it in NetInvestmentReceived
	public void ReceiveInvestment(TrainColor trainToInvest)
	{
		if(GameManager.instance.m_IsOnlineGame)
		{
			networkView.RPC("NetInvestmentReceived", RPCMode.All, (int)trainToInvest);
		}
		else
		{
			m_Investments.Add(trainToInvest);				
		}
	}

	[RPC]
	//Adds one to investments, tells bank to remove it too
	void NetInvestmentReceived(int color)
	{
		//Debug.Log ("Received color " + TrainColors.GetName((TrainColor)color) + " " + color.ToString() + " " + gameObject.name + m_PlayerName);
		m_Investments.Add((TrainColor)color);
		Banks.instance.RemoveTrain((TrainColor)color);
	}

	public void Invest(TrainColor tradeColor, TrainColor targetColor)
	{
		TrainColor toTrade = TrainColor.Null;
		foreach(TrainColor t in m_Investments)
		{
			if(t == tradeColor)
			{
				toTrade = t;
				break;
			}
		}

		if(toTrade == TrainColor.Null)
		{
			Debug.LogError("Attempting to trade a color this player doesn't have - " + m_PlayerName + TrainColors.GetName(tradeColor));
		}

		m_Investments.Remove(toTrade);
		if(GameManager.instance.m_IsOnlineGame)
		{
			networkView.RPC("NetInvestmentRemoved", RPCMode.Others, (int)toTrade, (int)targetColor);
		}

		Banks.instance.InvestIn(this, toTrade, targetColor);
		GameGUI.instance.LogMessageBox(m_PlayerName+" traded " + TrainColors.GetName(toTrade) + " for " + TrainColors.GetName(targetColor));
		GameGUI.instance.audio.PlayOneShot(GameManager.instance.m_InvestmentSound);
		EndTurn(false);
	}

	[RPC]
	//Player invested a train, now needs to remove it from all players
	void NetInvestmentRemoved(int colorToTradeInt, int targetColorInt)
	{
		TrainColor colorToTrade = (TrainColor)colorToTradeInt;
		//Debug.Log ("Received color " + TrainColors.GetName((TrainColor)color) + " " + color.ToString() + " " + gameObject.name + m_PlayerName);
		GameGUI.instance.LogMessageBox(m_PlayerName+" traded " + TrainColors.GetName(colorToTrade) + " for " + TrainColors.GetName((TrainColor) targetColorInt));
		m_Investments.Remove(colorToTrade);
		Banks.instance.InvestIn(colorToTrade);
	}

	
	public uint GetInvestmentTotal(TrainColor color)
	{
		uint investment = 0;
		foreach(TrainColor t in m_Investments)
		{
			if(t == color)
				investment += 1;
		}
		return investment;
	}
	
	public HashSet<TrainColor> GetColorsAvailableToInvest()
	{
		List<TrainColor> colorsToInvest = new List<TrainColor>();
		foreach(TrainColor t in m_Investments)
		{
			colorsToInvest.Add(t);
		}
		
		return new HashSet<TrainColor>(colorsToInvest);
	}

	public void EndTurn(bool hitBlue)
	{
		GameManager.instance.EndTurn(hitBlue);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
