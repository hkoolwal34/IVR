using System;
using Ozeki.Media;
using Ozeki.Network;
using Ozeki.VoIP;

namespace _08_DTMF_Authentication
{
    class Program
    {
        public static string ivrXml = @"<ivr>
	                            <menu forwardToUrl='http://mnrega.seemasbeauty360.com/dtmflevel1.php'>
                                    <init>
                                        <speak>
                                            Welcome to M G N R E G A IVR system. 
                                            
                                            मनरेगा के आई वी  आर  में  आपका  स्वागत  है। 
                                            
                                            Press 1 for english.

                                            हिंदी के लिए २ दबाये। 

                                        </speak>
                                        <play>../../test.mp3</play>
                                    </init>
		                            <keys>
			                            <key pressed='1'>
                                               <speak>
                                                      Thanks for calling to M G N R E G A IVR. Please wait while we process your request.
                                               </speak>
				                               <play>../../test.mp3</play>
			                            </key>
			                            <key pressed='2'>
				                               <speak>
                                                      Krapya line me ba ne rahiye. Hum aapki seva me tatpar hai.
                                               </speak>
			                            </key>
                                        <key pressed='3'>
				                               <menu forwardToUrl='http://mnrega.seemasbeauty360.com/dtmfauth.php'>
                                                      <init>
                                                            <speak>
                                                                You reached the lower menu.
                                                            </speak>
                                                      </init>
                                                    <keys>
                                                        <key pressed ='1'>
                                                          <speak>You pressed 1</speak>
  
                                                        </key>


                                                    </keys>
                                               </menu>
			                            </key>
		                            </keys>
	                            </menu>
                           </ivr>";

        static void Main(string[] args)
        {
            ShowHelp();
            
            Softphone softphone = new Softphone();

            SipAccountInitialization(softphone);

            softphone.IncomigCall += softphone_IncomigCall;

            Console.ReadLine();
        }

        private static void ShowHelp()
        {
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine("This is an example code for Interactive Voice Response (IVR) written in C#. This is a technology that allows a computer to interact with humans through the use of voice and DTMF tones input via keypad.");
            Console.WriteLine("We are going to create the IVR system by using an XML code. This makes the creation of a complex IVR so much easier.");
            Console.WriteLine("You will need to setup your SIP account (your authentication ID, username, display name etc.). If the registration succeeded and you call the number of the IVR you will hear a greeting message.");
            Console.WriteLine("After this you can choose the menu options, and give it a PIN code, with this you can access to your bank menu.\n");

            Console.ForegroundColor = ConsoleColor.White;

        }

        static void softphone_IncomigCall(object sender, VoIPEventArgs<Ozeki.VoIP.IPhoneCall> e)
        {
            //V 1.0
            //Constants.level++;
            var menu = IVRFactory.CreateIVR(ivrXml, new Menu());

            if (menu != null)
                menu.Start(e.Item);
            else
                e.Item.Reject();
        }

        private static void SipAccountInitialization(Softphone softphone)
        {
            Console.WriteLine("Setting up your SIP account. ");

            var authenticationId = Constants.sip_authenticationId;
            var userName = Constants.sip_userName;
            var displayName = Constants.sip_displayName;
            var registrationPassword = Constants.sip_registrationPassword;
            var domainHost = Constants.sip_domainHost;
            int domainPort = Constants.sip_domainPort;

            Console.WriteLine("\nCreating SIP account and trying to register...\n");
            softphone.Register(true, displayName, userName, authenticationId, registrationPassword, domainHost, domainPort);
        }

        private static string Read(string inputName, bool readWhileEmpty)
        {
            while (true)
            {
                string input = Console.ReadLine();

                if (!readWhileEmpty)
                {
                    return input;
                }

                if (!string.IsNullOrEmpty(input))
                {
                    return input;
                }

                Console.WriteLine(inputName + " cannot be empty!");
                Console.WriteLine(inputName + ": ");
            }
        }
    }
}
