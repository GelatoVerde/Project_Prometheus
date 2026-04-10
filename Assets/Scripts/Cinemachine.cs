using UnityEngine;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

[SaveDuringPlay]
[AddComponentMenu("")]
public class Cinemachine : CinemachineExtension
{
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam, CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage== CinemachineCore.Stage.Finalize)
        {
            var pos = state.RawPosition;
            state.RawPosition = pos;
        }
    }
}
