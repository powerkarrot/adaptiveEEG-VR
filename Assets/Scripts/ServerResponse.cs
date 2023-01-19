using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;

[Serializable]
public struct ServerResponse
{
    [SerializeField]
    public float slopet1;

    [SerializeField]
    public float slopet2;

    [SerializeField]
    public string error;
}

[Serializable]
public struct ServerIAFResponse
{
    [SerializeField]
    public float lowerAlpha;

    [SerializeField]
    public float upperAlpha;

    [SerializeField]
    public float iafDone;

    [SerializeField]
    public string error;
}

[Serializable]
public struct ServerAlphaBaselineResponse
{
    [SerializeField]
    public float baselineDone;

    [SerializeField]
    public string error;
}

public struct ServerAdaptationResponse2
{
    [SerializeField]
    public Array ratio1;

    [SerializeField]
    public Array ratio2;

    [SerializeField]
    public string error;
}


public struct ServerAdaptationResponse
{
    [SerializeField]
    public float curroi1;

    [SerializeField]
    public float curroi2;

    [SerializeField]
    public float basroi1;

    [SerializeField]
    public float basroi2;

    [SerializeField]
    public string error;
}