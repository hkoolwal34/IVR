using System;
using Ozeki.Media;
using Ozeki.VoIP;

namespace _08_DTMF_Authentication
{
    class Softphone
    {
        ISoftPhone _softphone;
        IPhoneLine _phoneLine;
        public event EventHandler<VoIPEventArgs<IPhoneCall>> IncomigCall;

        public Softphone()
        {
            _softphone = SoftPhoneFactory.CreateSoftPhone(5000, 10000);
            _softphone.IncomingCall += softphone_IncomingCall;
        }

        public void Register(bool registrationRequired, string displayName, string userName, string authenticationId, string registerPassword, string domainHost, int domainPort)
        {
            try
            {
                var account = new SIPAccount(registrationRequired, displayName, userName, authenticationId, registerPassword, domainHost, domainPort);
                Console.WriteLine("\n Creating SIP account {0}", account);

                _phoneLine = _softphone.CreatePhoneLine(account);
                Console.WriteLine("Phoneline created.");

                _phoneLine.RegistrationStateChanged += phoneLine_RegistrationStateChanged;

                _softphone.RegisterPhoneLine(_phoneLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during SIP registration" + ex);
            }
        }

        void phoneLine_RegistrationStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            Console.WriteLine("Phone line state changed to {0}", e.State);

            var handler = PhoneLineStateChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<RegistrationStateChangedArgs> PhoneLineStateChanged;

        void softphone_IncomingCall(object sender, VoIPEventArgs<IPhoneCall> e)
        {
            var handler = IncomigCall;
            if (handler != null) handler(this, e);
        }
    }
}
