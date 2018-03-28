using System.IO;
using System.Net;
using Ozeki.VoIP;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using System.Threading;
using Ozeki.Media;
using Timer = System.Timers.Timer;
using System.Threading.Tasks;

namespace _08_DTMF_Authentication
{
    class Menu : ICommand
    {
        Dictionary<int, MultipleCommandHandler> _keys;
        MultipleCommandHandler _init;
        ICall _call;
        Timer _greetingMessageTimer;

        bool _dtmfPressed;
        string _dtmfChain;
        Timer _keypressTimeoutTimer;

        public event EventHandler Completed;

        public string ForwardToUrl { get; set; }
        public string ResponseXml { get; set; }
        
        public Menu()
        {
            _keys = new Dictionary<int, MultipleCommandHandler>();
            _init = new MultipleCommandHandler();   
            _greetingMessageTimer = new Timer();
            _greetingMessageTimer.AutoReset = true;
            InitKeypressTimeoutTimer();
        }

        void greetingMessageTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _init.Cancel();
            _init.Start(_call);
        }

        public void Start(ICall call)
        {
            this._call = call;

            _greetingMessageTimer.Interval = Constants.greetingMsgTimer;
            _greetingMessageTimer.Start();

            Onsubscribe();
            call.Answer();

            _init.Start(call);
        }

        public void AddInitCommand(ICommand command)
        {
            _init.AddCommand(command);
        }

        public void AddKeypressCommand(int digit, ICommand command)
        {
            if (!_keys.ContainsKey(digit))
                _keys[digit] = new MultipleCommandHandler();

            _keys[digit].AddCommand(command);
        }

        public void Cancel()
        {

        }

        void call_CallStateChanged(object sender, CallStateChangedArgs e)
        {
            
        }

        void DtmfReceived(object sender, VoIPEventArgs<DtmfInfo> e)
        {
            _dtmfPressed = true;
            _dtmfChain += DtmfNamedEventConverter.DtmfNamedEventsToString(e.Item.Signal.Signal);
        }




        void call_DtmfReceived(object sender, string dtmfChain)
        {

            if (dtmfChain != null)
            {
                int pressedKey;
                if (!int.TryParse(dtmfChain, out pressedKey))
                {
                    Console.WriteLine("You did not add a valid number!");
                }

                MultipleCommandHandler command;
                if (_keys.TryGetValue(pressedKey, out command))
                {
                    Console.WriteLine(" command : " + command);
                    Console.WriteLine(" pressedKey : " + pressedKey);

                    if (ForwardToUrl != null)
                    {
                        ResponseXml = CreateHttpRequest(ForwardToUrl, pressedKey, _call.DialInfo.CallerID);
                        ForwardToUrl = null; 
                        //                    ResponseXml = CreateHttpRequest1(ForwardToUrl, pressedKey, _call.DialInfo.Dialed);
                        Console.WriteLine(ResponseXml);
//V 1.0                        Constants.level++;
                        var responseIvr = IVRFactory.CreateIVR(ResponseXml, new Menu());

                        StartCommand(responseIvr);
                    }
                    else
                    {
                           StartCommand(command);
                    }
                }
                else
                {
                    if (ForwardToUrl != null)
                    {
                        ResponseXml = CreateHttpRequest(ForwardToUrl, pressedKey, _call.DialInfo.Dialed);
//                        ResponseXml = CreateHttpRequest(ForwardToUrl, 6544, "1001");

                        var responseIvr = IVRFactory.CreateIVR(ResponseXml);

                        StartCommand(responseIvr);
                    }
                    else
                    {
                        Console.WriteLine("This is a not used option! Please try again!");
                    }
                }
            }
        }

        private int _commandStarted;
        ICommand _currentCommand;
        public void StartCommand(ICommand handler)
        {
            if(handler == null)
                return;

            if(_currentCommand != null)
                _currentCommand.Cancel();

            _currentCommand = handler;

            if(Interlocked.Exchange(ref _commandStarted, 1) != 0)
                return;

            Unsubscribe();
            
            handler.Completed -= handler_Completed;
            handler.Completed += handler_Completed;
            handler.Start(_call);
        }

        void Unsubscribe()
        {
            _init.Cancel();
            _greetingMessageTimer.Stop();

            _keypressTimeoutTimer.Elapsed -= KeypressTimeoutElapsed;
            _keypressTimeoutTimer.Stop();

            _call.CallStateChanged -= call_CallStateChanged;
            _call.DtmfReceived -= DtmfReceived;
            _greetingMessageTimer.Elapsed -= greetingMessageTimer_Elapsed;
        }

        void Onsubscribe()
        {
            Unsubscribe();

            _commandStarted = 0;
            _dtmfChain = null;
            _dtmfPressed = false;

            _keypressTimeoutTimer.Elapsed += KeypressTimeoutElapsed;
            _keypressTimeoutTimer.Start();

            _call.CallStateChanged += call_CallStateChanged;
            _call.DtmfReceived += DtmfReceived;
            _greetingMessageTimer.Elapsed += greetingMessageTimer_Elapsed;
            _greetingMessageTimer.Start();
        }

        void handler_Completed(object sender, EventArgs e)
        {
            Onsubscribe();

            _init.Start(_call);
        }

        void InitKeypressTimeoutTimer()
        {
            _keypressTimeoutTimer = new Timer(2500);
            _keypressTimeoutTimer.AutoReset = true;
            _keypressTimeoutTimer.Elapsed += KeypressTimeoutElapsed;
            _keypressTimeoutTimer.Start();
        }

        void KeypressTimeoutElapsed(object sender, ElapsedEventArgs e)
        {
            if (!_dtmfPressed)
            {
                call_DtmfReceived(sender, _dtmfChain);
                _dtmfChain = null;
            }

            _dtmfPressed = false;
        }

        string CreateHttpRequest(string url, int dtmf, string phoneNumber)
        {

//            try
//            {
                // Create a request using a URL that can receive a post.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            // Set the Method property of the request to POST.
            //                request.Method = "PUT";
            request.Method = "POST";
                // Create POST data and convert it to a byte array.
                string postDtmf = dtmf.ToString();
                string callerInfo = phoneNumber;
                string postData = postDtmf + " " + callerInfo;
                Console.WriteLine(postData);
                //            postData = "6544 1001";
                byte[] byteArray = Encoding.UTF8.GetBytes(postData);

                // Set the ContentType and ContentLength property of the WebRequest.
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = byteArray.Length;

                // Get the request stream and write the data in it.
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);

                // Close the Stream object.
                dataStream.Close();

                // Get the response.
                WebResponse response = request.GetResponse();

                // Display the status.
                //Console.WriteLine("HttpWebResponse StatusDescription: " + ((HttpWebResponse)response).StatusDescription);

                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                //Console.WriteLine(responseFromServer);

                // Clean up the streams.
                reader.Close();
                dataStream.Close();
                response.Close();
                return responseFromServer;
/*            }
            catch (WebException webex){
                WebResponse errResp = webex.Response;
                using (Stream respStream = errResp.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(respStream);
                    string text = reader.ReadToEnd();

                    Console.WriteLine(text);
                        
                }
                Environment.Exit(0);

            }

            return null;*/
        }

    }
}
