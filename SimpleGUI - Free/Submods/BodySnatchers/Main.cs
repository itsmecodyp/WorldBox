using BepInEx;
using UnityEngine;

namespace BodySnatchers {
    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class Main : BaseUnityPlugin {
        private const string pluginGuid = "apexlite.worldbox.bodysnatchers";
        private const string pluginName = "BodySnatchers";
        private const string pluginVersion = "1.0.0";
        public static Squad squad;

        void Update() {
            if (Input.GetKeyDown(KeyCode.H))
            {
               // squad.FireActor(MapBox.instance.getActorNearCursor());
            }
            //commented out all the stuff in the menu
            /*
            if (Input.GetKeyDown(KeyCode.P)) {
                Actor actor = ControlledActor.GetActor() == null ? MapBox.instance.getActorNearCursor() : null;
                ControlledActor.SetActor(actor);
                squad = new Squad(actor, 10);
            }

            if (Input.GetKeyDown(KeyCode.Y)) {
                squad.HireActor(MapBox.instance.getActorNearCursor());
            }

           
            if (Input.GetKeyDown(KeyCode.G)) {
                squad.IncrementLineX();
            }

            if (Input.GetKeyDown(KeyCode.J)) {
                squad.IncrementLineX(-1);
            }

            if (Input.GetKeyDown(KeyCode.L)) {
                squad.IncrementLineY();
            }

            if (Input.GetKeyDown(KeyCode.B)) {
                squad.IncrementLineY(-1);
            }

            if (Input.GetKeyDown(KeyCode.Z)) {
                //squad.CycleFormation();
            }

            if (Input.GetKeyDown(KeyCode.V)) {
                //squad.CycleAction();
            }
            */
            ControlledActor.Update();

            if (squad != null) {
                squad.Update();
            }
        }
    }
}
