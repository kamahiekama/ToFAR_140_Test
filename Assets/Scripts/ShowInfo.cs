using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using TofAr.V0.Color;
using TofAr.V0.Tof;
using UnityEngine;

public class ShowInfo : MonoBehaviour
{
    private TMP_Text text;

    public float colorFrameRate, tofFrameRate;
    public int colorWidth, colorHeight, tofWidth, tofHeight;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        TofArColorManager.OnFrameArrived += OnColorFrameArrived;
        TofArTofManager.OnFrameArrived += OnTofFrameArrived;
    }

    private void OnColorFrameArrived(object sender)
    {
        TofArColorManager mgr = (TofArColorManager)sender;
        ResolutionProperty rp = mgr.GetProperty<ResolutionProperty>();
        colorFrameRate = mgr.FrameRate;
        colorWidth = rp.width;
        colorHeight = rp.height;
    }

    private void OnTofFrameArrived(object sender)
    {
        TofArTofManager mgr = (TofArTofManager)sender;
        CameraConfigurationProperty ccp = mgr.GetProperty<CameraConfigurationProperty>();
        tofFrameRate = mgr.FrameRate;
        tofWidth = ccp.width;
        tofHeight = ccp.height;
    }

    // Update is called once per frame
    void Update()
    {
        string s = "";

        s += "Color:\r\n " + colorWidth + " x " + colorHeight + "\r\n " + (int)(colorFrameRate) + " FPS\r\n\r\n";
        s += "ToF:\r\n " + tofWidth + " x " + tofHeight + "\r\n " + (int)(tofFrameRate) + " FPS\r\n";
        text.text = s;
    }
}
