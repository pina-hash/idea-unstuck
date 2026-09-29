using System.Collections.Generic;

namespace IdeaUnstuck.Core
{
    public sealed class HelpStep
    {
        public string T { get; set; }
        public string W { get; set; }
    }
    public sealed class HelpEntry
    {
        public string Id { get; set; }
        public string Kind { get; set; }
        public string Title { get; set; }
        public string Q { get; set; }
        public string Fix { get; set; }
        public List<HelpStep> Steps { get; set; }
        public string Why { get; set; }
        public List<string> Related { get; set; }
    }
    public sealed class HelpResult
    {
        public HelpEntry Entry { get; internal set; }
        public double Score { get; internal set; }
    }
}
