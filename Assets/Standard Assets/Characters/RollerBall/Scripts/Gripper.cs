using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Gripper
{
    public class Gripper : MonoBehaviour
    {
        [SerializeField] private float m_MovePower = 500; // The force added to the gripper to move it.
        [SerializeField] private bool m_UseTorque = false; // Whether or not to use torque to move the gripper.
        [SerializeField] private float m_MaxAngularVelocity = 25; // The maximum velocity the gripper can rotate at.

		private float gripperStatus = -1f;

        private const float k_GroundRayLength = 1f; // The length of the ray to check if the gripper is grounded.
		private GameObject finger1;
		private GameObject finger2;
		private Vector3 finger1Open;
		private Vector3 finger2Open;
		private GameObject gripperCenter;

		public float gripper_spped = .5f;
		public bool smooth;
		public float smoothTime = 5f;



        private void Start()
        {

			finger1 = GameObject.Find("finger1");
			finger2 = GameObject.Find("finger2");
			gripperCenter = GameObject.Find("gripper_center");
			finger1Open = finger1.transform.localPosition;
			finger2Open = finger2.transform.localPosition;
        }


        public void Move(Vector3 moveDirection, Quaternion rotate)
        {
			if (gripperStatus == 0f) {
				finger1.transform.localPosition += new Vector3(0,0,-gripper_spped);;
				finger2.transform.localPosition -= new Vector3(0,0,-gripper_spped);;
			} else if (gripperStatus == 1f) {
				//finger1.transform.localPosition -= new Vector3(0,0,-gripper_spped);;
				//finger2.transform.localPosition += new Vector3(0,0,-gripper_spped);;
			}

			gripperCenter.GetComponent<Rigidbody>().AddForce(moveDirection*m_MovePower);
			//finger2.AddForce(moveDirection*m_MovePower);

			//finger1.transform.RotateAround(gripperCenter.transform.position, rotate.eulerAngles, smoothTime * Time.deltaTime);
			gripperCenter.transform.localRotation = Quaternion.Slerp (this.transform.localRotation, rotate, smoothTime * Time.deltaTime);

        }

		public void FixedUpdate() {
//			if (gripperStatus == 0f) {
//				finger1.velocity = new Vector3(0,0,-gripper_spped);
//				finger2.velocity = new Vector3(0,0,gripper_spped);
//			} else if (gripperStatus == 1f) {
//				finger1.velovelocity = new Vector3(0,0,gripper_spped);
//				finger2.velocity = new Vector3(0,0,-gripper_spped);
//			}



		}
			
			public void Toggle() {

			if (gripperStatus == 0f) {
				gripperStatus = 1f;
			} else {
				gripperStatus = 0f;
			}
		}
    }
}
