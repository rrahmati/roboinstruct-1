using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class GripperUserControl : MonoBehaviour
{

	private float mouseX = 0f;
	private float mouseY = 0f;
	private float initialMouseX = 0f;
	private float initialMouseY = 0f;
	public static float lastToggleTime = -10f;
	private Gripper gripper; // Reference to the gripper controller.
	float z = 0;
	private Quaternion rotate;
	private Vector3 move;
	// the world-relative desired move direction, calculated from the camForward and user input.

	private Transform cam; // A reference to the main camera in the scenes transform
	private Vector3 camForward; // The current forward direction of the camera
	private bool jump; // whether the jump button is currently pressed


	private void Awake ()
	{
		// Set up the reference.
		gripper = GetComponent<Gripper> ();
		initialMouseX = Input.mousePosition.x;
		initialMouseY = Input.mousePosition.y;
		rotate = this.transform.localRotation;
		//Cursor.visible = false;

	}

	private void FixedUpdate ()
	{
		if (UI.showTermsWindow)
			return;
		if (Time.realtimeSinceStartup > 1) {

			// Rect screenRect = new Rect(0,0, Screen.width, Screen.height);
			// if (!screenRect.Contains (Input.mousePosition)) {
			// 	Cursor.lockState = CursorLockMode.Locked;
			// 	if(Cursor.lockState == CursorLockMode.Locked) {
			// 		initialMouseX -= Input.mousePosition.x - Screen.width/2;
			// 		initialMouseY -= Input.mousePosition.y - Screen.height/2;
			// 	}
			// 	Cursor.lockState = CursorLockMode.None;
			// }

			// Get the axis input.
			float a = CrossPlatformInputManager.GetAxis ("Fire3");
			float b = CrossPlatformInputManager.GetAxis ("Horizontal");
			float g = CrossPlatformInputManager.GetAxis ("Vertical");

			mouseX = (Input.mousePosition.x - initialMouseX) / 50f;
			mouseY = (Input.mousePosition.y - initialMouseY) / 50f;

			float y = mouseX;
			float x = mouseY;
			//		x = CrossPlatformInputManager.GetAxis("Mouse Y");
			//		y = CrossPlatformInputManager.GetAxis("Mouse X");
			z += CrossPlatformInputManager.GetAxis ("Mouse ScrollWheel") / 1.5f;
			z += CrossPlatformInputManager.GetAxis ("position_z") / 50f;

			// we use world-relative directions in the case of no main camera
			move = (y * Vector3.forward + z * Vector3.up + x * Vector3.left);
			rotate *= Quaternion.Euler (a, b, g);




			// Call the Move function of the gripper controller
			gripper.Move (move, rotate);
			if (CrossPlatformInputManager.GetAxis ("Fire2") != 0f) {
				if (Time.time - lastToggleTime > 1) {
					gripper.Toggle ();
					lastToggleTime = Time.time;
				}
			}
		}
	}

}
