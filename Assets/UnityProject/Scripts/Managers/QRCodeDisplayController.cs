using System;
using Microsoft.MixedReality.Toolkit;
using QRTracking;
using TMPro;
using UnityEngine;

public class QRCodeDisplayController : MonoBehaviour
{
    [SerializeField]
    private float qrObservationTimeOut = 3500;


    /*
    private QRInfo lastSeenCode;


    private void Start()
    {
        if (!QRCodesManager.Instance.IsSupported)
        {
            return;
        }

        StartTracking();
       
    }


    private void QRCodeTrackingService_Initialized(object sender, EventArgs e)
    {
        StartTracking();
    }

    private void StartTracking()
    {
        QRCodesManager.Instance.QRCodeFound += QRCodeTrackingService_QRCodeFound;
        QRCodesManager.Instance.StartQRTracking();

    }

    private void QRCodeTrackingService_QRCodeFound(object sender, QRInfo codeReceived)
    {
        if (lastSeenCode?.Data != codeReceived.Data)
        {
            Debug.Log("code observed: " + codeReceived.Data);
          
        }
        lastSeenCode = codeReceived;
    }

    private void Update()
    {
        if (lastSeenCode == null)
        {
            return;
        }
        if (Math.Abs(
            (lastSeenCode.LastDetectedTime.UtcDateTime - DateTimeOffset.UtcNow).TotalMilliseconds) >
              qrObservationTimeOut)
        {
            lastSeenCode = null;
        }
    }*/
}
