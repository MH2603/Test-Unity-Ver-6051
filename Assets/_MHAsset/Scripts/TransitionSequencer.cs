using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace HSM {
    public class TransitionSequencer
    {
        public readonly StateMachine Machine;
        
        public TransitionSequencer(StateMachine machine)
        {
            Machine = machine;
        }
        
        // Request a transition from 'from' to 'to' this frame.
        public void RequestTransition(State from, State to)
        {
            Machine.ChangeState(from, to);
        }
        
         // compute the lowest common ancestor (LCA) of two states
         public static State Lca(State a, State b)
         {
             //create a set of all parents of 'a'
             var aPath = new HashSet<State>();
             for(var s = a; s != null; s = s.Parent) aPath.Add(s);
             
             // Find the first parent of 'b' that is also in the set of 'a's parents
             for(var s=b; s != null; s = s.Parent)
             {
                 if (aPath.Contains(s)) return s; // Found the LCA
             }
             
             // if no common ancestor is found, return null
             return null;
         }
    }
}