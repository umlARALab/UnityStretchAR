using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;
using TMPro;
using RosMessageTypes.StretchAr;
public class StatusConnectionCheck : MonoBehaviour
{
    ROSConnection ros;

    public TMPro.TextMeshProUGUI statusText;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // initialize ros connection
        ros = ROSConnection.GetOrCreateInstance();

        ros.Subscribe<RobotStatusMsg>("robot_feedback", StatusCallback);

        ros.Connect();
    }

    // callback function
    void StatusCallback(RobotStatusMsg feedback)
    {
        statusText.SetText(feedback.data);
    }
}
