
using Meta.XR.MRUtilityKit;
using Meta.XR;
using UnityEngine;
using UnityEngine.UI;
using PassthroughCameraSamples.CameraViewer;
using System.Collections;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.StretchAr;
using RosMessageTypes.Geometry;
using RosMessageTypes.Std;
using RosMessageTypes.Sensor;

public class InstantPlacementController : MonoBehaviour
{
    public Transform rightControllerAnchor;
    public GameObject prefabToPlace;
    public EnvironmentRaycastManager raycastManager;

    ROSConnection ros;
    public string rosIP = "192.168.10.3";
    public int rosPort = 10000;

    public string objectPosTopic = "quest_hit_point";
    public string stretchPosTopic = "quest_stretch_align";
    public string stopRobotTopic = "stop_robot";
    public string camFrameTopic = "quest_camera";

    // private PassthroughCameraAccess camInstant;

    private bool isHolding = false;
    private Vector3 startPoint;

    private void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        // camInstant = Instantiate(camAccess);
        // camInstant.enabled = true;
        // camAccess = FindAnyObjectByType<PassthroughCameraAccess>();

        ros.RegisterPublisher<HitPosMsg>(objectPosTopic);  
        ros.RegisterPublisher<HitPosMsg>(stretchPosTopic); 
        ros.RegisterPublisher<BoolMsg>(stopRobotTopic);

        ros.RegisterPublisher<ImageMsg>(camFrameTopic);
    }

    private void Update()
    {
        var ray = new Ray(
            rightControllerAnchor.position,
            rightControllerAnchor.forward
        );
        float maxDistance = 20f;

        raycastManager.Raycast(ray, out var hit, maxDistance);

        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger))
        {
            TryPlace(ray, hit);
            PubImageCapture();
        } else if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger)) {
            isHolding = true;
            startPoint = initLine(ray, hit);
        } else if (isHolding == true && OVRInput.GetUp(OVRInput.RawButton.RHandTrigger))
        {
            isHolding = false;
            drawLine(ray, hit, startPoint);
        }

        if (OVRInput.GetDown(OVRInput.RawButton.B)) {
            BoolMsg callToStop = new BoolMsg(true);

            ros.Publish(stopRobotTopic, callToStop);
        }
    }

    private void TryPlace(Ray ray, Meta.XR.EnvironmentRaycastHit hit)
    {
        var objectToPlace = Instantiate(prefabToPlace);
        Destroy(objectToPlace, 5.0f);

        // hit position and controller position to publish
        Vector3Msg hitVec = new Vector3Msg( // unity frames don't align with typical frames
            hit.point.z, // z
            -hit.point.x, // -x
            hit.point.y  // y
        );

        Vector3Msg conVec = new Vector3Msg(
            rightControllerAnchor.position.z,
            -rightControllerAnchor.position.x,
            rightControllerAnchor.position.y
        );

        // having both the hit point and the position of the camera will help us get transforms between each component
        HitPosMsg hitMsg = new HitPosMsg( 
            hitVec, // hit position
            conVec  // controller position
        );

        ros.Publish(objectPosTopic, hitMsg);

        objectToPlace.transform.SetPositionAndRotation(
            hit.point,
            Quaternion.LookRotation(hit.normal, Vector3.up)
        );

        if (MRUK.Instance?.IsWorldLockActive != true)
        {
            objectToPlace.AddComponent<OVRSpatialAnchor>();
        }
    }

    private void PubImageCapture()
    {
        RawImage rawCam = GetComponentInChildren<RawImage>();

        Texture2D imgTexture = rawCam.texture as Texture2D;
        byte[] rawImgData = imgTexture.GetRawTextureData();

        ImageMsg imgMsg = new ImageMsg
        {
            height = (uint)imgTexture.height,
            width = (uint)imgTexture.width,
            encoding = "rgb8",
            is_bigendian = 0,
            step = (uint)(imgTexture.width * 3),
            data = rawImgData
        };
        ros.Publish(camFrameTopic, imgMsg);
    }

    private Vector3 initLine(Ray ray, Meta.XR.EnvironmentRaycastHit hit)
    {
        Vector3 startPt = new Vector3(hit.point.x, hit.point.y, hit.point.z);
        var objectToPlace = Instantiate(prefabToPlace);
        objectToPlace.GetComponent<Renderer>().material.color = Color.blue;
        Destroy(objectToPlace, 5.0f);

        objectToPlace.transform.SetPositionAndRotation(
            startPt,
            Quaternion.LookRotation(hit.normal, Vector3.up)
        );

        // change ray color as indicator that user is drawing line
        /*
        Transform rayLine = transform.Find("RaycastLine");
        rayLine.GetComponent<LineRenderer>().startColor = Color.blue;
        rayLine.GetComponent<LineRenderer>().endColor = Color.blue;
        */

        return startPt;
    }
    private void drawLine(Ray ray, Meta.XR.EnvironmentRaycastHit hit, Vector3 startPt)
    {
        // change ray color as indicator that user is done drawing line
        /*
        GameObject rayViz = GameObject.Find("RaycastVisualize");
        LineRenderer rayLine = rayViz.GetComponentsInChildren<LineRenderer>()[0];
        rayLine.material.color = Color.white;
        */

        Vector3Msg endMsg = new Vector3Msg(
            hit.point.z,
            -hit.point.x, 
            startPt.y // match height with starting point
        );

        Vector3Msg startMsg = new Vector3Msg(
            startPt.z,
            -startPt.x, 
            startPt.y
        );

        var objectToPlace1 = Instantiate(prefabToPlace);
        objectToPlace1.GetComponent<Renderer>().material.color = Color.blue;

        objectToPlace1.transform.SetPositionAndRotation(
            hit.point,
            Quaternion.LookRotation(hit.normal, Vector3.up)
        );
        Destroy(objectToPlace1, 5.0f);

        HitPosMsg hitMsg = new HitPosMsg(
            startMsg,
            endMsg
        );

        ros.Publish(stretchPosTopic, hitMsg);
    }
}
