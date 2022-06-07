using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TWrecks_RPG {
    public class SquadLeader {
        public SquadLeader(Actor targetLeader)
        {
            this.squadLeaderActor = targetLeader;
        }
        public Actor squadLeaderActor;
        public SquadFormation squad;
    }
}
