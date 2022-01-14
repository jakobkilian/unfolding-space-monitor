using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ui_button : MonoBehaviour
{
    Image thisImg;
    Color32 initCol;
    // Start is called before the first frame update
    void Start()
    {
        thisImg = gameObject.GetComponent<Image>();
        initCol = thisImg.color;
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void updtVals(bool status)
    {
        //if false: color the blob green
        if (!status)
        {
            thisImg.color = new Color32(111, 166, 255, 255);
        }
        else
        {
            thisImg.color = initCol;
        }
    }
}
