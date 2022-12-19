using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.Extras;
using Valve.VR.InteractionSystem;

public class DestroyandPlay4s : MonoBehaviour
{
    public Mytask task;

    bool isTouched = false;
    public float timeRemaining = 4;
    public LaserPointerEnhanced laserPointer;

    public void Start()
    { 
        laserPointer.PointerClick += PointerClick; //it was laserPointer.PointerClick += PointerClick;
    }

    
    void Update()
    {
        // Is timer done? And not Touched?

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        
        if (timeRemaining < 0 && isTouched == false)
        {
            //Debug.Log("Kill");
            
            //TODO: Add logging when sphere gets destroyed because of time out
            
            Destroy(this.gameObject);
            if (task != null)
                task.generateSpheres();
            else
                Debug.LogError("No reference");
        }
    }
    
    public void PointerClick(object sender, PointerEnhancedEventArgs e)
    {
        double timestamp = UnixTime.GetTime();
        if (e.clickState == ClickState.Down)
        {
            //Set its touched and destroy THIS script not the object itself.
            isTouched = true;
            Destroy(this);
        }
    }

}
