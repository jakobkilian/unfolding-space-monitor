using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ui_blob : MonoBehaviour {

    Image thisImg;

    void Awake () {
        thisImg = gameObject.GetComponent<Image> ();
    }

    //Accessed from outside if there is a new value
    public void updtVals (bool status) {
        //if false: color the blob green
        if (!status) {
            thisImg.color = new Color32 (255, 0, 0, 200);
        }
        //if true: color it green
        else { thisImg.color = new Color32 (0, 255, 0, 200); }
    }
}