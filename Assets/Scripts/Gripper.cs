using System;
using UnityEngine;
using UnityEngine.UI;

public class Gripper : MonoBehaviour
{
  [SerializeField] private float m_MovePower = 500; // The force added to the gripper to move it.

  public static float gripperStatus = 1f;
  public static bool linked = false;
  public static float movementMagnitude = 100f;

  private const float k_GroundRayLength = 1f; // The length of the ray to check if the gripper is grounded.
  private Rigidbody finger1;
  private Rigidbody finger2;
  private GameObject gripperCenter;

  public float gripper_spped = .9f;

  public float max_velocity = 1f;
  public bool smooth;
  public float smoothTime = 5f;
  public Vector3 lastPosition;
  public Vector3 lastRotation;


  private Vector3 initialGripperPosition;



  private void Start()
  {

    finger1 = GameObject.Find("finger1").GetComponent<Rigidbody>();
    finger2 = GameObject.Find("finger2").GetComponent<Rigidbody>();
    gripperCenter = GameObject.Find("gripper_center");

    finger1.maxDepenetrationVelocity = max_velocity;
    finger2.maxDepenetrationVelocity = max_velocity;
    gripperCenter.GetComponent<Rigidbody> ().maxDepenetrationVelocity = max_velocity;
    finger1.maxAngularVelocity = max_velocity;
    finger2.maxAngularVelocity = max_velocity;
    gripperCenter.GetComponent<Rigidbody> ().maxAngularVelocity = max_velocity;

    initialGripperPosition = gripperCenter.transform.position;
  }


  public void Move(Vector3 moveDirection, Quaternion rotate)
  {

    if (Mathf.Abs(gripperStatus) < .5f) {
      //print(linked);
      if (!linked) {
        ConfigurableJoint finger1Joint = finger1.GetComponent<ConfigurableJoint>();
        if(finger1Joint.anchor.z < .78)
        finger1Joint.anchor += new Vector3(0,0,.01f);
        ConfigurableJoint finger2Joint = finger2.GetComponent<ConfigurableJoint>();
        if(finger2Joint.anchor.z > -.78)
        finger2Joint.anchor += new Vector3(0,0,-.01f);
      }
      //				finger1.AddRelativeForce(new Vector3(0,0,-.002f)*m_MovePower);
      //				finger2.AddRelativeForce(new Vector3(0,0,.002f)*m_MovePower);
      //				gripperCenter.GetComponent<Rigidbody>().a(new Vector3(0,0,-1000*m_MovePower));
      //				gripperCenter.GetComponent<Rigidbody>().AddForce(new Vector3(0,0,1000*m_MovePower));
    } else if (Mathf.Abs(gripperStatus-1) < .5f) {
      Destroy(GameObject.Find("gripper_center").GetComponent<FixedJoint>());
      linked = false;
      ConfigurableJoint finger1Joint = finger1.GetComponent<ConfigurableJoint>();
      if(finger1Joint.anchor.z > 0)
      finger1Joint.anchor += new Vector3(0,0,-.01f);
      ConfigurableJoint finger2Joint = finger2.GetComponent<ConfigurableJoint>();
      if(finger2Joint.anchor.z < 0)
      finger2Joint.anchor += new Vector3(0,0,.01f);
      Destroy(GameObject.Find("finger1").GetComponent("FixedJoint"));
      Destroy(GameObject.Find("finger2").GetComponent("FixedJoint"));
    }
    if (Record_playback.mode != Record_playback.Modes.Record)
    return;
    //		gripperCenter.GetComponent<Rigidbody>().position = Vector3.MoveTowards(
    //			gripperCenter.GetComponent<Rigidbody>().transform.position,
    //			gripperCenter.GetComponent<Rigidbody>().transform.position + moveDirection*m_MovePower , smoothTime * Time.deltaTime);
    initialGripperPosition = initialGripperPosition +
    (gripperCenter.transform.position - initialGripperPosition - moveDirection)/10f;
    Vector3 forceToApply = (initialGripperPosition + moveDirection - gripperCenter.transform.position) * m_MovePower;
    if(forceToApply.magnitude > 100)
    forceToApply = forceToApply.normalized*100;
    gripperCenter.GetComponent<Rigidbody>().AddForce(forceToApply);


    //finger1.transform.RotateAround(gripperCenter.transform.position, rotate.eulerAngles, smoothTime * Time.deltaTime);
    gripperCenter.transform.localRotation =
    rotate;

  }


  public void FixedUpdate() {

    // update the energy bar
    // movementMagnitude -= Vector3.Magnitude (transform.position - lastPosition);
    // movementMagnitude -= Vector3.Magnitude (transform.rotation.eulerAngles/180.0f - lastRotation)/2f;
    // movementMagnitude -= 0.05f;
    lastPosition = transform.position;
    lastRotation = transform.rotation.eulerAngles/180.0f;
    Slider slider = GameObject.Find("energySlider").GetComponent<Slider>();
    slider.value = movementMagnitude / 100f;


    //		GameObject fingertip = GameObject.Find ("fingertip1");
    //		foreach (Collider collider in Physics.OverlapSphere(fingertip.transform.position, attractionRadius)) {
    //			// calculate direction from target to me
    //			Vector3 forceDirection = fingertip.transform.position - collider.transform.position;
    //
    //			if(collider.GetComponent<Rigidbody>() != null) {
    //				collider.GetComponent<Rigidbody>().AddForce(forceDirection.normalized * attractionForce * Time.fixedDeltaTime);
    //			}
    //		}
    //		fingertip = GameObject.Find ("fingertip2");
    //		foreach (Collider collider in Physics.OverlapSphere(fingertip.transform.position, attractionRadius)) {
    //			Vector3 forceDirection = fingertip.transform.position - collider.transform.position;
    //
    //			if(collider.GetComponent<Rigidbody>() != null) {
    //				collider.GetComponent<Rigidbody>().AddForce(forceDirection.normalized * attractionForce * Time.fixedDeltaTime);
    //			}
    //		}
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
