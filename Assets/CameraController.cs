using Cinemachine;
using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class CameraController : MonoBehaviour
{

	[Header("Cinemachine")]
	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 70.0f;
	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -30.0f;
	[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	public float CameraAngleOverride = 0.0f;
	[Tooltip("For locking the camera position on all axis")]
	public bool LockCameraPosition = false;

	[SerializeField] StarterAssetsInputs _input;

	CinemachineVirtualCamera _cinemachineCamera;

	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;
	private const float _threshold = 0.01f;

    private void Awake()
    {
		_cinemachineCamera = GetComponent<CinemachineVirtualCamera>();
	}

    private void CameraRotation()
	{
		var localPlayer = PlayerController.Local;
		if (localPlayer == null)
			return;
		if(_cinemachineCamera.Follow == null)
			_cinemachineCamera.Follow = localPlayer.CinemachineCameraTarget?.transform;
		// if there is an input and camera position is not fixed
		if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
		{
			_cinemachineTargetYaw += _input.look.x * Time.deltaTime;
			_cinemachineTargetPitch += _input.look.y * Time.deltaTime;
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
