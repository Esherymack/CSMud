using System;

namespace CSMud
{
    // Class for parameterized events - included it here because it's very tiny.
    public class ParameterizedEvent : EventArgs
    {
        public string Command { get; set; }
        public string Action { get; set; }
    }

    public class User : IDisposable
    {
        public Connection Connection { get; }

        #region Events for commands.
        // These are unparameterized events - they are just called and always do the same thing.
        public event EventHandler RaiseHelpEvent;
        public event EventHandler RaiseLookEvent;
        public event EventHandler RaiseWhoEvent;
        public event EventHandler RaiseInventoryQueryEvent;
        public event EventHandler RaiseNoEvent;
        public event EventHandler RaiseYesEvent;

        // This is a parameterized event that affects a Thing or Entity
        public event EventHandler<ParameterizedEvent> RaiseParameterizedEvent;

        #endregion

        // a user belongs to a world
        private World World
        { get; }

        // a user has a unique inventory
        public Inventory Inventory
        { get; set; }

        // a user is set in a specific room
        public int CurrRoomId { get; set; }

        // a user has a name
        public string Name { get; }

        public User(Connection conn, World world, string name)
        {
            this.Connection = conn;
            this.World = world;
            this.Name = name;
            Inventory = new Inventory();

            this.CurrRoomId = 1;
        }

        // OnConnect handles the welcome messages and tells the server client that someone has connected.
        private void OnConnect()
        {
            // multiline string literal is making me very upset, many conniptions
            Connection.SendMessage(@"Welcome!
Send 'quit' to exit.
Send 'help' for help.");
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
                string[] splitLine = line.Split(new char[] { ' ' }, 2);

                if (splitLine == null || splitLine[0] == "quit")
                {
                    break;
                }
                else if (splitLine.Length == 1)
                {
                    switch (splitLine[0])
                    {
                        case "help":
                            OnRaiseHelpEvent();
                            break;
                        case "inventory":
                        case "i":
                            OnRaiseInventoryQueryEvent();
                            break;
                        case "no":
                        case "n":
                            OnRaiseNoEvent();
                            break;
                        case "yes":
                        case "y":
                            OnRaiseYesEvent();
                            break;
                        case "look":
                            OnRaiseLookEvent();
                            break;
                        case "who":
                            OnRaiseWhoEvent();
                            break;
                    }
                }
                else
                {
                    OnParameterizedEvent(new ParameterizedEvent { Command = splitLine[0], Action = splitLine[1] });
                }             
            }
        }

        #region Protected virtual funcs for event raises.
        protected virtual void OnRaiseHelpEvent()
        {
            EventHandler handler = RaiseHelpEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseLookEvent()
        {
            EventHandler handler = RaiseLookEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseWhoEvent()
        {
            EventHandler handler = RaiseWhoEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseInventoryQueryEvent()
        {
            EventHandler handler = RaiseInventoryQueryEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseNoEvent()
        {
            EventHandler handler = RaiseNoEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnRaiseYesEvent()
        {
            EventHandler handler = RaiseYesEvent;
            handler?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnParameterizedEvent(ParameterizedEvent e)
        {
            EventHandler<ParameterizedEvent> handler = RaiseParameterizedEvent;
            handler?.Invoke(this, e);
        }

        #endregion End events

        /*
		 * ProcessLine handles organizing messages on the MUD server
		 * as of right now it just trims and sends the message to SendMessasge
		 */
        public static string FormatMessage(string line, string name)
        {
            return $"{name} says, '{line.Trim()}'";
        }

        public void Dispose()
        {
            ((IDisposable)Connection).Dispose();
        }
    }
}
