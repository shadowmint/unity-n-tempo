using System;
using System.Collections.Generic;
using UnityEngine;

namespace N.Package.Tempo
{
    public class NTempoManager
    {
        private readonly List<NTempo> _active = new();

        private readonly List<NTempo> _inactive = new();

        private static NTempoManager _instance;

        public static NTempoManager Instance
        {
            get { return _instance ??= new NTempoManager(); }
        }

        public static void SetAmbientTempoManager(NTempoManager manager)
        {
            _instance = manager;
        }

        private void SetActive(NTempo tempo, bool active)
        {
            if (active)
            {
                if (_inactive.Contains(tempo))
                {
                    _inactive.Remove(tempo);
                }

                _active.Add(tempo);
            }
            else
            {
                if (_active.Contains(tempo))
                {
                    _active.Remove(tempo);
                }

                _inactive.Add(tempo);
            }
        }

        public void Add(NTempo tempo)
        {
            tempo.Bind(SetActive);
            SetActive(tempo, true);
        }

        public void Remove(NTempo tempo)
        {
            if (_active.Contains(tempo))
            {
                _active.Remove(tempo);
            }

            if (_inactive.Contains(tempo))
            {
                _inactive.Remove(tempo);
            }
        }

        public void Update(float deltaSeconds)
        {
            foreach (var tempo in _active)
            {
                tempo.Update( Mathf.RoundToInt(deltaSeconds * 1000f));
            }
        }
    }
}