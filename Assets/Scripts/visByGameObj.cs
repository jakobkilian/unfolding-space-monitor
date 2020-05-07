using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class visByGameObj {
    public static CanvasGroup cg;

    public static void setVis (GameObject obj, bool vis) {
        cg = obj.GetComponent<CanvasGroup> ();
        if (vis) {

            cg.alpha = 1.0f;
            cg.interactable = true;
            cg.blocksRaycasts = true;
        } else {
            cg.alpha = 0.0f;
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }
}