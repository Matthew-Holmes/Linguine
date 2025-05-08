using Infrastructure;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Linguine
{

    // TODO - what to do about readonly handles etc??

    public enum DataManagersState
    {
        NoDatabaseYet,
        RequestedReadonly,
        Requested,
        Initialised,
        IntialisedReadonly,
        Disposing,
        Disposed
    }// TODO - do we need the dispose stuff, or handle that with IDisposable

    partial class ServiceManager
    {


    }
}
