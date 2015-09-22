using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GitGameServer
{
    public class StateMessage : Message
    {
        private string state;
        private int round;
        
        private StateMessage(string state, int round)
        {
            this.state = state;
            this.round = round;
        }
    }
}
