using System;
using System.Collections.Generic;
using System.Linq;
using N.Package.Tempo.Errors;
using N.Package.Tempo.Internal;

namespace N.Package.Tempo
{
    public class NTempo : IDisposable
    {
        private Action<NTempo, bool> _setActive;
        private readonly NTempoManager _manager;
        private readonly List<NTempoBeat> _beats = new();
        private bool _active;
        private float _beatsPerMinute;
        private int _beatStep;
        private int _lastElapsedMs;

        public static NTempo FromBeat(long msPerBeat)
        {
            if (msPerBeat < 0) throw new NTempoException(NTempoError.InvalidBeatsPerMinute);
            return new NTempo(NTempoConsts.MSPerMinute / (float)msPerBeat);
        }

        public static NTempo FromBeatInSeconds(long secondsPerBeat)
        {
            return FromBeat(secondsPerBeat * 1000);
        }

        public NTempo(int beatsPerMinute) : this((float)beatsPerMinute)
        {
        }

        public NTempo(float beatsPerMinute)
        {
            BeatsPerMinute = beatsPerMinute;
            _manager = NTempoManager.Instance;
            _manager.Add(this);
        }

        public void Bind(Action<NTempo, bool> setActive)
        {
            _setActive = setActive;
        }

        public float BeatsPerMinute
        {
            get => _beatsPerMinute;
            set
            {
                _beatsPerMinute = value;
                _beatStep = (int)Math.Floor(NTempoConsts.MSPerMinute / _beatsPerMinute);
            }
        }

        public bool Active
        {
            get => _active;
            set
            {
                _setActive?.Invoke(this, value);
                _active = value;
            }
        }

        public void Update(int elapsedMs)
        {
            _lastElapsedMs = elapsedMs;
            _beats.ForEach(UpdateBeat);
        }

        private void UpdateBeat(NTempoBeat beat)
        {
            beat.Update(_lastElapsedMs, _beatStep);
        }

        public NTempoBeat Every(params int[] beats)
        {
            var beat = new NTempoBeat(this, beats.ToArray(), 0);
            _beats.Add(beat);
            return beat;
        }

        public NTempoBeat Every(int beats)
        {
            var beat = new NTempoBeat(this, new[] { beats }, 0);
            _beats.Add(beat);
            return beat;
        }

        public void Stop()
        {
            Active = false;
        }

        public void Remove(NTempoBeat beat)
        {
            if (!_beats.Contains(beat)) return;
            _beats.Remove(beat);
        }

        public void Dispose()
        {
            _manager.Remove(this);
        }
    }
}