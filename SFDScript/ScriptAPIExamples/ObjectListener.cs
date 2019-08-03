using SFDGameScriptInterface;

namespace SFDScript.ScriptAPIExamples
{
    // https://www.mythologicinteractiveforums.com/viewtopic.php?f=22&t=3774
    /// <summary>
    /// The following code demonstrates how to listen on objects being created, damaged and terminated in v.1.3.0.
    /// </summary>
    public class ObjectListener : GameScriptInterface
    {
        /// <summary>
        /// Placeholder constructor that's not to be included in the ScriptWindow!
        /// </summary>
        public ObjectListener() : base(null) { }

        // Example script to listen on objects created, damaged and terminated.
        public void OnStartup()
        {
            Events.ObjectCreatedCallback.Start(OnObjectCreated);
            Events.ObjectDamageCallback.Start(OnObjectDamage);
            Events.ObjectTerminatedCallback.Start(OnObjectTerminated);

        }

        public void OnObjectCreated(IObject[] objs)
        {
            foreach (IObject obj in objs)
            {
                Game.WriteToConsole(string.Format("Object {0} ({1}) created", obj.UniqueID, obj.Name));
            }
        }

        public void OnObjectDamage(IObject obj, ObjectDamageArgs args)
        {
            // object took damage
            if (args.DamageType != ObjectDamageType.Fire)
            {
                if (args.SourceID != 0)
                {
                    Game.WriteToConsole(string.Format("Object {0} took {1} {2} damage from {3} {4}", obj.UniqueID, args.Damage, args.DamageType, (args.IsPlayer ? "player" : "object"), args.SourceID));
                }
                else
                {
                    Game.WriteToConsole(string.Format("Object {0} took {1} {2} damage", obj.UniqueID, args.Damage, args.DamageType));
                }
            }
        }

        public void OnObjectTerminated(IObject[] objs)
        {
            // objects terminated. Note: This is run just before the object is about to be destroyed or removed. To see if it was destroyed, check the IObject.DestructionInitiated property.
            foreach (IObject obj in objs)
            {
                Game.WriteToConsole(string.Format("Object {0} was {1}", obj.UniqueID, (obj.DestructionInitiated ? "destroyed" : "removed")));
            }
        }
    }
}
