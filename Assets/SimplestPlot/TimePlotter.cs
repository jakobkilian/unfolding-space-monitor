using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TimePlotter : MonoBehaviour
{
    private SimplestPlot.PlotType PlotExample = SimplestPlot.PlotType.TimeSeries;
    public int DataPoints = 100;
    public const int instances = 4;
    private SimplestPlot SimplestPlotScript;
    private float Counter = 0;
    private Color[] MyColors;

    private System.Random MyRandom;
    private float[] XValues;
    private float[,] valArray;
    private float[] print1;
    private float[] print2;
    private float[] print0;
    private float[] print3;
    private float[] Y2Values;

    private Vector2 Resolution;
    // Use this for initialization

    private int curPos = 0;


    void Start()
    {
        SimplestPlotScript = GetComponent<SimplestPlot>();

        MyRandom = new System.Random();
        valArray = new float[instances, DataPoints];
        XValues = new float[DataPoints];
        print0 = new float[DataPoints];
        print1 = new float[DataPoints];
        print2 = new float[DataPoints];
        print3 = new float[DataPoints];
        Y2Values = new float[DataPoints];
        for (int plts = 0; plts < instances; plts++)
        {
            for (int Cnt = 0; Cnt < DataPoints; Cnt++)
            {
                print0[Cnt] = 0.0f;
                print1[Cnt] = 0.0f;
                print2[Cnt] = 0.0f;
                print3[Cnt] = 0.0f;
                Y2Values[Cnt] = 0.0f;
            }
        }
        MyColors = new Color[instances];
        MyColors[0] = Color.white;
        MyColors[1] = new Color();
        MyColors[2] = new Color();
        MyColors[3] = new Color();
        MyColors[1] = Color.HSVToRGB(0.15f, 0.5f, 0.8f);
        MyColors[2] = Color.HSVToRGB(0.9f, 0.5f, 0.8f);
        MyColors[3] = Color.HSVToRGB(0.6f, 0.5f, 0.8f);



        SimplestPlotScript.SetResolution(new Vector2(300, 300));
        SimplestPlotScript.BackGroundColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        SimplestPlotScript.TextColor = Color.white;
        for (int Cnt = 0; Cnt < instances; Cnt++)
        {
            SimplestPlotScript.SeriesPlotY.Add(new SimplestPlot.SeriesClass());
            SimplestPlotScript.SeriesPlotY[Cnt].MyColor = MyColors[Cnt];
        }

        Resolution = SimplestPlotScript.GetResolution();
    }

    // Update is called once per frame
    void Update()
    {

        Counter++;
        PrepareArrays();
        SimplestPlotScript.MyPlotType = PlotExample;
        SimplestPlotScript.SeriesPlotY[0].YValues = print0;
        SimplestPlotScript.SeriesPlotY[1].YValues = print1;
        SimplestPlotScript.SeriesPlotY[2].YValues = print2;
        SimplestPlotScript.SeriesPlotY[3].YValues = print3;
        SimplestPlotScript.SeriesPlotX = XValues;
        SimplestPlotScript.UpdatePlot();
    }
    private void PrepareArrays()
    {
        int i = curPos;

        for (int Cnt = 0; Cnt < DataPoints; Cnt++)
        {
            i++;
            if (i >= DataPoints) { i = 0; }
            print0[Cnt] = valArray[0, i];
            print1[Cnt] = valArray[1, i];
            print2[Cnt] = valArray[2, i];
            print3[Cnt] = valArray[3, i];
            XValues[Cnt] = Cnt;
        }

    }

    public void next()
    {
        curPos++;
    }


    public void addVal(int plot, float newVal)
    {

        if (curPos >= DataPoints)
        {
            //zurück zum Anfang
            curPos = 0;
        }
        //print (plot + " _ " + newVal);
        valArray[plot, curPos] = newVal;
    }


}