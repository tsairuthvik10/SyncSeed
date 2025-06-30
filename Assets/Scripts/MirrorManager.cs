using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class MirrorManager : MonoBehaviour
{
    public static MirrorManager Instance;

    public GameObject mirrorPrefab;
    public ARRaycastManager raycastManager;
    public ParticleSystem mirrorSpawnEffect;

    public int maxMirrorsPerLevel = 3;
    private int currentMirrorCount = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaceMirror()
    {
        if (currentMirrorCount >= maxMirrorsPerLevel)
        {
            Debug.Log("Mirror limit reached.");
            return;
        }

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        if (raycastManager.Raycast(new Vector2(Screen.width / 2, Screen.height / 2), hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            Vector3 directionToCamera = Camera.main.transform.position - hitPose.position;
            Quaternion faceCameraRotation = Quaternion.LookRotation(directionToCamera.normalized);
            GameObject mirror = Instantiate(mirrorPrefab, hitPose.position, faceCameraRotation);
            currentMirrorCount++;

            if (mirrorSpawnEffect != null)
            {
                ParticleSystem effect = Instantiate(mirrorSpawnEffect, mirror.transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, 2f);
            }
        }
    }

    public void SetMirrorLimit(int newLimit)
    {
        maxMirrorsPerLevel = newLimit;
        currentMirrorCount = 0;
    }

    public void ResetMirrorCount()
    {
        currentMirrorCount = 0;
    }
}