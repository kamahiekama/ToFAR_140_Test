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

    /// <summary>
    /// 
    /// </summary>
    public bool UseDepth = true;

    public bool MultiCamSupported = true;

    /// <summary>
    /// 
    /// </summary>
    public bool UseFrontCamera = false;

    /// <summary>
    /// -1 の場合は指定なし。それ以外の場合は一致する camera id のものを選択する
    /// </summary>
    public int CameraId = -1;

    /// <summary>
    /// -1 の場合は指定なし。
    /// </summary>
    public int CameraFPS = -1;

    /// <summary>
    /// -1 の場合は指定なし。
    /// </summary>
    public int TofCameraId = -1;

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
            log += "[" + i + "]:[" + rp.cameraId + "]: " + rp + "\r\n";

            if ((rp.lensFacing == 0) != UseFrontCamera)
            {
                continue;
            }

            /*
            if (UseDepth && !rp.avfDepthSupported)
            {
                continue;
            }
            //*/

            if (MultiCamSupported && !rp.avfMultiCamSupported)
            {
                continue;
            }

            if (CameraId != -1 && CameraId != int.Parse(rp.cameraId))
            {
                continue;
            }

            if (CameraFPS != -1 && CameraFPS != rp.frameRate)
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

        {
            ResolutionProperty rp = selectedResolutionProperty;
            Debug.Log("select: [" + selectedIndex + "]:[" + rp.cameraId + "]: " + selectedResolutionProperty + "\r\n");
            log = "color: [" + selectedIndex + "]:[" + rp.cameraId + "]: " + selectedResolutionProperty + "\r\n\r\n" + log;
        }


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

                // Android でのみ使う
                /*
                if (selectedResolutionProperty.cameraId != config.colorCameraId)
                {
                    continue;
                }
                //*/
                if (TofCameraId != -1 && TofCameraId != int.Parse(config.cameraId))
                {
                    continue;
                }

                log += "[" + i + "]:[" + config.cameraId + "] " + config.width + " x " + config.height + "\r\n";

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
                tofSelectedIndex = i;
            }

            if (selectedTofConfig != null)
            {
                Debug.Log("depth: " + selectedTofConfig.width + " x " + selectedTofConfig.height);
                log = ("depth: [" + tofSelectedIndex + "]:[" + selectedTofConfig.cameraId + "]: color cam id=" + selectedTofConfig.colorCameraId + ": " + selectedTofConfig.width + " x " + selectedTofConfig.height + " " + selectedTofConfig.frameRate + " FPS " + "\r\n\r\n") + log;
            }
            else
            {
                log = ("depth: not found.\r\n\r\n") + log;
            }
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
