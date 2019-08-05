using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using CSMud.Entity;
using CSMud.Utils;
using CSMud.Client;

namespace CSMud.Events
{
    public class WorldTimer
    {

        // a beat is a periodic message sent over the server
        private Timer Beat { get; }

        private User User { get; }

        public WorldTimer(User u)
        {
            User = u;
        }

        // OnTimedEvent goes with the Beat property and is the function containing whatever happens every time the timer runs out.
        public void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            User.Ping();
        }
    }
}
