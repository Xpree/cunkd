using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Mirror;

public class PlayerCameraController : NetworkBehaviour
{
    float mouseSensitivity = 25.0f;
    private Transform cameraTransform;
    public Camera playerCamera;

    float pitch = 0.0f;
    public GameObject visor;
    // Start is called before the first frame update
    void Awake()
    {
        playerCamera.enabled = false;
        cameraTransform = playerCamera.transform;
        playerCamera.GetComponent<AudioListener>().enabled = false;
    }

    public void onMovement(InputAction.CallbackContext context)
    {
        Vector2 position = context.ReadValue<Vector2>();

        float xMovement = position.x * mouseSensitivity * Time.deltaTime;
        float yMovement = position.y * mouseSensitivity * Time.deltaTime;

        pitch -= yMovement;
        pitch = Mathf.Clamp(pitch, -90.0f, 90.0f);
      
        cameraTransform.localRotation = Quaternion.Euler(pitch, 0.0f, 0.0f);
        transform.Rotate(Vector3.up * xMovement);
    }

    public override void OnStartLocalPlayer()
    {
        Camera.main.enabled = false;
        playerCamera.enabled = true;
        playerCamera.GetComponent<AudioListener>().enabled = true;

        Cursor.lockState = CursorLockMode.Locked;

        //if (playerCamera != null)
        //{
        //    // configure and make camera a child of player with 3rd person offset
        //    playerCamera.orthographic = false;
        //    playerCamera.transform.SetParent(transform);
        //    playerCamera.transform.localPosition = new Vector3(0f, 0.635f, 0.387f);
        //    playerCamera.transform.localEulerAngles = new Vector3(10f, 0f, 0f);
        //}
    }

    public override void OnStopLocalPlayer()
    {
        Camera.main.enabled = true;
        playerCamera.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        //if (playerCamera != null)
        //{
        //    playerCamera.transform.SetParent(null);
        //    SceneManager.MoveGameObjectToScene(playerCamera.gameObject.transform.parent.gameObject, SceneManager.GetActiveScene());
        //    playerCamera.orthographic = true;
        //    playerCamera.transform.localPosition = new Vector3(0f, 70f, 0f);
        //    playerCamera.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
        //}
    }
}
