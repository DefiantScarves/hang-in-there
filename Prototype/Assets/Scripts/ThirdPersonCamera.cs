using System.Collections;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform Player;
    public Transform CameraTransform;

    private Camera cam;

    private float distanceFromPlayer = 10.0f;
    private float distanceFromPlayerInSkill = 5.0f;
    private float currentX = 0.0f;
    private float currentY = 0.0f;
    private float sensitivityX = 4.0f;
    private float sensitivityY = 1.0f;

    private const float Y_ANGLE_MIN = -50.0f;
    private const float Y_ANGLE_MAX = 50.0f;
    private bool playerInSkill;
    private bool inAimingPosition;
    private bool inFollowingPosition;
    private Vector3 aimingOffset;
    private Vector3 aimingPosition;
    private Vector3 followingPosition;

    private void Start()
    {
        CameraTransform = transform; // Transform of object the script is on
        cam = Camera.main;
        playerInSkill = false;
        aimingOffset = transform.position - Player.transform.position;
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;
        inAimingPosition = false;
        inFollowingPosition = true;
    }

    private void Update()
    {
        currentX += Input.GetAxis("Mouse X");
        currentY += Input.GetAxis("Mouse Y");

        currentY = Mathf.Clamp(currentY, Y_ANGLE_MIN, Y_ANGLE_MAX);
    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(0, 0, -distanceFromPlayer);
        Quaternion rotation = Quaternion.Euler(-currentY, currentX, 0);

        // Orbit and zoomed positions
        followingPosition = Player.position + rotation * direction;
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;

        if (!playerInSkill)
        {
            inAimingPosition = false;
            Invoke("StopZoomOut", .5f);

            // This block is camera behavior while zooming back out to orbit
            if (!inFollowingPosition)
            {
                CameraTransform.position = Vector3.Lerp(CameraTransform.position, followingPosition, .1f);
                Quaternion newRotation = Quaternion.LookRotation(Player.position - CameraTransform.position);
                CameraTransform.rotation = Quaternion.RotateTowards(CameraTransform.rotation, newRotation, .9f);
            }
            // This block is camera orbit behavior
            else
            {
                CameraTransform.position = followingPosition;
                CameraTransform.LookAt(Player.transform);
            }
        }
        else
        {
            inFollowingPosition = false;
            Invoke("StopZoomIn", .3f);

            // This block is camera behavior while zooming in to aiming
            if (!inAimingPosition)
            {
                CameraTransform.rotation = Quaternion.Euler(-currentY, currentX, 0);
                CameraTransform.position = Vector3.Lerp(CameraTransform.position, Player.position + aimingPosition, .2f);
            }
            // This block is camera behavior while aiming
            else
            {
                CameraTransform.rotation = Quaternion.Lerp(CameraTransform.rotation, Quaternion.Euler(-currentY, currentX, 0), 0.5f);
                CameraTransform.position = Vector3.Lerp(CameraTransform.position, Player.position + aimingPosition, .2f);
            }
        }
    }

    // StartSkill tells the camera to zoom in
    public void StartSkill()
    {
        playerInSkill = true;
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;
    }

    // EndSkill tells the camera to zoom out
    public void EndSkill()
    {
        playerInSkill = false;
        aimingPosition = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0) * aimingOffset;
    }

    private void StopZoomIn()
    {
        inAimingPosition = true;
    }

    private void StopZoomOut()
    {
        inFollowingPosition = true;
    }
}
