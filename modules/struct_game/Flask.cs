namespace Triggered.modules.struct_game
{
    /// <summary>
    /// Represents a flask object.
    /// </summary>
    public class Flask : Triggerable
    {
        /// <summary>
        /// Assign a slot to the Flask to produce it.
        /// </summary>
        /// <param name="slot"></param>
        public Flask(int slot)
        {
            Slot = slot;
        }
    }
}
