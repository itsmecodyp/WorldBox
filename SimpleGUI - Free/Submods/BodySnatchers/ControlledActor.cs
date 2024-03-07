using System.Collections.Generic;
using UnityEngine;

namespace BodySnatchers {
    public static class ControlledActor {
        private static Actor controlledActor;

        public static void Update() {
            if (controlledActor == null) {
                return;
            }

            if (!controlledActor.isAlive()) {
                SetActor(null);
                return;
            }

            MoveActor(controlledActor);
            FollowActor(controlledActor);
        }

        public static void SetActor(Actor actor) {
            if (actor != null) {
                WorldTip.instance.show("Now controlling " + actor.getName(), false, "top");

                HotkeyLibrary.right.default_key_1 = KeyCode.None;
                HotkeyLibrary.up.default_key_1 = KeyCode.None;
                HotkeyLibrary.down.default_key_1 = KeyCode.None;
                HotkeyLibrary.left.default_key_1 = KeyCode.None;
            } else {
                if (controlledActor != null) {
                    WorldTip.instance.show("Stopped controlling " + controlledActor.getName(), false, "top");
                }

                HotkeyLibrary.right.default_key_1 = KeyCode.D;
                HotkeyLibrary.up.default_key_1 = KeyCode.W;
                HotkeyLibrary.down.default_key_1 = KeyCode.S;
                HotkeyLibrary.left.default_key_1 = KeyCode.A;
            }

            controlledActor = actor;
        }

        public static Actor GetActor() {
            return controlledActor;
        }

        private static void MoveActor(Actor actor) {
            if (actor == null) {
                return;
            }

            int xOffset = 0;
            int yOffset = 0;

            if (Input.GetKey(KeyCode.W)) {
                yOffset++;
            }
            if (Input.GetKey(KeyCode.S)) {
                yOffset--;
            }
            if (Input.GetKey(KeyCode.D)) {
                xOffset++;
            }
            if (Input.GetKey(KeyCode.A)) {
                xOffset--;
            }

            WorldTile moveTile = MapBox.instance.GetTile(Mathf.Clamp((int) actor.currentPosition.x + xOffset, 0, MapBox.width - 1), Mathf.Clamp((int) actor.currentPosition.y + yOffset, 0, MapBox.height - 1));

            if (xOffset == 0 && yOffset == 0) {
                actor.stopMovement();
                actor.cancelAllBeh();
                return;
            }

            actor.moveTo(moveTile);
            actor.ai.setTask("wait10", false, true);
            actor.findCurrentTile();
        }

        private static void FollowActor(Actor actor) {
            if (actor == null) {
                return;
            }

            Vector3 vector = actor.currentPosition;
            vector.z = MoveCamera.instance.transform.position.z;
            MoveCamera.instance.transform.position = vector;
        }
    }
}
