using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class ui_motorPanel : MonoBehaviour
{
    Image[] motorImg = new Image[9]; //motor vlaues 
    Text[] motorText = new Text[9]; //motor vlaues 
    Button[] buttons = new Button[9];

    UdpHandler udp;


    void Start()
    {
        udp = GameObject.Find("UdpHandler").GetComponent("UdpHandler") as UdpHandler;

        for (int i = 0; i < motorImg.Length; i++)
        {
            motorImg[i] = this.gameObject.transform.GetChild(i).gameObject.GetComponent<Image>();
            motorText[i] = this.gameObject.transform.GetChild(i).GetChild(0).gameObject.GetComponent<Text>();
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int closureIndex = i; // Prevents the closure problem
            buttons[closureIndex] = this.gameObject.transform.GetChild(i).gameObject.GetComponent<Button>();
            buttons[closureIndex].onClick.AddListener(() => TaskOnClick(closureIndex));
        }
    }

    public void updtVals(byte[] motorVal)
    {
        for (int i = 0; i < motorImg.Length; i++)
        {
            //brightness of this motor:
            byte b = motorVal[i];
            //Set Motor Color
            motorImg[i].color = new Color32(b, b, b, 255);
            //make the text color white/black depending on background
            if (b > 135)
            {
                motorText[i].color = new Color32(0, 0, 0, 255);
            }
            else
            {
                motorText[i].color = new Color32(255, 255, 255, 255);
            }
            //Set Text
            motorText[i].text = Convert.ToString(motorVal[i]);
            
        }

    }
    //Turn motor visualization off
    public void turnVisOff()
    {
        for (int i = 0; i < motorImg.Length; i++)
        {
            motorImg[i].color = new Color32(0, 0, 0, 255);
            motorText[i].text = "";
        }

    }
    // ...
    public void TaskOnClick(int buttonIndex)
    {
        udp.testMotorNo(buttonIndex);
        Debug.Log("You have clicked the button #" + buttonIndex, buttons[buttonIndex]);
    }

}