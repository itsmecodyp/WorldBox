using SimplerGUI.Menus;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace BodySnatchers
{
    public static class ControlledActor
    {
        private static Actor controlledActor;

        public static void Update()
        {
            if (controlledActor == null)
            {
                return;
            }

            if (!controlledActor.isAlive())
            {
                SetActor(null);
                return;
            }

            MoveActor(controlledActor);
            FollowActor(controlledActor);
        }
        public static bool tempBoolBeforeToggle;
        public static void SetActor(Actor actor)
        {
            if (actor != null)
            {
                WorldTip.instance.show("Now controlling " + actor.getName(), false, "top");

                HotkeyLibrary.right.default_key_1 = KeyCode.None;
                HotkeyLibrary.up.default_key_1 = KeyCode.None;
                HotkeyLibrary.down.default_key_1 = KeyCode.None;
                HotkeyLibrary.left.default_key_1 = KeyCode.None;
                tempBoolBeforeToggle = GuiOther.disableMouseDrag;
                GuiOther.disableMouseDrag = true; // disable mouse drag while in control
            }
            else
            {
                if (controlledActor != null)
                {
                    WorldTip.instance.show("Stopped controlling " + controlledActor.getName(), false, "top");
                }

                HotkeyLibrary.right.default_key_1 = KeyCode.D;
                HotkeyLibrary.up.default_key_1 = KeyCode.W;
                HotkeyLibrary.down.default_key_1 = KeyCode.S;
                HotkeyLibrary.left.default_key_1 = KeyCode.A;
                GuiOther.disableMouseDrag = tempBoolBeforeToggle; // reset mouse drag state
            }

            controlledActor = actor;
        }

        public static void Attack(Actor target)
        {
            if (target == null || controlledActor == null)
            {
                return;
            }

            if (Toolbox.DistVec3(controlledActor.currentPosition, target.currentPosition) >= (controlledActor.stats[S.range] + target.stats[S.size])/* * 2*/)
            {
                return;
            }

            float distance = Vector2.Distance(controlledActor.currentPosition, target.currentPosition) + target.getZ();
            Vector3 vector = new Vector3(target.currentPosition.x, target.currentPosition.y);
            Vector3 point = Toolbox.getNewPoint(controlledActor.currentPosition.x,
                controlledActor.currentPosition.y,
                vector.x,
                vector.y,
                distance - target.stats[S.size], true);

            if (target.isActor() && target.a.is_moving && target.isFlying())
            {
                vector = Vector3.MoveTowards(vector, target.a.nextStepPosition, target.stats[S.size] * 3f);
            }

            AttackData data = new AttackData(controlledActor, target.currentTile, point, target, AttackType.Weapon, controlledActor.haveMetallicWeapon(), false, true);

            controlledActor.punchTargetAnimation(target.currentPosition, true, controlledActor.s_attackType == WeaponType.Range, 40f);
            using (ListPool<CombatActionAsset> listPool = new ListPool<CombatActionAsset>())
            {
                if (controlledActor.asset.attack_spells != null && controlledActor.asset.attack_spells.Count > 0)
                {
                    controlledActor.addToAttackPool(CombatActionLibrary.combat_cast_spell, listPool);
                }

                CombatActionAsset combatActionAsset;

                if (listPool.Count > 0)
                {
                    if (controlledActor.s_attackType == WeaponType.Melee)
                    {
                        controlledActor.addToAttackPool(CombatActionLibrary.combat_attack_melee, listPool);
                    }
                    else
                    {
                        controlledActor.addToAttackPool(CombatActionLibrary.combat_attack_range, listPool);
                    }
                    combatActionAsset = listPool.GetRandom<CombatActionAsset>();
                    if (!combatActionAsset.action(data) && !combatActionAsset.basic)
                    {
                        if (controlledActor.s_attackType == WeaponType.Melee)
                        {
                            CombatActionLibrary.combat_attack_melee.action(data);
                        }
                        else
                        {
                            CombatActionLibrary.combat_attack_range.action(data);
                        }
                    }
                }
                else
                {
                    if (controlledActor.s_attackType == WeaponType.Melee)
                    {
                        combatActionAsset = CombatActionLibrary.combat_attack_melee;
                    }
                    else
                    {
                        combatActionAsset = CombatActionLibrary.combat_attack_range;
                    }
                    combatActionAsset.action(data);
                }
                if (combatActionAsset.play_unit_attack_sounds && !string.IsNullOrEmpty(controlledActor.asset.fmod_attack))
                {
                    MusicBox.playSound(controlledActor.asset.fmod_attack, controlledActor.currentTile.x, controlledActor.currentTile.y, false, false);
                }
                if (controlledActor.asset.needFood && Toolbox.randomBool())
                {
                    controlledActor.decreaseHunger(-1);
                }
            }
        }

        public static Actor GetActor()
        {
            return controlledActor;
        }

        private static void MoveActor(Actor actor)
        {
            if (actor == null)
            {
                return;
            }

            int xOffset = 0;
            int yOffset = 0;

            if (ActorControlMain.isJoyEnabled)
            {
                float verticalAxis = UltimateJoystick.GetVerticalAxis("JoyRight");
                float horizontalAxis = UltimateJoystick.GetHorizontalAxis("JoyRight");
                if(verticalAxis > 0.1f)
                {
                    yOffset++;
                }
                if (verticalAxis < -0.1f)
                {
                    yOffset--;
                }
                if (horizontalAxis > 0.1f)
                {
                    xOffset++;
                }
                if (horizontalAxis < -0.1f)
                {
                    xOffset--;
                }
            }

            if (Input.GetKey(KeyCode.W))
            {
                yOffset++;
            }
            if (Input.GetKey(KeyCode.S))
            {
                yOffset--;
            }
            if (Input.GetKey(KeyCode.D))
            {
                xOffset++;
            }
            if (Input.GetKey(KeyCode.A))
            {
                xOffset--;
            }

            WorldTile moveTile = MapBox.instance.GetTile(Mathf.Clamp((int)actor.currentPosition.x + xOffset, 0, MapBox.width - 1), Mathf.Clamp((int)actor.currentPosition.y + yOffset, 0, MapBox.height - 1));

            if (xOffset == 0 && yOffset == 0)
            {
                actor.stopMovement();
                actor.cancelAllBeh();
                return;
            }

            actor.moveTo(moveTile);
            actor.ai.setTask("wait10", false, true);
            actor.findCurrentTile();
        }

        private static void FollowActor(Actor actor)
        {
            if (actor == null)
            {
                return;
            }

            Vector3 vector = actor.currentPosition;
            vector.z = MoveCamera.instance.transform.position.z;
            MoveCamera.instance.transform.position = vector;
        }
    }
}
