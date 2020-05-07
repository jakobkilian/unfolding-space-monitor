// description: Script that hides all objects with the tag "initHidden" in Start()

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class initHide : MonoBehaviour {
    CanvasGroup[] cnvsgroups;
    void Start () {
        //Get all Gameobjects tagged with "initHidden"
        GameObject[] hiddens = GameObject.FindGameObjectsWithTag ("initHidden");
        cnvsgroups = new CanvasGroup[hiddens.Length];
        //iterate through them and get the CanvasGroup Components
        for (int i = 0; i < hiddens.Length; ++i) {
            cnvsgroups[i] = hiddens[i].GetComponent<CanvasGroup> ();
        }
        //iterate through the CanvasGroup Components and set their alpha value to 0
        foreach (CanvasGroup cg in cnvsgroups) {
visByGameObj.setVis(cg.gameObject, false);
        }
    }
}