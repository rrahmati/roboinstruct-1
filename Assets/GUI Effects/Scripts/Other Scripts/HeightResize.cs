using UnityEngine;
using System.Collections;

public class HeightResize : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		ResizeHeight();
	}
	
	void ResizeHeight()
	{
		SpriteRenderer sr=GetComponent<SpriteRenderer>();
		if(sr==null) return;
		
		transform.localScale=new Vector3(1,1,1);

		float height=sr.sprite.bounds.size.y;
		
		
		float worldScreenHeight=Camera.main.orthographicSize*2f;

		Vector3 yHeight = transform.localScale;
		yHeight.y=worldScreenHeight / height;
		transform.localScale=yHeight;
		
	}
}
