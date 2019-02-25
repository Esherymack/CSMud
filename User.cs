using System;

namespace CSMud
{
    public class User : IDisposable
    {
        public Connection Connection { get; }

        /*
         * Events for commands.
         */  
        public event EventHandler RaiseLookEvent;
        public event EventHandler RaiseHelpEvent;
        public event EventHandler RaiseInventoryQueryEvent;
        public event EventHandler RaiseJumpEvent;
        public event EventHandler RaiseListenEvent;
        public event EventHandler RaiseNoEvent;
        public event EventHandler RaisePrayEvent;
        public event EventHandler RaiseSingEvent;
        public event EventHandler RaiseSleepEvent;
        public event EventHandler RaiseSorryEvent;
        public event EventHandler RaiseSwimEvent;
        public event EventHandler RaiseThinkEvent;
        public event EventHandler RaiseWakeUpEvent;
        public event EventHandler RaiseWaveEvent;
        public event EventHandler RaiseYesEvent;
        public event EventHandler RaiseYeetEvent;

        /*
         * End Events
         */


        // a connection has a world
        private World World
        { get; }

        public string Name { get; }

        public User(Connection conn, World world, string name)
        {
            this.Connection = conn;
            this.World = world;
            this.Name = name;
        }

        // OnConnect handles the welcome messages and tells the server client that someone has connected.
        private void OnConnect()
        {
            // multiline string literal is making me very upset, many conniptions
            Connection.SendMessage("Welcome!\nSend 'quit' to exit.\nSend 'help' for help.");
        }

        // OnDisconnect handles removing the terminated connections and tells the sever client that someone has disconnected.
        private void OnDisconnect()
        {
            this.World.EndConnection(this);
            this.World.Broadcast($"{this.Name} has disconnected.");
            Console.WriteLine($"{this.Name} has disconnected.");
        }

        public void Start()
        {
            // Welcome the user to the game
            OnConnect();
            while (true)
            {
                // Get a message that's sent to the server
                string line = Connection.ReadMessage();

                  if (line == null || line == "quit")
                  {
                      break;
                  }

                // Parse commands; default assumes no command was passed and outputs as a Say
                switch (line)
                {
                    case "look":
                        OnRaiseLookEvent();
                        break;
                    case "help":
                        OnRaiseHelpEvent();
                        break;
                    case "inventory":
                    case "i":
                        OnRaiseInventoryQueryEvent();
                        break;
                    case "jump":
                        OnRaiseJumpEvent();
                        break;
                    case "listen":
                        OnRaiseListenEvent();
                        break;
                    case "no":
                    case "n":
                        OnRaiseNoEvent();
                        break;
                    case "pray":
                        OnRaisePrayEvent();
                        break;
                    case "sing":
                        OnRaiseSingEvent();
                        break;
                    case "sleep":
                        OnRaiseSleepEvent();
                        break;
                    case "sorry":
                        OnRaiseSorryEvent();
                        break;
                    case "swim":
                        OnRaiseSwimEvent();
                        break;
                    case "think":
                        OnRaiseThinkEvent();
                        break;
                    case "wake up":
                        OnRaiseWakeUpEvent();
                        break;
                    case "wave":
                        OnRaiseWaveEvent();
                        break;
                    case "yes":
                    case "y":
                        OnRaiseYesEvent();
                        break;
                    case "this bitch empty":
                        OnRaiseYeetEvent();
                        break;
                    default:
                        string textMessage = FormatMessage(line);
                        this.World.Broadcast(textMessage);
                        break;
                }
            }
        }

        /*
         * Protected virtual funcs for event raises.
         */ 
        protected virtual void OnRaiseLookEvent()
        {
            EventHandler handler = RaiseLookEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseHelpEvent()
        {
            EventHandler handler = RaiseHelpEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseInventoryQueryEvent()
        {
            EventHandler handler = RaiseInventoryQueryEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseJumpEvent()
        {
            EventHandler handler = RaiseJumpEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseListenEvent()
        {
            EventHandler handler = RaiseListenEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseNoEvent()
        {
            EventHandler handler = RaiseNoEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaisePrayEvent()
        {
            EventHandler handler = RaisePrayEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseSingEvent()
        {
            EventHandler handler = RaiseSingEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseSleepEvent()
        {
            EventHandler handler = RaiseSleepEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseSorryEvent()
        {
            EventHandler handler = RaiseSorryEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseSwimEvent()
        {
            EventHandler handler = RaiseSwimEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseThinkEvent()
        {
            EventHandler handler = RaiseThinkEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseWakeUpEvent()
        {
            EventHandler handler = RaiseWakeUpEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseWaveEvent()
        {
            EventHandler handler = RaiseWaveEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseYesEvent()
        {
            EventHandler handler = RaiseYesEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseYeetEvent()
        {
            EventHandler handler = RaiseYeetEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        /*
         *  End events
         */ 

        /*
		 * ProcessLine handles organizing messages on the MUD server
		 * as of right now it just trims and sends the message to SendMessasge
		 */
        string FormatMessage(string line)
        {
            return $"{this.Name} says, '{line.Trim()}'";
        }

        public void Dispose()
        {
            ((IDisposable)Connection).Dispose();
        }
    }
}
