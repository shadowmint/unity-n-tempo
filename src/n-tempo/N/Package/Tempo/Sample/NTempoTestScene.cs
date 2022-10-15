using UnityEngine;

namespace N.Package.Tempo.Sample
{
    [DefaultExecutionOrder(-100)]
    public class NTempoTestScene : MonoBehaviour
    {
        void Start()
        {
            GetComponent<NTempoSystem>().Initialize();
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.SetResolution(640, 480, false);
        }
    }
}
