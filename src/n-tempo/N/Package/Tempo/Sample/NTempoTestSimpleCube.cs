using System;
using System.Collections.Generic;
using UnityEngine;

namespace N.Package.Tempo.Sample
{
    public class NTempoTestSimpleCube : MonoBehaviour
    {
        private NTempo _tempo;

        public List<int> beats;
        public int offset = 0;
        public bool debug;
    
        void Start()
        {
            if (beats.Count == 0) throw new Exception("You must provide a beat set");
            _tempo = new NTempo(120);
            _tempo.Every(beats.ToArray()).WithOffset(offset).Progress((beat, tt, time, isBeat) =>
            {
                if (debug)
                {
                    Debug.Log($"Beat: {beat}, Offset: {tt}, Time: {time}, IsBeat: {isBeat}");
                }

                var distance = tt * 15;
                transform.position = new Vector3(distance, transform.position.y, transform.position.z);
                return true;
            });
        }
    }
}
