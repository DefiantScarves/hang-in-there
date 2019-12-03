using System.Collections;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform Player;
    public Transform CameraTransform;

    private Camera cam;

    public float distanceFromPlayer = 15.0f; // Orbiting distance
    private float distanceFromPlayerInSkill = 5.0f; // Aiming distance
    private float currentX = 0.0f; // Stores mouse input for camera
    private float currentY = 0.0f; // Stores mouse input for camera
    private float sensitivityX = 4.0f; // Not currently used, but could effect camera turn speed
    private float sensitivityY = 1.0f; // Not currently used, but could effect camera turn speed
    
    // Y_ANGLE_MIN and Y_ANGLE_MAX are used to stop the camera from being able to go all the way over or under
    // the player. They set min and max angles to help clamp the camera.
    private const float Y_ANGLE_MIN = -50.0f;
    private const float Y_ANGLE_MAX = 50.0f;
    
    private bool playerInSkill; // is the player aiming
    private bool inAimingPosition; // is the camera where it's supposed to be for aiming
    private bool inFollowingPosition; // is the camera where it's supposed to be for orbiting
    private Vector3 aimingOffset; // distance and direction between player and where camera should be when aiming
    private Vector3 aimingPosition; // position of where camera should be when aiming
    private Vector3 followingPosition; // position of where camera shoud be when orbiting

    private Vector3? clippingDirection;

    private void Start()
    {
        CameraTransform = transform; // Transform of object the script is on
        cam = Camera.main;
        playerInSkill = false; // camera should begin in player orbit position
        
        // aimingOffset is set based on where the player and camera are placed in the scene editor view.
        // This is why you have to make sure the camera is set to the proper aiming position in the scene
        // before play starts.
        aimingOffset = transform.position - Player.transform.position;
        
        // aimingPosition uses aimingOffset to set where the camera should be when aiming, and the 
        // Quaternion.Euler part of this assignment makes sure the camera is facing forward
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;
        
        // Start the scene in orbit, not in aiming position
        inAimingPosition = false;
        inFollowingPosition = true;

        clippingDirection = null;
    }

    private void Update()
    {
        // These change based on input from the mouse
        currentX += Input.GetAxis("Mouse X");
        currentY += Input.GetAxis("Mouse Y");

        // Clamp the Y rotation so it can't orbit all the way over or under the character
        currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);

        // direction is the direction and distance from the player to where the camera should be in orbit
        Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
        // rotation is how the camera should orbit (rotate) around the player based on mouse input
        Quaternion rotation = Quaternion.Euler(-currentY, currentX, 0);

        // followingPosition is based on the direction and rotation calculated above
        followingPosition = Player.position + rotation * direction;
        // aimingPosition is based on the direction the camera is facing and the offset calculated in Start()
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;

        // This block happens if the player is not holding the aim button and the camera needs to 
        // be either zooming out or is already zoomed out and should be orbiting the player
        if (!playerInSkill)
        {
            inAimingPosition = false; // Tell camera that it shouldn't be in aiming position anymore

            // Tell the camera to stop zooming out in half a second. This is a remnant from when I was trying to
            // do camera zooming constantly with coroutines. Coroutines need a forced stop and start, so this was
            // the call to stop the coroutine. I commandeered the coroutine and just had it set a flag, but I think
            // we could better handle this by having some logic to constantly check the distance between the camera
            // and the player and set the flags that way instead of after a set amount of time.
            Invoke("StopZoomOut", .5f);

            // This block is camera behavior while zooming back out from aiming, but when it hasn't reached full
            // orbit position yet.
            if (!inFollowingPosition)
            {
                // Lerp the camera's position smoothly out to where it's supposed to be for orbit
                CameraTransform.position = Vector3.Lerp(CameraTransform.position, followingPosition, .1f);

                // Smoothly rotate the camera to look at the player while it's zooming out. I didn't use 
                // CameraTransform.LookAt(Player.transform) here because that doesn't happen smoothly and creates
                // a jarring snap of the camera that's annoying to see.
                Quaternion newRotation = Quaternion.LookRotation(Player.position - CameraTransform.position);
                CameraTransform.rotation = Quaternion.RotateTowards(CameraTransform.rotation, newRotation, .9f);
            }
            // This block is when the camera is in orbit position and following normal behavior
            else
            {
                Vector3 movementDirection = transform.position + followingPosition;
                if (Vector3.Dot(movementDirection, clippingDirection.GetValueOrDefault()) > .9f && clippingDirection != null)
                {
                    print("Going in same direction");
                }
                //else
                //{
                    // Change the camera's position based on mouse input and a set distance from player.
                    CameraTransform.position = followingPosition;
                    CameraTransform.LookAt(Player.transform); // Look at the player
                //}
            }
        }
        // This block happens if the player is holding the aim button and the camera needs to either be zooming
        // in or sticking with the player while they aim
        else
        {
            // Tell the camera that it's no longer in orbiting position
            inFollowingPosition = false;

            // Tell the camera to stop zooming in after 3/10 of a second. This is a remnant from when I was trying to
            // do camera zooming constantly with coroutines. Coroutines need a forced stop and start, so this was
            // the call to stop the coroutine. I commandeered the coroutine and just had it set a flag, but I think
            // we could better handle this by having some logic to constantly check the distance between the camera
            // and the player and set the flags that way instead of after a set amount of time.
            Invoke("StopZoomIn", .3f);

            // This block is camera behavior while zooming in to aiming position but when it hasn't fully reached
            // it's zoomed aiming position destination
            if (!inAimingPosition)
            {
                // Allow aiming rotation of the camera while zooming in
                CameraTransform.rotation = Quaternion.Euler(-currentY, currentX, 0);
                // Smoothly move the camera from orbit to where it should be when aiming
                CameraTransform.position = Vector3.Lerp(CameraTransform.position, Player.position + aimingPosition, .2f);
            }
            // This block is camera behavior while aiming when it has reached its aiming position fully
            else
            {
                // Smoothly rotate the camera so it doesn't leave the player behind or clip over the player
                CameraTransform.rotation = Quaternion.Lerp(CameraTransform.rotation, Quaternion.Euler(-currentY, currentX, 0), 0.5f);
                // Smoothly move the camera's position based on the player so it will follow if the player moves
                CameraTransform.position = Vector3.Lerp(CameraTransform.position, Player.position + aimingPosition, .2f);
            }
        }
    }

    // StartSkill tells the camera to zoom in
    public void StartSkill()
    {
        playerInSkill = true; // Tell camera that player is in a skill and it should zoom in
        
        // Set a new aimingPosition (place where the camera should be when aiming) based on the direction the camera is facing 
        // (so you always aim where the camera is facing) and the aimingOffset distance we set in Start() 
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;
    }

    // EndSkill tells the camera to zoom out
    public void EndSkill()
    {
        playerInSkill = false; // Tell the camera that the player is not in a skill and it should orbit
        
        // Set new aimingPosition again. Not actually sure why this is here. It's probably unnecessary
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;
    }

    // These are both remnants of when I was trying to do camera zooming constantly with coroutines 
    // that'd need a forced stop signal. The idea was to set the flags to true after a set amount of 
    // time (assuming the camera made it to its intended position in that amount of time). These are 
    // also probably not necessary anymore, and it might be better to constantly check distance 
    // between camera and player and set the flags that way
    private void StopZoomIn()
    {
        inAimingPosition = true;
    }

    private void StopZoomOut()
    {
        inFollowingPosition = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        print("Camera triggered " + other.gameObject.name);
        Vector3 direction = transform.position + other.ClosestPointOnBounds(transform.position);
        clippingDirection = direction;
    }

    public void OnTriggerExit(Collider other)
    {
        clippingDirection = null;
    }
}
