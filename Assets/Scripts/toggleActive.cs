using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class toggleActive : MonoBehaviour
{
    public void off()
    {
        gameObject.SetActive(false);
    }
        public void on()
    {
        gameObject.SetActive(true);
    }

    public void toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
