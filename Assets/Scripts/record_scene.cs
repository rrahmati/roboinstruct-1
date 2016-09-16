using UnityEngine;
using System.Collections;
using System.IO;

public class record_scene : MonoBehaviour {

	// Use this for initialization
	public string toTrack = "movable";
	int resWidth;
	int resHeight;
	StreamWriter sw;
	int pic_num = 0;
	int delay = 20;
	int ctr = 0;
	Camera camera;
	float recordDelay = 0.02f;
	float lastRecordTime = 0f;
	public GameObject estimate_box;
	void Start () {
		//sw = new StreamWriter("pos_dataset/info.csv",true);
		resWidth=128;
		resHeight=128;
		camera=gameObject.GetComponent<Camera>();
		gameObject.GetComponent<Camera>().pixelRect = new Rect(0, 0,resWidth,resHeight);
	}

//	// Update is called once per frame
//	void Update () {
//		if(ctr % delay == (delay-1))
//		{
//				//check_collision();
//				record_scene_data();
//		}
//		ctr++;
//		if(pic_num>=50000)
//			UnityEditor.EditorApplication.isPlaying = false;
//	}
void FixedUpdate () {


	if( Time.time > lastRecordTime + recordDelay) {
			lastRecordTime = Time.time;
			record_scene_data();
			read_new_estimate();
	}
}
	void read_new_estimate(){
		StreamReader theReader = new StreamReader("./blocks/Convolution_code/output.txt");
		string line = theReader.ReadLine();
		string[] entries = line.Split(' ');
		float x,y,z,qx,qy,qz,qw;

		float.TryParse(entries[0],out x);
		float.TryParse(entries[1],out y);
		float.TryParse(entries[2],out z);
		float.TryParse(entries[3],out qx);
		float.TryParse(entries[4],out qy);
		float.TryParse(entries[5],out qz);
		float.TryParse(entries[6],out qw);

		//print(">>>>>>>>>>>>>>>>>>>>>>>>."+entries[0]+"    "+entries[1]+"    "+entries[2]);
		estimate_box.transform.position=new Vector3(x,y,z);
		estimate_box.transform.rotation=new Quaternion(qx,qy,qz,qw);
	}
	void take_screenshot(string filename)
	{
			foreach (GameObject estimate_object in GameObject.FindGameObjectsWithTag("estimate"))
			{
				estimate_object.GetComponent<Renderer>().enabled=false;
			}
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
			foreach (GameObject estimate_object in GameObject.FindGameObjectsWithTag("estimate"))
			{
				estimate_object.GetComponent<Renderer>().enabled=true;
			}
	}
	void record_scene_data()
	{
			string filename = "./blocks/Convolution_code/screenshot.png";
			//Application.CaptureScreenshot(filename);
			take_screenshot(filename);
			//foreach (GameObject traceable_object in GameObject.FindGameObjectsWithTag(toTrack))
			//		{
			//				//Vector3 object_pos_relative_to_camera = Camera.main.transform.position - traceable_object.transform.position;
			//				//Vector3 object_rotation_relative_to_camera = Camera.main.transform.rotation.eulerAngles - traceable_object.transform.rotation.eulerAngles;
			//				//if(traceable_object.name=="box (1)"){
			//				//	print("recording");
			//				//	sw.WriteLine(traceable_object.transform.position.x+","+ traceable_object.transform.position.y+","+ traceable_object.transform.position.z
			//				//	+","+traceable_object.transform.rotation.x+","+ traceable_object.transform.rotation.y+","+ traceable_object.transform.rotation.z+","+ traceable_object.transform.rotation.w,true);
			//				//}

			//		}
			//		pic_num++;
			//		print(pic_num);
			//sw.Flush();
	}
}
