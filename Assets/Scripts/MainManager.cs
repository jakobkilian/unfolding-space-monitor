using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using System.Text;

public class MainManager : MonoBehaviour
{

    //Variables to store data
    public bool online; //is there a connection to the Raspi?
    int onlineThresh = 700; //From what timespan on the device is considered as offline
    long lastOn; //when did we receive the last frame?

    int localFps; //frames per second on receiving side (local)
    byte[] motorVal = new byte[9]; //motor vlaues 
    int imgSize = 5; //1-9 steps for image resolution. multiplied by 20 (5 = 100px)
    long lastIncFrameCounter; //ticks of the last incoming message

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
    int processing;
    int gloveSending;
    int onNewData;
    bool muted = false;
    //Connect UDP Client
    UdpHandler udp;
    bool ConnectedToServer = false;
    bool setBackOnce = false;

    bool motorTest = false;

    //image array
    public GameObject imgObj;
    byte[] img = new byte[200000];
    public Material depImgMat;
    Texture2D tex;
    //texture can only be done in Main Thread
    bool newDepImgFrame = false;

    String IP = "";

    //Objects the Manager Script takes care of
    public ui_blob blobOnline;
    public ui_text textOnline;
    public ui_blob blobCamera;
    public ui_blob blobCapturing;
    public ui_motorPanel motorPanel;
    public ui_text textFps;
    //Value Panel
    public ui_text textFrameCounter;
    public ui_text textCycle;
    public ui_text textPause;
    public ui_text textOnNewData;
    public ui_text textProcessing;
    public ui_text textGloveSending;
    public ui_text textDeliveredFrames;
    public ui_text textDropsBridge;
    public ui_text textDropsFc;
    public ui_text textDrops10s;
    public ui_text TextLibCrashes;

    public ui_text textIP;
    public ui_text textTemp;

    public GameObject waitingPane;
    public ui_button buttonMute;
    public ui_button buttonTest;

    private long lastFpsCalc;
    private int lastFpsCalcFrameCounter;
    // Start is called before the first frame update
    void Start()
    {

        //init texture – later passed to material 
        tex = new Texture2D(100, 100, TextureFormat.RGBA32, false);
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
            if (newDepImgFrame)
            {
                //Todo: nicht jedes mal anmachen...
                // imgObj.SetActive(false);
                // imgObj.SetActive(true);
                tex.LoadRawTextureData(img);
                tex.Apply();
                newDepImgFrame = false;
                var data = tex.GetRawTextureData<Color32>();

            }
            //has to run every update

            depImgMat.mainTexture = tex;
        }
        else
        {
            blobOnline.updtVals(online);
            blobCapturing.updtVals(false);
            blobCamera.updtVals(false);
            textOnline.updtVals("last: " + (lastOn / 1000).ToString() + "s");

        }
        //check timespan since last frame
        lastOn = (DateTime.Now.Ticks - lastIncFrameCounter) / 10000;
        //print ("last on: " + lastOn);
        //print (DateTime.Now.Ticks + " - " + lastIncFrameCounter);
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
            imgObj.SetActive(false);
            camConnected = false;
            visByGameObj.setVis(waitingPane, true);
        }
        else
        {

            online = true;
            //issue: Unity only updates img when tunred off and on
            imgObj.SetActive(false);
            imgObj.SetActive(true);
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

    public void setNewValue(string key, byte[] values)
    {
        if (key == "frameCounter")
        {
            frameCounter = BitConverter.ToInt32(values, 0);
            lastIncFrameCounter = DateTime.Now.Ticks; //save the arrival time of this data
            //If one second has passed: calc local fps
            if ((DateTime.Now.Ticks - lastFpsCalc) > 10000000)
            {
                lastFpsCalc = DateTime.Now.Ticks;
                localFps = frameCounter - lastFpsCalcFrameCounter;
                lastFpsCalcFrameCounter = frameCounter;
            }
        }
        else if (key == "1")
        {
            timeSinceLastNewData = BitConverter.ToInt32(values, 0);
        }
        else if (key == "2")
        {
            longestTimeNoData = BitConverter.ToInt32(values, 0);
        }
        else if (key == "fps")
        {
            remotefps = BitConverter.ToInt32(values, 0);
        }
        else if (key == "coreTemp")
        {
            coreTemp = BitConverter.ToSingle(values, 0).ToString("0.00");
        }
        else if (key == "isConnected")
        {
            camConnected = BitConverter.ToBoolean(values, 0);
        }
        else if (key == "isCapturing")
        {
            camCapturing = BitConverter.ToBoolean(values, 0);
        }
        else if (key == "libCrashes")
        {
            libraryCrashNo = BitConverter.ToInt32(values, 0);
        }
        else if (key == "drpBridge")
        {
            droppedAtBridge = BitConverter.ToInt32(values, 0);

        }
        else if (key == "drpFC")
        {
            droppedAtFC = BitConverter.ToInt32(values, 0);
        }
        else if (key == "drpMinute")
        {
            tenSecsDrops = BitConverter.ToInt32(values, 0);
        }
        else if (key == "delivFrames")
        {
            deliveredFrames = BitConverter.ToInt32(values, 0);
        }

        else if (key == "isMuted")
        {
            muted = BitConverter.ToBoolean(values, 0);
        }

        else if (key == "isTestMode")
        {
            motorTest = BitConverter.ToBoolean(values, 0);
        }
        else if (key == "motors")
        {
            motorVal = values;
        }
        else if (key == "img")
        {
            //Debug.Log(BitConverter.ToString(values).Replace("-"," "));
            int b = 0;
            int ind = 0;
            while (ind < values.Length)
            {

                img[b] = (byte)(Mathf.Abs((int)(values[ind] - 255)));
                img[b + 1] = (byte)(Mathf.Abs((int)(values[ind] - 255)));
                img[b + 2] = (byte)(Mathf.Abs((int)(values[ind] - 255)));
                if (values[ind] > 250 && values[ind] > 15)
                {
                    img[b + 3] = 0;
                }
                else if (values[ind] <= 150 || values[ind] <= 15)
                {
                    img[b + 3] = 255;
                }
                ind++;
                b += 4;
            }
            newDepImgFrame = true;
        }
        else if (key == "onNewData")
        {
            onNewData = BitConverter.ToInt32(values, 0);
        }
        else if (key == "processing")
        {
            processing = BitConverter.ToInt32(values, 0);
        }
        else if (key == "gloveSending")
        {
            gloveSending = BitConverter.ToInt32(values, 0);
        }

        else if (key == "wholeCycle")
        {
            globalCycleTime = BitConverter.ToInt32(values, 0);
        }
        else if (key == "pause")
        {
            globalPauseTime = BitConverter.ToInt32(values, 0);
        }


        /*
                
                frameCounter = int.Parse(data["0"]);
                timeSinceLastNewData = int.Parse(data["1"]);
                longestTimeNoData = int.Parse(data["2"]);
                remotefps = int.Parse(data["3"]);
 
                camConnected = Convert.ToBoolean(int.Parse(data["6"]));
                camCapturing = Convert.ToBoolean(int.Parse(data["7"]));
                libraryCrashNo = int.Parse(data["8"]);
                droppedAtBridge = int.Parse(data["9"]);
                droppedAtFC = int.Parse(data["10"]);
                tenSecsDrops = int.Parse(data["11"]);
                deliveredFrames = int.Parse(data["12"]);
                globalCycleTime = int.Parse(data["13"]);
                globalPauseTime = int.Parse(data["14"]);
                muted = Convert.ToBoolean(int.Parse(data["15"]));
                motorTest = Convert.ToBoolean(int.Parse(data["16"]));

                for (int i = 0; i < motorVal.Length; i++)
                {
                    motorVal[i] = data[" motor" + i + 17];
                }
        */
    }


    public T FromByteArray<T>(byte[] data)
    {
        if (data == null)
            return default(T);
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(data))
        {
            object obj = bf.Deserialize(ms);
            return (T)obj;
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
        textFrameCounter.updtVals(frameCounter.ToString());
        textFps.updtVals(localFps.ToString());

        textDropsBridge.updtVals(droppedAtBridge.ToString());
        textDropsFc.updtVals(droppedAtFC.ToString());
        textDrops10s.updtVals(tenSecsDrops.ToString());
        TextLibCrashes.updtVals(libraryCrashNo.ToString());
        textDeliveredFrames.updtVals(deliveredFrames.ToString());
        buttonMute.updtVals(!muted);
        buttonTest.updtVals(!motorTest);
        textIP.updtVals(IP);
        textTemp.updtVals(coreTemp + "°C");
        //times
        textCycle.updtVals((globalCycleTime/1000).ToString() + "ms ");
        textPause.updtVals((globalPauseTime/1000).ToString() + "ms ");
        textOnNewData.updtVals(onNewData.ToString() + "us ");
        textProcessing.updtVals(processing.ToString() + "us ");
        textGloveSending.updtVals(gloveSending.ToString() + "us ");
    }


    //Set the imgSize variable (1-9) to modify resolution. 
    public void setImgSizefromFloat(float size)
    {
        imgSize = (int)(size);
        udp.setImgSize = imgSize;
        tex.Resize(imgSize * 20, imgSize * 20, TextureFormat.RGBA32, false);
    }
}