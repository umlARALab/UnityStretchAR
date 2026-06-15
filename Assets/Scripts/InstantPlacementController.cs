
using Meta.XR.MRUtilityKit;
using Meta.XR;
using UnityEngine;

public class InstantPlacementController : MonoBehaviour
{
    public Transform rightControllerAnchor;
    public GameObject prefabToPlace;
    public EnvironmentRaycastManager raycastManager;

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

        if (MRUK.Instance?.IsWorldLockActive != true)
        {
            objectToPlace.AddComponent<OVRSpatialAnchor>();
        }
    }
}
