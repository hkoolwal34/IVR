using Ozeki.VoIP;
using System;
using Ozeki.Media;
using System.Threading;
using System.Speech;
using System.Speech.Synthesis;


namespace _08_DTMF_Authentication
{
    class SpeakCommand : ICommand
    {
        //        GoogleTTS googleAPI;
        GoogleTTSSelf googleAPI;

        TextToSpeech _tts;
        PhoneCallAudioSender _phoneCallAudioSender;
        MediaConnector _mediaConnector;
        ICall _call;
        string _text;
        bool _isStarted;

        public event EventHandler Completed;
        

        public SpeakCommand(string text)
        {
            _text = text;
        }

        public void Start(ICall call)
        {
            Cancel();
            _isStarted = true;
            _call = call;
            _phoneCallAudioSender = new PhoneCallAudioSender();
            _mediaConnector = new MediaConnector();
            _phoneCallAudioSender.AttachToCall(call);
            TextToSpeech(_text);
        }

        public void Cancel()
        {
            Cleanup();
        }
/*
        void Cleanup()
        {
            if (_tts != null)
            {
                _mediaConnector.Disconnect(_tts, _phoneCallAudioSender);
                _tts.Stopped -= tts_Stopped;
                _tts.Dispose();
                _tts = null;
            }
        }
        */
        void Cleanup()
        {
            if (googleAPI != null)
            {
                _mediaConnector.Disconnect(googleAPI, _phoneCallAudioSender);
                googleAPI.Stopped -= googleAPI_Stopped;
                googleAPI.Dispose();
                googleAPI = null;
            }
        }

        void TextToSpeech(string p)
        {
/*            _tts = new TextToSpeech();
/*                        _tts.AddTTSEngine(new MSSpeechPlatformTTS());
                        _tts.ChangeLanguage("en-IN");
                        _tts.ChangeVoice("Heera");

                        SpeechSynthesizer synthesizer = new SpeechSynthesizer();
                        foreach (InstalledVoice voice in            synthesizer.GetInstalledVoices())
                        {
                            System.Speech.Synthesis.VoiceInfo info = voice.VoiceInfo;
                            OutputVoiceInfo(info);
                        }
  
            
            _tts.Stopped += tts_Stopped;
            _mediaConnector.Connect(_tts, _phoneCallAudioSender);
            _tts.AddAndStartText(p);
            */
          
            googleAPI = new GoogleTTSSelf(GoogleLanguage.Hindi);
            googleAPI.Stopped += googleAPI_Stopped;

            _phoneCallAudioSender.AttachToCall(_call);
            _mediaConnector.Connect(googleAPI, _phoneCallAudioSender);

            googleAPI.AddAndStartText(p);


        }
        void googleAPI_Stopped(object sender, EventArgs e)
        {
            if (!_isStarted)
                return;
            _isStarted = false;

            Cleanup();

            var handler = Completed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        /*
        private void OutputVoiceInfo(System.Speech.Synthesis.VoiceInfo info)
        {
            Console.WriteLine("  Name: {0}, culture: {1}, gender: {2}, age: {3}.",
              info.Name, info.Culture, info.Gender, info.Age);
            Console.WriteLine("    Description: {0}", info.Description);
        }
        */

        void tts_Stopped(object sender, EventArgs e)
        {
            if (!_isStarted)
                return;
            _isStarted = false;
            
            Cleanup();

            var handler = Completed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
    }


}
