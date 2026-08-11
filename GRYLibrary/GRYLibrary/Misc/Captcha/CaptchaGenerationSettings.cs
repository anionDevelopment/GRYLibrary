using System;

namespace GRYLibrary.Core.Misc.Captcha
{
    public class CaptchaGenerationSettings
    {
        public ushort Length { get; set; }
        public string Alphabet { get; set; }

        /// <remarks>
        /// The picture always gets exactly this size, independent of the characters it shows. A picture whose
        /// size depends on its content would move every element around it whenever a new captcha is generated.
        /// The default is large enough for the text of a captcha of the usual length, which is drawn centered
        /// into it.
        /// </remarks>
        public ushort PictureWidth { get; set; } = 260;
        public ushort PictureHeight { get; set; } = 80;
        public TimeSpan ExpireDurationOfCaptcha { get; set; }
        public TimeSpan ExpireDurationOfAccessToken { get; set; }
    }
}
