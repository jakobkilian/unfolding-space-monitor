using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ui_toggle : MonoBehaviour {
    bool on;
    Image img;
    Color32 initCol;
    // Start is called before the first frame update
    void Start () {
        img = this.GetComponent<Image> ();
initCol=img.color;
    }

    // Update is called once per frame
    void Update () {

    }

    public void toggle () {
        on = !on;
        if (on) {
            img.color = new Color32 (130, 130, 130, 255);
        } else {
            img.color = initCol;
        }
    }
}