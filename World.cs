using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace CSMud
{
    // A World holds connections and handles broadcasting messages from those connections
    public class World
    {
        // a world has connections
        public List<User> Users
        { get; }

        // a beat is a periodic message sent over the server
        private Timer Beat
        { get; }

        public Map WorldMap { get; }

        // constructor
        public World()
        {
            // create a list object of connections
            this.Users = new List<User>();
            // Create a beat on the server
            this.Beat = new Timer(45000)
            {
                AutoReset = true,
                Enabled = true
            };
            this.Beat.Elapsed += OnTimedEvent;
        }

        // OnTimedEvent goes with the Beat property and is the function containing whatever happens every time the timer runs out.
        void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            Broadcast("The world is dark and silent.");
        }

        // add new Connections to the world
        public void NewConnection(Socket clientSock)
        {
            User handshake(Connection conn)
            {
                conn.SendMessage("Please enter a name: ");
                while (true)
                {
                    string username = conn.ReadMessage();
                    if (username is null)
                    {
                        continue;
                    }
                    else if (username == "")
                    {
                        username = "Someone";
                    }
                    User user = new User(conn, this, username);
                    lock (Users)
                    {
                        Users.Add(user);
                    }

                    #region Subscribe the user to events. 
                    user.RaiseLookEvent += this.HandleLookEvent;
                    user.RaiseHelpEvent += this.HandleHelpEvent;
                    user.RaiseInventoryQueryEvent += this.HandleInventoryQueryEvent;
                    user.RaiseJumpEvent += this.HandleJumpEvent;
                    user.RaiseListenEvent += this.HandleListenEvent;
                    user.RaiseNoEvent += this.HandleNoEvent;
                    user.RaisePrayEvent += this.HandlePrayEvent;
                    user.RaiseSingEvent += this.HandleSingEvent;
                    user.RaiseSleepEvent += this.HandleSleepEvent;
                    user.RaiseSorryEvent += this.HandleSorryEvent;
                    user.RaiseSwimEvent += this.HandleSwimEvent;
                    user.RaiseThinkEvent += this.HandleThinkEvent;
                    user.RaiseWakeUpEvent += this.HandleWakeUpEvent;
                    user.RaiseWaveEvent += this.HandleWaveEvent;
                    user.RaiseYesEvent += this.HandleYesEvent;
                    user.RaiseParameterizedEvent += this.HandleParameterizedEvent;
                    #endregion

                    return user;
                }
            }
            
            Task.Run(() =>
            {
                using (Connection conn = new Connection(clientSock))
                {
                    User user = null;

                    try
                    {
                        user = handshake(conn);

                        // tell the server that a user has connected
                        Console.WriteLine($"{user.Name} has connected.");
                        Broadcast($"{user.Name} has connected.");
                    }
                    catch (System.IO.IOException e) when (e.InnerException is SocketException)
                    {
                        Console.WriteLine("Connection attempt aborted: client disconnected.");
                        return;
                    }

                    try
                    {
                        user.Start();
                    }
                    catch (System.IO.IOException e) when (e.InnerException is SocketException)
                    {
                        Console.WriteLine("Connection aborted by client.");
                    }
                    finally
                    {
                        this.EndConnection(user);
                        this.Broadcast($"{user.Name} has disconnected.");
                        Console.WriteLine($"{user.Name} has disconnected.");
                    }
                }
            });
        }

        // when someone leaves, endconnection removes the connection from the list
        public void EndConnection(User user)
        {
            lock (Users)
            {
                Users.Remove(user);
            }
        }

        // Broadcast handles sending a message over the server - y'know, broadcasting it.
        public void Broadcast(string msg)
        {
            lock (Users)
            {
                foreach (User user in Users)
                {
                    user.Connection.SendMessage(msg);
                }
            }
        }


        #region Handlers for events 

        void HandleLookEvent(object sender, EventArgs e)
        {
           (sender as User).Connection.SendMessage("You are in a room. It is very plain, white walls, featureless, doorless.");
        }

        void HandleHelpEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage(@"Help:
'help' : Display this message
'quit' : Exit the game
'inventory' or 'i' : Display inventory.
'jump' : Jump in place.
'listen' : Take in the ambient sounds.
'no' or 'n' : Decline.
'pray' : Offer a prayer to your deity.
'sing' : Sing a little tune.
'sleep' : Take a nap.
'sorry' : Apologize.
'swim' : Take a dip.
'think' : Ponder.
'wake up' : Wake yourself up.
'wave' : Wave.
'yes' or 'y' : Agree.");
        }

        void HandleInventoryQueryEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You turn out your pockets. You have pocket lint, and a single Rhinu.");
        }

        void HandleJumpEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You give a little hop.");
        }

        void HandleListenEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You strain your ears. The world is silent, undeveloped and abandoned.");
        }

        void HandleNoEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You firmly shake your head no.");
        }

        void HandlePrayEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You offer a quick prayer to your god.");
        }

        void HandleSingEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("Five hundred bottles of beer on the wall, five hundred bottles of beer... take one down, pass it 'round, 499 bottles of beer on the wall...");
        }

        void HandleSleepEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You flop over onto the floor to catch up on your sleep.");
        }

        void HandleSorryEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You express deepest apologies for your transgressions.");
        }

        void HandleSwimEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("This undeveloped white box does not have any water to swim in.");
        }

        void HandleThinkEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You sit on the ground and ponder the universe for a while. What even is entropy?");
        }

        void HandleWakeUpEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You wake yourself up and stand.");
        }

        void HandleWaveEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You wave at the wall. Unsurprisingly, it does not wave back.");
        }

        void HandleYesEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You nod affirmatively.");
        }

        void HandleParameterizedEvent(object sender, ParameterizedEvent e)
        {
            /*(sender as User).Connection.SendMessage(e.Command);
            if (e.Action != null)
            {
                 (sender as User).Connection.SendMessage(e.Action);
            }*/



        }

        #endregion
    }
}
