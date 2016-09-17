using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class UI : MonoBehaviour
{
	public int task;
	public static int tryInLevel = 0;
	public static int difficultyLevel = 4;
	public static int ObjectCreationDelay = 20;
	public static int minObjectCreationDelay = 10;
	public static int level;
	GameObject[] boxes;
	// level 2- put the box into the shelf
	// level 11- rotate 90 around y
	// level 12- rotate 180 aound y
	// level 13- rotate 90 around z
	// level 14- rotate -90 around z
	// level 15- rotate -90 around z and put the box into the shelf
	// level 16- rotate 90 around y by pushing not gripping and put the box into the shelf
	// level 17- rotate 180 around y by pushing not gripping and put the box into the shelf
	// level 18- push and rotate to reach the desired pose
	static int[] boxCount = new int[] {0,0,1,0,1,0,0,0,0,0,0,
                                         1,1,1,1,1,1,1,1,1,1}; // box count for each level
	static int[] levelTimeLimit = new int[] {50,30,50,50,350,50,20,50,50,50,50,
                                              50,50,50,50,50,50,50,180,50,50}; // box count for each level

	static int obstacleCount = 1;
	public static float lastLevelReset = 0f;
	float lastLevelCompleteCheck = 0f;
	float levelCompleteCheckInterval = 1f;
	public AudioClip levelSuccessAudio;
	public AudioClip levelFailedAudio;
	public AudioSource audioSource;
	public static int success = 0;
	public static int fail = 0;
	public float lastSuccessTime = 0f;
	public float lastFailTime = 0f;
	public float totalSuccessTime = 0f;
	public float total_time_error = 0f;
	public float total_box_z_error = 0f;
	public static UI ui;
	public static bool showTermsWindow = false;
	public static Vector2 taskLengthRange = new Vector2 (.3f, .31f);
	public static Vector2 boxGoalZRange = new Vector2 (-.8f, 1.2f);
	public static float expectedDuration = 0f;

	static IEnumerator goNextLevelCoroutine ()
	{
		ui.lastLevelCompleteCheck = Time.time + 2;
		level++;
		yield return new WaitForSeconds (0);
		tryInLevel = 0;
		print ("level " + level);
		ui.audioSource.clip = ui.levelSuccessAudio;
		ui.audioSource.Play ();
		// SwitchTask();
		switch (level) {
		case 1:

			break;
		case 2:
                // Hide(GameObject.Find("goal"));
			Hide (GameObject.Find ("ball"));
			break;
		case 3:
			for (int i = 0; i < boxCount[level]; i++)
				Hide (GameObject.Find ("box (" + (i + 1) + ")"));
			break;
		case 4:
			Hide (GameObject.Find ("book (1)"));
			break;
		case 5:
			for (int i = 0; i < boxCount[level]; i++)
				Hide (GameObject.Find ("box (" + (i + 1) + ")"));
			break;
		case 6:
			Show (GameObject.Find ("big_box_goal_sq_bottom"), false);
			Hide (GameObject.Find ("Frying Pan"));
			Hide (GameObject.Find ("hamburger"));
			Hide (GameObject.Find ("bookshelf_lower_part"));
			Hide (GameObject.Find ("bookshelf_upper_part"));
			Hide (GameObject.Find ("bookshelf_back_part"));
			break;
		case 7:
			Show (GameObject.Find ("big_box_goal_bottom"), false);
			break;
		default:

			break;
		}
		resetLevel ();
	}

	public static void goNextLevel ()
	{
		ui.StartCoroutine (goNextLevelCoroutine ());
	}

	public static void resetLevel ()
	{
		if (fail + success >= 1000)
			Time.timeScale = 0;
		lastLevelReset = Time.time;
		tryInLevel++;
		expectedDuration = Random.Range (taskLengthRange.x, taskLengthRange.y);
		GameObject.Find ("Manager").GetComponent<Record_playback> ().task_length = expectedDuration;
		float new_box_goal_z = Random.Range (boxGoalZRange.x, boxGoalZRange.y);
		GameObject.Find ("Manager").GetComponent<Record_playback> ().box_goal_z = new_box_goal_z;
		GameObject.Find ("small_box_goal").transform.position = new Vector3 (0.81f, 1.074f, new_box_goal_z);
		Gripper.movementMagnitude = 40f;
		Gripper.linked = false;
		if (level != 6 && level != 7 && level != 18)
			Gripper.gripperStatus = 1f;
		GripperUserControl.lastToggleTime = Time.time;
		Destroy (GameObject.Find ("gripper_center").GetComponent<FixedJoint> ());
		Gripper.movementMagnitude = (float)levelTimeLimit [level];
		switch (level) {
		case 1:
			ResetGameObject (GameObject.Find ("ball"));
			Show (GameObject.Find ("goal"), false);
			break;
		case 3:
			ResetGameObject (GameObject.Find ("book (1)"));
			break;
		case 4:
			for (int i = 0; i < boxCount[level]; i++)
				ResetGameObject (GameObject.Find ("box (" + (i + 1) + ")"));
			for (int i = 0; i < obstacleCount; i++)
				moveObstacle (GameObject.Find ("obstacle (" + (i + 1) + ")"));
			break;
		case 5:
			Show (GameObject.Find ("Frying Pan"), false);
			ResetGameObject (GameObject.Find ("hamburger"));
			ResetGameObject (GameObject.Find ("spatula"));
			break;
		case 6:
			moveBigBox (GameObject.Find ("big_box"));
			moveBigBoxGoal (GameObject.Find ("big_box_goal_sq"));
                //for (int i = 0; i < obstacleCount; i++)
                //    moveObstacle(GameObject.Find("obstacle (" + (i + 1) + ")"));
			break;
		case 7:
			moveBigBox (GameObject.Find ("big_box"));
			break;
		default:
			for (int i = 0; i < boxCount[level]; i++)
				if (Record_playback.visualizeObject != 1)
					ResetGameObject (GameObject.Find ("box (" + (i + 1) + ")"));
			break;
		}
		Text levelText = GameObject.Find ("level").GetComponent<Text> ();

		if (level < 20) {
			levelText.text = "level " + level;
			levelText.enabled = true;
			levelText.GetComponent<GUIEffects> ().ApplyRandomEffects ();
			GameObject.Find ("Manager").GetComponent<UI> ().StartCoroutine (DismissObjectDelayed (GameObject.Find ("level")));
		} else {
			levelText.fontSize = 40;
			levelText.text = "Congratulations! You finished the game.";
			levelText.enabled = true;
			levelText.GetComponent<GUIEffects> ().ApplyRandomEffects ();
		}



	}

	public void reset ()
	{
		level = 0;
		Application.LoadLevel (0);
	}

	static void SwitchTask ()
	{
		if (level > 1)
			GameObject.Find ("task (" + (level - 1) + ")").GetComponent<Text> ().GetComponent<GUIEffects> ().DismissNow ();
		GameObject.Find ("task (" + (level) + ")").GetComponent<Text> ().enabled = true;
		GUIEffects guiEffect = GameObject.Find ("task (" + (level) + ")").GetComponent<Text> ().GetComponent<GUIEffects> ();
		guiEffect.ApplyRandomEffects ();
	}

	static void ResetGameObject (GameObject gameobj)
	{
		Hide (gameobj);
		Show (gameobj, true);
	}

	static void Hide (GameObject gameobj)
	{
		gameobj.GetComponent<Renderer> ().enabled = false;
		foreach (Collider c in gameobj.GetComponents<Collider>())
			c.enabled = false;
		if ((gameobj.GetComponent<Rigidbody> ()) != null) {
			gameobj.GetComponent<Rigidbody> ().isKinematic = true;
			gameobj.GetComponent<Rigidbody> ().useGravity = false;
		}
	}

	static void Show (GameObject gameobj, bool randomPosition = true, bool randomRotation = false, float xc = 1.4f, float yc = 1.1f, float zc = .2f,
        float xr = .25f, float yr = 0f, float zr = .5f)
	{
		if (randomPosition)
			gameobj.transform.position = new Vector3 (xc + Random.Range (-xr, xr), yc + Random.Range (-yr, yr), zc + Random.Range (-zr, zr));
		if (randomRotation)
			gameobj.transform.rotation = Quaternion.Euler (0, Random.Range (-180f, 180f), 0);
		else
			gameobj.transform.rotation = Quaternion.Euler (0, 0, 0);
		if ((gameobj.GetComponent<Rigidbody> ()) != null) {
			gameobj.GetComponent<Rigidbody> ().isKinematic = false;
			gameobj.GetComponent<Rigidbody> ().useGravity = true;
		}
		if ((gameobj.GetComponent<Renderer> ()) != null)
			gameobj.GetComponent<Renderer> ().enabled = true;
		foreach (Collider c in gameobj.GetComponents<Collider>())
			c.enabled = true;

	}

	static void moveObstacle (GameObject gameobj)
	{
		Show (gameobj, true, false, 1.4f, 1.1f, .2f, 0f, 0f, .5f);
	}

	static void moveBigBoxGoal (GameObject gameobj)
	{
		Show (gameobj, true, false, 1.4f, 1.075f, .2f, .2f, 0f, .5f);
	}

	static void moveBigBox (GameObject gameobj)
	{
		Hide (gameobj);
		Show (gameobj, true, true, 1.4f, 1.075f, .2f, .2f, 0f, .5f);
	}

	//	IEnumerator GenerateObjects() {
	//		while (true) {
	//			yield return new WaitForSeconds (ObjectCreationDelay);
	//			if(ObjectCreationDelay > minObjectCreationDelay)
	//				ObjectCreationDelay -= difficultyLevel;
	//			if(UI.level > 1) {
	//				generatedObjects++;
	//				if(generatedObjects % 5 == 1) {
	//					goNextLevel();
	//				}
	//
	//				GameObject clone;
	//				clone = Instantiate(GameObject.Find("box"), new Vector3 (1.523f, 1.469087f, 0.6076558f)
	//				                    , GameObject.Find("box").transform.rotation) as GameObject;
	//				clone.GetComponent<Rigidbody> ().velocity = Vector3.zero;
	//			}
	//		}
	//	}

	static IEnumerator DismissObjectDelayed (GameObject gameObj)
	{
		yield return new WaitForSeconds (2);
		gameObj.GetComponent<Text> ().enabled = false;

	}

	void ShowTermsWindow (int windowID)
	{
		GUIStyle style = new GUIStyle ();
		style.richText = true;

		GUI.TextArea (new Rect (10, 30, 350, 80),
            "The information about the movement of the objects in this game is sent to a server to be used for research purposes."
            //<h1>Terms of Service (\"Terms\")</h1> <p>Last updated: September 22, 2015</p>  <p>Please read these Terms of Service (\"Terms\", \"Terms of Service\") carefully before using the  website (the \"Service\") operated by  (\"us\", \"we\", or \"our\").</p>  <p>Your access to and use of the Service is conditioned on your acceptance of and compliance with these Terms. These Terms apply to all visitors, users and others who access or use the Service.</p>  <p>By accessing or using the Service you agree to be bound by these Terms. If you disagree with any part of the terms then you may not access the Service.</p>   <p><strong>Links To Other Web Sites</strong></p>  <p>Our Service may contain links to third-party web sites or services that are not owned or controlled by .</p>  <p> has no control over, and assumes no responsibility for, the content, privacy policies, or practices of any third party web sites or services. You further acknowledge and agree that  shall not be responsible or liable, directly or indirectly, for any damage or loss caused or alleged to be caused by or in connection with use of or reliance on any such content, goods or services available on or through any such web sites or services.</p>  <p>We strongly advise you to read the terms and conditions and privacy policies of any third-party web sites or services that you visit.</p>   <p><strong>Governing Law</strong></p>  <p>These Terms shall be governed and construed in accordance with the laws of Florida, United States, without regard to its conflict of law provisions.</p>  <p>Our failure to enforce any right or provision of these Terms will not be considered a waiver of those rights. If any provision of these Terms is held to be invalid or unenforceable by a court, the remaining provisions of these Terms will remain in effect. These Terms constitute the entire agreement between us regarding our Service, and supersede and replace any prior agreements we might have between us regarding the Service.</p>  <p><strong>Changes</strong></p>  <p>We reserve the right, at our sole discretion, to modify or replace these Terms at any time. If a revision is material we will try to provide at least 30 days notice prior to any new terms taking effect. What constitutes a material change will be determined at our sole discretion.</p>  <p>By continuing to access or use our Service after those revisions become effective, you agree to be bound by the revised terms. If you do not agree to the new terms, please stop using the Service.</p>  <p>Our Terms of Service agreement was created by TermsFeed.</p>      <p><strong>Contact Us</strong></p>  <p>If you have any questions about these Terms, please contact us.</p>"
		);
		if (GUI.Button (new Rect (10, 120, 80, 20), "Accept")) {
			showTermsWindow = false;
		}
	}

	void OnGUI ()
	{
		if (showTermsWindow)
			GUI.Window (0, new Rect ((Screen.width - 400) / 2, (Screen.height - 150) / 2, 400, 150), ShowTermsWindow, "Terms of use");

	}

	void Start ()
	{
		level = task - 1;
		ui = GameObject.Find ("Manager").GetComponent<UI> ();
		goNextLevel ();
		boxes = new GameObject[5];
		for (int i = 0; i < boxCount[level]; i++)
			boxes [i] = GameObject.Find ("box (" + (i + 1) + ")");
		//		StartCoroutine (GenerateObjects ());

		//		goNextLevel();
		//		goNextLevel();

	}

	public void failure ()
	{

		lastFailTime = Time.time;
		fail++;
		audioSource.clip = levelFailedAudio;
		audioSource.Play ();
		print ("Average time: " + Time.time / success + " Success: " + success + " Fail: " + fail + " Success rate: " + success / (float)(success + fail));
		resetLevel ();
		if (lastSuccessTime > lastFailTime) {

		}
	}

	public void levelSuccess ()
	{
		float last_try = Mathf.Max (lastSuccessTime, lastFailTime);
		totalSuccessTime += Time.time - last_try;
		float taskLengthRatio = 150f / 10f;
		float expected_time = GameObject.Find ("Manager").GetComponent<Record_playback> ().task_length * taskLengthRatio;
		float current_box_goal_z = GameObject.Find ("box (1)").transform.position.z;
		float expected_box_goal_z = GameObject.Find ("Manager").GetComponent<Record_playback> ().box_goal_z;
		total_time_error += Mathf.Abs (Time.time - last_try - expected_time);
		total_box_z_error += Mathf.Abs (current_box_goal_z - expected_box_goal_z);
		success++;
		lastSuccessTime = Time.time;
		print ("Average time: " + totalSuccessTime / success + " Time error: " + total_time_error / success +
			"  " + total_time_error / success / (taskLengthRange.y - taskLengthRange.x) / taskLengthRatio * 100f + "%" +
			" Box error: " + total_box_z_error / success * 100f +
			"  " + total_box_z_error / success / (boxGoalZRange.y - boxGoalZRange.x) * 100f + "%" +
			" Success rate: " + success / (float)(success + fail) + " Success: " + success + " Fail: " + fail);
		resetLevel ();
	}

	void FixedUpdate ()
	{
		level = task;
		Text timeText = GameObject.Find ("time-text").GetComponent<Text> ();
		timeText.text = "Expected duration:\t" + (int)(expectedDuration * 15f) + " s\nElapsed time: \t\t" + (int)(Time.time - Mathf.Max (lastSuccessTime, lastFailTime)) + " s";

		float last_try = Mathf.Max (lastSuccessTime, lastFailTime);
		// if (Gripper.movementMagnitude < 0)
		if (Time.time - last_try > Gripper.movementMagnitude) {
			failure ();
		}
		// check level passed
		if (Time.time - lastLevelCompleteCheck > levelCompleteCheckInterval) {
			lastLevelCompleteCheck = Time.time;
			if (level == 1) {
				GameObject.Find ("gripper_center").transform.position = new Vector3 (1.85f, 1.31f, .237f);
			}


			if (level == 2) {
				GameObject bookshelf_volume = GameObject.Find ("bookshelf_volume");
				int i = 0;
				for (; i < boxCount[level]; i++)
					if (!bookshelf_volume.GetComponent<Collider> ().bounds.Contains (GameObject.Find ("box (" + (i + 1) + ")").transform.position))
						break;
				if (i == boxCount [level] && Gripper.gripperStatus >= .5f) {
					levelSuccess ();
				}
			} else if (level == 3) {
				GameObject book1 = GameObject.Find ("book (1)");
				float rotation_threshold = 0.01f;
				if (Mathf.Abs (book1.transform.rotation.x) < rotation_threshold && Mathf.Abs (book1.transform.rotation.z) < rotation_threshold) {
					goNextLevel ();
				}
			} else if (level == 4) {
				GameObject bookshelf_volume = GameObject.Find ("bookshelf_volume");
				int i = 0;
				for (; i < boxCount[level]; i++)
					if (!bookshelf_volume.GetComponent<Collider> ().bounds.Contains (GameObject.Find ("box (" + (i + 1) + ")").transform.position))
						break;
				if (i == boxCount [level] && Gripper.gripperStatus >= .5f) {
					levelSuccess ();
				}

			} else if (level == 5) {
				if (GameObject.Find ("FryingPan_inside").GetComponent<Collider> ().bounds.Contains (GameObject.Find ("hamburger").transform.position)) {
					goNextLevel ();
				}
			} else if (level == 6) {
				Bounds big_box_goal_bounds = GameObject.Find ("big_box_goal_sq").GetComponent<Collider> ().bounds;
				Bounds big_box_bounds = GameObject.Find ("big_box").GetComponent<Collider> ().bounds;
				if (big_box_goal_bounds.Contains (big_box_bounds.min) && big_box_goal_bounds.Contains (big_box_bounds.max)) {
					levelSuccess ();
				}
			} else if (level == 7) {
				Bounds big_box_goal_bounds = GameObject.Find ("big_box_goal").GetComponent<Collider> ().bounds;
				Bounds big_box_bounds = GameObject.Find ("big_box").GetComponent<Collider> ().bounds;
				if (big_box_goal_bounds.Contains (big_box_bounds.min) && big_box_goal_bounds.Contains (big_box_bounds.max)) {
					levelSuccess ();
				}
			} else if (level == 11) {
				float rotation_threshold = 10f;
				if (Mathf.Abs (Mathf.Abs (GameObject.Find ("box (1)").transform.rotation.eulerAngles.y - 180f) - 90f) < rotation_threshold && Gripper.gripperStatus >= .5f) {
					levelSuccess ();
				}
			} else if (level == 12) {
				float rotation_threshold = 10f;
				if (Mathf.Abs (GameObject.Find ("box (1)").transform.rotation.eulerAngles.y - 180f) < rotation_threshold && Gripper.gripperStatus >= .5f) {
					levelSuccess ();
				}
			} else if (level == 13) {
				float rotation_threshold = 10f;
				if (Mathf.Abs (GameObject.Find ("box (1)").transform.rotation.eulerAngles.z - 90f) < rotation_threshold && Gripper.gripperStatus >= .5f) {
					levelSuccess ();
				}
			} else if (level == 14) {
				float rotation_threshold = 10f;
				if (Mathf.Abs (GameObject.Find ("box (1)").transform.rotation.eulerAngles.z - 270f) < rotation_threshold && Gripper.gripperStatus >= .5f) {
					levelSuccess ();
				}
			} else if (level == 15) {
				GameObject bookshelf_volume = GameObject.Find ("bookshelf_volume");
				float rotation_threshold = 10f;
				if (Mathf.Abs (GameObject.Find ("box (1)").transform.rotation.eulerAngles.z - 270f) < rotation_threshold && Gripper.gripperStatus >= .5f
					&& bookshelf_volume.GetComponent<Collider> ().bounds.Contains (GameObject.Find ("box (1)").transform.position)) {
					levelSuccess ();
				}
			} else if (level == 16) {
				GameObject bookshelf_volume = GameObject.Find ("bookshelf_volume");
				float rotation_threshold = 10f;
				if (Mathf.Abs (Mathf.Abs (GameObject.Find ("box (1)").transform.rotation.eulerAngles.y - 180f) - 90f) < rotation_threshold && Gripper.gripperStatus >= .5f
					&& bookshelf_volume.GetComponent<Collider> ().bounds.Contains (GameObject.Find ("box (1)").transform.position)) {
					levelSuccess ();
				}
			} else if (level == 17) {
				GameObject bookshelf_volume = GameObject.Find ("bookshelf_volume");
				float rotation_threshold = 10f;
				if (Mathf.Abs (GameObject.Find ("box (1)").transform.rotation.eulerAngles.y - 180f) < rotation_threshold && Gripper.gripperStatus >= .5f
					&& bookshelf_volume.GetComponent<Collider> ().bounds.Contains (GameObject.Find ("box (1)").transform.position)) {
					levelSuccess ();
				}
			} else if (level == 18) {
				Bounds box_goal_bounds = GameObject.Find ("box_goal").GetComponent<Collider> ().bounds;
				Bounds box_bounds = GameObject.Find ("box (1)").GetComponent<Collider> ().bounds;
				if (box_goal_bounds.Contains (box_bounds.min) && box_goal_bounds.Contains (box_bounds.max)) {
					levelSuccess ();
				}
			}
		}
	}
}
 
