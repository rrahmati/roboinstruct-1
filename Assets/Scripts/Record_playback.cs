using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

public class Record_playback : MonoBehaviour
{

	public static Modes mode = Modes.PlayPrediction; 		// Record, Playback, PlayPrediction
	// Record is used when the user wants to freely move the gripper and demonstrate a trajectory
	// Playback is used to playback object poses from a recorded file in the Record mode
	// PlayPrediction is used to report current environment to neural networks and play their prediction in real-time

	public static int visualizeObject = 0;							// Enable only when visualizing the real world objects from Kinect in Unity
	private string playbackFileName = "blocks/predictions/sceneState";
	private string predictionFileName = "blocks/predictions/prediction";
	public float task_length = 100;
	public float box_goal_z = 1f;
	GameObject finger1;
	GameObject finger2;
	GameObject[] objects;
	List<GameObject> objectsToRecord;
	List<ObjectTransformation> transformations = new List<ObjectTransformation> ();
	public Vector3 nextGripperPosition;
	Quaternion nextGripperRotation;
	float nextGripperStatus;
	public Vector3 objectVisualPosition;
	Quaternion objectVisualRotation;
	List<float> gripperStatus = new List<float> ();
	int totalFramesRecorded = 0;
	int recentFramesRecorded = 0;
	int counter = 0;
	int playID;
	int framesCountToSend = 30;
	float recordDelay = .03f;
	float lastRecordTime = 0f;
	bool predictionUpdated = false;

	public enum Modes
	{
		Record,
		Playback,
		PlayPrediction }
	;
	// Use this for initialization
	void Start ()
	{
		if (mode == Modes.Record)
			playbackFileName = "blocks/trajectories/" + UnityEngine.Random.Range (0, 10000000);
		objectsToRecord = GameObject.FindGameObjectsWithTag ("movable").ToList<GameObject> ();

		objectsToRecord.Sort (delegate(GameObject c1, GameObject c2) {
			return c1.name.CompareTo (c2.name);
		});
		foreach (GameObject gameobject in objectsToRecord) {
			transformations.Add (new ObjectTransformation (gameobject.name));
		}

		finger1 = GameObject.Find ("finger1");
		finger2 = GameObject.Find ("finger2");
		playID = UnityEngine.Random.Range (1, 1000000000);


		if (mode == Modes.Record) {
			File.Delete (playbackFileName);
			writeFileHeader (true, false);
		} else {
			float physicsChange = .01f;
			finger1.GetComponent<Rigidbody> ().mass = physicsChange;
			finger1.GetComponent<Rigidbody> ().drag = physicsChange;
			finger1.GetComponent<Rigidbody> ().angularDrag = physicsChange;
			finger2.GetComponent<Rigidbody> ().mass = physicsChange;
			finger2.GetComponent<Rigidbody> ().drag = physicsChange;
			finger2.GetComponent<Rigidbody> ().angularDrag = physicsChange;
			if (mode == Modes.Playback) {
				print ("Playing back...");
				readPlaybackFile ();
			} else {
				framesCountToSend = 1;
				readPredictionFile ();
			}
		}
	}

	// Update is called once per frame
	void FixedUpdate ()
	{


		if (Time.time > lastRecordTime + recordDelay) {

			lastRecordTime = Time.time;
			if (mode == Modes.Record) {
				//System.Random rand = new System.Random(); //reuse this if you are generating many
				//double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
				//double u2 = rand.NextDouble();
				//double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
				//             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
				//recordDelay = 0.03f + 0.002f * (float)randStdNormal; //random normal(mean,stdDev^2)
				//print(recordDelay);

				foreach (ObjectTransformation objectTransformation in transformations) {
					objectTransformation.positions.Add (objectTransformation.gameObject.transform.position);
					objectTransformation.rotations.Add (objectTransformation.gameObject.transform.rotation);
				}
				gripperStatus.Add (Gripper.gripperStatus);
				recentFramesRecorded++;
				totalFramesRecorded++;
				if (totalFramesRecorded % framesCountToSend == 0) {
					writeToFile (true, false);
				}

			} else {
				if (mode == Modes.Playback) {
					if (counter < totalFramesRecorded) {
						foreach (ObjectTransformation objectTransformation in transformations) {
							if (objectTransformation.gameObject.name != "gripper_center")
								continue;
							//Vector3 towardsNextPosition =  objectTransformation.positions[counter] - objectTransformation.gameObject.transform.position;
							//objectTransformation.gameObject.GetComponent<Rigidbody>().AddForce(towardsNextPosition * 15);
							// objectTransformation.gameObject.transform.position = objectTransformation.positions[counter];

							objectTransformation.gameObject.transform.position = Vector3.Lerp (
                               objectTransformation.gameObject.transform.position, objectTransformation.positions [counter], Time.deltaTime * 200);

							objectTransformation.gameObject.transform.rotation = Quaternion.
                                Slerp (objectTransformation.gameObject.transform.rotation, objectTransformation.rotations [counter], Time.deltaTime * 200);
						}
						Gripper.gripperStatus = gripperStatus [counter];
						counter++;
					}
				} else if (mode == Modes.PlayPrediction) {
					playPrediction ();

				}
			}
		}


	}

	public void playPrediction ()
	{
		if (counter > 1) {
			// Vector3 towardsNextPosition = nextGripperPosition - GameObject.Find("gripper_center").transform.position;
			// GameObject.Find("gripper_center").GetComponent<Rigidbody>().AddForce(towardsNextPosition * .1f, ForceMode.Impulse);

			// Vector3 x = Vector3.Cross(GameObject.Find("gripper_center").transform.rotation.eulerAngles.normalized, nextGripperRotation.eulerAngles.normalized);
			// print(x);
			// float theta = Mathf.Asin(x.magnitude);
			// Vector3 w = x.normalized * theta / Time.fixedDeltaTime;
			// Quaternion q = GameObject.Find("gripper_center").transform.rotation * GameObject.Find("gripper_center").GetComponent<Rigidbody>().inertiaTensorRotation;
			// Vector3 T = q * Vector3.Scale(GameObject.Find("gripper_center").GetComponent<Rigidbody>().inertiaTensor, (Quaternion.Inverse(q) * w));
			// GameObject.Find("gripper_center").GetComponent<Rigidbody>().AddTorque(T, ForceMode.Impulse);

			// GameObject.Find("gripper_center").transform.position = nextGripperPosition;
			// GameObject.Find("gripper_center").transform.rotation = nextGripperRotation;

			GameObject.Find ("gripper_center").transform.position = Vector3.Lerp (
                GameObject.Find ("gripper_center").transform.position, nextGripperPosition, Time.deltaTime * 15);
			GameObject.Find ("gripper_center").transform.rotation = Quaternion.
               Slerp (GameObject.Find ("gripper_center").transform.rotation, nextGripperRotation, Time.deltaTime * 15);
			if (visualizeObject == 1) {
				GameObject.Find ("box (1)").transform.position = objectVisualPosition;
				GameObject.Find ("box (1)").transform.rotation = objectVisualRotation;
			}
		}

		foreach (ObjectTransformation objectTransformation in transformations) {
			objectTransformation.positions.Add (objectTransformation.gameObject.transform.position);
			objectTransformation.rotations.Add (objectTransformation.gameObject.transform.rotation);
		}
		Gripper.gripperStatus = nextGripperStatus;
		gripperStatus.Add (Gripper.gripperStatus);
		recentFramesRecorded++;
		if (visualizeObject != 1)
			writeToFile (true, false);
		predictionUpdated = false;
	}

	public void writeFileHeader (bool writeLocal, bool sendToServer)
	{
		string lines = "";
		lines += DateTime.UtcNow.ToString ("yyyy-MM-dd hh:mm:ss.fff") + "\n";
		for (int obj = 0; obj < transformations.Count; obj++) {
			lines += transformations [obj].gameObject.name + ",";
		}
		lines += "\n";
		lines += "time,task,try_in_task,gripper" + ",";
		for (int obj = 0; obj < transformations.Count; obj++) {
			lines += transformations [obj].gameObject.name + "_p_x" + ","
				+ transformations [obj].gameObject.name + "_p_y" + ","
				+ transformations [obj].gameObject.name + "_p_z" + ","
				+ transformations [obj].gameObject.name + "_r_x" + ","
				+ transformations [obj].gameObject.name + "_r_y" + ","
				+ transformations [obj].gameObject.name + "_r_z" + ","
				+ transformations [obj].gameObject.name + "_r_w" + ",";
		}
		lines += "task_length,box_goal_z\n";
		if (writeLocal)
			File.WriteAllText (playbackFileName, lines);
		if (sendToServer)
			StartCoroutine (DoWWW (lines));

	}

	public void writeToFile (bool writeLocal, bool sendToServer)
	{

		string lines = "";
		if (mode == Modes.PlayPrediction) {
			lines += "time,task,try_in_task,gripper" + ",";
			for (int obj = 0; obj < transformations.Count; obj++) {
				lines += transformations [obj].gameObject.name + "_p_x" + ","
					+ transformations [obj].gameObject.name + "_p_y" + ","
					+ transformations [obj].gameObject.name + "_p_z" + ","
					+ transformations [obj].gameObject.name + "_r_x" + ","
					+ transformations [obj].gameObject.name + "_r_y" + ","
					+ transformations [obj].gameObject.name + "_r_z" + ","
					+ transformations [obj].gameObject.name + "_r_w" + ",";
			}
			lines += "task_length,box_goal_z,\n";
		}
		for (int frame = 0; frame < recentFramesRecorded; frame++) {
			lines += DateTime.UtcNow.ToString ("yyyy-MM-dd-hh:mm:ss.fff") + ",";
			lines += UI.level + ",";
			lines += UI.tryInLevel + ",";
			lines += gripperStatus [frame] + ",";
			for (int obj = 0; obj < transformations.Count; obj++) {
				lines += transformations [obj].positions [frame].x + ","
					+ transformations [obj].positions [frame].y + ","
					+ transformations [obj].positions [frame].z + ","
					+ transformations [obj].rotations [frame].x + ","
					+ transformations [obj].rotations [frame].y + ","
					+ transformations [obj].rotations [frame].z + ","
					+ transformations [obj].rotations [frame].w + ",";
			}
			lines += task_length + "," + box_goal_z + ",\n";
		}
		foreach (ObjectTransformation transformation in transformations) {
			transformation.positions.RemoveRange (0, framesCountToSend);
			transformation.rotations.RemoveRange (0, framesCountToSend);
		}
		gripperStatus.RemoveRange (0, framesCountToSend);
		recentFramesRecorded -= framesCountToSend;
		if (writeLocal)
		if (mode == Modes.PlayPrediction)
			File.WriteAllText (playbackFileName, lines);
		else
			File.AppendAllText (playbackFileName, lines);
		if (sendToServer)
			StartCoroutine (DoWWW (lines));

	}

	private IEnumerator DoWWW (string lines)
	{
		var form = new WWWForm ();
		form.AddField ("time", DateTime.UtcNow.ToString ("yyyy-MM-dd hh:mm:ss.fff"));
		form.AddField ("fileContent", lines);
		form.AddField ("playID", playID + "");
		WWW download = new WWW ("http://www.nemleca.com/upload_v1.php", form);
		yield return download;
		Debug.Log (download.data);
	}

	public void readPlaybackFile ()
	{
		var reader = new StreamReader (File.OpenRead (playbackFileName));
		reader.ReadLine ();
		reader.ReadLine ();
		reader.ReadLine ();
		for (int frame = 0; !reader.EndOfStream; frame++) {
			totalFramesRecorded = frame;
			var line = reader.ReadLine ();
			var values = line.Split (',');
			int valuesCounter = 0;
			int level = int.Parse (values [valuesCounter++]);
			int try_in_level = int.Parse (values [valuesCounter++]);
			gripperStatus.Add (float.Parse (values [valuesCounter++]));
			for (int obj = 0; obj < transformations.Count; obj++) {
				transformations [obj].positions.Add (new Vector3 (
					float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++])));
				transformations [obj].rotations.Add (new Quaternion (
					float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]),
					float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++])));
			}
		}
	}

	public void readPredictionFile ()
	{
		StartCoroutine (readPredictionFileCoroutine ());

	}

	public IEnumerator readPredictionFileCoroutine ()
	{

		while (true) {

			int gripperIndex = 0;
			for (int obj = 0; obj < transformations.Count; obj++) {
				if (transformations [obj].gameObject.name == "gripper_center")
					gripperIndex = obj;
			}

			int frame;
			var line = "";
			try {
				if (visualizeObject == 1) {
					var reader1 = new StreamReader (File.Open (playbackFileName, FileMode.Open));
					reader1.ReadLine ();
					line = reader1.ReadLine ();
					reader1.Close ();
					var values = line.Split (',');
					int valuesCounter = 9;
					objectVisualPosition = new Vector3 (
											float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]));
					objectVisualRotation = new Quaternion (
									float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]),
									float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]));

				}
				var reader = new StreamReader (File.Open (predictionFileName, FileMode.Open));
				for (frame = 0; !reader.EndOfStream; frame++) {
					line = reader.ReadLine ();

				}

				reader.Close ();
				if (frame > 0) {
					var values = line.Split (',');
					int valuesCounter = 0;
					nextGripperStatus = float.Parse (values [valuesCounter++]);
					//valuesCounter += 7;
					Vector3 newGripperPosition = new Vector3 (
                        float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]));
					if (Vector3.SqrMagnitude (newGripperPosition - nextGripperPosition) > 9.9E-11f) {
						predictionUpdated = true;
						counter++;
					}
					nextGripperPosition = newGripperPosition;
					nextGripperRotation = new Quaternion (
                    float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]),
                    float.Parse (values [valuesCounter++]), float.Parse (values [valuesCounter++]));
					//-0.000611848f, 0.00111663f, 0.243943f, 0.976817f
					//);
					totalFramesRecorded++;
					//print(frame);
					//print(totalFramesRecorded);

				}
			} catch (Exception e) {
				print (e.Message);
			}
			yield return new WaitForSeconds (recordDelay);
		}
	}

}

public class ObjectTransformation
{

	public GameObject gameObject;
	public List<Vector3> positions = new List<Vector3> ();
	public List<Quaternion> rotations = new List<Quaternion> ();

	public ObjectTransformation (string objectName)
	{
		gameObject = GameObject.Find (objectName);
	}



}
 
 
 
