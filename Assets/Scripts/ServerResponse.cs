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

    /*public float slope3 {
        get {
            return _slope3;
        }
        set {
            _slope3 = value;
        }
    }
    
    public float slope1 {
        get {
            return _slope1;
        }
        set {
            _slope1 = value;
        }
    }*/
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
