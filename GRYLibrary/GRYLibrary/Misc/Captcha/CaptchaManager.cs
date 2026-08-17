using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GUtilities = GRYLibrary.Core.Misc.Utilities;

namespace GRYLibrary.Core.Misc.Captcha
{
    public class CaptchaManager : ICaptchaManager
    {
        private readonly ConcurrentDictionary<string, CaptchaInstance> _Captchas = new ConcurrentDictionary<string, CaptchaInstance>();
        private readonly ConcurrentDictionary<string, DateTimeOffset> _AccessKeys = new ConcurrentDictionary<string, DateTimeOffset>();

        /// <summary>The minimal duration between two runs of <see cref="RemoveExpiredEntries"/>.</summary>
        private readonly TimeSpan _MinimalDurationBetweenTwoCleanups;
        private readonly object _CleanupLock = new object();
        private DateTimeOffset _MomentOfLastCleanup = DateTimeOffset.MinValue;

        public CaptchaManager() : this(TimeSpan.FromMinutes(1))
        {
        }

        /// <remarks>The duration is only variable so that the cleanup can be verified without waiting for it.</remarks>
        internal CaptchaManager(TimeSpan minimalDurationBetweenTwoCleanups)
        {
            this._MinimalDurationBetweenTwoCleanups = minimalDurationBetweenTwoCleanups;
        }

        /// <remarks>Only exists so that the cleanup can be verified.</remarks>
        internal int AmountOfStoredCaptchas => this._Captchas.Count;

        /// <remarks>Only exists so that the cleanup can be verified.</remarks>
        internal int AmountOfStoredAccessKeys => this._AccessKeys.Count;

        public CaptchaInstance GetNewCaptcha(CaptchaGenerationSettings settings)
        {
            this.RemoveExpiredEntries();
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
            this.RemoveExpiredEntries();
            // A captcha is consumed by the attempt to solve it, regardless of whether the attempt succeeds. Keeping it
            // would allow to replay a solved pair of id and answer until the captcha expires, which removes the
            // automation-barrier the captcha exists for, and it would allow to guess the answer by repeating the
            // attempt with the same id. A further attempt therefore requires a new captcha.
            if (this._Captchas.TryRemove(captchaId, out CaptchaInstance captcha))
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
            this.RemoveExpiredEntries();
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
                            this._AccessKeys.TryRemove(accessToken, out _);
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

        /// <summary>Removes the captchas and access-keys which are expired.</summary>
        /// <remarks>
        /// Without this both collections would only grow: a captcha which is generated but never solved is not removed
        /// anywhere else, and every visitor of a page which shows a captcha creates one. That is a slow
        /// memory-exhaustion which any anonymous client can drive.
        /// This happens on access instead of in a background-service so that this object does not need a lifecycle
        /// (start, stop, disposal) which every user of it would have to handle. It is throttled because an anonymous
        /// client can trigger it with every request, and iterating both collections on every request would make the
        /// cost of a request grow with the amount of the stored items.
        /// </remarks>
        private void RemoveExpiredEntries()
        {
            DateTimeOffset now = GetCurrentTime();
            lock (this._CleanupLock)
            {
                if (now - this._MomentOfLastCleanup < this._MinimalDurationBetweenTwoCleanups)
                {
                    return;
                }
                this._MomentOfLastCleanup = now;
            }
            foreach (KeyValuePair<string, CaptchaInstance> captcha in this._Captchas)
            {
                if (captcha.Value.ValidUntil <= now)
                {
                    this._Captchas.TryRemove(captcha.Key, out _);
                }
            }
            foreach (KeyValuePair<string, DateTimeOffset> accessKey in this._AccessKeys)
            {
                if (accessKey.Value <= now)
                {
                    this._AccessKeys.TryRemove(accessKey.Key, out _);
                }
            }
        }
    }
}
