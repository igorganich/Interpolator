using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camerascript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        float xFactor = (float)Screen.width / 8f;
        float yFactor = (float)Screen.height / 9f;

        float dd = xFactor / yFactor;
        Debug.Log("dd = " + dd);

        Camera.main.rect = new Rect(0, 0, 1, dd);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
