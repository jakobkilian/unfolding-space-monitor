using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{

    //Variables to store data
    public bool online; //is there a connection to the Raspi?
    int onlineThresh = 700; //From what timespan on the device is considered as offline
    long lastOn; //when did we receive the last frame?

    int udpfps; //frames per second on receiving side (local)
    String[] motorVal = new String[9]; //motor vlaues 
    int imgSize = 5; //1-9 steps for image resolution. multiplied by 20 (5 = 100px)
    long lastIncMsg; //ticks of the last incoming message

    int frameCounter;
    int timeSinceLastNewData;
    int longestTimeNoData;
    int remotefps; //frames per second on the device
    //int globalPotiVal;
    String coreTemp = "";
    bool camConnected; //camera is plugged in
    bool camCapturing; //camera is in capturing mode
    int libraryCrashNo; //no of libroyal crashes
    int droppedAtBridge; //frames drops at "Bridge" in libroyal
    int droppedAtFC; //frames drops at "FC" in libroyal
    int tenSecsDrops; //sum of all drops over 10 secs
    int deliveredFrames;
    int globalCycleTime; //time raspi needed for one cycle
    int globalPauseTime; //time rapsi waited for the next frame to come from libroyal
    bool muted = false;
    //Connect UDP Client
    UdpHandler udp;
    bool ConnectedToServer = false;
    bool setBackOnce = false;

    bool motorTest = false;

    String IP = "";

    //Objects the Manager Script takes care of
    public ui_blob blobOnline;
    public ui_text textOnline;
    public ui_blob blobCamera;
    public ui_blob blobCapturing;
    public ui_motorPanel motorPanel;
    public ui_text textFps;
    //Value Panel
    public ui_text textCycle;
    public ui_text textPause;
    public ui_text textMaxPause;
    public ui_text textDropsBridge;
    public ui_text textDropsFc;
    public ui_text textDrops10s;
    public ui_text TextLibCrashes;

    public ui_text textIP;
    public ui_text textTemp;

    public GameObject waitingPane;
    public ui_button buttonMute;
    public ui_button buttonTest;

    // Start is called before the first frame update
    void Start()
    {
        //search udp client
        udp = GameObject.Find("UdpHandler").GetComponent("UdpHandler") as UdpHandler;
        if (udp == null)
        {
            print("error at connecting UdpHandler");
        }

        if (udp.searchForServer())
        {
            //Dosth
        }

    }

    // Update is called once per frame
    void Update()
    {
        //Update Ui Elements
        if (online)
        {
            updateUiElements();
        }
        else
        {
            blobOnline.updtVals(online);
            blobCapturing.updtVals(false);
            blobCamera.updtVals(false);
            textOnline.updtVals("last: " + (lastOn / 1000).ToString() + "s");

        }
        //check timespan since last frame
        lastOn = (DateTime.Now.Ticks - lastIncMsg) / 10000;
        //print ("last on: " + lastOn);
        //print (DateTime.Now.Ticks + " - " + lastIncMsg);
        //if bigger than

        if (lastOn > 10000)
        {
            if (setBackOnce)
            {
                setBackOnce = false;
                udp.setBack();
                //only do this one time. Then wait for the server to be online again
            }
        }
        else if (lastOn > onlineThresh)
        {
            online = false;
            camConnected = false;
            visByGameObj.setVis(waitingPane, true);
        }
        else
        {
            online = true;
            textOnline.updtVals("online");
            setBackOnce = true;
            visByGameObj.setVis(waitingPane, false);
        }
        if (camConnected)
        {
            visByGameObj.setVis(waitingPane, false);
        }
        else
        {
            visByGameObj.setVis(waitingPane, true);
        }

    }

    public void setNewValues(String[] data)
    {

        lastIncMsg = DateTime.Now.Ticks; //save the arrival time of this data
        frameCounter = int.Parse(data[0]);
        timeSinceLastNewData = int.Parse(data[1]);
        longestTimeNoData = int.Parse(data[2]);
        remotefps = int.Parse(data[3]);
        //find decimal dot
        int found = data[5].IndexOf(".");
        //add two digits after decimal dot
        coreTemp = data[5].Substring(0, found + 3);
        camConnected = Convert.ToBoolean(int.Parse(data[6]));
        camCapturing = Convert.ToBoolean(int.Parse(data[7]));
        libraryCrashNo = int.Parse(data[8]);
        droppedAtBridge = int.Parse(data[9]);
        droppedAtFC = int.Parse(data[10]);
        tenSecsDrops = int.Parse(data[11]);
        deliveredFrames = int.Parse(data[12]);
        globalCycleTime = int.Parse(data[13]);
        globalPauseTime = int.Parse(data[14]);
        muted = Convert.ToBoolean(int.Parse(data[15]));
        motorTest = Convert.ToBoolean(int.Parse(data[16]));

        for (int i = 0; i < motorVal.Length; i++)
        {
            motorVal[i] = data[i + 17];
        }

    }

    public void setIP(String s)
    {
        IP = s;
    }
    public bool getOnline()
    {
        return online;
    }

    void updateUiElements()
    {
        blobCamera.updtVals(camConnected);
        blobOnline.updtVals(online);
        blobCapturing.updtVals(camCapturing);
        motorPanel.updtVals(motorVal);
        //motorPanel.updtVals(motorVal); else motorPanel.turnVisOff();
        textFps.updtVals(remotefps.ToString());
        textCycle.updtVals(globalCycleTime.ToString() + "ms ");
        textPause.updtVals(globalPauseTime.ToString() + "ms ");
        textMaxPause.updtVals(longestTimeNoData.ToString() + "ms ");
        textDropsBridge.updtVals(droppedAtBridge.ToString());
        textDropsFc.updtVals(droppedAtFC.ToString());
        textDrops10s.updtVals(tenSecsDrops.ToString());
        TextLibCrashes.updtVals(libraryCrashNo.ToString());
        //textPause.updtVals (globalPauseTime.ToString () + "ms ");
        buttonMute.updtVals(!muted);
        buttonTest.updtVals(!motorTest);
        textIP.updtVals(IP);
        textTemp.updtVals(coreTemp + "°C");
    }
}