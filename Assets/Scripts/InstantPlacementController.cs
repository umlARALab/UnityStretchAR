
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

    public string rosTopicName = "quest_hit_position";
    
    private void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();

        ros.RegisterPublisher<HitPosMsg>(rosTopicName);
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
        }
    }

    private void TryPlace(Ray ray, Meta.XR.EnvironmentRaycastHit hit)
    {
        var objectToPlace = Instantiate(prefabToPlace);
        objectToPlace.transform.SetPositionAndRotation(
            hit.point,
            Quaternion.LookRotation(hit.normal, Vector3.up)
        );

        HitPosMsg hitMsg = new HitPosMsg(
            hit.point.x,
            hit.point.y,
            hit.point.z
        );

        ros.Publish(rosTopicName, hitMsg);

        if (MRUK.Instance?.IsWorldLockActive != true)
        {
            objectToPlace.AddComponent<OVRSpatialAnchor>();
        }
    }
}
