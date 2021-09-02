using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

/*
 * This script controls player movement; allows players to move and look around
 * 
 */

public class PlayerMovement : NetworkBehaviour
{
    public float playerSpeed;
    public float pitch;

    CharacterController cc;

    public Transform cameraTransform;

    // Start is called before the first frame update
    void Start()
    {
        if (!IsLocalPlayer)
        {
            cameraTransform.GetComponent<AudioListener>().enabled = false;
            cameraTransform.GetComponent<Camera>().enabled = false;

        }
        else
            cc = GetComponent<CharacterController>();

    }

    // Update is called once per frame
    void Update()
    {
        if (IsLocalPlayer)
        {
            MovePlayer();
            LookPlayer();
        }
    }

    void MovePlayer()
    {
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        move = Vector3.ClampMagnitude(move, 1f);
        move = transform.TransformDirection(move);
        cc.SimpleMove(move * playerSpeed);
    }

    void LookPlayer()
    {
        float mousex = Input.GetAxis("Mouse X") * 8f;
        float mousey = Input.GetAxis("Mouse Y") * 5f;
        transform.Rotate(0, mousex, 0);

        pitch -= mousey;
        pitch = Mathf.Clamp(pitch, -85, 85);
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0, 0);

    }
}