using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataClasses
{
    public enum EntryMethod
    {
        // initial methods
        Machine = 0,
        User = 1,
        // when choosing from options
        UserOverwriteMachine = 2,
        UserOverwriteUser = 3,
        MachineOverwriteMachine = 4,
        MachineOverwriteUser = 5,
        // when the data is textual
        UserGeneratesNewMachineOverwritesMachine = 6,
        UserGeneratesNewMachineOverwritesUser = 7,
        UserWritesCustomOverwritesMachine = 8,
        UserWritesCustomOverwritesUser = 9 ,
    }
}
