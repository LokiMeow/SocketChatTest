using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class background : MonoBehaviour {
	// Use this for initialization
    float width = 658;
    float height = 941;
	void Start () {
        float rate = width / height;
        if((float)Screen.width/(float)Screen.height>rate)
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.width/rate);
        else
            transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.height*rate, Screen.height);
	}
	
	// Update is called once per frame
	void Update () {
	}
}
