using DNTCaptcha.Core;
using Microsoft.Extensions.Options;
using SkiaSharp;
using System;
using System.Linq;

namespace GRYLibrary.Core.Misc.Captcha
{

    public class CaptchaInstance
    {
        private static readonly Random _Random = new Random();
        public string Id { get; set; }
        public string ExpectedUserInput { get; set; }
        public byte[] PictureContent { get; set; }
        public DateTimeOffset ValidUntil { get; set; }
        public DateTimeOffset AccessTokenValidUntil { get; set; }
        public CaptchaInstance(CaptchaGenerationSettings settings)
        {
            this.Id = Guid.NewGuid().ToString();
            this.ExpectedUserInput = GetNewRandomExpectedUserInput(settings);
            this.PictureContent = GetPictureForString(this.ExpectedUserInput, settings);
            this.ValidUntil = CaptchaManager.GetCurrentTime().Add(settings.ExpireDurationOfCaptcha);
            this.AccessTokenValidUntil = CaptchaManager.GetCurrentTime().Add(settings.ExpireDurationOfAccessToken);
        }

        internal static string GetNewRandomExpectedUserInput(CaptchaGenerationSettings settings)
        {
            return new string([.. Enumerable.Repeat(settings.Alphabet, settings.Length).Select(s => s[_Random.Next(s.Length)])]);
        }

        internal static byte[] GetPictureForString(string expectedUserInput, CaptchaGenerationSettings settings)
        {
            RandomNumberProvider rng = new RandomNumberProvider();
            IOptions<DNTCaptchaOptions> options = Options.Create(new DNTCaptchaOptions());
            CaptchaImageProvider imageProvider = new CaptchaImageProvider(rng, options);
            byte[] drawnCaptcha = imageProvider.DrawCaptcha(expectedUserInput, "black", "white", 25, "Tahoma");
            return DrawOnPictureWithFixedSize(drawnCaptcha, settings.PictureWidth, settings.PictureHeight);
        }

        /// <summary>
        /// Draws <paramref name="picture"/> centered onto a white picture with the given size and returns it.
        /// </summary>
        /// <remarks>
        /// The picture which the captcha-library draws is exactly as wide as the text it contains, so its size
        /// depends on the characters which were chosen randomly. Every page which shows a captcha would
        /// therefore look slightly different with every request, which makes it impossible to compare the
        /// appearance of such a page with an expected appearance.
        /// </remarks>
        private static byte[] DrawOnPictureWithFixedSize(byte[] picture, ushort width, ushort height)
        {
            using SKBitmap drawnCaptcha = SKBitmap.Decode(picture);
            using SKBitmap pictureWithFixedSize = new SKBitmap(width, height);
            using (SKCanvas canvas = new SKCanvas(pictureWithFixedSize))
            {
                canvas.Clear(SKColors.White);
                // The drawn captcha is scaled down if it does not fit, but it is never scaled up, because
                // scaling it up would only make it blurry without making it more readable.
                float scale = Math.Min(1f, Math.Min((float)width/drawnCaptcha.Width, (float)height/drawnCaptcha.Height));
                float scaledWidth = drawnCaptcha.Width*scale;
                float scaledHeight = drawnCaptcha.Height*scale;
                canvas.DrawBitmap(drawnCaptcha, SKRect.Create((width-scaledWidth)/2, (height-scaledHeight)/2, scaledWidth, scaledHeight));
            }
            using SKImage image = SKImage.FromBitmap(pictureWithFixedSize);
            using SKData encodedPicture = image.Encode(SKEncodedImageFormat.Jpeg, 100);
            return encodedPicture.ToArray();
        }
    }
}
