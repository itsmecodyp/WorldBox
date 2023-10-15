using System;
using System.Collections.Generic;
using UnityEngine;

namespace BodySnatchers {
    public enum FormationType {
        Dot,
        Line,
        CenteredLine,
        Rectangle,
        Circle
    }

    public enum FormationAction {
        Follow,
        Wait,
        Roam
    }

    public delegate void SquadAction(List<Actor> targetSquad);

    public class Squad {
        public SquadAction actionToDo;
        //actionToDo can assign this and continue some behaviour
        public SquadAction nextActionToDo;
        public float lastActionTime = 0f;
        //customizable timing?
        public float nextActionTimer = 3f;

        public Vector2[] positions => GetFormationPositions(leader);
        public FormationType formation;
        public FormationAction action;
        public int lineX;
        public int lineY;
        public int maxSize;
        public List<Actor> squad;
        public Actor leader;

        public Squad(Actor leader, int maxSize, int lineX = 1, int lineY = 0, FormationType formation = FormationType.CenteredLine, FormationAction action = FormationAction.Follow) {
            this.leader = leader;
            this.maxSize = maxSize;
            this.lineX = lineX;
            this.lineY = lineY;
            this.formation = formation;
            this.action = action;
            squad = new List<Actor>();
        }

        public void Update() {
            if (leader == null) {
                return;
            }

            if (!leader.isAlive()) {
                SetLeader(squad[0]);
                return;
            }

            if (actionToDo != null)
            {
                if (lastActionTime == 0 || lastActionTime + nextActionTimer < Time.timeSinceLevelLoad)
                {
                    //check if delegate is assigned
                    actionToDo(squad);
                    //check for nextAction (assigned by actionToDo)
                    if (nextActionToDo != null)
                    {
                        actionToDo = nextActionToDo;
                        nextActionToDo = null;
                    }
                    else
                    {
                        //unassign if custom actions are finished
                        actionToDo = null;
                    }
                    //after action, start cooldown
                    lastActionTime = Time.timeSinceLevelLoad;
                }
            }

            else
            {
                for (int i = 0; i < squad.Count; i++)
                {
                    Actor actor = squad[i];

                    if (!actor.isAlive())
                    {
                        squad.Remove(actor);
                        i--;
                        continue;
                    }

                    switch (action)
                    {
                        case FormationAction.Follow:
                            MoveActor(actor);
                            break;
                        case FormationAction.Wait:
                            actor.stopMovement();
                            actor.cancelAllBeh();
                            break;
                        case FormationAction.Roam:
                            break;
                    }
                }
            }
        }

        public void CycleFormation() {
            if (leader == null) {
                return;
            }

            formation = (FormationType)(((int)formation + 1) % Enum.GetNames(typeof(FormationType)).Length);
            WorldTip.instance.show("Changed formation to " + formation, false, "top");
        }

        public FormationType GetFormation() { 
            return formation; 
        }

        public void CycleAction() {
            if (leader == null) {
                return;
            }

            action = (FormationAction)(((int)action + 1) % Enum.GetNames(typeof(FormationAction)).Length);
            WorldTip.instance.show("Changed squad action to " + action, false, "top");
        }

        public FormationAction GetAction() {
            return action;
        }

        public void IncrementLineX(int increment = 1) {
            lineX += increment;
            WorldTip.instance.show("lineX: " + lineX, false, "top");
        }

        public int GetLineX() {
            return lineX;
        }

        public void IncrementLineY(int increment = 1) {
            lineY += increment;
            WorldTip.instance.show("lineY: " + lineY, false, "top");
        }

        public int GetLineY() {
            return lineY;
        }

        public void HireActor(Actor actor, bool showToolTip = true) {
            if (leader == null || actor == leader) {
                return;
            }

            if (actor == null) {
                if(showToolTip)
                WorldTip.instance.show("Select a unit to hire", false, "top");
                return;
            }

            if (squad.Count >= maxSize) {
                if(showToolTip)
                WorldTip.instance.show("Max squad size of " + maxSize + " reached", false, "top");
                return;
            }

            if (squad.Contains(actor)) {
                if(showToolTip)
                WorldTip.instance.show(actor.getName() + " is already hired", false, "top");
                return;
            }
            if(showToolTip)
            WorldTip.instance.show("Hired " + actor.getName(), false, "top");
            squad.Add(actor);
        }

        public void FireActor(Actor actor) {
            if (leader == null || actor == leader) {
                return;
            }

            if (actor == null) {
                WorldTip.instance.show("Select a unit to fire", false, "top");
                return;
            }

            if (!squad.Contains(actor)) {
                WorldTip.instance.show(actor.getName() + " is not hired", false, "top");
                return;
            }

            WorldTip.instance.show("Fired " + actor.getName(), false, "top");
            squad.Remove(actor);
        }

        public List<Actor> GetSquad() {
            return squad;
        }

        public void SetLeader(Actor actor) {
            if (actor == null) {
                squad.Clear();
            }

            if (squad.Contains(actor)) {
                if (leader != null) {
                    squad.Add(leader);
                }

                squad.Remove(actor);
            }

            leader = actor;
        }

        public Actor GetLeader() {
            return leader;
        }

        public void MoveActor(Actor actor) {
            if (actor == null || !squad.Contains(actor)) {
                return;
            }

            moveTo(actor, positions[squad.IndexOf(actor)]);
            actor.ai.setTask("wait10", false, true);
            actor.findCurrentTile();
        }

        private void moveTo(Actor actor, Vector2 position) {
            WorldTile moveTile = MapBox.instance.GetTile(Mathf.Clamp((int) position.x, 0, MapBox.width - 1), Mathf.Clamp((int) position.y, 0, MapBox.height - 1));
            
            actor.setPosDirty();
            actor.setIsMoving();

            actor.nextStepTile = moveTile;

            if (Toolbox.DistTile(actor.currentTile, moveTile) > 2f) {
                actor.dirty_current_tile = true;
            } else {
                actor.setCurrentTile(actor.nextStepTile);
            }

            if (actor.currentTile.Type.stepAction != null && Toolbox.randomChance(moveTile.Type.stepActionChance)) {
                actor.currentTile.Type.stepAction(moveTile, actor);
            }

            actor.nextStepPosition = new Vector3(Mathf.Clamp(position.x, 0, MapBox.width - 1), Mathf.Clamp(position.y, 0, MapBox.height - 1));
        }

        private Vector2[] GetFormationPositions(Actor actor) {
            if (actor == null || leader == null || squad.Count == 0) {
                return null;
            }

            Vector2 origin = leader.nextStepPosition == Globals.emptyVector ? leader.currentPosition : (Vector2) leader.nextStepPosition;
            Vector2[] positions = new Vector2[squad.Count];

            switch (formation) {
                case FormationType.Dot:
                    for (int i = 0; i < squad.Count; i++) {
                        positions[i] = origin;
                    }
                    break;
                case FormationType.Line:
                    for (int i = 1; i <= squad.Count; i++) {
                        positions[i - 1] = new Vector2(origin.x + (lineX * i), origin.y + (lineY * i));
                    }
                    break;
                case FormationType.CenteredLine:
                    int n = 1;

                    for (int i = 0; i < squad.Count; i++) {
                        positions[i] = new Vector2(origin.x + (lineX * n), origin.y + (lineY * n));
                        n = i % 2 == 0 ? n * -1 : (n * -1) + 1;
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
                    while (index < squad.Count) {
                        if (lx == 0 || ly == 0) {
                            positions[index] = origin;
                            index++;
                            continue;
                        }

                        for (int x = 0; x < lx; x++) {
                            for (int y = 0; y < ly; y++) {
                                if (index >= squad.Count) {
                                    break;
                                }

                                positions[index] = new Vector2(sx + (x * mx), sy - (y * my));
                                index++;
                            }
                        }
                    }
                    break;
                case FormationType.Circle:
                    double section = 2 * Math.PI / squad.Count;
                    for (int i = 0; i < squad.Count; i++) {
                        double radian = section * i;
                        double x = lineX * Math.Cos(radian) + origin.x;
                        double y = lineY * Math.Sin(radian) + origin.y;

                        positions[i] = new Vector2((float) x, (float) y);
                    }
                    break;
            }

            return positions;
        }
    }
}
