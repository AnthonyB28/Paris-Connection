using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {

	private uint m_Value;

	private TrainColor m_Train1;
	private TrainColor m_Train2;
	private bool m_IsFull;
	private bool m_CanSelect;
	private bool m_IsParisTile;

	private int m_NeighborID;
	private List<int> m_Neighbors;

	// Use this for initialization
	void Start () {
		switch(gameObject.name)
		{
		case "Blue(Clone)" :
			m_Value = 4;
			break;

		case "Red(Clone)" :
			m_Value = 3;
			break;

		case "Purple(Clone)" :
			m_Value = 2;
			break;

		case "White(Clone)" :
			m_Value = 1;
			break;
		
		case "Paris(Clone)" :
			m_IsParisTile = true;
			m_Value = 0;
			break;

		default :
			m_Value = 0;
			break;
		}
	}

	void OnMouseOver() {
		if(m_IsParisTile)
		{
			GameGUI.instance.UnsetHoveringTile();
		}
		else if(GameGUI.instance.IsPlacingTrains())
		{
			if(IsMoveValid(GameGUI.instance.GetColorChosenToPlace()))
			{
				//TODO: this is valid, cue here
				GameGUI.instance.SetHoveringTile(this);
			}
			else
			{
				//TODO:not a valid move, cue here
				//Maybe account for the fact we have double color?
				GameGUI.instance.UnsetHoveringTile();
			}
		}
	}

	void OnMouseExit()
	{
		GameGUI.instance.UnsetHoveringTile();
	}
	
	public void AddNeighbors(List<int> neighbors, int myID)
	{
			m_NeighborID = myID;
			m_Neighbors = neighbors;
			GameManager.instance.AddTile(m_NeighborID, this);
	}

	//returns true if we placed a train (and didnt end game on blue)
	public bool Occupy(TrainColor trainToPlace, Player player) 
	{
		if(!m_IsParisTile && !m_IsFull)
		{
			if(IsMoveValid(trainToPlace))
			{
				//City tile, only 1 train
				if(m_Value > 0)
				{
					if(m_Train1 == TrainColor.Null)
					{
						m_Train1 = trainToPlace;
					}
					m_IsFull = true;
				}
				else
				{
					//rural tile, 2 trains
					if(m_Train1 == TrainColor.Null)
					{
						m_Train1 = trainToPlace;
					}
					else if(m_Train2 == TrainColor.Null)
					{
						if(m_Train1 == trainToPlace)
						{
							Debug.Log ("Placed double colors");
							return false;
						}
						m_Train2 = trainToPlace;
						m_IsFull = true;
					}
					else
					{
						Debug.LogError("How did we not set full on this tile - " + gameObject.name);
					}
				}

				if(GameManager.instance.m_IsOnlineGame)
				{
					GameManager.instance.networkView.RPC("NetTileOccupy", RPCMode.Others, (int)trainToPlace, m_NeighborID);
				}
				PlaceTrainModel(trainToPlace);
				Banks.instance.RemoveTrain(trainToPlace);

				if(m_Value > 0)
				{
					Scoreboard.instance.AddScore(trainToPlace, m_Value);
				}

				if(m_Value == 4)
				{
					player.EndTurn(true); //end game
					return false;
				}
				else
				{
					return true; //placed train
				}
			}
			else
			{
				Debug.LogError("Invalid move reached inside Occupy");
				return false;
			}
		}
		else
		{
			Debug.LogError("Tried to occupy a full tile? - " + gameObject.name);
			return false;
		}
	}

	public void OccupyParis(TrainColor color)
	{
		if(gameObject.tag == "Paris")
		{
			if(GameManager.instance.m_IsOnlineGame)
			{
				GameManager.instance.networkView.RPC("NetTileOccupy", RPCMode.All, (int)color, m_NeighborID);
			}
			else
			{
				m_Train1 = color;
				PlaceTrainModel(color);
				m_IsFull = true;
			}
		}
		else
		{
			Debug.LogException(new System.Exception("We're occupying a paris tile thats not paris - " + gameObject.name));
		}
	}
	
	public void NetOccupy(TrainColor trainToPlace)
	{
		//We can assume that the valid check has been done from the receiver
		//Therefore, we just fill the train where we think it should go
		if(m_Value > 0)
		{
			m_Train1 = trainToPlace;
		}
		else
		{
			if(m_Train1 == TrainColor.Null)
			{
				m_Train1 = trainToPlace;
			}
			else
			{
				m_Train2 = trainToPlace;
				m_IsFull = true;
			}
		}
		PlaceTrainModel(trainToPlace);
		//Debug.Log ("Got a train RPC");
		
		if(gameObject.tag != "Paris")
		{
			Banks.instance.RemoveTrain(trainToPlace);
		}
		else
		{
			m_IsFull = true;
		}
		
		if(m_Value > 0)
		{
			m_IsFull = true;
			Scoreboard.instance.AddScore(trainToPlace, m_Value);
		}
	}

	//A valid move is only one that checks for neighbors.
	//Does not do any check about what the tile contains
	//EX: checks neighbor tiles for color, but does not check
	//Train1 and Train2 to see if another color can go here.
	private bool IsMoveValid(TrainColor colorToCheck)
	{
		//Debug.Log (m_NeighborID);
		if(Banks.instance.GetNumOfColorAvailable(colorToCheck) == 0)
		{
			//TODO: something else?
			return false;
		}
		foreach(int neighborID in m_Neighbors)
		{
			Tile neighborToCheck = GameManager.instance.GetTile(neighborID);
			TrainColor m_Color1 = TrainColor.Null;
			TrainColor m_Color2 = TrainColor.Null;
			if(neighborToCheck.GetTrain1() != TrainColor.Null)
			{
				m_Color1 = neighborToCheck.GetTrain1();
			}
			if(neighborToCheck.GetTrain2() != TrainColor.Null)
			{
				m_Color2 = neighborToCheck.GetTrain2();
			}
			if(m_Color1 == colorToCheck)
			{
				return true;
			}
			if(m_Color2 == colorToCheck)
			{
				return true;
			}
		}

		return false;
	}

	private void PlaceTrainModel(TrainColor color)
	{
		GameObject train = (GameObject) GameObject.Instantiate(GameManager.instance.GetTrainPrefab(), transform.position, GameManager.instance.GetTrainPrefab().transform.rotation);
		train.transform.Translate(new Vector3(0,0,-0.5f));
		train.GetComponent<TrainPrefab>().SetColor(color);
		if(m_IsFull && m_Value == 0 && !m_IsParisTile)
		{
			train.transform.Translate(new Vector3(0.3f,0.5f));
		}
		GameGUI.instance.audio.PlayOneShot(GameManager.instance.m_TrainPlacedSound);
	}

	public int GetID()
	{
		return m_NeighborID;
	}

	public TrainColor GetTrain1()
	{
		return m_Train1;
	}

	public TrainColor GetTrain2()
	{
		return m_Train2;
	}

	public bool IsFull()
	{
		return m_IsFull;
	}

	// Update is called once per frame
	void Update () {
	}
}
