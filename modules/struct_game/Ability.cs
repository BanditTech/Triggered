using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Triggered.modules.struct_game
{
    /// <summary>
    /// Define the shape of an ability object
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
