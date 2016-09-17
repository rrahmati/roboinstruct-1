using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FingerTip : MonoBehaviour
{

	public static GameObject collidedObject;
	// Use this for initialization
	void Start ()
	{

	}

	// Update is called once per frame
	void Update ()
	{

	}

	public void OnTriggerEnter (Collider collision)
	{
		if (collidedObject != null && collision.gameObject == collidedObject && collidedObject.tag == "movable" && !Gripper.linked
			&& UI.level != 6 && UI.level != 7 && UI.level != 18 && Record_playback.visualizeObject != 1) {
			FixedJoint joint = GameObject.Find ("gripper_center").AddComponent<FixedJoint> ();
			joint.connectedBody = collidedObject.GetComponent<Rigidbody> ();
			joint.breakForce = 10f;
			Gripper.linked = true;
			// print("linked");
		}
		if (collision.gameObject.tag == "movable") {
			collidedObject = collision.gameObject;
		}
	}

	public void OnTriggerExit (Collider collision)
	{
		collidedObject = null;
	}
}
