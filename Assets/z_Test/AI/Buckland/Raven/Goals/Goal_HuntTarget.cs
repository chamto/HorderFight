﻿using System;
using UnityEngine;
using UtilGS9;

namespace Raven
{
    public class Goal_HuntTarget : Goal_Composite<Raven_Bot>
    {

        //this value is set to true if the last visible position of the target
        //bot has been searched without success
        bool m_bLVPTried;


        public Goal_HuntTarget(Raven_Bot pBot) : base(pBot, (int)eGoal.hunt_target)
        {
            m_bLVPTried = false;
        }

        //the usual suspects
        override public void Activate()
        {
            m_iStatus = (int)eStatus.active;

            //if this goal is reactivated then there may be some existing subgoals that
            //must be removed
            RemoveAllSubgoals();

            //it is possible for the target to die whilst this goal is active so we
            //must test to make sure the bot always has an active target
            if (m_pOwner.GetTargetSys().isTargetPresent())
            {
                //grab a local copy of the last recorded position (LRP) of the target
                Vector3 lrp = m_pOwner.GetTargetSys().GetLastRecordedPosition();

                //if the bot has reached the LRP and it still hasn't found the target
                //it starts to search by using the explore goal to move to random
                //map locations
                if (Misc.IsZero(lrp) || m_pOwner.isAtPosition(lrp))
                {
                    AddSubgoal(new Goal_Explore(m_pOwner));
                }

                //else move to the LRP
                else
                {
                    AddSubgoal(new Goal_MoveToPosition(m_pOwner, lrp));
                }
            }

            //if their is no active target then this goal can be removed from the queue
            else
            {
                m_iStatus = (int)eStatus.completed;
            }

        }
        override public int Process()
        {
            //if status is inactive, call Activate()
            ActivateIfInactive();

            m_iStatus = ProcessSubgoals();

            //if target is in view this goal is satisfied
            if (m_pOwner.GetTargetSys().isTargetWithinFOV())
            {
                m_iStatus = (int)eStatus.completed;
            }

            return m_iStatus;
        }
        override public void Terminate() { }

        override public void Render()
        {

            //render last recorded position as a green circle
            if (m_pOwner.GetTargetSys().isTargetPresent())
            {
                DebugWide.DrawCircle(m_pOwner.GetTargetSys().GetLastRecordedPosition(), 3, Color.green);
            }

            //forward the request to the subgoals
            base.Render();

        }
    }

}//end namespace

