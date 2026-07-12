using Microsoft.EntityFrameworkCore;
using System;

namespace GRYLibrary.Core.APIServer.CommonAuthenticationTypes
{
    [PrimaryKey(nameof(Value))]
    public class RefreshToken
    {
        public string Value { get; set; }
        public DateTime ExpiredMoment { get; set; }
    }
}
