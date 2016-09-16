using UnityEngine;
using System.Collections;
using System.IO;

public class scene_estimate : MonoBehaviour {

	// Use this for initialization
	public string toTrack = "movable";
	int resWidth;
	int resHeight;
	StreamWriter sw;
	int pic_num = 0;
	int delay = 20;
	int ctr = 0;
	Camera camera;
	float recordDelay = 0.2f;
	float lastRecordTime = 0f;
	public GameObject estimate_box;
	void Start () {
		sw = new StreamWriter("pos_dataset/info.csv",true);
		resWidth=128;
		resHeight=128;
		camera=gameObject.GetComponent<Camera>();
		gameObject.GetComponent<Camera>().pixelRect = new Rect(0, 0,resWidth,resHeight);
	}

	// Update is called once per frame
//	void Update () {
//		if(ctr % delay == (delay-1))
//		{
//				//check_collision();
//				record_scene_data();
//		}
//		ctr++;
//		if(pic_num>=30000)
//			UnityEditor.EditorApplication.isPlaying = false;
//	}
void FixedUpdate () {
	if( Time.time > lastRecordTime + recordDelay) {
			lastRecordTime = Time.time;
			record_scene_data();
			//read_new_estimate();
	}
	if(pic_num>=30000)
				UnityEditor.EditorApplication.isPlaying = false;
}
	void take_screenshot(string filename)
	{
			RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
			camera.targetTexture = rt;
			Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
			camera.Render();
			RenderTexture.active = rt;
			screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
			camera.targetTexture = null;
			RenderTexture.active = null; // JC: added to avoid errors
			Destroy(rt);
			byte[] bytes = screenShot.EncodeToPNG();
			System.IO.File.WriteAllBytes(filename, bytes);
	}
	void record_scene_data()
	{
			string filename = "./pos_dataset/screenshot/screenshot"+pic_num+".png";
			//Application.CaptureScreenshot(filename);
			take_screenshot(filename);
			foreach (GameObject traceable_object in GameObject.FindGameObjectsWithTag(toTrack))
					{
							//Vector3 object_pos_relative_to_camera = Camera.main.transform.position - traceable_object.transform.position;
							//Vector3 object_rotation_relative_to_camera = Camera.main.transform.rotation.eulerAngles - traceable_object.transform.rotation.eulerAngles;
							if(traceable_object.name=="box (1)"){
								print("recording");
								sw.WriteLine(traceable_object.transform.position.x+","+ traceable_object.transform.position.y+","+ traceable_object.transform.position.z
								+","+traceable_object.transform.rotation.x+","+ traceable_object.transform.rotation.y+","+ traceable_object.transform.rotation.z+","+ traceable_object.transform.rotation.w,true);
							}

					}
					pic_num++;
					print(pic_num);
			sw.Flush();
	}
}
