using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class MirrorManager : MonoBehaviour
{
    public GameObject mirrorPrefab;
    private ARRaycastManager raycastManager;
    private List<ARRaycastHit> hits = new();

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public void PlaceMirror(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            Pose hitPose = hits[0].pose;
            Instantiate(mirrorPrefab, hitPose.position, hitPose.rotation);
        }
    }
}