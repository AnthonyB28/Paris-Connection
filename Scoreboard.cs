using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Scoreboard {

	private static Scoreboard _instance;
	
	public static Scoreboard instance
	{
		get
		{
			if(_instance == null)
			{_instance = new Scoreboard();}
			return _instance;
		}
	}

	private Dictionary<TrainColor, uint> m_TrainValue;

	// Use this for initialization
	public Scoreboard()
	{
		m_TrainValue = new Dictionary<TrainColor, uint>();
		m_TrainValue[TrainColor.Blue] = 0;
		m_TrainValue[TrainColor.Red] = 0;
		m_TrainValue[TrainColor.Black] = 0;
		m_TrainValue[TrainColor.Brown] = 0;
		m_TrainValue[TrainColor.Purple] = 0;
		m_TrainValue[TrainColor.Yellow] = 0;
	}

	public uint GetTrainValue(TrainColor color)
	{
		return m_TrainValue[color];
	}

	public void SetPlayerScore(Player p, int playerCount)
	{
		uint blackInvestments = p.GetInvestmentTotal(TrainColor.Black);
		uint redInvestments = p.GetInvestmentTotal(TrainColor.Red);
		uint blueInvestments = p.GetInvestmentTotal(TrainColor.Blue);
		uint brownInvestments = p.GetInvestmentTotal(TrainColor.Brown);
		uint purpleInvestments = p.GetInvestmentTotal(TrainColor.Purple);
		uint yellowInvestments = p.GetInvestmentTotal(TrainColor.Yellow);
		uint totalInvestments = blackInvestments + redInvestments + blueInvestments + purpleInvestments + yellowInvestments + brownInvestments;
		uint maxInvestments = 0;
		switch(playerCount)
		{
		case 3:
			maxInvestments = 20;
			break;
		case 4:
			maxInvestments = 15;
			break;
		case 5:
			maxInvestments = 12;
			break;
		case 6:
			maxInvestments = 10;
			break;
		}

		int penalty = 0;
		if(totalInvestments > maxInvestments)
		{
			for(int i = 0; i < totalInvestments-maxInvestments; ++i)
			{
				penalty += 20;
			}
		}

		uint scoreTally = 0;
		scoreTally += blackInvestments * m_TrainValue[TrainColor.Black];
		scoreTally += redInvestments * m_TrainValue[TrainColor.Red];
		scoreTally += blueInvestments * m_TrainValue[TrainColor.Blue];
		scoreTally += brownInvestments * m_TrainValue[TrainColor.Brown];
		scoreTally += purpleInvestments * m_TrainValue[TrainColor.Purple];
		scoreTally += yellowInvestments * m_TrainValue[TrainColor.Yellow];
		p.SetScore(System.Convert.ToInt32(scoreTally)-penalty);
	}

	public void AddScore(TrainColor color, uint value)
	{
		//Debug.Log (TrainColors.GetName(color) + " has increased " + value.ToString());
		GameGUI.instance.LogMessageBox(TrainColors.GetName(color)+ " has increased " + value.ToString());
		m_TrainValue[color] += value;
	}
}
