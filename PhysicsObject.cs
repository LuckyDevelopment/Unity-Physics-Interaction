using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class PhysicsObject : MonoBehaviour
{
    //* Place this script on things to have physics
    public float waitOnPickup = 0.2f;
    public float breakForce = 15f;
    [HideInInspector] public bool pickedUp = false;
    [HideInInspector] public PickupSystem playerInteractions;
 
 
    private void OnCollisionEnter(Collision collision)
    {
        if(pickedUp)
        {
            if(collision.relativeVelocity.magnitude > breakForce)
            {
                playerInteractions.BreakConnection();
            }
 
        }
    }
 
    //this is used to prevent the connection from breaking when you just picked up the object as it sometimes fires a collision with the ground or whatever it is touching
    public IEnumerator PickUp()
    {
        yield return new WaitForSecondsRealtime(waitOnPickup);
        pickedUp = true;
 
    }
}