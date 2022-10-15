using System;
using System.Collections.Generic;
using UnityEngine;

namespace N.Package.Tempo.Sample
{
    public class NTempoTestSimpleRotatingObject : MonoBehaviour
    {
        private NTempo _tempo;

        public List<int> beats;
        public int offset = 0;
        public bool debug;
        public int haltAfter = -1;
    
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

                transform.rotation = Quaternion.Euler(0, 0,tt * 360);
                
                return haltAfter < 0 || haltAfter > time;
            });
        }
    }
}