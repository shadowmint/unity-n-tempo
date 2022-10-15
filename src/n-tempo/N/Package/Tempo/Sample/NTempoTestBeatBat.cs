using System.Linq;
using UnityEngine;

namespace N.Package.Tempo.Sample
{
    public class NTempoTestBeatBat : MonoBehaviour
    {
        public NTempoTestGlobalSyncTempo tempo;
        public AnimationCurve curve;
        public int[] beats;
        public float distance = -5f;

        private NTempoBeat _beat;
        private Vector3 _initial;

        private void Start()
        {
            _beat = tempo.WorldBeat.Progress((beat, progress, time, isStart) =>
            {
                // Move on the 4th beat
                if (beats.Contains(beat))
                {
                    if (isStart)
                    {
                        if (transform.position.x < -14f)
                        {
                            transform.transform.position = new Vector3(14f, _initial.y, _initial.z);
                        }

                        _initial = transform.position;
                    }

                    Move(progress);
                }

                return true;
            });
        }

        private void OnDestroy()
        {
            _beat.Dispose();
        }

        private void Move(float progress)
        {
            var currentStep = curve.Evaluate(progress) * distance;
            transform.transform.position = new Vector3(_initial.x + currentStep, _initial.y, _initial.z);
        }
    }
}