using UnityEngine;
using System.Collections;

public class WidthResize : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		ResizeWidth();
	}

	void ResizeWidth()
	{
		SpriteRenderer sr=GetComponent<SpriteRenderer>();
		if(sr==null) return;
		
		transform.localScale=new Vector3(1,1,1);
		
		float width=sr.sprite.bounds.size.x;
		
		float worldScreenHeight=Camera.main.orthographicSize*2f;
		float worldScreenWidth=worldScreenHeight/Screen.height*Screen.width;
		
		Vector3 xWidth = transform.localScale;
		xWidth.x=worldScreenWidth / width;
		transform.localScale=xWidth;
		
	}
}

