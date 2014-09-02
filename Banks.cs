using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Banks {

	private static Banks _instance;
	
	public static Banks instance
	{
		get
		{
			if(_instance == null)
			{ _instance = new Banks();}
			return _instance;
		}
	}

	private Dictionary<TrainColor, int> m_TrainsAvailable;

	public Banks()
	{
		m_TrainsAvailable = new Dictionary<TrainColor, int>();
		Reset();
	}

	void Reset()
	{
		m_TrainsAvailable[TrainColor.Blue] = 33;
		m_TrainsAvailable[TrainColor.Red] = 33;
		m_TrainsAvailable[TrainColor.Black] = 33;
		m_TrainsAvailable[TrainColor.Brown] = 33;
		m_TrainsAvailable[TrainColor.Purple] = 33;
		m_TrainsAvailable[TrainColor.Yellow] = 33;
	}

	public void Distribute(Player p, int max)
	{
		for(int i = 0; i != max; ++i)
		{
			TrainColor random = (TrainColor)Random.Range(1, 6); // not maximally inclusive

			//We'll get an RPC telling us to remove this later
			if(!GameManager.instance.m_IsOnlineGame)
			{

				m_TrainsAvailable[random] -= 1;
			}

			p.ReceiveInvestment(random);
		}
	}

	public void RemoveBoardPieces()
	{
		m_TrainsAvailable[TrainColor.Blue] -= 2;
		m_TrainsAvailable[TrainColor.Red] -= 2;
		m_TrainsAvailable[TrainColor.Black] -= 2;
		m_TrainsAvailable[TrainColor.Brown] -= 2;
		m_TrainsAvailable[TrainColor.Purple] -= 2;
		m_TrainsAvailable[TrainColor.Yellow] -= 2;
	}

	public void PlaceParisPieces()
	{
		RemoveBoardPieces();
		if(GameManager.instance.m_IsOnlineGame)
		{
			GameManager.instance.networkView.RPC("NetRemoveBoardPieces", RPCMode.Others);
		}

		GameObject[] parisTiles = GameObject.FindGameObjectsWithTag("Paris");
		HashSet<TrainColor> colorsAvailable = new HashSet<TrainColor>(m_TrainsAvailable.Keys);
		int parisColor;
		foreach(GameObject tile in parisTiles)
		{
			TrainColor[] colorsToPick = new TrainColor[colorsAvailable.Count];
			colorsAvailable.CopyTo(colorsToPick);
			parisColor = Random.Range (0,colorsAvailable.Count);
			Tile t = tile.GetComponent<Tile>();
			t.OccupyParis(colorsToPick[parisColor]);
			colorsAvailable.Remove(colorsToPick[parisColor]);
		}
	}

	public void RemoveTrain(TrainColor toRemove)
	{
		if(m_TrainsAvailable[toRemove] == 0)
		{
			Debug.LogError("Trying to remove a color that has none in bank");
			return;
		}
		m_TrainsAvailable[toRemove] -= 1;
	}

	public int GetNumOfColorAvailable(TrainColor color)
	{
		return m_TrainsAvailable[color];
	}

	public HashSet<TrainColor> GetColorsAvailable()
	{
		HashSet<TrainColor> colorsAvailable = new HashSet<TrainColor>();
		foreach(KeyValuePair<TrainColor, int> kvp in m_TrainsAvailable)
		{
			if(kvp.Value > 0)
			{
				colorsAvailable.Add (kvp.Key);
			}
		}

		return colorsAvailable;
	}

	//Add one train, remove up to 2 if available
	public void InvestIn(Player p, TrainColor trainToTrade, TrainColor colorToInvest)
	{
		m_TrainsAvailable[trainToTrade] += 1;

		//If online game, don't remove from bank. We'll get an RPC to handle it.
		if(m_TrainsAvailable[colorToInvest] == 1)
		{
			if(!GameManager.instance.m_IsOnlineGame)
			{
				m_TrainsAvailable[colorToInvest] -= 1;
			}

			p.ReceiveInvestment(colorToInvest);
		}
		else
		{	
			if(!GameManager.instance.m_IsOnlineGame)
			{
				m_TrainsAvailable[colorToInvest] -= 2;
			}
			p.ReceiveInvestment(colorToInvest);
			p.ReceiveInvestment(colorToInvest);
		}
	}

	//MP only
	//Other players, not local, will recieve this to remove 1 train from bank
	public void InvestIn(TrainColor trainToAdd)
	{
		m_TrainsAvailable[trainToAdd] += 1;
	}

	// Update is called once per frame
	void Update () {
	
	}
}
