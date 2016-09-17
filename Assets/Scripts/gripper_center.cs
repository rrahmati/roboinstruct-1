using UnityEngine;
using System.Collections;

public class gripper_center : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void FixedUpdate() {

	}

	void OnJointBreak(float breakForce) {
		Gripper.linked = false;
	}
}
