using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineAnimator : MonoBehaviour
{
    public float TimeToAnimate = 20;
    public float StartingHeight = 0;
    public float EndHeight = 10;
    public float StartingTwists = 0;
    public float EndTwists = 3;
    public float StartingMajorRadius = 0.5f;
    public float EndMajorRadius = 1;
    public float StartingMinorRadius = 0.5f;
    public float EndMinorRadius = 1;

    private Helix helix;
    private bool running = true;

    // Use this for initialization
    void Start ()
    {
        helix = GetComponent<Helix>();
        helix.RebuildLive = true;
    }
    
    // Update is called once per frame
    void Update ()
    {
        float t = Time.timeSinceLevelLoad /TimeToAnimate;
        if (t < 1)
        {
            helix.HelixHeight = Mathf.Lerp(StartingHeight, EndHeight, t);
            helix.HelixTurns = Mathf.Lerp(StartingTwists, EndTwists, t);
            helix.HelixMinorRadius = Mathf.Lerp(StartingMinorRadius, EndMinorRadius, t);
            helix.HelixMajorRadius = Mathf.Lerp(StartingMajorRadius, EndMajorRadius, t);
        }
        else
        {
            if (running)
            {
                helix.RebuildLive = false;
                running = false;
            }
        }

    }
}
