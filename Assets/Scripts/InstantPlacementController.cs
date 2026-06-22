
using Meta.XR.MRUtilityKit;
using Meta.XR;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.StretchAr;


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
    
    private void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        ros.RegisterPublisher<HitPosMsg>(objectPosTopic);  // publisher 0
        ros.RegisterPublisher<HitPosMsg>(stretchPosTopic); // publisher 1
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
            TryPlace(ray, hit, 0);
        } else if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger)) {
            TryPlace(ray, hit, 1);
        }
    }

    private void TryPlace(Ray ray, Meta.XR.EnvironmentRaycastHit hit, int publisher)
    {
        var objectToPlace = Instantiate(prefabToPlace);
        Destroy(objectToPlace, 5.0f);

        HitPosMsg hitMsg = new HitPosMsg(
            hit.point.x,
            hit.point.y,
            hit.point.z
        );

        if (publisher == 0) // object localization
        {
            ros.Publish(objectPosTopic, hitMsg);
        }
        else if (publisher == 1) // stretch align
        {
            objectToPlace.GetComponent<Renderer>().material.color = Color.blue;

            ros.Publish(stretchPosTopic, hitMsg);
        }

        objectToPlace.transform.SetPositionAndRotation(
            hit.point,
            Quaternion.LookRotation(hit.normal, Vector3.up)
        );






        if (MRUK.Instance?.IsWorldLockActive != true)
        {
            objectToPlace.AddComponent<OVRSpatialAnchor>();
        }
    }
}
