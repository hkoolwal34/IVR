using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ozeki.Media;
using Ozeki.VoIP;

namespace _08_DTMF_Authentication
{
    class RecordCall
    {
        static ICall call;

        static string filename;
        static string caller;
        static Microphone microphone;
        static Speaker speaker;
        static PhoneCallAudioSender mediaSender;
        static PhoneCallAudioReceiver mediaReceiver;
        static MediaConnector connector;
        static WaveStreamRecorder recorder;

        public static void Init(PhoneCallAudioSender sender,MediaConnector mediaconnector)
        {
            microphone = Microphone.GetDefaultDevice();
            speaker = Speaker.GetDefaultDevice();

            mediaSender = sender;
            connector = mediaconnector;

            mediaReceiver = new PhoneCallAudioReceiver();
     
        }

        public static void SetupDevices()
        {
            microphone.Start();
            connector.Connect(microphone, mediaSender);

            speaker.Start();
            connector.Connect(mediaReceiver, speaker);

            filename = string.Format("{0}-{1}-{2}-{3}.wav",caller,DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
            recorder = new WaveStreamRecorder(filename);

            connector.Connect(microphone, recorder);
            connector.Connect(mediaReceiver, recorder);

            mediaSender.AttachToCall(call);
            mediaReceiver.AttachToCall(call);

            recorder.Start();

            
        }

        public static void CloseDevices()
        {
            recorder.Dispose();
            microphone.Dispose();
            speaker.Dispose();
            try {
                mediaReceiver.Detach();
                mediaSender.Detach();
                connector.Dispose();
            }
            catch(Exception e)
            {
            }
        }


    }
}
