using Cinemachine;
using StarterAssets;
using UnityEngine;


public class CameraController : MonoBehaviour
{

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public CinemachineVirtualCamera CinemachineCamera;
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 70.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -30.0f;
	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float CameraAngleOverride = 0.0f;
	[Tooltip("For locking the camera position on all axis")]
	public bool LockCameraPosition = false;

	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;
	private const float _threshold = 0.01f;
	private void CameraRotation()
	{
		var localPlayer = PlayerController.Local;
		if (localPlayer == null)
			return;
		if (CinemachineCamera == null)
			return;
		CinemachineCamera.Follow = localPlayer.CinemachineCameraTarget?.transform;
		var input = localPlayer.GetComponent<StarterAssetsInputs>();
		// if there is an input and camera position is not fixed
		if (input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
		{
			_cinemachineTargetYaw += input.look.x * Time.deltaTime;
			_cinemachineTargetPitch += input.look.y * Time.deltaTime;
		}

		// clamp our rotations so our values are limited 360 degrees
		_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

		// Cinemachine will follow this target
		localPlayer.CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
	}
	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

    private void LateUpdate()
	{
		CameraRotation();
	}
}
