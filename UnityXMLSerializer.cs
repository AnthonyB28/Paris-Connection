/*!**********************************************

 * XMLSerializer.cs

 * 18 April 2013

 * V 1.0

 * Cahman Games

 * www.cahman.com

 * 

 * 

 * Provides static functions for 

 * 1) serializing to XML then saving playerprefs

 * 2) serializing to XML, then saving to a local file

 * 3) serializing to XML, then sending to a web address

 * 

 *

 *************************************************/

/***************************************************

 * LICENSE:

 * Copyright (c) 2013 CahmanGames www.cahman.com

 * 

 * This software is provided 'as-is', without any express or implied

 * warranty. In no event will the authors be held liable for any damages

 * arising from the use of this software.

 * 

   **************************************************************/



using UnityEngine;

using System.Collections.Generic;

using System.Xml.Serialization;

using System.IO;

using System;



public static class UnityXMLSerializer{
	
	
	
	private static System.Type[] extraSerializeTypes;
	
	
	
	//public static void SetupSerializer(System.Type[] serializeTypes)
	
	//{
	
	//  serializer = new XmlSerializer(
	
	
	
	
	
	/// <summary>
	
	/// Serialize an object to an xml file.
	
	/// </summary>
	
	/// <returns>
	
	/// True if written, false if file exists and overWrite is false
	
	/// </returns>
	
	/// <param name='writePath'>
	
	/// Where to write the file.  Consider using Application.PersistentData
	
	/// </param>
	
	/// <param name='serializableObject'>
	
	/// The Object to be serialized.  The generic needs to be the type of the object to be serialized
	
	/// </param>
	
	/// <param name='overWrite'>
	
	/// If set to <c>true</c> over write the file if it exists.
	
	/// </param>
	
	public static bool SerializeToXMLFile<T>(string writePath, object serializableObject, bool overWrite = true)
	{
		if(File.Exists(writePath) && overWrite == false)
			
			return false;
		
		XmlSerializer serializer = new XmlSerializer(typeof(T));
		
		using( var writeFile = File.Create(writePath))
			
		{
			
			serializer.Serialize(writeFile, serializableObject);
			
		}
		
		return true;
		
	}


	
	/// <summary>
	
	/// Deserialize an object from an XML file.
	
	/// </summary>
	
	/// <returns>
	
	/// The deserialized list.  If the file doesn't exist, returns the default for 'T'
	
	/// </returns>
	
	/// <param name='readPath'>
	
	/// Where to read the file from.
	
	/// </param>
	
	/// <typeparam name='T'>
	
	/// Type of object being loaded from the file
	
	/// </typeparam>
	/// 

	public static T DeserializeFromXMLFile<T>(string readPath)
		
	{
		
		if(!File.Exists (readPath))
			
			return default(T);
		
		
		
		XmlSerializer serializer = new XmlSerializer(typeof(T));
		
		
		
		using (var readFile = File.OpenRead (readPath))
			
			return (T)serializer.Deserialize(readFile);
	}

	public static T DeserializeFromXMLFile<T>(TextAsset file)
		
	{
		XmlSerializer serializer = new XmlSerializer(typeof(T));

		using (var readFile = new System.IO.StringReader(file.text))
			
			return (T)serializer.Deserialize(readFile);
	}
}

[XmlRoot("Hex")]
public struct HexTileSerializable
{
	public SerializableVector3 position;
	public SerializableVector3 scale;
	public HexType type;
	public int ID;
	public List<int> neighbors;
	
	public HexTileSerializable(Transform transform, HexType t, int id)
	{
		this.position = new SerializableVector3(transform.position);
		this.scale = new SerializableVector3(transform.localScale);
		this.type = t;
		this.ID = id;
		neighbors = new List<int>();
	}

	public void AddNeighborList(List<int> nList)
	{
		neighbors = nList;
	}
	
	public Vector3 GetPosition()
	{
		return position.GetVector();
	}
};

[Serializable]
public class SerializableVector3
{
	public double X;
	public double Y;
	public double Z;
	
	public Vector3 GetVector()
	{
		return new Vector3((float)X, (float)Y, (float)Z);    
	}
	
	public SerializableVector3() { }
	public SerializableVector3(Vector3 vector) 
	{
		double val;
		X = double.TryParse(vector.x.ToString(), out val) ? val : 0.0;
		Y = double.TryParse(vector.y.ToString(), out val) ? val : 0.0;
		Z = double.TryParse(vector.z.ToString(), out val) ? val : 0.0;
	}
}