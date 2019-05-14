using System;

namespace CSMud
{
    // Class for parameterized events
    public class ParameterizedEvent : EventArgs
    {
        public string Command { get; set; }
        public string Action { get; set; }
    }
}