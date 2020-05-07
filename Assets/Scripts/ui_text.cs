using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ui_text : MonoBehaviour {

        Text thisText;

        void Awake () {
            thisText = gameObject.GetComponent<Text> ();
        }

        //Accessed from outside if there is a new value
        public void updtVals (string str) {
            //Set Text
            thisText.text = str;
        }
}