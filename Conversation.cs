using System;


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
            if(World.FuzzyEquals(Subject.Faction, "ally"))
            {
                GreetingFlavor = $"Hello, {Sender.Name}!";
                WhoFlavor = $"I'm {Subject.Name}";
                NewsFlavor = $"I got some news just recently!";
                TradeFlavor = $"With you? Always.";
                QuestFlavor = $"There was something I could use your help on.";
                ByeFlavor = $"See you around, {Sender.Name}!";
                return;
            }
            if(World.FuzzyEquals(Subject.Faction, "neutral"))
            {
                GreetingFlavor = $"Who are you?";
                WhoFlavor = $"I am {Subject.Name}.";
                NewsFlavor = $"Let me think...";
                TradeFlavor = $"We might come to a deal.";
                QuestFlavor = $"Let me think...";
                ByeFlavor = $"Goodbye, stranger.";
                return;
            }
            if(World.FuzzyEquals(Subject.Faction, "hostile"))
            {
                GreetingFlavor = $"Who are you?";
                WhoFlavor = $"I don't trust you enough to tell you who I am.";
                NewsFlavor = $"Go find out on your own.";
                TradeFlavor = $"With the likes of you?";
                QuestFlavor = $"You don't have any skills of use to me.";
                ByeFlavor = $"Good riddance.";
                return;
            }
            if(World.FuzzyEquals(Subject.Faction, "wildlife"))
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
            Sender.Connection.SendMessage(TradeFlavor);
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
