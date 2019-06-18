using System;
using CSMud.Client;
using CSMud.Events;

namespace CSMud.Utils
{
    // Class for handling commands user sends
    public class HandleCommand
    {
        public User SendingUser { get; }

        public HandleCommand(User user)
        {
            SendingUser = user;
        }

        // These are unparameterized events - they are just called and always do the same thing.
        public event EventHandler RaiseHelpEvent;
        public event EventHandler RaiseLookEvent;
        public event EventHandler RaiseWhoEvent;
        public event EventHandler RaiseInventoryQueryEvent;
        public event EventHandler RaiseNoEvent;
        public event EventHandler RaiseYesEvent;

        // This is a parameterized event that affects a Thing, Player, or Entity
        public event EventHandler<ParameterizedEvent> RaiseParameterizedEvent;

        public void ProcessCommand(string[] commands)
        { 
            if(commands.Length == 1)
            {
                switch (commands[0])
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
                OnParameterizedEvent(new ParameterizedEvent { Command = commands[0], Action = commands[1] });
            }
        }

        protected virtual void OnRaiseHelpEvent()
        {
            EventHandler handler = RaiseHelpEvent;
            handler?.Invoke(SendingUser, EventArgs.Empty);
        }

        protected virtual void OnRaiseLookEvent()
        {
            EventHandler handler = RaiseLookEvent;
            handler?.Invoke(SendingUser, EventArgs.Empty);
        }

        protected virtual void OnRaiseWhoEvent()
        {
            EventHandler handler = RaiseWhoEvent;
            handler?.Invoke(SendingUser, EventArgs.Empty);
        }

        protected virtual void OnRaiseInventoryQueryEvent()
        {
            EventHandler handler = RaiseInventoryQueryEvent;
            handler?.Invoke(SendingUser, EventArgs.Empty);
        }

        protected virtual void OnRaiseNoEvent()
        {
            EventHandler handler = RaiseNoEvent;
            handler?.Invoke(SendingUser, EventArgs.Empty);
        }

        protected virtual void OnRaiseYesEvent()
        {
            EventHandler handler = RaiseYesEvent;
            handler?.Invoke(SendingUser, EventArgs.Empty);
        }

        protected virtual void OnParameterizedEvent(ParameterizedEvent e)
        {
            EventHandler<ParameterizedEvent> handler = RaiseParameterizedEvent;
            handler?.Invoke(SendingUser, e);
        }    

    }
}
