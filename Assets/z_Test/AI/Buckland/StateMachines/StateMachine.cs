﻿using Buckland;

namespace WestWorld
{

    public class State<entity_type> where entity_type : class
    {

        //this will execute when the state is entered
        public virtual void Enter(entity_type type) { }

        //this is the states normal update function
        public virtual void Execute(entity_type type) { }

        //this will execute when the state is exited. 
        public virtual void Exit(entity_type type) { }

        //this executes if the agent receives a message from the 
        //message dispatcher
        public virtual bool OnMessage(entity_type type, Telegram tel) { return false; }
    }


    public class StateMachine<entity_type> where entity_type : class
    {

        //a pointer to the agent that owns this instance
        entity_type m_pOwner;

        State<entity_type> m_pCurrentState;

        //a record of the last state the agent was in
        State<entity_type> m_pPreviousState;

        //this is called every time the FSM is updated
        State<entity_type> m_pGlobalState;



        public StateMachine(entity_type owner)
        {
            m_pOwner = owner;
            m_pCurrentState = null;
            m_pPreviousState = null;
            m_pGlobalState = null;
        }


        //use these methods to initialize the FSM
        public void SetCurrentState(State<entity_type> s) { m_pCurrentState = s; }
        public void SetGlobalState(State<entity_type> s) { m_pGlobalState = s; }
        public void SetPreviousState(State<entity_type> s) { m_pPreviousState = s; }

        //call this to update the FSM
        public void Update()
        {
            //if a global state exists, call its execute method, else do nothing
            if (null != m_pGlobalState) m_pGlobalState.Execute(m_pOwner);

            //same for the current state
            if (null != m_pCurrentState) m_pCurrentState.Execute(m_pOwner);
        }

        public bool HandleMessage(Telegram msg)
        {
            //first see if the current state is valid and that it can handle
            //the message
            if (null != m_pCurrentState && m_pCurrentState.OnMessage(m_pOwner, msg))
            {
                return true;
            }

            //if not, and if a global state has been implemented, send 
            //the message to the global state
            if (null != m_pGlobalState && m_pGlobalState.OnMessage(m_pOwner, msg))
            {
                return true;
            }

            return false;
        }

        //change to a new state
        public void ChangeState(State<entity_type> pNewState)
        {
            //assert(pNewState && "<StateMachine::ChangeState>:trying to assign null state to current");

            //keep a record of the previous state
            m_pPreviousState = m_pCurrentState;

            //call the exit method of the existing state
            m_pCurrentState.Exit(m_pOwner);

            //change state to the new state
            m_pCurrentState = pNewState;

            //call the entry method of the new state
            m_pCurrentState.Enter(m_pOwner);
        }

        //change state back to the previous state
        public void RevertToPreviousState()
        {
            ChangeState(m_pPreviousState);
        }

        //returns true if the current state's type is equal to the type of the
        //class passed as a parameter. 
        public bool isInState(State<entity_type> st)
        {

            if (m_pCurrentState.GetType() == st.GetType()) return true;
            return false;
        }

        public State<entity_type> CurrentState() { return m_pCurrentState; }
        public State<entity_type> GlobalState() { return m_pGlobalState; }
        public State<entity_type> PreviousState() { return m_pPreviousState; }

        //only ever used during debugging to grab the name of the current state
        string GetNameOfCurrentState()
        {
            string s = m_pCurrentState.GetType().ToString();

            //remove the 'class ' part from the front of the string
            //if (s.size() > 5)
            //{
            //    s.erase(0, 6);
            //}

            return s;
        }
    }
}

