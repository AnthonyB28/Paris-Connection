using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HexEditor : MonoBehaviour {

	private bool markDelete = true;
	private Color originalColor;

	void Awake () 
	{
		if(!Editor.instance.m_EnabledEditor) 
		{
			this.enabled = false;
		} 
		else
		{
			if(this.enabled)
			{
				originalColor = this.renderer.material.color;
				DeleteColor();
				gameObject.name = transform.GetInstanceID().ToString();
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseOver()
	{
		if(this.enabled)
		{
			if(Input.GetMouseButtonDown(0))
			{
				DeleteChild();
				if(!markDelete)
					Editor.instance.LeftClickedHex(this.gameObject);
			}
			if(Input.GetMouseButtonDown(1))
			{
				DeleteChild();
				if(!markDelete)
				{
					markDelete = true;
					DeleteColor();
					Editor.instance.RightClickHex(this.gameObject);
				}
				else
				{
					markDelete = false;
					this.renderer.material.color = originalColor;
				}
			}
		}

	} 

	void DeleteColor()
	{
		Color deleteColor = this.renderer.material.color;
		deleteColor = Color.red;
		deleteColor.a = 0.1f;
		this.renderer.material.color = deleteColor;
	}

	void DeleteChild()
	{
		if(this.transform.childCount > 0)
		{
			Transform old = this.transform.GetChild(0);
			Destroy(old.gameObject);
		}
	}

	private void OnDrawGizmosSelected() {
		if(this.enabled)
		{
			Gizmos.color = Color.red;
			//Use the same vars you use to draw your Overlap SPhere to draw your Wire Sphere.
			Gizmos.DrawWireSphere (transform.position, 1.5f);
		}

	}
}
