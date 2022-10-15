using NUnit.Framework;
using UnityEngine;

namespace N.Package.Tempo.Editor
{
    public class NTempoBeatTests
    {
        [Test]
        public void TestBeatSingleStep()
        {
            var tempo = new NTempo(60);
            var beat = new NTempoBeat(tempo, new[] { 1 }, 0);
            beat.CatchErrors = false;

            var started = false;
            var ticks = 0;
            var newBeats = 0;

            beat.Start(() => { started = true; }).Progress((count, progress, time, isNewBeat) =>
            {
                // We only have one beat, so we're always in the first beat
                Assert.AreEqual(count, 0);
                
                // The progress should scale across the beat
                var expectedProgress = ticks / 2f;
                Assert.IsTrue(Mathf.Abs(progress- expectedProgress) < 0.001f);
                
                // The global time should increment over time
                var expectedGlobalTime = ticks * 50; 
                Assert.AreEqual(expectedGlobalTime, time);

                ticks += 1;
                
                if (isNewBeat)
                {
                    newBeats += 1;
                }
                
                return true;
            });

            beat.Update(0, 100);
            beat.Update(50, 100);
            beat.Update(50, 100);

            Assert.AreEqual(started, true);
            Assert.AreEqual(ticks, 3);
            Assert.AreEqual(newBeats, 1);
        }
    }
}