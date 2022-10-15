using System;
using System.Collections.Generic;
using N.Package.Tempo.Internal;
using UnityEngine;

namespace N.Package.Tempo
{
    public class NTempoBeat : IDisposable
    {
        private readonly NTempo _parent;
        private readonly int[] _beats;

        private long _totalElapsed;
        private bool _started;

        private readonly List<StartBinding> _start = new();
        private readonly List<ProgressBinding> _progress = new();
        private int _beatGroup;
        private bool _beatGroupJustStarted;
        private long _beatGroupElapsed;
        private int _offset;

        public bool CatchErrors { get; set; } = true;

        public bool Paused { get; set; }

        /// Create a beat that fires every 'beats', with the given offset.
        /// So for example, if the the beats list of 1, it fires every beat.
        ///
        /// If the beats is 1, 1, 2, for a sequence of beats, the fire would be [1, 1, 1, 0, 1, 1, 1, 0]
        /// ie. the n beat is longer in duration.
        ///
        /// If an offset is given, the offset is subtracted from global world time before calculating the beat.
        /// So for example:
        ///
        ///     Offset 0, step 5, beat 50:
        ///         World time:     0       5       10      15      20      25    30    35    40    45    50
        ///         Event:          START   -       -       -       -       -     -     -     -     -     BEAT
        ///
        ///     Offset 20, step 5 (ie. run 20ms later): 
        ///         World time:     -20     -15     -10     -5      0       5     10    15    20    25    30   35    40    45   50
        ///         Event:          -       -       -       -       START   -     -     -     -     -     -    -     -     -    BEAT
        /// 
        ///     Offset 50, step 5 (ie. run 50ms later): 
        ///         World time:     -50     -45     -40     -35     -30     -25   -20   -15   -10   -5     0    5     10    15   20
        ///         Event:          -       -       -       -       -       -     -     -     -     -     START -     -     -    ...
        ///
        /// Note that when the offset is a multiple of the beatStep, there's no meaningful difference between  
        ///
        /// The beatStep should be passed in the update, so that the beat is always in sync with the BPM of the tempo it is associated
        /// with. 
        public NTempoBeat(NTempo parent, int[] beats, int offset)
        {
            _parent = parent;
            _beats = beats;
            _totalElapsed = -offset;
            _offset = offset;
        }

        public void Dispose()
        {
            _parent.Remove(this);
        }

        public NTempoBeat WithOffset(int offset)
        {
            _totalElapsed += _offset;
            _offset = offset;
            _totalElapsed -= _offset;
            return this;
        }

        /// Trigger an update on this beat; the beatStep should be derived from the BPM on the parent.
        /// For example, if the BPM is 120, the beatStep should be 60 * 1000 / 120 = 500ms.
        public void Update(int elapsedMs, int beatStep)
        {
            if (Paused) return;
            
            // If we've never run any start events, run them now, if the time is valid
            if (!_started)
            {
                if (_totalElapsed + elapsedMs >= 0)
                {
                    RunStartEvents();
                    StartNewBeatGroup(0);
                }
            }

            // Update world time
            _totalElapsed += elapsedMs;
            if (_totalElapsed < 0)
            {
                return;
            }

            // Set the distance into the current beat group into the left over
            _beatGroupElapsed += elapsedMs;

            // Update the beat group.
            var expectedBeatGroupLength = _beats[_beatGroup] * beatStep;
            while (_beatGroupElapsed > expectedBeatGroupLength)
            {
                var extraTime = _beatGroupElapsed - expectedBeatGroupLength;
                StartNewBeatGroup(null, extraTime);
                expectedBeatGroupLength = _beats[_beatGroup] * beatStep;
            }

            // Process all progress events
            foreach (var progress in _progress)
            {
                RunProgress(progress, _beatGroupElapsed, expectedBeatGroupLength, _beatGroupJustStarted);
            }

            // Prune events that are old now
            _progress.RemoveAll(i => i.remove);

            // End first beat marker
            _beatGroupJustStarted = false;
        }


        private void StartNewBeatGroup(int? useGroup = null, long initialElapsedTime = 0)
        {
            if (useGroup.HasValue)
            {
                _beatGroup = useGroup.Value;
            }
            else
            {
                _beatGroup += 1;
                if (_beatGroup >= _beats.Length)
                {
                    _beatGroup = 0;
                }
            }

            _beatGroupJustStarted = true;
            _beatGroupElapsed = initialElapsedTime;
        }

        private void RunStartEvents()
        {
            _started = true;
            _start.ForEach(RunStart);
            _start.Clear();
        }

        private void RunProgress(ProgressBinding progress, long beatGroupElapsed, long beatGroupLength, bool beatGroupJustStarted)
        {
            var currentBeat = _beatGroup;
            var globalTime = _totalElapsed;
            var beatGroupProgress = Mathf.Clamp(beatGroupElapsed / (float)beatGroupLength, 0f, 1f);
            try
            {
                if (!progress.action(currentBeat, beatGroupProgress, globalTime, beatGroupJustStarted))
                {
                    progress.remove = true;
                }
            }
            catch (Exception error)
            {
                if (!CatchErrors)
                {
                    throw;
                }

                Debug.LogError(error);
                progress.remove = true;
            }
        }

        private void RunStart(StartBinding obj)
        {
            try
            {
                obj.action(0);
            }
            catch (Exception error)
            {
                if (!CatchErrors)
                {
                    throw;
                }

                Debug.LogError(error);
            }
        }

        public NTempoBeat Start(Action action)
        {
            _start.Add(new StartBinding()
            {
                action = (_) => action()
            });
            return this;
        }

        public NTempoBeat Start(Action<int> action)
        {
            _start.Add(new StartBinding()
            {
                action = action
            });
            return this;
        }

        /// Return true from the progress function to keep running the function.
        public NTempoBeat Progress(Func<bool> action)
        {
            _progress.Add(new ProgressBinding()
            {
                action = (_, _, _, _) => action()
            });
            return this;
        }

        /// Action in the for (int beat) => { ... }
        /// Return true from the progress function to keep running the function.
        public NTempoBeat Progress(Func<int, bool> action)
        {
            _progress.Add(new ProgressBinding()
            {
                action = (beat, _, _, _) => action(beat)
            });
            return this;
        }

        /// Action in the for (int beat, float progressThroughMinute) => { ... }
        /// Return true from the progress function to keep running the function.
        public NTempoBeat Progress(Func<int, float, bool> action)
        {
            _progress.Add(new ProgressBinding()
            {
                action = (beat, tt, _, _) => action(beat, tt)
            });
            return this;
        }

        /// Action in the for (int beat, float progressThroughMinute, float totalElapsedTime) => { ... }
        /// Return true from the progress function to keep running the function.
        public NTempoBeat Progress(Func<int, float, long, bool> action)
        {
            _progress.Add(new ProgressBinding()
            {
                action = (beat, tt, time, _) => action(beat, tt, time)
            });
            return this;
        }

        /// Action in the for (int beat, float progressThroughMinute, float totalElapsedTime, bool beatStarted) => { ... }
        /// Return true from the progress function to keep running the function.
        public NTempoBeat Progress(Func<int, float, long, bool, bool> action)
        {
            _progress.Add(new ProgressBinding()
            {
                action = action
            });
            return this;
        }

        private class ProgressBinding
        {
            /// Arguments are beat, offset into beat between 0 and 1, total elapsed time, is this the first beat progress callback.
            /// It should return false to stop running and true to keep running.
            public Func<int, float, long, bool, bool> action;
            public bool remove;
        }

        private struct StartBinding
        {
            public Action<int> action;
        }
    }
}