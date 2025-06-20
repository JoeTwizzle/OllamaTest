//namespace Backend;


//class Npc { }
//sealed class Quest
//{
//    Npc QuestGiver;
//    string Description;
//    TaskBase TaskBase;

//    public Quest(Npc questGiver, string description, TaskBase taskBase)
//    {
//        QuestGiver = questGiver;
//        Description = description;
//        TaskBase = taskBase;
//    }
//}

//// A -> Will stein von npc B
//// NPC gibt stein wenn du den seife gibst
//enum ProgressionState
//{
//    // This quest is not available yet
//    // It is either a subquest of a quest which has not yet been accepted
//    // or it requires certain world events to happen first
//    Locked,
//    // If the player can accept this quest. 
//    // (Some quests skip this and are directly set to InProgress)
//    Avaliable,
//    // If the player has accepted this quest, 
//    // may mark direct subquest as available (see ContaningTask derrived classes)
//    InProgress,
//    // The quest was completed
//    Completed,
//    // The quest was failed
//    // (May not need this one)
//    Failed
//}

//abstract class TaskBase
//{
//    public readonly string Id;
//    public readonly ContaningTask? ParentTask;
//    // Not all ContaningTasks need to provide a Description, all other types do however
//    public readonly string? Description;
//    private ProgressionState progressionState;
//    public ProgressionState ProgressionState
//    {
//        get { return progressionState; }
//        protected set
//        {
//            var oldState = progressionState;
//            progressionState = value;
//            if (oldState != progressionState)
//            {
//                OnProgressionStateChanged();
//            }
//        }
//    }

//    private void OnProgressionStateChanged()
//    {
//        ParentTask?.OnChildTaskProgressionStateChanged(this);
//    }
//}

//abstract class ContaningTask : TaskBase
//{
//    public TaskBase[] ChildTasks { get; protected set; }
//    public abstract void OnChildTaskProgressionStateChanged(TaskBase childTask);
//}

//// Player needs to complete all Tasks in sequence, to set ProgressionState to completed
//// The first subtask has its ProgressionState set to InProgress,
//// upon completion its state is set to Completed
//// and the next subtask has its ProgressionState set to InProgress
//sealed class SequeceTask : ContaningTask
//{
//    public SequeceTask(TaskBase[] tasks)
//    {
//        ChildTasks = tasks;
//    }

//    public override void OnChildTaskProgressionStateChanged(TaskBase childTask) { }
//}

//// Player needs to complete only one Task in order to set ProgressionState to completed
//sealed class ParallelTask : ContaningTask
//{
//    public override void OnChildTaskProgressionStateChanged(TaskBase childTask) { }
//}

//// Player needs to complete only one Task in order to set ProgressionState to completed
//sealed class OptionTask : ContaningTask
//{
//    public override void OnChildTaskProgressionStateChanged(TaskBase childTask) { }
//}

//class GetItemFromTask : TaskBase
//{
//    string Location;
//    string Item;

//    public GetItemFromTask(string location, string item)
//    {
//        Location = location;
//        Item = item;
//    }
//}

//// All other tasks derrive from TaskBase
//class DeliveryTask : TaskBase
//{
//    string Item;
//    string Npc;

//    public DeliveryTask(string item, string rock)
//    {
//        Item = item;
//        Npc = rock;
//    }

//    // Deliver Item to Barnabas
//}

//class TradeItemTask:TaskBase
//{
//    string Item;
//    string ItemWanted;

//    public TradeItemTask(string item, string itemWanted)
//    {
//        Item = item;
//        ItemWanted = itemWanted;
//    }
//}

//class MyClass
//{
//    public void Test()
//    {
//        List<string> items = new List<string>();

//        new Quest(new(), "Deliver rock to barnabas", new SequeceTask([new GetItemFromTask("Forest", "Rock"), new DeliveryTask("Rock", "Barnabas")]));

//        Npc Barnabs;
//        Npc bob;
//        new Quest(new(), "Deliver rock to barnabas", new SequeceTask([
//            /* new Quest(new(), "Trade soap for rock with bob to give tock to barnabas", new SequeceTask([new TradeItemTask("Rock", "Soap"), new DeliveryTask("Rock", "Barnabas")]));*/ , 
//            new DeliveryTask("Rock", "Barnabas")]));
        
//    }
//}