using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* CAMERAMANAGER:
 * Handles camera movement and is driven by GameManager.
 * Pans camera to average (x,y) position of all alive pawns.
 * Moves camera on z plane to attempt to keep all players in frame as they spread/contract.
 * */
public class CameraManager : MonoBehaviour
{
    public float dampeningTime = 0.2f;
    public float screenEdgeBuffer = 12f;
    public float minBounds = 6.5f;
    public float maxBounds = 30f;
    [HideInInspector] public List<Transform> targets;

    private Camera cameraRenderer;
    private float zoomSpeed;
    private Vector3 moveVelocity;
    private Vector3 desiredPosition;

    private void Awake()
    {
        cameraRenderer = GetComponentInChildren<Camera>();
        targets = new List<Transform>();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
       CalculateTargetPosition();
       transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref moveVelocity, dampeningTime);
    }

    private void CalculateTargetPosition()
    {
        Vector3 averagePos = new Vector3();

        int numTargets = 0;

        for(int i = 0; i < targets.Count; i++)
        {
            if (!targets[i].gameObject.activeSelf)
                continue;

            averagePos += targets[i].position;
            numTargets++;
        }

        if (numTargets > 0)
            averagePos /= numTargets;

        averagePos.z = -CalculateCameraDistance();

        desiredPosition = averagePos;
    }

    private float CalculateCameraDistance()
    {
        Vector3 desiredLocalPos = transform.InverseTransformPoint(desiredPosition);

        float bounds = 0f;

        for (int i = 0; i < targets.Count; i++)
        {
            if (!targets[i].gameObject.activeSelf)
                continue;

            Vector3 targetLocalPos = transform.InverseTransformPoint(targets[i].position);

            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            bounds = Mathf.Max(bounds, Mathf.Abs(desiredPosToTarget.y));

            bounds = Mathf.Max(bounds, Mathf.Abs(desiredPosToTarget.x) / cameraRenderer.aspect);
        }

        bounds += screenEdgeBuffer;
        bounds = Mathf.Max(bounds, minBounds);

        float dist = (bounds / 2f) / Mathf.Tan(Mathf.Deg2Rad * cameraRenderer.fieldOfView / 2f);

        return dist;
    }

    public void SetStartPositionAndSize()
    {
        CalculateTargetPosition();

        transform.position = desiredPosition;
    }

    public void SetTarget(Transform target)
    {
        targets.Add(target);
    }

    public void RemoveTarget(Transform target)
    {
        targets.Remove(target);
    }
}
