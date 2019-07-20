using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFDScript.BotExtended
{
    public enum ZombieStatus
    {
        // Not infected by zombie. Do not turn into zombie when dying
        Human,

        // Infected by zombie or other infected. Start turning into zombie when dying
        Infected,

        // Most zombies dont turn again after dying (TODO)
        Zombie,
    }
}
