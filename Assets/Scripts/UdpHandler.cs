using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class UdpHandler : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Which IP should be called")]
    string IP = "192.168.0.12"; // default is localhost 

    [SerializeField]
    [Tooltip("Which port should the messages be sent to?")]
    int remotePort = 9009;

    [SerializeField]
    [Tooltip("Which port should be opened for incoming messages?")]
    int localPort = 9010;

    [SerializeField]
    [Tooltip("Which port should be opened to look for open servers?")]
    int localFindPort = 9008;

    [SerializeField]
    [Tooltip("Print debug logs?")]
    bool debug = false;

    [SerializeField]
    [Tooltip("Start App with Video Stream?")]
    bool callImg = true;

    [SerializeField]
    [Tooltip("Add the server selection panel here.")]
    GameObject serverPanel;

    [SerializeField]
    [Tooltip("Add the Dropdown for Server Selection here.")]
    Dropdown drop;

    [SerializeField]
    [Tooltip("Add the looking for servers text here.")]
    Text lookingText;

    Dictionary<string, string> servers = new Dictionary<string, string>();

    // udp stuff
    IPEndPoint remoteEndPoint;
    IPEndPoint anyIP;
    IPEndPoint msgIP;
    Thread receiveThread;
    Thread findThread;
    UdpClient findClient;
    UdpClient client;
    String udpStr = "\0";

    bool motorTestOn = false;

    // gui
    string ident = "";
    string val1 = "";
    string val2 = "";
    int int2 = 0;
    bool ConnectedToServer = false;
    bool serversFound = false;

    //VON Processing TODO!

    String[] inVal = new String[130];
    byte[] img = new byte[200000];
    public Material depImgMat;
    Texture2D tex;

    public GameObject imgObj;
    bool isFindingThreadInit = false;
    bool isReveivingThreadInit = false;
    //pass variables to manager
    MainManager mngr;

    [SerializeField]
    public int setImgSize
    {
        get { return imgSize; }
        set
        {
            imgSize = value;
            //setImgSizefromFloat(imgSize);
        }
    }
    public int imgSize = 5;

    [SerializeField]
    [Tooltip("Time between every ping to the server. Currently raspi resets after 50 frames")]
    float pingTime = 1f;

    //texture can only be done in Main Thread
    bool newDepImgFrame = false;

    public void Start()
    {
        //init texture – later passed to material 
        //tex = new Texture2D(100, 100, TextureFormat.RGBA32, false);

        //find main
        mngr = GameObject.Find("MainManager").GetComponent("MainManager") as MainManager;
        if (mngr == null)
        {
            print("error at connecting MainManager");
        }

        anyIP = new IPEndPoint(IPAddress.Any, 0);
        msgIP = new IPEndPoint(IPAddress.Any, 0);

    }

    void Update()
    {

        if (!ConnectedToServer)
        {
            if (!isFindingThreadInit) initFindingThread();
            if (serversFound)
            {
                serversFound = false;
                drop.transform.gameObject.SetActive(true);
                lookingText.transform.gameObject.SetActive(false);
            }
        }
        else
        {
            if (!isReveivingThreadInit) initReceivingThread();
            // if (newDepImgFrame)
            // {
            //     tex.LoadRawTextureData(img);
            //     tex.Apply();
            //     newDepImgFrame = false;
            // }
            //has to run every update
            //depImgMat.mainTexture = tex;
        }

    }

    //initialize recieving thread

    void initReceivingThread()
    {
        isReveivingThreadInit = true;
        if (findClient != null) findClient.Close();
        client = new UdpClient(localPort);

        //Send the IP to the MainManager
        mngr.setIP(IP);


        //init message
        if (debug) print("UDP Client Initialized –> Sending to " + IP + ":" + remotePort);
        if (debug) print("–> Sending to " + IP + ":" + remotePort);
        if (debug) print("<- waiting for packets on port " + localPort);

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), remotePort);
        msgIP = new IPEndPoint(IPAddress.Parse(IP), localPort);
        // define local end point
        // create thread for listening
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
        //Timer for sending the udp messages
        InvokeRepeating("sendValuesInvRep", 0.3f, pingTime);
        visByGameObj.setVis(serverPanel, false);

        //imgObj.SetActive(true);
    }

    void initFindingThread()
    {
        isFindingThreadInit = true;
        findClient = new UdpClient(localFindPort);
        if (client != null) client.Close();
        visByGameObj.setVis(serverPanel, true);
        //imgObj.SetActive(false);
        findThread = new Thread(new ThreadStart(lookForServers));
        findThread.IsBackground = true;
        findThread.Start();
    }

    // (called by receiveThread) - looks for new incoming messages from an connected server
    private void ReceiveData()
    {
        while (true)
        {
            //Do every ms
            //Todo: good way to slow thread down?
            Thread.Sleep(1);
            try
            {
                // get bytes.
                byte[] data = client.Receive(ref msgIP);
                parseData(data);
                // Bytes mit der UTF8-Kodierung in das Textformat kodieren.
                string text = "";
                if (debug) text = Encoding.UTF8.GetString(data);
                // show received raw text in console
                if (debug) print("Raw incoming:  " + text + "\n");
            }

            //if error –> print it
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    // (called by findThread) - looks for available servers every 500ms
    private void lookForServers()
    {
        while (true)
        {
            //Do every 0.5s
            Thread.Sleep(500);
            if (debug) print("try to find client");
            String serverName = "";
            String serverIP = "";
            try
            {
                // get bytes.
                byte[] data = findClient.Receive(ref anyIP);
                // Convert and save name to string
                serverName = Encoding.UTF8.GetString(data);
                if (debug) print("GOT client" + serverName);
                // convert and save IP to string
                serverIP = anyIP.Address.ToString();
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
            //If server has a name..
            if (serverName != "")
            {
                try
                {
                    if (debug) print("ADD to list");
                    //Add the item to the servers list to save the IP
                    servers.Add(serverName, serverIP);
                    //Add the name to the Dropdown to make it selectable
                    drop.options.Add(new Dropdown.OptionData(serverName));
                    //print ("added server: " + serverName + " at IP " + serverIP + " to the Dropdown");
                    //print ( " at IP " + serverIP + " to the Dropdown");
                    //when there is 1 server:
                    if (servers.Count == 1)
                    {
                        //...will display msg and the dropdown 
                        serversFound = true;
                    }
                    //refresh to ensure that the first Server of the list is selected
                    drop.RefreshShownValue();
                }
                catch (Exception err) { }
            }
        }
    }

    public void sendValuesInvRep()
    {
        if (true)
        {
            if (callImg) { udpStr = "i" + imgSize + "\0"; } else { udpStr = "\0"; }
            //if (motorTestOn){ udpStr = "t\0"; } else { udpStr = "\0";}
            // send whole string
            try
            {
                // Daten mit der UTF8-Kodierung in das Binärformat kodieren.
                byte[] data = Encoding.UTF8.GetBytes(udpStr);
                // send message to remote client
                client.Send(data, data.Length, remoteEndPoint);
                if (debug) print(udpStr);
                if (debug) print(" >> has been sent to ");
                if (debug) print(remoteEndPoint);
            }

            //if error -> print error
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    void parseData(byte[] data)
    { // <-- extended handler
        int idDelim = 0;
        int valuesLength = 0;

        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] == ':')
            {
                idDelim = i;
                break;
            }
        }
        //If there is an id
        if (idDelim > 0)
        {
            //parse id into string
            string key = "";
            for (int i = 0; i < idDelim; i++)
            {
                key += (char)data[i];
            }
            valuesLength = data.Length - idDelim - 1;
            byte[] values = new byte[valuesLength];
            //print("idDelim is at: " + idDelim);
            //print("Data Length is: " + data.Length);
            //print("Length of Values: " + valuesLength);
            Buffer.BlockCopy(data, idDelim + 1, values, 0, valuesLength);
            //print("Incoming Block: \t\t\t " + key + ": \t" + BitConverter.ToString(values));
            mngr.setNewValue(key, values);
        }
        //GetComponent<Renderer> ().material.mainTexture = texture;
        /*
        int b = 0;

        while (ind < data.Length && imgDelim == true)
        {
            img[b] = (byte)(Mathf.Abs((int)(data[ind] - 255)));
            img[b + 1] = (byte)(Mathf.Abs((int)(data[ind] - 255)));
            img[b + 2] = (byte)(Mathf.Abs((int)(data[ind] - 255)));
            if (data[ind] > 250 && data[ind] > 15)
            {
                img[b + 3] = 0;
            }
            else if (data[ind] <= 150 || data[ind] <= 15)
            {
                img[b + 3] = 255;
            }
            ind++;
            b += 4;
        }
        if (imgDelim == true) newDepImgFrame = true;
        */
    }

    void udpSendString(String str)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            client.Send(data, data.Length, remoteEndPoint);
            if (debug) print("String: " + str + "has been sent");
        }
        //if error -> print error
        catch (Exception err)
        {
            print(err.ToString());
        }
    }

    //Mute the vibration of all motors
    public void muteMotors()
    {
        //send the char 'm' and an empty byte 
        udpSendString("m\0");
        if (debug) print("mute variable toggled\n");
    }

    //Add the call for the depth image to the regulary sent udp command
    public void toggleImg()
    {
        callImg = !callImg;
        if (debug) print("img variable toggled\n");
    }

    //let specific motor vibrate
    public void testMotorNo(int i)
    {
        //send the char 'm' to toggle test
        udpSendString("z" + i + "\0");
        print("test motor no" + i + " \n");
    }

    public void toggleMotorTest()
    {
        //muteMotors();
        udpSendString("t\0");
    }

    public void triggerCalibration()
    {
        //muteMotors();
        udpSendString("c\0");
    }



    public bool searchForServer()
    {
        return false;
    }

    void OnDestroy()
    {
        findThread.Abort();
        if (findClient != null)
            findClient.Close();
        receiveThread.Abort();
        if (client != null)
            client.Close();
    }

    //Gets called by the start Button in Unity
    public void setServer()
    {
        //get IP String from servers list depending on dropdown value
        IP = servers[drop.options[drop.value].text];
        ConnectedToServer = true;
        findThread.Abort();
        isFindingThreadInit = false;
    }

    public void setBack()
    {
        servers.Clear();
        drop.ClearOptions();
        drop.RefreshShownValue();
        drop.transform.gameObject.SetActive(false);
        lookingText.transform.gameObject.SetActive(true);
        receiveThread.Abort();
        isReveivingThreadInit = false;
        ConnectedToServer = false;
        serversFound = false;
        CancelInvoke();
    }

    public void setCameraUseCase(int useCase)
    {

        udpSendString("u" + useCase + "\0");
    }
}