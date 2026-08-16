using GRYLibrary.Core.APIServer.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;

namespace GRYLibrary.Core.APIServer.CommonAuthenticationTypes
{
    [PrimaryKey(nameof(Value))]
    public class AccessToken
    {
        public string OwnerUserId { get; set; }
        public string Value { get; set; }
        public DateTimeOffset ExpiredMoment { get; set; }
        public bool IsValid(ITimeService timeService)
        {
            return timeService.GetCurrentLocalTimeAsDateTimeOffset().ToUniversalTime() < this.ExpiredMoment.ToUniversalTime();
        }
    }
}
