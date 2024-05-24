using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PickupSystem : MonoBehaviour
{
    //* This script handles picking up and dropping physics objects.

    // PUBLICS
    [Header("Settings")]
    
    [SerializeField] private float SphereCastRadius = 0.1f; 
    [SerializeField] private LayerMask PhysicsLayer; // Layer that the physics objects are on.
    [SerializeField] private float RotationSpeed = 0f; // Keep that at zero if you don't want objects rotating to face you.
    [SerializeField] private float MinSpeed = 0f; // The minimum speed an object should move, recommend at 0.
    [SerializeField] private float MaxSpeed = 300f; // The max speed an object can move.
    [SerializeField] private float MaxDistance = 2.5f; // Max distance to pickup object.
    [SerializeField] private float MinAddedDistance = 1.15f; // How close you can scroll your wheel to get closer.
    [SerializeField] private float MaxAddedDistance = 3f; // How far you can scrol lyour wheel to get the object closer.
    [SerializeField] private float AddedDistanceSpeed = 50f; //
    [SerializeField] private float LookSpeed = 50f; // When holding your input to look at it. 
    [SerializeField] private float WeightRatio = 10f; //The ratio of mass/lbs, 10 is recommend, then a weight of 500 mass/lb won't be pickupabble.
    [SerializeField] private float MaxWeight = 500f; //Heaviest object you can pick up in mass.


    [Header("Objects")]
    [SerializeField] private GameObject LookObject;
    [SerializeField] private Transform PickupParent;
    [SerializeField] private Transform OriginalPickupArea;
    [SerializeField] private GameObject CurrentPickedUpObject;
    [SerializeField] private Camera camera;
    public bool holdingObject{get; private set;}

    

    // PRIVATES
    private PhysicsObject physicsObject;
    private Vector3 raycastPos;
    private Rigidbody pickedupRB;
    private float currentSpeed = 0f;
    private float currentDist= 0f;
    private  Quaternion lookRotation;
    private float addedDistance = 0f;
    private ToolSystem toolSystem;
    private PlayerController playerController;
    private float SpeedMultiplier = 0f;

    
    void Start()
    {
        toolSystem = GetComponent<ToolSystem>();
        playerController = GetComponent<PlayerController>();
    }
    

    // Update is called once per frame
    void Update()
    {
        raycastPos = camera.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, 0f));
        RaycastHit hit;
        if (Physics.SphereCast(raycastPos, SphereCastRadius, camera.transform.forward, out hit, MaxDistance, PhysicsLayer))
        {
                    if (hit.collider.gameObject.GetComponent<Rigidbody>().mass + 0.01 < MaxWeight)
                    {
                    LookObject = hit.collider.gameObject;
                    }
                    else
                    {
                        LookObject = null;
                    }
        }
        else
        {
            LookObject = null;
        }
        
        if (Vector3.Distance(PickupParent.position, OriginalPickupArea.position ) < MaxAddedDistance)
        {
            PickupParent.Translate(camera.transform.forward * Time.deltaTime * InputManager.instance.input.User.ObjectDistance.ReadValue<Vector2>().y * AddedDistanceSpeed, Space.World);
        }
        else
        {
            PickupParent.Translate(camera.transform.forward * -1, Space.World);
        }

         if (Vector3.Distance(PickupParent.position, transform.position ) < MinAddedDistance)
        {
            PickupParent.Translate(camera.transform.forward * 1, Space.World);
        }

        // Rotating The objects
        if (InputManager.instance.input.User.ObjectRotate.ReadValue<float>() > 0f && holdingObject)
        {
            playerController.lookAround = false;
            if (pickedupRB.gameObject != null)
            {
                
                pickedupRB.gameObject.transform.Rotate(new Vector3(InputManager.instance.input.User.Look.ReadValue<Vector2>().y, InputManager.instance.input.User.Look.ReadValue<Vector2>().x, 0) * Time.deltaTime * LookSpeed);
            }
        }
        if (InputManager.instance.input.User.ObjectRotate.WasReleasedThisFrame())
        {
            playerController.lookAround = true;
        }
        
    
        if (InputManager.instance.input.User.Pickup.ReadValue<float>() > 0 && InputManager.instance.input.User.Pickup.triggered && !toolSystem.holdingTool)
        {
            if (CurrentPickedUpObject == null)
            {
                if (LookObject != null)
                {
                    
                    PickUpObject();
                   
                }
            }
            
        }
        if (InputManager.instance.input.User.Pickup.WasReleasedThisFrame())
        {
            if (CurrentPickedUpObject != null)
            {
                BreakConnection();
                
            }
            playerController.lookAround = true;
            holdingObject = false;
        }
        

        if (CurrentPickedUpObject != null && currentDist > MaxDistance - addedDistance) BreakConnection();
    }

    private void FixedUpdate()
    {
        if (CurrentPickedUpObject != null)
        {
            currentDist = Vector3.Distance(PickupParent.position, pickedupRB.position);
            currentSpeed = Mathf.SmoothStep(MinSpeed, MaxSpeed, currentDist / MaxDistance);
            currentSpeed *= Time.fixedDeltaTime;
            Vector3 direction = PickupParent.position - pickedupRB.position;
            pickedupRB.velocity = direction.normalized * currentSpeed * SpeedMultiplier;
            //Rotation
            lookRotation = Quaternion.LookRotation(camera.transform.position - pickedupRB.position);
            lookRotation = Quaternion.Slerp(camera.transform.rotation, lookRotation, RotationSpeed * Time.fixedDeltaTime);
            //pickedupRB.MoveRotation(lookRotation);
        }   
    }

    void PickUpObject()
    {   
        holdingObject = true;
        PickupParent.position = OriginalPickupArea.position;
        physicsObject = LookObject.GetComponentInChildren<PhysicsObject>();
        CurrentPickedUpObject = LookObject;
        pickedupRB =CurrentPickedUpObject.GetComponent<Rigidbody>();
        //pickedupRB.constraints = RigidbodyConstraints.FreezeRotation;
        physicsObject.playerInteractions = this;
        SpeedMultiplier = Mathf.Clamp(500 / (pickedupRB.mass * WeightRatio), 0.001f, 6f);
        StartCoroutine(physicsObject.PickUp());
    }

    public void BreakConnection()
    {
        holdingObject = false;
        PickupParent.position = OriginalPickupArea.position;
        addedDistance = 0f;
        CurrentPickedUpObject = null;
        physicsObject.pickedUp = false;
        currentDist = 0;
    }
}
