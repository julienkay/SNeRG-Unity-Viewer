using UnityEngine;

/// <summary>
/// Enables <see cref="DepthTextureMode.Depth"/> on the main camera.
/// ExecuteInEditMode is needed, so that this setting also gets 
/// applied on the scene camera when not in Play Mode.
/// </summary>
[ExecuteInEditMode]
public class EnableDepthTexture : MonoBehaviour {

    private void OnEnable() {
        Camera camera = Camera.main;

        if (camera == null) {
            Debug.LogError("Could not find main camera." +
                " For proper depth composition between SNeRGs and other opaque objects in the scene, " +
                "ensure that DeptextureMode.Depth is active on the camera used for rendering.",
                camera.gameObject);
            return;
        }

        camera.depthTextureMode = DepthTextureMode.Depth;
    }
}