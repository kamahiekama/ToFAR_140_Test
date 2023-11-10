using System.Collections;
using System.Collections.Generic;
using TMPro;
using TofAr.V0;
using TofAr.V0.Color;
using TofAr.V0.Tof;
using UnityEngine;

public class CameraSelector : MonoBehaviour
{
    public TMP_Text text;

    public int DesiredWidth = 1920;
    public int DesiredHeight = 1440;
    public bool UseDepth = true;
    public bool UseFrontCamera = false;

    public int ToFDelayCounter = 0;

    TofArTofManager tmgr;
    CameraConfigurationProperty selectedTofConfig;

    // Start is called before the first frame update
    void Start()
    {
        string log = "";

        TofArManager tofmgr = FindObjectOfType<TofArManager>();        

        var platformConfigProperty = TofArManager.Instance.GetProperty<PlatformConfigurationProperty>();
        platformConfigProperty.platformConfigurationIos.cameraApi = IosCameraApi.AvFoundation;
        TofArManager.Instance.SetProperty(platformConfigProperty);

        log += "IsUsingAvfoundation: " + tofmgr.IsUsingAVFoundation() + "\r\n";

        TofArColorManager cmgr = FindObjectOfType<TofArColorManager>();
        cmgr.DesiredFrameRate = 30;

        AvailableResolutionsProperty properties = cmgr.GetProperty<AvailableResolutionsProperty>();

        int minDiff = 10000;
        int selectedIndex = 0;
        ResolutionProperty selectedResolutionProperty = null;
        for (int i = 0; i < properties.resolutions.Length; i++)
        {
            ResolutionProperty rp = properties.resolutions[i];
            log += "[" + i + "]: " + rp + "\r\n";

            if ((rp.lensFacing == 0) != UseFrontCamera)
            {
                continue;
            }

            int diff = Mathf.Abs(rp.width - DesiredWidth) + Mathf.Abs(rp.height - DesiredHeight);
            if (diff < minDiff)
            {
                selectedIndex = i;
                selectedResolutionProperty = rp;
                minDiff = diff;
            }
        }

        Debug.Log("select: [" + selectedIndex + "] : " + selectedResolutionProperty + "\r\n");
        log += "select: [" + selectedIndex + "] : " + selectedResolutionProperty + "\r\n";


        tmgr = FindObjectOfType<TofArTofManager>();

        selectedTofConfig = null;
        if (UseDepth && tmgr)
        {
            CameraConfigurationsProperty tofProperties = tmgr.GetProperty<CameraConfigurationsProperty>();

            // seach same aspect configuration
            float colorAspect = (float)selectedResolutionProperty.width / selectedResolutionProperty.height;
            int tofSelectedIndex = 0;
            for(int i = 0; i < tofProperties.configurations.Length; i++){
                CameraConfigurationProperty config = tofProperties.configurations[i];

                if (config.colorCameraId != selectedResolutionProperty.cameraId)
                {
                    continue;
                }

                log += "[" + i + "]: " + config.width + " x " + config.height + "\r\n";

                if ((config.lensFacing == 0) != UseFrontCamera)
                {
                    continue;
                }

                float aspect = (float)config.width / config.height;
                if (Mathf.Abs(aspect - colorAspect) > 0.001f)
                {
                    continue;
                }
                selectedTofConfig = config;
            }

            Debug.Log("depth selected: " + selectedTofConfig.width + " x " + selectedTofConfig.height);
            log += ("depth selected: " + selectedTofConfig.width + " x " + selectedTofConfig.height + "\r\n");
        }

        if (UseDepth && tmgr && selectedTofConfig != null && ToFDelayCounter == 0)
        {
            Debug.Log("StartStreamWithColor (ToF)");
            log += "StartStreamWithColor (ToF)";

            tmgr.StartStreamWithColor(selectedTofConfig, selectedResolutionProperty, true, true);
        }
        else
        {
            Debug.Log("StartStream (Color)");
            log += ("StartStream (Color)\r\n");

            cmgr.StartStream(selectedResolutionProperty);
        }

        text.text = log;
    }

    // Update is called once per frame
    void Update()
    {
        if (ToFDelayCounter > 0)
        {
            ToFDelayCounter--;

            if (ToFDelayCounter == 0)
            {
                tmgr.StartStream(selectedTofConfig, true);
                text.text += "StartStream (ToF)\r\n";
            }
        }
    }
}
