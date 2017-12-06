using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DramaticCamera : MonoBehaviour
{
    public Transform target;
    public float StartRadius = 3;
    public float EndRadius = 3;
    public float StartHeight = 0;
    public float EndHeight = 7;
    public float RotSpeed = 1;
    public float AnimationTime = 20;
    public float StartDownLookingFudge = 1;
    public float EndDownLookingFudge = 0;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float r = UnityEngine.Time.timeSinceLevelLoad * RotSpeed;
        float t = UnityEngine.Time.timeSinceLevelLoad / AnimationTime;
        float offset = Mathf.Lerp(StartHeight, EndHeight, UnityEngine.Time.timeSinceLevelLoad / AnimationTime);
        float radius = Mathf.Lerp(StartRadius, EndRadius, t);
        float downFudge = Mathf.Lerp(StartDownLookingFudge, EndDownLookingFudge, t);

        // set rotation
        Vector3 rotVec = new Vector3(Mathf.Cos(r), 0, Mathf.Sin(r));
        transform.rotation = Quaternion.LookRotation(-rotVec + Vector3.down * downFudge, Vector3.up);

        //rotVec.y = offset;
        Vector3 pos = target.position + radius * rotVec;
        pos.y = offset + target.position.y;
        transform.position = pos;

        Vector3 targetLook = target.position;
        targetLook.y = offset;
    }
}
