using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using UnityEngine.UI;
using TMPro;
using RosMessageTypes.StretchAr;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;

public class TrajDisplay : MonoBehaviour
{
    ROSConnection ros;
    private LineRenderer displayLine;

    private void Awake()
    {
        displayLine = GetComponent<LineRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TrajectoryPlanMsg>("robot_plan_trajectory", TrajectoryCallback);

        ros.Connect();
    }

    // Update is called once per frame
    void TrajectoryCallback(TrajectoryPlanMsg msg)
    {
        displayLine.positionCount = msg.trajectory.Length;
        Vector3[] pos = new Vector3[msg.trajectory.Length];
        for (int i = 0; i < msg.trajectory.Length; i++)
        {
            Vector3 pt;
            pt.x = (float)msg.trajectory[i].x;
            pt.y = (float)msg.trajectory[i].y;
            pt.z = (float)msg.trajectory[i].z;
            pos[i] = pt;
        }
        displayLine.SetPositions(pos);
    }
}
