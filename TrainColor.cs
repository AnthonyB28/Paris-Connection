using UnityEngine;
using System.Collections;

public class TrainColors {

	static public string GetName(TrainColor color)
	{
		return System.Enum.GetName(typeof(TrainColor), color);
	}
}

public enum TrainColor
{
	Null = 0,
	Purple,
	Brown,
	Black,
	Yellow,
	Blue,
	Red
}