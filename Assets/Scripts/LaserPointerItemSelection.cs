using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

public class LaserPointerItemSelection : MonoBehaviour
{

    public DataLogger logger;
    public LaserPointerEnhanced laserPointer;
    //public UnityEvent onTriggerUp;
    public GameObject activeObject;
    public string selectableObjectTag;


    private void Start()
    {
        laserPointer.PointerClick += PointerClick;
    }
    
    public void PointerClick(object sender, PointerEnhancedEventArgs e)
    {
        double timestamp = UnixTime.GetTime();
        if (e.clickState == ClickState.Down && e.target.tag == selectableObjectTag)
        {
            //Debug.Log("TODO: Check nbacktask DOWN");
            logger.writeSphereClick(timestamp, "down");

            Rigidbody r = e.target.gameObject.GetComponent<Rigidbody>();
            if (r != null) { 
                r.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
            } else
            {
                //Debug.Log("PointerClick" + e.target.gameObject.name);
            }

            try
            {
                activeObject = e.target.gameObject;
                activeObject.transform.parent = gameObject.transform;
            }
            catch
            {
                activeObject = null;
                //Debug.Log("Object Missing Down");
            }
        }
        else if (e.clickState == ClickState.Up && e.target.tag == selectableObjectTag)
        {
            //Debug.Log("TODO: Check nback UP");
            logger.writeSphereClick(timestamp, "up");
            try
            {
                if (activeObject != null)
                {
                    activeObject.transform.parent = null;
                    Rigidbody r = activeObject.GetComponent<Rigidbody>();
                    if (r != null) { 
                        r.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    }
                    activeObject = null;
                }//do stuff
            }
            catch
            {
                activeObject = null;
                //Debug.Log("Object Missing UP");
            }
        }
    }

    void Update()
    {
    }

}