using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace UnityStandardAssets.Vehicles.Gripper
{
    public class GripperUserControl : MonoBehaviour
    {
		public float rotation_speed = 50f;

		private float mouseX = 0f;
		private float mouseY = 0f;
		private float initialMouseX = 0f;
		private float initialMouseY = 0f;
		private float lastToggleTime = -10f;
        private Gripper gripper; // Reference to the gripper controller.

		private Quaternion rotate;
        private Vector3 move;
        // the world-relative desired move direction, calculated from the camForward and user input.

        private Transform cam; // A reference to the main camera in the scenes transform
        private Vector3 camForward; // The current forward direction of the camera
        private bool jump; // whether the jump button is currently pressed


        private void Awake()
        {
            // Set up the reference.
            gripper = GetComponent<Gripper>();

			initialMouseX = Input.mousePosition.x;
			initialMouseY = Input.mousePosition.y;
			rotate = this.transform.localRotation;
            
        }


        private void Update()
        {
            // Get the axis and jump input.

            float y = CrossPlatformInputManager.GetAxis("Horizontal");
			float x = CrossPlatformInputManager.GetAxis("Fire3");
			float z = CrossPlatformInputManager.GetAxis("Vertical");

			mouseX = (Input.mousePosition.x - initialMouseX)/100f;
			mouseY = (Input.mousePosition.y - initialMouseY)/100f;
			initialMouseX = Input.mousePosition.x;
			initialMouseY = Input.mousePosition.y;

			float a = mouseX * rotation_speed;
			float b = mouseY * rotation_speed;
			float g = CrossPlatformInputManager.GetAxis("Mouse ScrollWheel") * rotation_speed;

			// we use world-relative directions in the case of no main camera
			move = (y*Vector3.forward + z*Vector3.up + x*Vector3.left).normalized;
			rotate *= Quaternion.Euler (a, b, g);

		}
		
		private void FixedUpdate()
        {
            // Call the Move function of the gripper controller
            gripper.Move(move, rotate);
			if (CrossPlatformInputManager.GetAxis ("Fire2") != 0f) {
				if(Time.time - lastToggleTime > 1) {
					gripper.Toggle ();
					lastToggleTime = Time.time;
				}
			}
        }

    }
}
