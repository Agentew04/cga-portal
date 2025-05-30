﻿using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PortalPlayerObject : PortalTraveller
{
    float rotTimeElapsed;
    float rotLerpDuration = 1.0f;

    public Vector3 desiredScale;
    public Vector3 originalScale;

    public float force = 10f;
    new Rigidbody rigidbody;

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public override void Teleport(Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot, bool isRotated)
    {
        base.Teleport(fromPortal, toPortal, pos, rot);
        
        rigidbody.angularVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rigidbody.angularVelocity));
        if(isRotated)
        {
            rigidbody.linearVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rigidbody.linearVelocity)) * force;
            StartCoroutine(RotBack());
        }
        else
        {
            rigidbody.linearVelocity = toPortal.TransformVector(fromPortal.InverseTransformVector(rigidbody.linearVelocity));
        }
    }

    public override void EnterPortalThreshold(bool isRotated)
    {
        base.EnterPortalThreshold(isRotated);
        if (isRotated)
        {
            AvoidClipping(desiredScale);
        }
    }

    public override void ExitPortalThreshold(bool isRotated)
    {
        base.ExitPortalThreshold(isRotated);
        if(isRotated)
        {
            AvoidClipping(originalScale);
        }
    }

    private void AvoidClipping(Vector3 scale)
    {
        //LeanTween.scale(gameObject, scale, 0.5f)
        StartCoroutine(Scale(gameObject, scale, 0.5f));
    }

    private IEnumerator Scale(GameObject go, Vector3 to, float time) {
        Vector3 a = go.transform.localScale;
        Vector3 b = to;

        float t = 0f;
        while (t < 1f) {
            t += Time.deltaTime / time;
            go.transform.localScale = Vector3.Lerp(a, b, t);
            yield return null;
        }
    }

    IEnumerator RotBack()
    {
        Quaternion currentRot = transform.rotation;
        Quaternion buffer;
        //rigidbody.freezeRotation = false;
        while (rotTimeElapsed < rotLerpDuration)
        {
            buffer = Quaternion.Slerp(currentRot, Quaternion.identity, rotTimeElapsed / rotLerpDuration);
            rigidbody.MoveRotation(buffer);
            rotTimeElapsed += Time.smoothDeltaTime;
            yield return null;
        }
        rotTimeElapsed = 0;
        //rigidbody.freezeRotation = true;
    }
}