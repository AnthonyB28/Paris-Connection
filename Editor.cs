using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;


/*
 * Editor Class - main control for hex map editor
 * 
 * If you drag and drop to the Hex Type prefabs, also add to the enums and OnGUI selection
 * 
 * Enabled Editor = main edit mode that allows the modification of the field.
 * Deseriaze = if XML exists in specified directory, it will try to open it and seriealize it 
 * into Unity before editing.
 */

public class Editor : MonoBehaviour {
	private static Editor _instance;

	public static Editor instance
	{
		get
		{
			if(_instance == null)
			{_instance = GameObject.FindObjectOfType<Editor>();}
			return _instance;
		}
	}
	
	public bool m_EnabledEditor;
	public bool m_Deserialize;
	public string m_SavePath;
	public string m_LoadPath;
	public TextAsset m_TextToLoad;

	public GameObject[] m_HexTypePrefabs;

	private bool m_IsSerialized;
	private HexType m_SelectedType = HexType.Blue;
	private Dictionary<int, HexTileSerializable> m_SavedHex;
	private List<HexTileSerializable> m_ToSerialize;

	// Use this for initialization
	void Start () 
	{
		if(m_EnabledEditor)
			m_SavedHex = new Dictionary<int, HexTileSerializable>();
		m_ToSerialize = new List<HexTileSerializable>();

		if(m_Deserialize)
		{
			DeSerializeHexes();
		}
	}

	void OnGUI()
	{
		if(m_EnabledEditor)
		{
			if(GUI.Button(new Rect(20,40,80,20), "Serial"))
			{
				SerializeHexes();
			}

			HexType[] values = (HexType[])System.Enum.GetValues(typeof(HexType));

			int x = 120;
			foreach( HexType type in values )
			{
				if(GUI.Button(new Rect(20,x,80,20), (string)System.Enum.GetName(typeof(HexType), type)))
				{
					m_SelectedType = type;
				}

				x += 20;
   			}
		}
	}

	public bool IsSerialized()
	{ return m_IsSerialized; }

	public void LeftClickedHex(GameObject hex)
	{
		HexTileSerializable toSave = new HexTileSerializable(hex.transform, m_SelectedType, hex.transform.GetInstanceID());
		m_SavedHex[hex.transform.GetInstanceID()] = toSave;
		Color transparent = hex.renderer.material.color;
		transparent.a = 0f;
		hex.renderer.material.color = transparent;
		GameObject child = GameObject.Instantiate(m_HexTypePrefabs[(int)m_SelectedType], hex.transform.position, hex.transform.rotation) as GameObject;
		child.transform.parent = hex.transform;
	}

	public void RightClickHex(GameObject hex)
	{
		m_SavedHex.Remove(hex.transform.GetInstanceID());
	}

	public void SerializeHexes()
	{
		m_ToSerialize.Clear();
		foreach(KeyValuePair<int, HexTileSerializable> kvp in m_SavedHex)
		{
			Collider[] hitColliders = Physics.OverlapSphere(kvp.Value.GetPosition(),1.5f);
			List<int> partners = new List<int>();
			foreach(Collider c in hitColliders)
			{
				int id = c.gameObject.transform.GetInstanceID();
				if(id != kvp.Key && m_SavedHex.ContainsKey(id))
				{
					partners.Add(id);
				}
			}
			var toAdd = kvp.Value;
			toAdd.AddNeighborList(partners);
			m_ToSerialize.Add(toAdd);
		}
		UnityXMLSerializer.SerializeToXMLFile<List<HexTileSerializable>>(@m_SavePath, m_ToSerialize, true);
	}

	public void DeSerializeHexes()
	{
		List<HexTileSerializable> hexList;
		if(Application.platform == RuntimePlatform.OSXWebPlayer
		   || Application.platform == RuntimePlatform.WindowsWebPlayer)
		{
			hexList = UnityXMLSerializer.DeserializeFromXMLFile<List<HexTileSerializable>>(m_TextToLoad);
		}
		else
		{
			hexList = UnityXMLSerializer.DeserializeFromXMLFile<List<HexTileSerializable>>(@m_LoadPath);
		}
		 
		foreach(HexTileSerializable hex in hexList)
		{
			GameObject spawnedHex = GameObject.Instantiate(m_HexTypePrefabs[(int)hex.type], hex.GetPosition(), Quaternion.identity) as GameObject;
			if(hex.type != HexType.ParisCenter)
			{
				Tile t = spawnedHex.GetComponent<Tile>();
				t.AddNeighbors(hex.neighbors, hex.ID);
			}

			spawnedHex.transform.Rotate(new Vector3(0f,270f,0f));
			//spawnedHex.transform.Translate(new Vector3(0,-0.1f,0));
		}
		m_IsSerialized = true;
	}

}


//MUST MATCH THE POSITION OF HEX TYPE PREFABS
[System.Serializable]
public enum HexType
{
	Paris = 0,
	White,
	Blue,
	Purple,
	Red,
	Terrain,
	ParisCenter
};
