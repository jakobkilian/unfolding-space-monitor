using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class getMainManagerVal : MonoBehaviour {

    [SerializeField]
    [Tooltip ("The Nane of the variable in MainManager")]
    string varName = "";
    [SerializeField]
    [Tooltip ("How often should we look for the variable in Hz")]
    int hz = 25;

    bool val;

    MainManager mngr;
    void Start () {
        //find main
        mngr = GameObject.Find ("MainManager").GetComponent ("MainManager") as MainManager;
        if (mngr == null) {
            print ("error at connecting MainManager");
        }

        float s = hz / 1000f;
        //Timer for updating values
        InvokeRepeating ("dosth", 1f, 0.1f); //1s delay, repeat every 1s

    }

    void dosth () {
        val=mngr.getOnline();
        print (val);
    }

    void Update () {
        if (!val) {
            gameObject.GetComponent<Image> ().color = new Color32 (255, 0, 0, 200);
        } else { gameObject.GetComponent<Image> ().color = new Color32 (0, 255, 0, 200); }
    }
}