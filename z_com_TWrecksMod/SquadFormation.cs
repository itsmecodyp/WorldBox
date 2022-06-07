using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TWrecks_RPG;
using UnityEngine;

namespace TWrecks_RPG {
    public class SquadFormation {
        public List<Actor> actorList = new List<Actor>();
        public Vector3 movementPos;
        public string formationType = "dot";
        public int radius = 5;
        public int lineX = 5;
        public int lineY = 5;

        public string squadName = "";
        public string squadID = (TWrecks_Main.squadsInUse.Count + 1).ToString();
        public bool followsControlledPos = true;
        public bool followsOffset = false;
        public Vector3 offsetPos = Vector3.one;
    }
}
