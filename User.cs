using System;
using System.Collections.Generic;

namespace CSMud
{
    #region Classes for parameterized events
    public class AnswerData : EventArgs
    {
        public string Command { get; set; }
    }

    public class AskForData : EventArgs
    {
        public string Command { get; set; }
    }

    public class AskAboutData : EventArgs
    {
        public string Command { get; set; }
    }

    public class AttackData : EventArgs
    {
        public string Command { get; set; }
    }

    public class BlowData : EventArgs
    {
        public string Command { get; set; }
    }

    public class BurnData : EventArgs
    {
        public string Command { get; set; }
    }

    public class BuyData : EventArgs
    {
        public string Command { get; set; }
    }

    public class ClimbData : EventArgs
    {
        public string Command { get; set; }
    }

    public class CloseData : EventArgs
    {
        public string Command { get; set; }
    }

    public class CutData : EventArgs
    {
        public string Command { get; set; }
    }

    public class DigData : EventArgs
    {
        public string Command { get; set; }
    }

    public class DrinkData : EventArgs
    {
        public string Command { get; set; }
    }

    public class DropData : EventArgs
    {
        public string Command { get; set; }
    }

    public class EatData : EventArgs
    {
        public string Command { get; set; }
    }

    public class EnterData : EventArgs
    {
        public string Command { get; set; }
    }

    public class ExamineData : EventArgs
    {
        public string Command { get; set; }
    }

    public class FillData : EventArgs
    {
        public string Command { get; set; }
    }

    public class GetOffData : EventArgs
    {
        public string Command { get; set; }
    }

    public class GiveToData : EventArgs
    {
        public string Command { get; set; }
    }

    public class JumpOverData : EventArgs
    {
        public string Command { get; set; }
    }

    public class KissData : EventArgs
    {
        public string Command { get; set; }
    }

    public class ListenToData : EventArgs
    {
        public string Command { get; set; }
    }

    public class LockWithData : EventArgs
    {
        public string Command { get; set; }
    }

    public class LookInData : EventArgs
    {
        public string Command { get; set; }
    }

    public class LookUnderData : EventArgs
    {
        public string Command { get; set; }
    }

    public class LookUpData : EventArgs
    {
        public string Command { get; set; }
    }

    public class OpenData : EventArgs
    {
        public string Command { get; set; }
    }

    public class PullData : EventArgs
    {
        public string Command { get; set; }
    }

    public class PushData : EventArgs
    {
        public string Command { get; set; }
    }

    public class PutInData : EventArgs
    {
        public string Command { get; set; }
    }

    public class PutOnData : EventArgs
    {
        public string Command { get; set; }
    }
    
    public class RubData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SayData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SearchData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SetToData : EventArgs
    {
        public string Command { get; set; }
    }

    public class ShowToData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SitOnData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SmellData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SqueezeData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SwingData : EventArgs
    {
        public string Command { get; set; }
    }

    public class SwitchOnOffData : EventArgs
    {
        public string Command { get; set; }
    }

    public class TalkToData : EventArgs
    {
        public string Command { get; set; }
    }

    public class TakeData : EventArgs
    {
        public string Command { get; set; }
    }

    public class TakeOffData : EventArgs
    {
        public string Command { get; set; }
    }

    public class TasteData : EventArgs
    {
        public string Command { get; set; }
    }

    public class TellAboutData : EventArgs
    {
        public string Command { get; set; }
    }

    public class TouchData : EventArgs
    {
        public string Command { get; set; }
    }

    public class TurnData : EventArgs
    {
        public string Command { get; set; }
    }

    public class UnlockWithData : EventArgs
    {
        public string Command { get; set; }
    }

    public class WakeData : EventArgs
    {
        public string Command { get; set; }
    }

    public class WaveThingData : EventArgs
    {
        public string Command { get; set; }
    }

    public class WearData : EventArgs
    {
        public string Command { get; set; }
    }

    #endregion

    public class User : IDisposable
    {
        public Connection Connection { get; }

        #region Events for commands.  
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

        public event EventHandler<AnswerData> RaiseAnswerEvent;
        public event EventHandler<AskForData> RaiseAskForEvent;
        public event EventHandler<AskAboutData> RaiseAskAboutEvent;
        public event EventHandler<AttackData> RaiseAttackEvent;
        public event EventHandler<BlowData> RaiseBlowOnEvent;
        public event EventHandler<BurnData> RaiseBurnEvent;
        public event EventHandler<BuyData> RaiseBuyEvent;
        public event EventHandler<ClimbData> RaiseClimbEvent;
        public event EventHandler<CloseData> RaiseCloseEvent;
        public event EventHandler<CutData> RaiseCutEvent;
        public event EventHandler<DigData> RaiseDigEvent;
        public event EventHandler<DrinkData> RaiseDrinkEvent;
        public event EventHandler<DropData> RaiseDropEvent;
        public event EventHandler<EatData> RaiseEatEvent;
        public event EventHandler<EnterData> RaiseEnterEvent;
        public event EventHandler<ExamineData> RaiseExamineEvent;
        public event EventHandler<FillData> RaiseFillEvent;
        public event EventHandler<GetOffData> RaiseGetOffEvent;
        public event EventHandler<GiveToData> RaiseGiveToEvent;
        public event EventHandler<JumpOverData> RaiseJumpOverEvent;
        public event EventHandler<KissData> RaiseKissEvent;
        public event EventHandler<ListenToData> RaiseListenToEvent;
        public event EventHandler<LockWithData> RaiseLockWithEvent;
        public event EventHandler<LookInData> RaiseLookInEvent;
        public event EventHandler<LookUnderData> RaiseLookUnderEvent;
        public event EventHandler<LookUpData> RaiseLookUpEvent;
        public event EventHandler<OpenData> RaiseOpenEvent;
        public event EventHandler<PullData> RaisePullEvent;
        public event EventHandler<PushData> RaisePushEvent;
        public event EventHandler<PutInData> RaisePutInEvent;
        public event EventHandler<PutOnData> RaisePutOnEvent;
        public event EventHandler<RubData> RaiseRubEvent;
        public event EventHandler<SayData> RaiseSayEvent;
        public event EventHandler<SearchData> RaiseSearchEvent;
        public event EventHandler<SetToData> RaiseSetToEvent;
        public event EventHandler<ShowToData> RaiseShowToEvent;
        public event EventHandler<SitOnData> RaiseSitOnEvent;
        public event EventHandler<SmellData> RaiseSmellEvent;
        public event EventHandler<SqueezeData> RaiseSqueezeEvent;
        public event EventHandler<SwingData> RaiseSwingEvent;
        public event EventHandler<SwitchOnOffData> RaiseSwitchOnOffEvent;
        public event EventHandler<TalkToData> RaiseTalkToEvent;
        public event EventHandler<TakeData> RaiseTakeEvent;
        public event EventHandler<TakeOffData> RaiseTakeOffEvent;
        public event EventHandler<TasteData> RaiseTasteEvent;
        public event EventHandler<TellAboutData> RaiseTellAboutEvent;
        public event EventHandler<TouchData> RaiseTouchEvent;
        public event EventHandler<TurnData> RaiseTurnEvent;
        public event EventHandler<UnlockWithData> RaiseUnlockWithEvent;
        public event EventHandler<WakeData> RaiseWakeEvent;
        public event EventHandler<WaveThingData> RaiseWaveThingEvent;
        public event EventHandler<WearData> RaiseWearEvent;

        #endregion

        // a user belongs to a world
        private World World
        { get; }

        // a user has a unique inventory
        private Inventory Inventory
        { get; set; }

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

                if (line == null || line == "quit")
                {
                    break;
                }
                else if (line.StartsWith("examine"))
                {
                    int itemExamined = line.IndexOf("examine ");
                    string examinedItem = line.Substring(itemExamined + 8);
                    OnExamineEvent(new ExamineData { Command = examinedItem });
                }
                else
                {
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
        }



        #region Protected virtual funcs for event raises.
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

        protected virtual void OnRaiseAnswerEvent(AnswerData e)
        {
            EventHandler<AnswerData> handler = RaiseAnswerEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnRaiseAskForEvent(AskForData e)
        {
            EventHandler<AskForData> handler = RaiseAskForEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnAskAboutEvent(AskAboutData e)
        {
            EventHandler<AskAboutData> handler = RaiseAskAboutEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnAttackEvent(AttackData e)
        {
            EventHandler<AttackData> handler = RaiseAttackEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnBlowOnEvent(BlowData e)
        {
            EventHandler<BlowData> handler = RaiseBlowOnEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnBurnEvent(BurnData e)
        {
            EventHandler<BurnData> handler = RaiseBurnEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnBuyEvent(BuyData e)
        {
            EventHandler<BuyData> handler = RaiseBuyEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnClimbEvent(ClimbData e)
        {
            EventHandler<ClimbData> handler = RaiseClimbEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnCloseEvent(CloseData e)
        {
            EventHandler<CloseData> handler = RaiseCloseEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnCutEvent(CutData e)
        {
            EventHandler<CutData> handler = RaiseCutEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnDigEvent(DigData e)
        {
            EventHandler<DigData> handler = RaiseDigEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnDrinkEvent(DrinkData e)
        {
            EventHandler<DrinkData> handler = RaiseDrinkEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnDropEvent(DropData e)
        {
            EventHandler<DropData> handler = RaiseDropEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnEatEvent(EatData e)
        {
            EventHandler<EatData> handler = RaiseEatEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnEnterEvent(EnterData e)
        {
            EventHandler<EnterData> handler = RaiseEnterEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnExamineEvent(ExamineData e)
        {
            EventHandler<ExamineData> handler = RaiseExamineEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnFillEvent(FillData e)
        {
            EventHandler<FillData> handler = RaiseFillEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnGetOffEvent(GetOffData e)
        {
            EventHandler<GetOffData> handler = RaiseGetOffEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnGiveToEvent(GiveToData e)
        {
            EventHandler<GiveToData> handler = RaiseGiveToEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnJumpOverEvent(JumpOverData e)
        {
            EventHandler<JumpOverData> handler = RaiseJumpOverEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnKissEvent(KissData e)
        {
            EventHandler<KissData> handler = RaiseKissEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnListenToEvent(ListenToData e)
        {
            EventHandler<ListenToData> handler = RaiseListenToEvent;
            handler?.Invoke(this, e); 
        }

        protected virtual void OnLockWithEvent(LockWithData e)
        {
            EventHandler<LockWithData> handler = RaiseLockWithEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnLookInEvent(LookInData e)
        {
            EventHandler<LookInData> handler = RaiseLookInEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnLookUnderEvent(LookUnderData e)
        {
            EventHandler<LookUnderData> handler = RaiseLookUnderEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnLookUpEvent(LookUpData e)
        {
            EventHandler<LookUpData> handler = RaiseLookUpEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnOpenEvent(OpenData e)
        {
            EventHandler<OpenData> handler = RaiseOpenEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnPullEvent(PullData e)
        {
            EventHandler<PullData> handler = RaisePullEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnPushEvent(PushData e)
        {
            EventHandler<PushData> handler = RaisePushEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnPutInEvent(PutInData e)
        {
            EventHandler<PutInData> handler = RaisePutInEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnPutOnEvent(PutOnData e)
        {
            EventHandler<PutOnData> handler = RaisePutOnEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnRubEvent(RubData e)
        {
            EventHandler<RubData> handler = RaiseRubEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSearchEvent(SearchData e)
        {
            EventHandler<SearchData> handler = RaiseSearchEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSitOnEvent(SitOnData e)
        {
            EventHandler<SitOnData> handler = RaiseSitOnEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSmellEvent(SmellData e)
        {
            EventHandler<SmellData> handler = RaiseSmellEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSqueezeEvent(SqueezeData e)
        {
            EventHandler<SqueezeData> handler = RaiseSqueezeEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSwingEvent(SwingData e)
        {
            EventHandler<SwingData> handler = RaiseSwingEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSwitchOnOffEvent(SwitchOnOffData e)
        {
            EventHandler<SwitchOnOffData> handler = RaiseSwitchOnOffEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTalkToEvent(TalkToData e)
        {
            EventHandler<TalkToData> handler = RaiseTalkToEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTakeEvent(TakeData e)
        {
            EventHandler<TakeData> handler = RaiseTakeEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTakeOffEvent(TakeOffData e)
        {
            EventHandler<TakeOffData> handler = RaiseTakeOffEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTasteEvent(TasteData e)
        {
            EventHandler<TasteData> handler = RaiseTasteEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTellAboutEvent(TellAboutData e)
        {
            EventHandler<TellAboutData> handler = RaiseTellAboutEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTouchEvent(TouchData e)
        {
            EventHandler<TouchData> handler = RaiseTouchEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnTurnEvent(TurnData e)
        {
            EventHandler<TurnData> handler = RaiseTurnEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnUnlockWithEvent(UnlockWithData e)
        {
            EventHandler<UnlockWithData> handler = RaiseUnlockWithEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnWakeEvent(WakeData e)
        {
            EventHandler<WakeData> handler = RaiseWakeEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnWaveThingEvent(WaveThingData e)
        {
            EventHandler<WaveThingData> handler = RaiseWaveThingEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnWearEvent(WearData e)
        {
            EventHandler<WearData> handler = RaiseWearEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSayEvent(SayData e)
        {
            EventHandler<SayData> handler = RaiseSayEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnSetToEvent(SetToData e)
        {
            EventHandler<SetToData> handler = RaiseSetToEvent;
            handler?.Invoke(this, e);
        }

        protected virtual void OnShowToEvent(ShowToData e)
        {
            EventHandler<ShowToData> handler = RaiseShowToEvent;
            handler?.Invoke(this, e);
        }

        #endregion End events


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
