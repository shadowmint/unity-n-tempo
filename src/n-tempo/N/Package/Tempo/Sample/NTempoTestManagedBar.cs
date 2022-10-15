using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace N.Package.Tempo.Sample
{
    public class NTempoTestManagedBar : MonoBehaviour
    {
        private NTempo _tempo;

        public GameObject activeZone;

        public GameObject active;

        public List<GameObject> objects;

        private List<Vector3> _origins;

        void Start()
        {
            _tempo = NTempo.FromBeat(500);
            _tempo.Every(1, 2).Progress((beat, offset, time, isBeat) =>
            {
                if (isBeat && beat == 1)
                {
                    active = objects[0];
                    active.transform.position = activeZone.transform.position;
                    objects.RemoveAt(0);
                }

                if (beat == 1)
                {
                    // Move active object!
                    var distance = offset * 10f;
                    active.transform.position = new Vector3(active.transform.position.x, active.transform.position.y, distance);
                }

                return objects.Count > 0;
            });

            _tempo.Every(1, 2).Progress((beat, offset, time, isBeat) =>
            {
                if (isBeat && beat == 0)
                {
                    _origins = objects.Select(i => i.transform.position).ToList();
                }

                if (beat == 0)
                {
                    for (var i = 0; i < objects.Count; i++)
                    {
                        objects[i].transform.position = _origins[i] - new Vector3(10, 0, 0) * offset;
                    }
                }

                return objects.Count > 0;
            });
        }
    }
}