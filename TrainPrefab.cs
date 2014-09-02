using UnityEngine;
using System.Collections;

public class TrainPrefab : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public void SetColor(TrainColor color)
	{
		foreach(Transform child in transform)
		{
			switch(color)
			{
			case TrainColor.Black:
				child.renderer.material.color = Color.black;
				break;
			case TrainColor.Red:
				child.renderer.material.color = Color.red;
				break;
			case TrainColor.Blue:
				child.renderer.material.color = new Color32(0,0,204,100);
				break;
			case TrainColor.Brown:
				child.renderer.material.color = new Color32(160,82,45,100);
				break;
			case TrainColor.Yellow:
				child.renderer.material.color = Color.yellow;
				break;
			case TrainColor.Purple:
				child.renderer.material.color = new Color32(140,0,140,100);
				break;
			}
		}

	}

	// Update is called once per frame
	void Update () {
	
	}
}
