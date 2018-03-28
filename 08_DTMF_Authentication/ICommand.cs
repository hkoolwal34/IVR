using Ozeki.VoIP;
using System;

namespace _08_DTMF_Authentication
{
    interface ICommand
    {
        void Start(ICall call);
        void Cancel();
        event EventHandler Completed;
    }
}
