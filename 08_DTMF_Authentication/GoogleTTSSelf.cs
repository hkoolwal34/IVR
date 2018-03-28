using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Ozeki.VoIP;
using System;
using Ozeki.Media;
using System.Threading;
using System.Speech;
using System.Speech.Synthesis;


namespace _08_DTMF_Authentication
{
    class GoogleTTSSelf : GoogleTTS
    {
        public GoogleTTSSelf()
        {
        }

        public GoogleTTSSelf(string languageCode)
        {
            LanguageCode = languageCode;
        }
        public GoogleTTSSelf(GoogleLanguage language)
        {
            Language = language;
        }



        public event EventHandler<EventArgs> Stopped;
    }
}
