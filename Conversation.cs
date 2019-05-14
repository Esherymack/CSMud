using System.Collections.Generic;
using System.Linq;


/* Conversation class for interacting with friendly NPCs.
 * Conversation is initiated through a 'talk' command
 */
namespace CSMud
{
    public class Conversation
    {
        public Entity Subject { get; set; }
        public User Sender { get; set; }

        public string GreetingFlavor { get; set; }
        public string WhoFlavor { get; set; }
        public string NewsFlavor { get; set; }
        public string TradeFlavor { get; set; }
        public string QuestFlavor { get; set; }
        public string ByeFlavor { get; set; }

        public Conversation(Entity subject, User sender)
        {
            Subject = subject;
            Sender = sender;
            GetFlavorText();
        }

        public void GetFlavorText()
        {
            if(CommandUtils.FuzzyEquals(Subject.Faction, "ally"))
            {
                GreetingFlavor = $"Hello, {Sender.Name}!";
                WhoFlavor = $"I'm {Subject.Name}";
                NewsFlavor = $"I got some news just recently!";
                TradeFlavor = $"With you? Always.";
                QuestFlavor = $"There was something I could use your help on.";
                ByeFlavor = $"See you around, {Sender.Name}!";
                return;
            }
            if(CommandUtils.FuzzyEquals(Subject.Faction, "neutral"))
            {
                GreetingFlavor = $"Who are you?";
                WhoFlavor = $"I am {Subject.Name}.";
                NewsFlavor = $"Let me think...";
                TradeFlavor = $"We might come to a deal.";
                QuestFlavor = $"Let me think...";
                ByeFlavor = $"Goodbye, stranger.";
                return;
            }
            if(CommandUtils.FuzzyEquals(Subject.Faction, "hostile"))
            {
                GreetingFlavor = $"Who are you?";
                WhoFlavor = $"I don't trust you enough to tell you who I am.";
                NewsFlavor = $"Go find out on your own.";
                TradeFlavor = $"With the likes of you?";
                QuestFlavor = $"You don't have any skills of use to me.";
                ByeFlavor = $"Good riddance.";
                return;
            }
            if(CommandUtils.FuzzyEquals(Subject.Faction, "wildlife"))
            {
                GreetingFlavor = $"The {Subject.Name} gives you a suspicious look.";
                WhoFlavor = $"The {Subject.Name} hisses at you.";
                NewsFlavor = $"The {Subject.Name} clearly mistrusts you.";
                TradeFlavor = $"The {Subject.Name} is very territorial of its belongings.";
                QuestFlavor = $"The {Subject.Name} has no interests that you could resolve.";
                ByeFlavor = $"The {Subject.Name} seems relieved you are leaving.";
                return;
            }
                
        }

        public void Greeting()
        {
            Sender.Connection.SendMessage(GreetingFlavor);
        }
        public void Who()
        {
            Sender.Connection.SendMessage(WhoFlavor);
            Sender.Connection.SendMessage(Subject.Description);
        }
        public void News()
        {
            Sender.Connection.SendMessage(NewsFlavor);
        }
        public void Trade()
        {
            Sender.Player.IsTrading = true;
            List<Thing> TradeItemsTo = new List<Thing>();
            List<Thing> TradeItemsFrom = new List<Thing>();
            Sender.Connection.SendMessage(TradeFlavor);
            while(Sender.Player.IsTrading)
            {
                Sender.Connection.SendMessage(@"What would you like to do?
ao: Add item to trade pool from your inventory
at: Add item to trade pool from entity's inventory
ro: Remove item from your trade pool
rt: Remove item from entity's trade pool
v: View current trade pools
s: Submit trade
q: Quit trading");
                string action = Sender.Connection.ReadMessage();
                if (CommandUtils.FuzzyEquals(action, "ao"))
                {
                    Sender.Connection.SendMessage("You have: ");
                    foreach (Thing thing in Sender.Inventory.Things)
                    {
                        Sender.Connection.SendMessage($"{thing.Name} - Value: {thing.Value}");
                    }
                    Sender.Connection.SendMessage("What would you like to trade?");
                    string tradeItem = Sender.Connection.ReadMessage();
                    var trade = Sender.Inventory.Things.Find(t => CommandUtils.FuzzyEquals(t.Name, tradeItem));
                    if(trade == null)
                    {
                        Sender.Connection.SendMessage($"You do not have a {trade.Name} to offer.");
                    }
                    else
                    {
                        TradeItemsTo.Add(trade);
                        Sender.Inventory.RemoveFromInventory(trade);
                    }
                }
                if (CommandUtils.FuzzyEquals(action, "at"))
                {
                    Sender.Connection.SendMessage($"{Subject.Name} has: ");
                    foreach (Thing thing in Subject.Inventory.Things)
                    {
                        Sender.Connection.SendMessage($"{thing.Name} - Value: {thing.Value}");
                    }
                    Sender.Connection.SendMessage("What would you like to request?");
                    string tradeItem = Sender.Connection.ReadMessage();
                    var trade = Subject.Inventory.Things.Find(t => CommandUtils.FuzzyEquals(t.Name, tradeItem));
                    if (trade == null)
                    {
                        Sender.Connection.SendMessage($"{Subject.Name} does not have a {trade.Name}.");
                    }
                    else
                    {
                        TradeItemsFrom.Add(trade);
                        Subject.Inventory.RemoveFromInventory(trade);
                    }
                }
                if (CommandUtils.FuzzyEquals(action, "ro"))
                {
                    Sender.Connection.SendMessage("You offered: ");
                    foreach (Thing thing in TradeItemsTo)
                    {
                        Sender.Connection.SendMessage($"{thing.Name} - Value: {thing.Value}");
                    }
                    Sender.Connection.SendMessage("What would you like to remove from the offers?");
                    string removeItem = Sender.Connection.ReadMessage();
                    var remove = TradeItemsTo.Find(t => CommandUtils.FuzzyEquals(t.Name, removeItem));
                    if(remove == null)
                    {
                        Sender.Connection.SendMessage($"You never offered a {remove.Name} to {Subject.Name}.");
                    }
                    else
                    {
                        TradeItemsTo.Remove(remove);
                        Sender.Inventory.AddToInventory(remove);
                    }
                }
                if (CommandUtils.FuzzyEquals(action, "rt"))
                {
                    Sender.Connection.SendMessage("You requested: ");
                    foreach (Thing thing in TradeItemsFrom)
                    {
                        Sender.Connection.SendMessage($"{thing.Name} - Value: {thing.Value}");
                    }
                    Sender.Connection.SendMessage($"What would you like to return to {Subject.Name}?");
                    string removeItem = Sender.Connection.ReadMessage();
                    var remove = TradeItemsFrom.Find(t => CommandUtils.FuzzyEquals(t.Name, removeItem));
                    if(remove == null)
                    {
                        Sender.Connection.SendMessage($"You never requested a {remove.Name} from {Subject.Name}");
                    }
                    else
                    {
                        TradeItemsFrom.Remove(remove);
                        Subject.Inventory.AddToInventory(remove);
                    }
                }
                if (CommandUtils.FuzzyEquals(action, "v"))
                {
                    Sender.Connection.SendMessage("Current trade pools:");
                    foreach(Thing thing in TradeItemsTo)
                    {
                        Sender.Connection.SendMessage($"Giving {thing.Name} to {Subject.Name} (Value: {thing.Value})");
                    }
                    foreach(Thing thing in TradeItemsFrom)
                    {
                        Sender.Connection.SendMessage($"Getting {thing.Name} from {Subject.Name} (Value: {thing.Value})");
                    }
                    int TradeToValue = TradeItemsTo.Sum(t => t.Value);
                    int TradeFromValue = TradeItemsFrom.Sum(t => t.Value);
                    Sender.Connection.SendMessage($"Total value trading to {Subject.Name} is {TradeToValue}");
                    Sender.Connection.SendMessage($"Total value taking from {Subject.Name} is {TradeFromValue}");
                }
                if (CommandUtils.FuzzyEquals(action, "s"))
                {
                    if(TradeItemsTo.Sum(t => t.Value) >= TradeItemsFrom.Sum(t => t.Value))
                    {
                        if(TradeItemsFrom.Sum(t => t.Weight) <= Sender.Inventory.CurrentCapacity)
                        {
                            foreach (Thing thing in TradeItemsFrom)
                            {
                                Sender.Inventory.setCurrentRaisedCapacity(thing.Weight);
                                Sender.Inventory.AddToInventory(thing);
                            }
                            foreach (Thing thing in TradeItemsTo)
                            {
                                Sender.Inventory.setCurrentLoweredCapacity(thing.Weight);
                                Subject.Inventory.AddToInventory(thing);
                            }
                            Sender.Connection.SendMessage("Trade succesful.");
                            TradeItemsFrom.Clear();
                            TradeItemsTo.Clear();
                            break;
                        }
                        Sender.Connection.SendMessage("Trade failed - you're trying to take more than you can carry!");
                    }
                    else
                    {
                        Sender.Connection.SendMessage("Trade failed - you're trying to take more than you're giving!");
                    }
                }
                if (CommandUtils.FuzzyEquals(action, "q"))
                {
                    foreach(Thing thing in TradeItemsTo)
                    {
                        Sender.Inventory.AddToInventory(thing);
                    }
                    
                    foreach(Thing thing in TradeItemsFrom)
                    {
                        Subject.Inventory.AddToInventory(thing);
                    }
                    Sender.Connection.SendMessage("Trade cancelled.");
                    TradeItemsFrom.Clear();
                    TradeItemsTo.Clear();
                    break;
                }
            }
        }
        public void Quest()
        {
            Sender.Connection.SendMessage(QuestFlavor);
        }
        public void Bye()
        {
            Sender.Connection.SendMessage(ByeFlavor);
        }
    }
}
