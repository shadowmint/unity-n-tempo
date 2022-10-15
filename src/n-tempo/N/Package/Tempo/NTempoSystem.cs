using N.Package.GameSystems;
using UnityEngine;

namespace N.Package.Tempo
{
    public class NTempoSystem : GameSystem
    {
        private NTempoManager _manager;
        
        public override void Initialize()
        {
            _manager = new NTempoManager();
            NTempoManager.SetAmbientTempoManager(_manager);
        }

        public override void Dispose()
        {
        }

        public void FixedUpdate()
        {
            _manager.Update(Time.deltaTime);
        }
    }
}