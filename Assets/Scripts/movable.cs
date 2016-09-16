using UnityEngine;
using System.Collections;

public class movable : MonoBehaviour {

	float lastLevelCompleteCheck = 0f;
	float levelCompleteCheckInterval = 1f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void FixedUpdate() {

		if(Time.time - lastLevelCompleteCheck > levelCompleteCheckInterval) {
			lastLevelCompleteCheck = Time.time;
			GameObject working_area = GameObject.Find("working_area");
			if(name != "gripper_center" && !working_area.GetComponent<Collider>().bounds.Contains(transform.position))
            {
                bringObjectToTable();
                GameObject.Find("Manager").GetComponent<UI>().failure();
            }
				
		}

	}

	void bringObjectToTable(float xc = 1.4f, float yc = 1.1f, float zc = .2f,
        float xr = .25f, float yr = 0f, float zr = .5f) {
        transform.position = new Vector3(xc + Random.Range(-xr, xr), yc + Random.Range(-yr, yr), zc + Random.Range(-zr, zr));
        //transform.GetComponent<Rigidbody> ().velocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        StartCoroutine (Wait ());
        
    }

	IEnumerator Wait() {
		yield return new WaitForSeconds(5);
	}
	
}
