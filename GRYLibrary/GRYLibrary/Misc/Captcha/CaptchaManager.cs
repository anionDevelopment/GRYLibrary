using System;
using System.Collections.Concurrent;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc.Captcha
{
    public class CaptchaManager : ICaptchaManager
    {
        private readonly ConcurrentDictionary<string, CaptchaInstance> _Captchas = new ConcurrentDictionary<string, CaptchaInstance>();
        private readonly ConcurrentDictionary<string, DateTimeOffset> _AccessKeys = new ConcurrentDictionary<string, DateTimeOffset>();

        public CaptchaManager()
        {
            //TODO start cleanup service to remove expired things
        }
        public CaptchaInstance GetNewCaptcha(CaptchaGenerationSettings settings)
        {
            CaptchaInstance result = new CaptchaInstance(settings);
            if (this._Captchas.TryAdd(result.Id, result))
            {
                return result;
            }
            else
            {
                throw new NotImplementedException();
            }
        }
        internal static DateTimeOffset GetCurrentTime()
        {
            return GUtilities.GetNow();
        }

        public bool TrySolve(string captchaId, string userInput, out string accessKey, out string failMessage)
        {
            if (this._Captchas.TryGetValue(captchaId, out CaptchaInstance captcha))
            {
                if (captcha.ExpectedUserInput == userInput)
                {
                    DateTimeOffset now = GetCurrentTime();
                    if (now < captcha.ValidUntil)
                    {
                        failMessage = null;
                        accessKey = Guid.NewGuid().ToString();
                        if (this._AccessKeys.TryAdd(accessKey, captcha.AccessTokenValidUntil))
                        {
                            return true;
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }
                    }
                    else
                    {
                        failMessage = "Captcha expired";
                    }
                }
                else
                {
                    failMessage = "Wrong captcha-text";
                }
            }
            else
            {
                failMessage = "Unknown cpatcha";
            }
            accessKey = null;
            return false;
        }

        public bool UserHasAlreadySolvedTheCaptcha(string accessToken, out string failMessage)
        {
            failMessage = null;
            if (accessToken is null)
            {
                failMessage = "No accesstoken provided";
            }
            else
            {
                if (this._AccessKeys.ContainsKey(accessToken))
                {
                    if (this._AccessKeys.TryGetValue(accessToken, out DateTimeOffset validUntil))
                    {
                        DateTimeOffset now = GetCurrentTime();
                        if (now < validUntil)
                        {
                            return true;
                        }
                        else
                        {
                            failMessage = "Accesstoken expired";
                        }
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else
                {
                    failMessage = "Unknown accesstoken";
                }
            }
            return false;
        }
    }
}
