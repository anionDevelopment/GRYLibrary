using System;

namespace GRYLibrary.Core.Misc.Captcha
{
    public class CaptchaGenerationSettings
    {
        public ushort Length { get; set; }
        public string Alphabet { get; set; }
        public TimeSpan ExpireDurationOfCaptcha { get; set; }
        public TimeSpan ExpireDurationOfAccessToken { get; set; }
    }
}
