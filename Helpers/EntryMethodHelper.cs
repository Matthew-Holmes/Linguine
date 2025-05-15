using DataClasses;
using Linguine.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class EntryMethodHelper
    {
        public static EntryMethod NewEntryMethodForTextual(EntryMethod old, TextualEditMethod editMethod)
        {
            if (editMethod is TextualEditMethod.NotEdited) { return old; }

            if (editMethod is TextualEditMethod.MachineEdited)
            {
                switch (old)
                {
                    case EntryMethod.Machine:
                        return EntryMethod.UserGeneratesNewMachineOverwritesMachine;
                    case EntryMethod.User:
                        return EntryMethod.UserGeneratesNewMachineOverwritesUser;
                    case EntryMethod.UserOverwriteMachine:
                        throw new ArgumentException();
                    case EntryMethod.UserOverwriteUser:
                        throw new ArgumentException();
                    case EntryMethod.MachineOverwriteMachine:
                        throw new ArgumentException();
                    case EntryMethod.MachineOverwriteUser:
                        throw new ArgumentException();
                    case EntryMethod.UserGeneratesNewMachineOverwritesMachine:
                        return EntryMethod.UserGeneratesNewMachineOverwritesUser;
                    case EntryMethod.UserGeneratesNewMachineOverwritesUser:
                        return EntryMethod.UserGeneratesNewMachineOverwritesUser;
                    case EntryMethod.UserWritesCustomOverwritesMachine:
                        return EntryMethod.UserGeneratesNewMachineOverwritesUser;
                    case EntryMethod.UserWritesCustomOverwritesUser:
                        return EntryMethod.UserGeneratesNewMachineOverwritesUser;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (editMethod is TextualEditMethod.UserEdited)
            {
                switch (old)
                {
                    case EntryMethod.Machine:
                        return EntryMethod.UserWritesCustomOverwritesMachine;
                    case EntryMethod.User:
                        return EntryMethod.UserWritesCustomOverwritesUser;
                    case EntryMethod.UserOverwriteMachine:
                        throw new ArgumentException();
                    case EntryMethod.UserOverwriteUser:
                        throw new ArgumentException();
                    case EntryMethod.MachineOverwriteMachine:
                        throw new ArgumentException();
                    case EntryMethod.MachineOverwriteUser:
                        throw new ArgumentException();
                    case EntryMethod.UserGeneratesNewMachineOverwritesMachine:
                        return EntryMethod.UserWritesCustomOverwritesUser;
                    case EntryMethod.UserGeneratesNewMachineOverwritesUser:
                        return EntryMethod.UserWritesCustomOverwritesUser;
                    case EntryMethod.UserWritesCustomOverwritesMachine:
                        return EntryMethod.UserWritesCustomOverwritesUser;
                    case EntryMethod.UserWritesCustomOverwritesUser:
                        return EntryMethod.UserWritesCustomOverwritesUser;
                    default:
                        throw new NotImplementedException();
                }
            }

            throw new NotImplementedException();
        }
    }
}
