using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class movecircle : MonoBehaviour
{
    bool is_moving = false;
    // Start is called before the first frame update
    void Start()
    {
     //   Debug.Log("smth\n");
    }

    void OnMouseDown()
    {
       // Debug.Log("smth\n");
        is_moving = true;
    }

    void OnMouseUp()
    {
        is_moving = false;
    }
    // -3.9 - -2.5 = 1.4
    // Update is called once per frame
    void Update()
    {
        if (is_moving)
        {
            Vector3 myv = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 last = this.transform.position;
            last[1] = (myv[1] > -3.9 - 1.4 * 6) ? ((myv[1] < 4.6) ? myv[1] : (float)4.6) : (float)(-3.9 - 1.4 * 6);
            this.transform.position = last;
         //   Debug.Log(myv);
        }
    }
}
