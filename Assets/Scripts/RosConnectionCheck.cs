using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;
using TMPro;

public class RosConnectionCheck : MonoBehaviour
{
    ROSConnection ros;

    public Image connectionImg;
    public TMPro.TextMeshProUGUI connectionText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize ros connection
        ros = ROSConnection.GetOrCreateInstance();

        ros.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        // change indicator based off of current robot connection to unity
        if (ros.HasConnectionError)
        {
            connectionImg.color = Color.red;
            connectionText.SetText("No Connection");
        } else
        {
            connectionImg.color = Color.green;
            connectionText.SetText("Connected");
        }
    }
}
