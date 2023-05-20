namespace Triggered.modules.struct_game
{
    /// <summary>
    /// Represent an ability object.
    /// </summary>
    public class Ability : Triggerable
    {
        /// <summary>
        /// Assign a slot to the Ability to produce it.
        /// </summary>
        /// <param name="slot"></param>
        public Ability(int slot)
        {
            Slot = slot;
        }
    }
}
