using System;

namespace IdeaUnstuck.Core
{
    public sealed class ProgressTelemetry
    {
        readonly Telemetry log;
        string document;
        DateTime started;
        bool recorded;
        public ProgressTelemetry(Telemetry log) { this.log = log; }
        public void Observe(StateSnapshot state)
        {
            if (state.ReadIssues.Count > 0) return;
            if (document != state.DocumentKey) { document = state.DocumentKey; started = state.Utc; recorded = state.BodyCount > 0; }
            if (state.DocumentType == 1 && !recorded && state.BodyCount > 0) {
                recorded = true;
                log.Write("first_body", new { seconds = Math.Max(0, (state.Utc-started).TotalSeconds), existingBody = false });
            }
        }
    }
}
