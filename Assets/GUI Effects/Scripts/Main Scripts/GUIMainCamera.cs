using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class GUIMainCamera : MonoBehaviour {


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ButtonDismissExample(){
		EventSystem.current.currentSelectedGameObject.GetComponent<GUIEffects>().DismissObjects();
	}
	
	public void ReplayScene(){
		Application.LoadLevel( Application.loadedLevel );
	}

	/*
	public void PauseMenuExample(){

	}
	*/
}
