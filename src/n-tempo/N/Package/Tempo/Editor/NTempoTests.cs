using NUnit.Framework;

namespace N.Package.Tempo.Editor
{
    public class NTempoTests
    {
        [Test]
        public void TestTempoEnd2End()
        {
            var manager = new NTempoManager();
            NTempoManager.SetAmbientTempoManager(manager);
            
            var tempo = new NTempo(60);
            var started = false;
            var ticks = 0;
            var beats = 0;
            tempo.Every(1).Start(() => { started = true; }).Progress((_, _, _, isBeat) =>
            {
                ticks += 1;
                if (isBeat)
                {
                    beats += 1;
                }
                
                return true;
            });
            
            manager.Update(0.01f);
            manager.Update(0.1f);

            Assert.AreEqual(started, true);
            Assert.AreEqual(ticks, 2);
            Assert.AreEqual(beats, 1);
        }
    }
}