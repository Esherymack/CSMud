using System;

namespace CSMud.Events
{
    // Class for parameterized events
    public class ParameterizedEvent : EventArgs
    {
        public string Command { get; set; }
        public string Action { get; set; }
    }
}