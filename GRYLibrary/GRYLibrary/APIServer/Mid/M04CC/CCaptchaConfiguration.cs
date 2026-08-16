using GRYLibrary.Core.APIServer.MidT.Captcha;
using System;

namespace GRYLibrary.Core.APIServer.Mid.M04CC
{
    public class CCaptchaConfiguration : CaptchaConfiguration, ICCaptchaConfiguration
    {
        public ushort Length { get; set; } = 8;
        public string Alphabet { get; set; } = "ABDEFGHKLMNPQRSTUVYabdefghjkmnpqrstuvy23456789";
        public TimeSpan ExpireDurationOfCaptcha { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan ExpireDurationOfAccessToken { get; set; } = TimeSpan.FromHours(25);
        public string Encoding { get; set; } = "utf-8";
        public string CaptchaCookieName { get; set; } = "Captcha";
        public string CaptchaPage { get; set; } = @$"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"">
</head>
<body>
    <style>
    </style>
    <div>
        __[message]__</br>
    <img src=""data:image/jpeg;base64, __[captchabase64]__"" />
    <form action="""">
        <button type=""submit"">↻</button>
    </form>
        <form>
            <label for=""Value""> Value:</label> <br />
            <input type=""text"" name=""__[captchavaluequerykey]__"" required> <br />
            <input type=""hidden"" name=""__[captchaidquerykey]__"" value=""__[captchaid]__"" required> <br />
            <input type=""submit"" value=""Enter"">
        </form>
    </div>
</body>
</html>";

    }
}
