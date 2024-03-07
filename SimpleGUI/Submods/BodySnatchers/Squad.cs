using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace BodySnatchers
{
    public enum FormationType
    {
        Dot,
        Line,
        CenteredLine,
        Rectangle,
        Circle
    }

    public enum FormationAction
    {
        Follow,
        Wait,
        Roam,
        Aggression
    }

    public delegate void SquadAction(List<Actor> squad);

    public class Squad
    {
        public Actor leader { get; private set; }
        public List<Actor> squad { get; private set; }
        public int maxSize;
        public int lineX;
        public int lineY;
        public FormationType formation;
        public FormationAction action;
        public Vector2 waitPosition;
        public SquadAction squadAction;
        public SquadAction nextSquadAction;
        public float nextActionTime;
        private float lastActionTime;
        private Vector2[] positions => GetFormationPositions();
        private static List<Squad> squads = new List<Squad>();

        public Squad(Actor leader, int maxSize, int lineX = 1, int lineY = 0, FormationType formation = FormationType.CenteredLine, FormationAction action = FormationAction.Follow)
        {
            this.leader = leader;
            this.maxSize = maxSize;
            this.lineX = lineX;
            this.lineY = lineY;
            this.formation = formation;
            this.action = action;
            nextActionTime = 3f;
            lastActionTime = 0f;
            squad = new List<Actor>();
            squads.Add(this);
        }

        public static void UpdateAll()
        {
            foreach (Squad squad in squads)
            {
                if (squad != null)
                {
                    squad.Update();
                }
            }
        }

        public static bool InSquad(Actor actor)
        {
            foreach (Squad squad in squads)
            {
                if (squad.squad.Contains(actor) || squad.leader == actor)
                {
                    return true;
                }
            }

            return false;
        }
        public void IncrementLineX(int increment = 1)
        {
            lineX += increment;
            WorldTip.instance.show("lineX: " + lineX, false, "top");
        }

        public int GetLineX()
        {
            return lineX;
        }

        public void IncrementLineY(int increment = 1)
        {
            lineY += increment;
            WorldTip.instance.show("lineY: " + lineY, false, "top");
        }

        public int GetLineY()
        {
            return lineY;
        }

        public Actor targetedActor = null;

        public void Update()
        {
            if (leader == null)
            {
                return;
            }

            if (!leader.isAlive())
            {
                if(squad != null && squad.Count > 0)
                {
                    if(squad[0] != null)
                    {
                        SetLeader(squad[0]);
                    }
                }
                return;
            }
            //MapBox.instance.kingdoms.get("mad") mad kingdom lets them aggro most things
            EnemyFinderData enemyFinderData = null;
            if (leader.kingdom != null)
            {
                enemyFinderData = EnemiesFinder.findEnemiesFrom(leader.currentTile, leader.kingdom, -1);
            }

            for (int i = 0; i < squad.Count; i++)
            {
                Actor squadActor = squad[i];
                if (!squadActor.isAlive())
                {
                    squad.Remove(squadActor);
                    i--;
                    continue;
                }

                if (squadAction != null)
                {
                    break; //was continue, but we dont want "unintended" movement during actions
                }

                switch (action)
                {
                    case FormationAction.Follow:
                        MoveActor(squadActor);
                        break;
                    case FormationAction.Wait:
                        squadActor.stopMovement();
                        squadActor.cancelAllBeh();
                        break;
                    case FormationAction.Aggression:
                        if (enemyFinderData.list == null && targetedActor == null)
                        {
                            //default to follow behaviour
                            MoveActor(squadActor);
                        }
                        else
                        {
                            BaseSimObject baseSimObject = null;
                            if(targetedActor != null && targetedActor.isAlive())
                            {
                                baseSimObject = targetedActor;
                            }
                            else
                            {
                                checkObjectList(squadActor, enemyFinderData.list, true, out baseSimObject);
                            }
                            if (baseSimObject != null)
                            {
                                if (squadActor.isInAttackRange(baseSimObject) == false)
                                {
                                    if(squadActor.tileTarget == baseSimObject.currentTile)
                                    {

                                    }
                                    else
                                    {
                                        squadActor.goTo(baseSimObject.currentTile);
                                    }
                                }
                                else
                                {
                                    squadActor.tryToAttack(baseSimObject, true);
                                }
                            }
                        }
                        break;
                }
            }

            if (squadAction != null && (lastActionTime == 0 || lastActionTime + nextActionTime < Time.timeSinceLevelLoad))
            {
                squadAction(squad);
                if(nextSquadAction != null)
                {
                    squadAction = nextSquadAction;
                    nextSquadAction = null;
                }
                else
                {
                    squadAction = null;
                }
                lastActionTime = Time.timeSinceLevelLoad;
            }
        }

        //custom check to allow aggro to most actors
        //only prevent if target is in squad or same kingdom as squad
        public bool canAttackTarget(BaseSimObject target)
        {
            if(target == null)
            {
                return false;
            }
            if (squad.Contains(target.a))
            {
                return false;
            }
            if(ControlledActor.GetActor() != null)
            {
                if(target == ControlledActor.GetActor() || target.kingdom == ControlledActor.GetActor().kingdom)
                {
                    return false;
                }
            }
            return true;
        }

        //custom check to allow aggro to "anyone"
        //swapped canAttackTarget, think thats it, mostly copy paste aside from that
        public void checkObjectList(Actor actorChecking, ListPool<BaseSimObject> pList, bool pFindClosest, out BaseSimObject pBestObjectLast)
        {
            float num = 99999f;
            BaseSimObject baseSimObject = null;
            for (int i = 0; i < pList.Count; i++)
            {
                BaseSimObject baseSimObject2 = pList[i];
                if (baseSimObject2.isAlive() && !(baseSimObject2 == actorChecking) && canAttackTarget(baseSimObject2) && (!actorChecking.isActor() || actorChecking.a.s_attackType != WeaponType.Melee || baseSimObject2.currentTile.isSameIsland(actorChecking.currentTile) || (!baseSimObject2.currentTile.Type.block && actorChecking.currentTile.region.island.isConnectedWith(baseSimObject2.currentTile.region.island))) && (!baseSimObject2.isBuilding() || !actorChecking.kingdom.isCiv() || !baseSimObject2.b.asset.cityBuilding || baseSimObject2.b.asset.tower || baseSimObject2.kingdom.race != actorChecking.kingdom.race) && !actorChecking.shouldIgnoreTarget(baseSimObject2))
                {
                    if (!pFindClosest)
                    {
                        pBestObjectLast = baseSimObject2;
                        return;
                    }
                    float num2 = Toolbox.DistTile(baseSimObject2.currentTile, actorChecking.currentTile);
                    if (num2 < num)
                    {
                        baseSimObject = baseSimObject2;
                        num = num2;
                        if (num < 2f)
                        {
                            pBestObjectLast = baseSimObject;
                            return;
                        }
                    }
                }
            }
            pBestObjectLast = baseSimObject;
        }

        public void CycleFormation()
        {
            if (leader == null)
            {
                return;
            }

            formation = (FormationType)(((int)formation + 1) % Enum.GetNames(typeof(FormationType)).Length);
            WorldTip.instance.show("Changed formation to " + formation, false, "top");
        }

        public void CycleAction()
        {
            if (leader == null)
            {
                return;
            }

            action = (FormationAction)(((int)action + 1) % Enum.GetNames(typeof(FormationAction)).Length);
            WorldTip.instance.show("Changed squad action to " + action, false, "top");
        }

        public void HireActor(Actor actor, bool showTip = true)
        {
            if (leader == null || actor == leader)
            {
                return;
            }

            if (actor == null)
            {
                WorldTip.instance.show("Select a unit to hire", false, "top");
                return;
            }

            if (InSquad(actor))
            {
                WorldTip.instance.show(actor.getName() + " is in another squad", false, "top");
                //return;
            }

            if (squad.Count >= maxSize)
            {
                WorldTip.instance.show("Max squad size of " + maxSize + " reached", false, "top");
                return;
            }

            if (squad.Contains(actor))
            {
                WorldTip.instance.show(actor.getName() + " is already hired", false, "top");
                return;
            }

            if (showTip)
            {
                WorldTip.instance.show("Hired " + actor.getName(), false, "top");
            }

            squad.Add(actor);
            return;
        }

        public void FireActor(Actor actor, bool showTip = true)
        {
            if (leader == null || actor == leader)
            {
                return;
            }

            if (actor == null)
            {
                WorldTip.instance.show("Select a unit to fire", false, "top");
                return;
            }

            if (!squad.Contains(actor))
            {
                WorldTip.instance.show(actor.getName() + " is not hired", false, "top");
                return;
            }

            if (showTip)
            {
                WorldTip.instance.show("Fired " + actor.getName(), false, "top");
            }

            squad.Remove(actor);
            return;
        }

        public void SetLeader(Actor actor)
        {
            if (actor == null)
            {
                squad.Clear();
            }

            if (squad.Contains(actor))
            {
                if (leader != null)
                {
                    squad.Add(leader);
                }

                squad.Remove(actor);
            }

            if (InSquad(actor))
            {
                WorldTip.instance.show(actor.getName() + " is in another squad", false, "top");
                return;
            }

            leader = actor;
        }

        public void MoveActor(Actor actor)
        {
            if (actor == null || !squad.Contains(actor))
            {
                return;
            }

            moveToVector(actor, positions[squad.IndexOf(actor)]);
            actor.ai.setTask("wait10", false, true);
            actor.findCurrentTile();
            return;
        }

        public void moveToVector(Actor actor, Vector2 position)
        {
            WorldTile moveTile = MapBox.instance.GetTile(Mathf.Clamp((int)position.x, 0, MapBox.width - 1), Mathf.Clamp((int)position.y, 0, MapBox.height - 1));

            actor.setPosDirty();
            actor.setIsMoving();

            actor.nextStepTile = moveTile;

            if (Toolbox.DistTile(actor.currentTile, moveTile) > 2f)
            {
                actor.dirty_current_tile = true;
            }
            else
            {
                actor.setCurrentTile(actor.nextStepTile);
            }

            if (actor.currentTile.Type.stepAction != null && Toolbox.randomChance(moveTile.Type.stepActionChance))
            {
                actor.currentTile.Type.stepAction(moveTile, actor);
            }

            actor.nextStepPosition = new Vector3(Mathf.Clamp(position.x, 0, MapBox.width - 1), Mathf.Clamp(position.y, 0, MapBox.height - 1));
        }

        private Vector2[] GetFormationPositions()
        {
            if (leader == null || squad.Count == 0)
            {
                return null;
            }

            Vector2[] positions = new Vector2[squad.Count];
            Vector2 origin = action == FormationAction.Wait && waitPosition != null ? waitPosition : (leader.nextStepPosition == Globals.emptyVector ? leader.currentPosition : (Vector2)leader.nextStepPosition);

            switch (formation)
            {
                case FormationType.Dot:
                    for (int i = 0; i < squad.Count; i++)
                    {
                        positions[i] = origin;
                    }
                    break;
                case FormationType.Line:
                    for (int i = 1; i <= squad.Count; i++)
                    {
                        positions[i - 1] = new Vector2(origin.x + (lineX * i), origin.y + (lineY * i));
                    }
                    break;
                case FormationType.CenteredLine:
                    int n = 1;

                    for (int i = 0; i < squad.Count; i++)
                    {
                        positions[i] = new Vector2(origin.x + (lineX * n), origin.y + (lineY * n));
                        n = i % 2 == 0 ? -n : -n + 1;
                    }
                    break;
                case FormationType.Rectangle:
                    int lx = Math.Abs(lineX);
                    int ly = Math.Abs(lineY);
                    float mx = lx / 2f;
                    float my = ly / 2f;
                    float sx = origin.x - mx;
                    float sy = origin.y + my;

                    int index = 0;
                    while (index < squad.Count)
                    {
                        if (lx == 0 || ly == 0)
                        {
                            positions[index] = origin;
                            index++;
                            continue;
                        }

                        for (int x = 0; x < lx; x++)
                        {
                            for (int y = 0; y < ly; y++)
                            {
                                if (index >= squad.Count)
                                {
                                    break;
                                }

                                positions[index] = new Vector2(sx + (x * mx), sy - (y * my));
                                index++;
                            }
                        }
                    }
                    break;
                case FormationType.Circle:
                    double section = 2 * Math.PI / maxSize;
                    for (int i = 0; i < squad.Count; i++)
                    {
                        double radian = section * i;
                        double x = lineX * Math.Cos(radian) + origin.x;
                        double y = lineY * Math.Sin(radian) + origin.y;

                        positions[i] = new Vector2((float)x, (float)y);
                    }
                    break;
            }

            return positions;

        }
    }
}
