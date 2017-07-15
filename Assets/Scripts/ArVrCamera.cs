//
//  This script uses iOS ARKit tracking to drive a camera as if it were a true VR camera.
//  The initial position of the camera determines the player's starting location. The camera
//  height is determined by the distance to the lowest tracked plane.
//
//  Created by Eric Phillips on July 14, 2017.
//

using UnityEngine;
using UnityEngine.VR;
using UnityEngine.XR.iOS;

[RequireComponent(typeof(Camera))]
public class ArVrCamera : MonoBehaviour
{
	private Camera _camera = null;
	private UnityARSessionNativeInterface _session = null;
	private UnityARAnchorManager _anchorManager = null;
	private Vector3 _initialPosition = Vector3.zero;
	private Quaternion _initialRotation = Quaternion.identity;


	/// <summary>
	/// Awake is called when the script instance is being loaded.
	/// </summary>
	void Awake()
	{
		_camera = GetComponent<Camera>();
		// Store initial values
		_initialPosition = transform.localPosition;
		_initialRotation = transform.localRotation;
		// Refresh at the maximum possible speed
		Application.targetFrameRate = 60;
		// Setup ARKit tracking
		_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();
		ARKitWorldTackingSessionConfiguration config = new ARKitWorldTackingSessionConfiguration();
		config.planeDetection = UnityARPlaneDetection.Horizontal;
		config.alignment = UnityARAlignment.UnityARAlignmentGravity;
		config.getPointCloudData = true;
		config.enableLightEstimation = false;
		_session.RunWithConfig(config);
		// Start anchor manager
		_anchorManager = new UnityARAnchorManager();
		// Disable native VR tracking
		VRDevice.DisableAutoVRCameraTracking(_camera, true);
	}

	/// <summary>
	/// Update is called every frame, if the MonoBehaviour is enabled.
	/// </summary>
	void Update()
	{
		// Calculate vertical offset above ground plane
		Vector3 planeOffset = Vector3.zero;
		foreach (ARPlaneAnchorGameObject anchor in _anchorManager.GetCurrentPlaneAnchors())
			planeOffset.y = Mathf.Min(planeOffset.y, UnityARMatrixOps.GetPosition(anchor.planeAnchor.transform).y);
		// Calculate camera positioning
		Matrix4x4 matrix = _session.GetCameraPose();
		transform.localPosition = _initialPosition - planeOffset + UnityARMatrixOps.GetPosition(matrix);
		transform.localRotation = _initialRotation * UnityARMatrixOps.GetRotation(matrix);
	}

	/// <summary>
	/// This function is called when the MonoBehaviour will be destroyed.
	/// </summary>
	void OnDestroy()
	{
		_anchorManager.Destroy();
	}
}
