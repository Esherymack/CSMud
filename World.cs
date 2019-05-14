using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Timers;

namespace CSMud
{
    // A World holds connections and handles broadcasting messages from those connections
    // The world also handles taking care of all of the commands, both parameterized and unparameterized.
    public class World
    {
        // a world has connections
        public List<User> Users { get; }

        // a beat is a periodic message sent over the server
        private Timer Beat { get; }

        public MapBuild WorldMap { get; set; }

        public ProcessUnparameterizedCommand PUC { get; set; }

        // constructor
        public World()
        {
            // create a list object of connections
            Users = new List<User>();
            // Create a beat on the server
            Beat = new Timer(45000)
            {
                AutoReset = true,
                Enabled = true
            };
            Beat.Elapsed += OnTimedEvent;

            // Generate the map
            WorldMap = new MapBuild();

            // Initialize command processors 
            PUC = new ProcessUnparameterizedCommand(WorldMap, Users);

            foreach(Entity e in WorldMap.Entities)
            {
                e.PopulateInventory();
            }
        }

        // OnTimedEvent goes with the Beat property and is the function containing whatever happens every time the timer runs out.
        void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            Broadcast("Something scurries around in the distance.");
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
                    if (username == "")
                    {
                        username = "Someone";
                    }
                    User user = new User(conn, this, username);
                    lock (Users)
                    {
                        Users.Add(user);
                    }

                    #region Subscribe the user to events. 
                    user.Command.RaiseHelpEvent += PUC.HandleHelpEvent;
                    user.Command.RaiseLookEvent += PUC.HandleLookEvent;
                    user.Command.RaiseWhoEvent += PUC.HandleWhoEvent;
                    user.Command.RaiseInventoryQueryEvent += HandleInventoryQueryEvent;
                    user.Command.RaiseNoEvent += HandleNoEvent;
                    user.Command.RaiseYesEvent += HandleYesEvent;
                    user.Command.RaiseParameterizedEvent += HandleParameterizedEvent;
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
                        EndConnection(user);
                        Broadcast($"{user.Name} has disconnected.");
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

        // Broadcast handles sending a message over the entire server (all players can see it regardless of their room)
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

        // RoomSay handles sending a message to all Users within a Room 
        // Semantically this is the same as Broadcast, but with the exception that it only sends to players whose CurrRoomId's match the speaker's.
        public void RoomSay(string msg, User sender)
        {
            lock (Users)
            {
                foreach (User user in Users)
                {
                    if (user.CurrRoomId == sender.CurrRoomId)
                    {
                        user.Connection.SendMessage(msg);
                    }
                }
            }
        }

        #region Handlers for events
        #region Inventory handler
        // Inventory gets the user's inventory.
        void HandleInventoryQueryEvent(object sender, EventArgs e)
        {
            User s = sender as User;
            s.Connection.SendMessage($"Your current carry capacity is {s.Inventory.CurrentCapacity}/{s.Inventory.CarryCapacity}");
            if (s.Inventory.Empty)
            {
                s.Connection.SendMessage($"You turn out your pockets. You have nothing.");
            }
            else
            {
                s.Connection.SendMessage($"Your inventory consists of:\n{s.Inventory.ToString()}");
            }
        }
        #endregion
        #region Yes/No handlers
        // No says no.
        void HandleNoEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You firmly shake your head no.");
        }

        // Yes says yes.
        void HandleYesEvent(object sender, EventArgs e)
        {
            (sender as User).Connection.SendMessage("You nod affirmatively.");
        }
        #endregion
        #region Parameterized Event handlers
        // Switch statement for handling events with params
        void HandleParameterizedEvent(object sender, ParameterizedEvent e)
        {
            User s = sender as User;
            string action = e.Action;
            switch (e.Command)
            {
                case "say":
                    RoomSay($"{User.FormatMessage(action, s.Name)}", s);
                    break;
                case "whisper":
                    HandleWhisper(s, action);
                    break;
                case "take":
                    HandleTake(s, action);
                    break;
                case "drop":
                    HandleDrop(s, action);
                    break;
                case "eat":
                    HandleEat(s, action);
                    break;
                case "drink":
                    HandleDrink(s, action);
                    break;
                case "examine":
                    HandleExamine(s, action);
                    break;
                case "go":
                    HandleGo(s, action);
                    break;
                case "equip":
                case "wear":
                    HandleEquip(s, action);
                    break;
                case "remove":
                    HandleRemove(s, action);
                    break;
                case "hold":
                    HandleHold(s, action);
                    break;
                case "attack":
                    HandleAttack(s, action);
                    break;
                case "talk":
                    HandleTalkTo(s, action);
                    break;
                default:
                    s.Connection.SendMessage("You cannot do that.");
                    break;
            }
        }
        #region Take handler
        /* HandleTake and HandleDrop are relatively similar functions that just perform the inverses of each other
         * HandleTake checks if the item a user wants to take exists in the context of the room they are in.
         * if item exists, remove it from the room's list of references and add it to the player's inventory list instead       
         */
        void HandleTake(User sender, string e)
        {
            if(sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int roomId = sender.CurrRoomId;
            var target = WorldMap.Rooms[roomId].Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Name, e));
            
            if (target == null)
            {
                sender.Connection.SendMessage("No such object exists.");
                return;
            }
            
            if (!target.Actual.Commands.Contains("take"))
            {
                sender.Connection.SendMessage("You cannot take that.");
                return;
            }
            
            // Prevent the chance that two users try to take the same item at the same time.
            lock (WorldMap.Rooms[roomId].Things)
            {
                if (WorldMap.Rooms[roomId].Things.Remove(target))
                {
                    sender.Inventory.setCurrentRaisedCapacity(target.Actual.Weight);
                    if(sender.Inventory.CurrentCapacity <= sender.Inventory.CarryCapacity)
                    {
                        sender.Inventory.AddToInventory(target.Actual);
                        return;
                    }
                    sender.Connection.SendMessage("You're carrying too much to take that.");
                }
                else
                {
                    sender.Connection.SendMessage("That item is no longer there.");
                }
            }
        }
        #endregion
        #region Drop handlers
        /* Like Handletake, HandleDrop checks for an item's existence, but in this case it has to be in the 
         * requesting user's inventory.
         * if item exists, remove it from the user inventory's list of refs and add to the room's Things instead
         */
        void HandleDrop(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int roomId = sender.CurrRoomId;
            Thing target = sender.Inventory.Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));

            if (target == null)
            {
                HandleHoldDrop(sender, e);
                return;
            }
            
            if (!target.Commands.Contains("drop"))
            {
                sender.Connection.SendMessage("You cannot drop that.");
                return;
            }

             XMLReference<Thing> thing = new XMLReference<Thing> { Actual = target };
             sender.Player.Drop(target);
             sender.Inventory.setCurrentLoweredCapacity(target.Weight);
             sender.Inventory.RemoveFromInventory(target);
             WorldMap.Rooms[roomId].Things.Add(thing);
        }

        void HandleHoldDrop(User sender, string e)
        {
            int roomID = sender.CurrRoomId;
            var target = sender.Player.Held.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));

            if (target == null)
            {
                sender.Connection.SendMessage("You are not holding that.");
                return;
            }
            
            if (!target.Commands.Contains("drop"))
            {
                sender.Connection.SendMessage("You cannot drop that.");
                return;
            }
            sender.Player.Drop(target);
            sender.Inventory.setCurrentRaisedCapacity(target.Weight);
            CommandUtils.RemoveItemChangeStats(target, sender);
            sender.Inventory.AddToInventory(target);
        }
        #endregion
        #region Eat and Drink handlers
        void HandleEat(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            var target = sender.Inventory.Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            
            if(!target.IsConsumable)
            {
                sender.Connection.SendMessage("You cannot eat that.");
                return;
            }
            if(!CommandUtils.FuzzyEquals(target.ConsumableType, "food"))
            {
                sender.Connection.SendMessage("You cannot eat that.");
                return;
            }
            if(target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            sender.Inventory.RemoveFromInventory(target);
            CommandUtils.ChangeStats(target, sender);
            sender.Inventory.setCurrentLoweredCapacity(target.Weight);
            return;
        }
        void HandleDrink(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            var target = sender.Inventory.Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            if(!target.IsConsumable)
            {
                sender.Connection.SendMessage("You cannot drink that.");
                return;
            }
            if(!CommandUtils.FuzzyEquals(target.ConsumableType, "drink"))
            {
                sender.Connection.SendMessage("You cannot drink that.");
                return;
            }
            if(target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            sender.Inventory.RemoveFromInventory(target);
            CommandUtils.ChangeStats(target, sender);
            sender.Inventory.setCurrentLoweredCapacity(target.Weight);
            return;
        }
        #endregion
        #region Examine handlers
        /* HandleExamine allows a user to examine an object or themselves
         * 'examine self' returns a description of the user's player entity, including what they're wearing and appearance.
         * 'examine <item>' returns the description of the item. The item can be either in the room or in the user's inventory.
         * If the item is in neither, there is no description.       
         */
        void HandleExamine(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            if (e == null)
            {
                sender.Connection.SendMessage("Examine what?");
                return;
            }

            if (CommandUtils.FuzzyEquals(e, "self"))
            {
                sender.Connection.SendMessage("You look yourself over.");
                if (sender.Player.Equipped.Count > 0)
                {
                    sender.Connection.SendMessage($"Equipped: {string.Join(", ", sender.Player.Equipped)}");
                }
                if (sender.Player.Held.Count > 0)
                {
                    sender.Connection.SendMessage($"Held: {string.Join(", ", sender.Player.Held)}");
                }
                sender.Connection.SendMessage($"Health: {sender.Player.Stats.CurrHealth}; Defense: {sender.Player.Stats.Defense}");
                return;
            }

            int roomId = sender.CurrRoomId;
            Thing thing = WorldMap.Rooms[roomId].Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Name, e))?.Actual ?? sender.Inventory.Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            if(thing == null)
            {
                // If it's not a thing, try and see if the target is an entity
                HandleEntityExamine(sender, e);
                return;
            }
            if(!thing.Commands.Contains("examine"))
            {
                sender.Connection.SendMessage("You cannot examine that.");
                return;
            }

            sender.Connection.SendMessage($"You examine the {e}.");
            sender.Connection.SendMessage($"{thing.Description}");
        }

        void HandleEntityExamine(User sender, string e)
        {
            int roomId = sender.CurrRoomId;
            Entity entity = WorldMap.Rooms[roomId].Entities.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Name, e))?.Actual;
            if(entity == null)
            {
                // If it's not an entity or a thing, it's not examinable.
                sender.Connection.SendMessage("That does not exist here!");
                return;
            }
            if (!entity.Commands.Contains("examine"))
            {
                sender.Connection.SendMessage("You cannot examine that.");
                return;
            }
            sender.Connection.SendMessage($"You examine the {e}.");
            sender.Connection.SendMessage($"{entity.Description}");
        }
        #endregion
        #region Equip handler
        void HandleEquip(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            var target = sender.Inventory.Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            bool isWearing = false;

            if(!target.IsWearable)
            {
                sender.Connection.SendMessage("You cannot wear that.");
                return;
            }

            if (target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            if (target.IsWearable)
            {

                if (sender.Player.Equipped.Count == 6)
                {
                    sender.Connection.SendMessage("You have no other room to wear that.");
                    return;
                }

                foreach (Thing thing in sender.Player.Equipped)
                {
                    if (target.Slot == thing.Slot)
                    {
                        sender.Connection.SendMessage($"You are already wearing a {target.Slot} item.");
                        isWearing = true;
                        break;
                    }
                }

                if (!isWearing)
                {
                    sender.Inventory.RemoveFromInventory(target);
                    sender.Player.Equip(target);
                    CommandUtils.ChangeStats(target, sender);
                    sender.Inventory.setCurrentLoweredCapacity(target.Weight);
                    return;
                }
            }
        }
        #endregion
        #region Remove handler
        void HandleRemove(User sender, string e)
        {
            var target = sender.Player.Equipped.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));
            if (target == null)
            {
                sender.Connection.SendMessage("You are not wearing that.");
                return;
            }
            
            if(target.Commands.Contains("remove"))
            {
                sender.Player.Unequip(target);
                sender.Inventory.setCurrentRaisedCapacity(target.Weight);
                CommandUtils.RemoveItemChangeStats(target, sender);
                sender.Inventory.AddToInventory(target);
                return;
            }
           
            sender.Connection.SendMessage("You cannot remove that.");
        }
        #endregion
        #region Hold handler
        void HandleHold(User sender, string e)
       {
            var target = sender.Inventory.Things.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Name, e));

            if (target == null)
            {
                sender.Connection.SendMessage("You must have the object in your inventory.");
                return;
            }

            if (target.Commands.Contains("hold"))
            {
                if(sender.Player.Held.Count == 2)
                {
                    sender.Connection.SendMessage("You have no free hands.");
                    return;
                }

                sender.Inventory.RemoveFromInventory(target);
                sender.Inventory.setCurrentLoweredCapacity(target.Weight);
                CommandUtils.ChangeStats(target, sender);
                sender.Player.Hold(target);
                return;
            }
            
            sender.Connection.SendMessage("You cannot hold that.");
        }
        #endregion
        #region Go handler
        /* HandleGo handles moving between rooms.
         * TODO: Find a better way to handle the RoomsIConnect list - currently, in the XML, rooms must be 
         * placed in the order of CURRENT ROOM in index 0 and CONNECTING ROOM in index 1    
         * there's probably a better way to handle this situation.
         */
        void HandleGo(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            if (CommandUtils.FuzzyEquals(e, "north"))
            {
                e = "n";
            }
            if(CommandUtils.FuzzyEquals(e, "south"))
            {
                e = "s";
            }
            if(CommandUtils.FuzzyEquals(e, "east"))
            {
                e = "e";
            }
            if(CommandUtils.FuzzyEquals(e, "west"))
            {
                e = "w";
            }
            int currentRoomId = sender.CurrRoomId;
            int numDoors = WorldMap.Rooms[currentRoomId].Doors.Select(t => t.Actual).Count();
            var directionGone = WorldMap.Rooms[currentRoomId].Doors.FirstOrDefault(t => CommandUtils.FuzzyEquals(t.Actual.Direction, e))?.Actual;
            if (numDoors == 0)
            {
                sender.Connection.SendMessage("There are no doors here. You cannot go anywhere.");
                return;
            }
                
            if(directionGone == null)
            {
                sender.Connection.SendMessage("There is no door there.");
                return;
            }
                
            void proceed()
            {
                sender.CurrRoomId = directionGone.RoomsIConnect[1];
            }
            
            if(!directionGone.Locked)
            {
                proceed();
                return;
            }
              
            if(sender.Player.Stats.Dexterity >= directionGone.minDexterity)
            {
                sender.Connection.SendMessage("Although the door is locked, you are deft enough to pick your way in.");
                proceed();
                directionGone.Locked = false;
                return;
            }
                
            if(directionGone.HasKey)
            {
                if(CommandUtils.OwnsThing(sender, "key"))
                {
                    proceed();
                    return;
                }
                sender.Connection.SendMessage("This door is locked and has a key. Look around!");
            }
            else
            {
                sender.Connection.SendMessage("You are unable to open the door.");
            }
        }
        #endregion
        #region Whisper handler
        // Send a message to a specific player
        void HandleWhisper(User sender, string msg)
        {
            string[] sl = CommandUtils.splitLine(msg);
            User recipient = CommandUtils.GetRecipient(sl[0], Users);
            if (recipient == null)
            {
                sender.Connection.SendMessage("You cannot talk to that person.");
                return;
            }
            if (recipient.Name == "Someone")
            {
                sender.Connection.SendMessage("You cannot whisper to strangers.");
                return;
            }
            recipient.Connection.SendMessage($"{sender.Name} whispers, '{sl[1]}'");
        }
        #endregion
        #region Attack handler
        // Attack an enemy
        void HandleAttack(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int currRoom = sender.CurrRoomId;
            Entity target = CommandUtils.GetTarget(currRoom, e, WorldMap);
            // Make sure the target is actually attackable
            if(target == null)
            {
                sender.Connection.SendMessage("You cannot attack that.");
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "ally"))
            {
                sender.Connection.SendMessage("You cannot attack friendly people!");
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "dead"))
            {
                sender.Connection.SendMessage("You cannot attack dead bodies!");
                return;
            }
            // If these checks pass, then there is an available target to fight
            // Check and see if the target is already engaged : if so, join the target session
            RoomSay($"{sender.Name} is attacking the {target.Name}!", sender);
            if (target.Combat == null)
            {
                target.Combat = new Combat(target);
            }
            target.Combat.Combatants.Add(sender);
            sender.Player.Combat = target.Combat;

            // Pre-emptively get the direction to run in in case the user decides to try to run.
            var runDir = WorldMap.Rooms[sender.CurrRoomId].Doors.Where(d => !d.Actual.Locked).FirstOrDefault()?.Actual;

            // Combat loop: check turns            
            while(target.Combat != null && sender.Player.Combat != null)
            {
                sender.Player.IsBlocking = false;
                sender.Connection.SendMessage(@"a: Attack
d: Defend
h: Heal
r: Run");
                if (sender.Player.Stats.CurrHealth <= 0)
                {
                    break;
                }
                else
                {
                    string action = sender.Connection.ReadMessage();
                    if (CommandUtils.FuzzyEquals(action, "a"))
                    {
                        target.Combat.Attack(sender);
                    }
                    if (CommandUtils.FuzzyEquals(action, "d"))
                    {
                        target.Combat.Defend(sender);
                    }
                    if(CommandUtils.FuzzyEquals(action, "h"))
                    {
                        sender.Connection.SendMessage("Consume what?");
                        var consumables = sender.Inventory.Things.Where(t => t.IsConsumable);
                        foreach(Thing thing in consumables)
                        {
                            sender.Connection.SendMessage(thing.Name);
                        }
                        string choice = sender.Connection.ReadMessage();
                        HandleEat(sender, choice);
                    }
                    if (CommandUtils.FuzzyEquals(action, "r"))
                    {
                        target.Combat.Run(sender, runDir);
                        target.Combat = null;
                        break;
                    }
                }
                if(target.Health <= 0)
                {
                    break;
                }
                target.Combat.EnemyTurn();
            }

            if (sender.Player.Stats.CurrHealth <= 0)
            {
                sender.Player.Combat = null;
                target.Combat = null;
                sender.Connection.SendMessage("You have been defeated!");
                if(Users.Count > 2)
                {
                    sender.Connection.SendMessage("Would you like to wait for a revive? (y/n)");
                    string action = sender.Connection.ReadMessage();
                    if(CommandUtils.FuzzyEquals(action, "y"))
                    {
                        sender.Player.IsDead = true;
                        return;
                    }
                }
                sender.Connection.SendMessage("As you were the only soul in this place, you have been sent back to the start.");
                foreach(Thing things in sender.Inventory.Things.ToList())
                {
                    sender.Inventory.RemoveFromInventory(things);
                    XMLReference<Thing> thing = new XMLReference<Thing> { Actual = things };
                    WorldMap.Rooms[sender.CurrRoomId].Things.Add(thing);
                }
                sender.Player.Stats.CurrHealth = sender.Player.Stats.MaxHealth;
                sender.CurrRoomId = 0001;
                return;
            }
            // After this loop, the target is dead. Remove them from the entity list and add them to the dead list/faction.
            target.Faction = "dead";
            // Upon death, an entity's inventory gets dropped to the room.
            foreach(Thing things in target.Inventory.Things.ToList())
            {
                target.Inventory.RemoveFromInventory(things);
                XMLReference<Thing> thing = new XMLReference<Thing> { Actual = things };
                WorldMap.Rooms[sender.CurrRoomId].Things.Add(thing);
            }

            foreach (var i in WorldMap.Rooms[sender.CurrRoomId].Entities.ToList())
            {
                if(i.Actual.Name == target.Name)
                {
                    WorldMap.Rooms[sender.CurrRoomId].Entities.Remove(i);
                    WorldMap.Rooms[sender.CurrRoomId].DeadEntities.Add(i);
                }
            }

            sender.Connection.SendMessage("Send 'q' to exit combat.");
            sender.Player.Combat = null;
            target.Combat = null;
        }

        #endregion
        #region NPC Conversation handler
        // Talk to an NPC
        void HandleTalkTo(User sender, string e)
        {
            if (sender.Player.IsDead)
            {
                sender.Connection.SendMessage("You cannot do that while defeated!");
                return;
            }
            int currRoom = sender.CurrRoomId;
            Entity target = CommandUtils.GetTarget(currRoom, e, WorldMap);
            if(target == null)
            {
                sender.Connection.SendMessage("You babble to no one in particular for a moment.");
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "enemy"))
            {
                sender.Connection.SendMessage("While talking your way out of confrontation is admirable, it won't work in this situation.");
                HandleAttack(sender, e);
                return;
            }
            if(CommandUtils.FuzzyEquals(target.Faction, "dead"))
            {
                sender.Connection.SendMessage("He's dead, Jim.");
                return;
            }
            target.Conversation = new Conversation(target, sender);
            sender.Player.Conversation = target.Conversation;

            target.Conversation.Greeting();

            while(target.Conversation != null && sender.Player.Conversation != null)
            {
                sender.Connection.SendMessage(@"w: Who
n: News
t: Trade
q: Quest
b: Bye");
                string action = sender.Connection.ReadMessage();
                if(CommandUtils.FuzzyEquals(action, "w"))
                {
                    target.Conversation.Who();
                }
                if(CommandUtils.FuzzyEquals(action, "n"))
                {
                    target.Conversation.News();
                    sender.Connection.SendMessage("News placeholder");
                }
                if(CommandUtils.FuzzyEquals(action, "t"))
                {
                    if(CommandUtils.FuzzyEquals(target.Faction, "ally") || CommandUtils.FuzzyEquals(target.Faction, "neutral"))
                    {
                        target.Conversation.Trade();
                    }
                    else
                    {
                        sender.Connection.SendMessage(target.Conversation.TradeFlavor);
                    }
                }
                if(CommandUtils.FuzzyEquals(action, "q"))
                {
                    if (CommandUtils.FuzzyEquals(target.Faction, "ally") || (CommandUtils.FuzzyEquals(target.Faction, "neutral")))
                    {
                        if(target.HasQuest)
                        {
                            target.Conversation.Quest();
                        }
                        else
                        {
                            sender.Connection.SendMessage($"{target.Name} does not have a quest for you.");
                        }
                    }
                    else 
                    {
                        sender.Connection.SendMessage(target.Conversation.QuestFlavor);
                    }
                }
                if(CommandUtils.FuzzyEquals(action, "b"))
                {
                    target.Conversation.Bye();
                    break;
                }
            }
            target.Conversation = null;
            sender.Player.Conversation = null;
        }
        #endregion
        #endregion
        #endregion
    }
}
