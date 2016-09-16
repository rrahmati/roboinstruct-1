using UnityEngine;
using System.Collections;

public class EnvironmentScript : MonoBehaviour {

	public GameObject[] clouds ;

	// Use this for initialization
	void Start () {
		for( int i = 0 ; i < 6; i ++){
			CreateClouds();
		}
	}
	
	// Update is called once per frame
	public void CreateClouds () {
		int whichCloud = Random.Range( 0,6 );
		GameObject.Instantiate( clouds[whichCloud], new Vector3( 15.0f, Random.Range( 2.0f, 4.0f), 0f), Quaternion.identity );
	}
}
