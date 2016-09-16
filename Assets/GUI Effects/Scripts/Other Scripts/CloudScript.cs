using UnityEngine;
using System.Collections;

public class CloudScript : MonoBehaviour {

	private float velocityValue = 0;

	// Use this for initialization
	void Start () {
		velocityValue = Random.Range( -3.0f , 0f );
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		GetComponent<Rigidbody2D>().velocity = new Vector3( velocityValue,0,0 );
		if( transform.localPosition.x <= - 15 ){
			GameObject.Find( "Env").GetComponent<EnvironmentScript>().CreateClouds();
			Destroy( gameObject );
		}
	}
}
