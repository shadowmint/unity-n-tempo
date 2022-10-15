using UnityEngine;

namespace N.Package.Tempo.Sample
{
    [DefaultExecutionOrder(-50)]
    public class NTempoTestGlobalSyncTempo : MonoBehaviour
    {
        public NTempo Tempo;

        public NTempoBeat WorldBeat { get; set; }
        public NTempoBeat SimpleBeat { get; set; }
        
        private void Start()
        {
            Tempo = NTempo.FromBeat(500);
            
            // daa da da
            WorldBeat = Tempo.Every(2, 1, 1);
            SimpleBeat = Tempo.Every(2, 2);
        }
    }
}
