using Ozeki.VoIP;
using System;
using Ozeki.Media;

namespace _08_DTMF_Authentication
{
    class PlayCommand : ICommand
    {
        MP3StreamPlayback _mp3Player;
        PhoneCallAudioSender _phoneCallAudioSender = new PhoneCallAudioSender();
        MediaConnector _mediaConnector = new MediaConnector();
        ICall _call;
        string _path;

        public event EventHandler Completed;
       
        public PlayCommand(string path)
        {
            _path = path; 
        }

        public void Start(ICall call)
        {
            Cancel();
            _call = call;
            _phoneCallAudioSender.AttachToCall(call);
            MP3ToSpeaker(_path);
        }

        public void Cancel()
        {
            DisposeMediaConnection();
        }

        void MP3ToSpeaker(string path)
        {
            DisposeMediaConnection();
            _mediaConnector = new MediaConnector();

            _mp3Player = new MP3StreamPlayback(path);
            _mp3Player.Stopped += mp3Player_Stopped;
           
            _mediaConnector.Connect(_mp3Player, _phoneCallAudioSender);
            _mp3Player.Start();
        }

        void mp3Player_Stopped(object sender, EventArgs e)
        {
            DisposeMediaConnection();

            var handler = Completed;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }
        
        void DisposeMediaConnection()
        {
            _mediaConnector.Dispose();

            if (_mp3Player != null)
            {
                _mp3Player.Stopped -= mp3Player_Stopped;
                _mp3Player.Dispose();
            }
        }
    }
}
