//======= Copyright (c) Valve Corporation, All rights reserved. ===============
using UnityEngine;
using System.Collections;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

public class LaserPointerEnhanced : MonoBehaviour
{
    public SteamVR_Behaviour_Pose pose;

    //public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.__actions_default_in_InteractUI;
    public SteamVR_Action_Boolean interactWithUI = SteamVR_Input.GetBooleanAction("InteractUI");

    //public bool active = true;
    public Color color;
    
    public float rayLength = 10f;
    public float thickness = 0.002f;
    public Color clickColor = Color.green;
    //public GameObject holder;
    public GameObject pointer;
    public GameObject pointerModel;
    public bool addRigidBody = false;
    public Transform reference;
    public event PointerEventHandler PointerIn;
    public event PointerEventHandler PointerOut;
    public event PointerEventHandler PointerClick;
    public bool isPressed = false;

    
    Transform previousContact = null;
    
    Ray raycast;

    private void Start()
    {
        if (pose == null)
            pose = this.GetComponent<SteamVR_Behaviour_Pose>();

        if (pose == null)
            Debug.LogError("No SteamVR_Behaviour_Pose component found on this object", this);

        if (interactWithUI == null)
            Debug.LogError("No ui interaction action has been set on this component.", this);


        //holder = new GameObject();
        //holder.transform.parent = this.transform;
        //holder.transform.localPosition = Vector3.zero;
        //holder.transform.localRotation = Quaternion.identity;
        //holder.transform.localScale = new Vector3(1, 1, 1);

        //pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointer = Instantiate(pointerModel, new Vector3(0, 0, 0), Quaternion.identity);
        pointer.name = "LaserPointerRay";
        pointer.transform.parent = this.transform;
        pointer.transform.localScale = new Vector3(thickness, thickness, rayLength);
        pointer.transform.localPosition = new Vector3(0f, 0f, 0.02f);// = new Vector3(0f, 0f, 50f);
        pointer.transform.localRotation = Quaternion.identity;
        //MeshCollider collider = pointer.AddComponent<MeshCollider>();
        //collider.convex = true;
        
        if (addRigidBody)
        {
            if (GetComponent<Collider>())
            {
                GetComponent<Collider>().isTrigger = true;
            }
            Rigidbody rigidBody = pointer.AddComponent<Rigidbody>();
            rigidBody.isKinematic = true;
        }
        else
        {
            if (GetComponent<Collider>())
            {
                Object.Destroy(GetComponent<Collider>());
            }
        }

        Material newMaterial = new Material(Shader.Find("Unlit/Color"));
        newMaterial.SetColor("_Color", color);
        pointer.GetComponent<MeshRenderer>().material = newMaterial;

        
        this.transform.GetChild(0).gameObject.SetActive(true);

        raycast = new Ray(this.transform.position, transform.forward);
    }

    public virtual void OnPointerIn(PointerEnhancedEventArgs e)
    {
        if (PointerIn != null)
            PointerIn(this, e);
    }

    public virtual void OnPointerClick(PointerEnhancedEventArgs e)
    {
        if (PointerClick != null)
            PointerClick(this, e);
    }



    public virtual void OnPointerOut(PointerEnhancedEventArgs e)
    {
        if (PointerOut != null)
            PointerOut(this, e);
    }


    private void FixedUpdate()
    {
        //test.transform.position = this.transform.position + new Vector3(0f, 0f, 0.02f) +  transform.forward;
        raycast.origin = this.pointer.transform.position; //this.pointer.transform.position;
        raycast.direction = this.transform.forward;

        //Debug.DrawRay(raycast.origin, raycast.direction, Color.red);
        //Physics.autoSyncTransforms();
        int layer = 5;
        int layerMask = 1 << layer;
        RaycastHit hit;
        bool bHit = Physics.Raycast(raycast, out hit, 100.0f, layerMask);
        //Debug.Log(bHit);

        /***
        * 1) pointer hits nothing but the previousContact is not null
        * 2) pointer hits object but previousContact is different
        * => Move out of object
        ***/
        if ((bHit == false && previousContact != null) || (bHit == true && previousContact != null && previousContact != hit.transform))
        {
            //Debug.Log("Move Out Object" + previousContact.transform);
            PointerEnhancedEventArgs args = new PointerEnhancedEventArgs();
            args.fromInputSource = pose.inputSource;
            args.distance = 0f;
            args.flags = 0;
            args.target = previousContact;
            OnPointerOut(args);
            previousContact = null;
        }

        /***
        *  pointer moves onto object area
        ***/
        if (bHit == true && previousContact != hit.transform)
        {
            //Debug.Log("Move In Object" + hit.transform);
            PointerEnhancedEventArgs argsIn = new PointerEnhancedEventArgs();
            argsIn.fromInputSource = pose.inputSource;
            argsIn.distance = hit.distance;
            argsIn.flags = 0;
            argsIn.target = hit.transform;
            OnPointerIn(argsIn);
            previousContact = hit.transform;
        }
        
        // Change length of ray
        if (bHit)
        {
            pointer.transform.localScale = new Vector3(thickness, thickness, hit.distance);
        }
        else
        {
            pointer.transform.localScale = new Vector3(thickness, thickness, rayLength);
        }

        if (bHit && interactWithUI.GetStateDown(pose.inputSource))
        {
            //Debug.Log("Click Down");
            PointerEnhancedEventArgs argsClick = new PointerEnhancedEventArgs();
            argsClick.fromInputSource = pose.inputSource;
            argsClick.distance = hit.distance;
            argsClick.flags = 0;
            argsClick.target = hit.transform;
            argsClick.clickState = ClickState.Down;
            OnPointerClick(argsClick);
            isPressed = true;
        }
        else if (bHit && interactWithUI.GetStateUp(pose.inputSource))
        {
            //Debug.Log("Click Up");
            PointerEnhancedEventArgs argsClick = new PointerEnhancedEventArgs();
            argsClick.fromInputSource = pose.inputSource;
            argsClick.distance = hit.distance;
            argsClick.flags = 0;
            argsClick.target = hit.transform;
            argsClick.clickState = ClickState.Up;
            OnPointerClick(argsClick);
            isPressed = false;
        }

        // Change color if needed
        if (this.transform.childCount == 3){
            pointer.GetComponent<MeshRenderer>().material.color = clickColor;
        } else{
            pointer.GetComponent<MeshRenderer>().material.color = color;
        }

        
    }
}

public struct PointerEnhancedEventArgs
{
    public SteamVR_Input_Sources fromInputSource;
    public uint flags;
    public float distance;
    public Transform target;
    public ClickState clickState;
}


public enum ClickState
{
    Down,
    Up,
}

public delegate void PointerEventHandler(object sender, PointerEnhancedEventArgs e);