using System;
using System.Collections.Generic;
using System.Linq;

namespace IdeaUnstuck.Core
{
    // Bounded samples: long sessions cannot grow this diagnostic buffer forever.
    public sealed class PollStatistics
    {
        readonly Queue<double> samples = new Queue<double>();
        int active;
        public int Count { get; private set; }
        public int MaxConcurrent { get; private set; }
        public int BlockedOverlap { get; private set; }
        public bool Enter() { if (active != 0) { BlockedOverlap++; return false; } active++; MaxConcurrent = Math.Max(active, MaxConcurrent); return true; }
        public void Leave(double milliseconds) { active = Math.Max(0, active - 1); Count++; samples.Enqueue(milliseconds); if (samples.Count > 4096) samples.Dequeue(); }
        public void Reset() { if (active != 0) return; samples.Clear(); Count = 0; MaxConcurrent = 0; BlockedOverlap = 0; }
        public object Snapshot()
        {
            var sorted = samples.OrderBy(v => v).ToArray();
            double P(double p) => sorted.Length == 0 ? 0 : sorted[Math.Max(0, (int)Math.Ceiling(p * sorted.Length) - 1)];
            return new { Count, retained = sorted.Length, medianMs = P(.5), p95Ms = P(.95), maxMs = P(1), MaxConcurrent, BlockedOverlap };
        }
    }
}
