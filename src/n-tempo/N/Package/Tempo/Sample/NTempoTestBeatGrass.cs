using System;
using UnityEngine;

namespace N.Package.Tempo.Sample
{
    public class NTempoTestBeatGrass : MonoBehaviour
    {
        public NTempoTestGlobalSyncTempo tempo;
        public AnimationCurve curve;
        public float scale = 0.5f;
        public bool simpleBeat = false;

        private NTempoBeat _beat;
        private Vector3 _initial;

        private void Start()
        {
            _initial = transform.localScale;
            if (simpleBeat)
            {
                _beat = tempo.SimpleBeat.Progress((beat, progress) =>
                {
                    if (beat == 0)
                    {
                        SetShear(progress);
                    }

                    return true;
                });
            }
            else
            {
                _beat = tempo.WorldBeat.Progress((beat, progress) =>
                {
                    SetShear(progress);
                    return true;
                });
            }
        }

        private void OnDestroy()
        {
            _beat.Dispose();
        }

        private void SetShear(float offset)
        {
            var distort = curve.Evaluate(offset) * scale;
            transform.localScale = new Vector3(_initial.x + distort, _initial.y, _initial.z);
        }
    }
}