using UnityEngine;
using System.Collections;

public class ball : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {

    }

    public void OnTriggerEnter(Collider collider)
    {

        if (collider.gameObject.name == "goal_inside")
        {
            UI.goNextLevel();
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(5);
    }
}
