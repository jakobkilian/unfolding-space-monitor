using System.Collections;
using UnityEngine.UI; // The namespace for the UI stuff.
using System.Collections.Generic;
using UnityEngine;

public class depImgres_text : MonoBehaviour {

    UdpHandler udp;
    // Start is called before the first frame update

    private Text resText;

    void Start () {
        //find main
        //search udp client
        udp = GameObject.Find ("UdpHandler").GetComponent ("UdpHandler") as UdpHandler;
        if (udp == null) {
            print ("error at connecting UdpHandler");
        }
        //Timer for sending the udp messages
        InvokeRepeating ("updateRes", 1f, 0.1f); //1s delay, repeat every 1s

        //find text
        resText = GetComponent<Text> ();
    }

    void updateRes () {
        string displayed = udp.imgSize * 20 + " x " + udp.imgSize * 20;
        resText.text = displayed;

    }
}