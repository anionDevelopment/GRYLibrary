using GRYLibrary.Core.APIServer.Services.GDPR;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.Settings.Regulations.GDPR
{
    public class GDPRegulation : Regulation
    {
        public bool ServiceProcessesPersonalData { get; set; }
        public bool ServiceIsSubjectOfGDPR { get; set; }
        public override IList<Type> RequiredServices { get; set; } = [typeof(IGDPRService)];

        public GDPRegulation(bool serviceProcessesPersonalData, bool serviceIsSubjectOfGDPR)
        {
            this.ServiceProcessesPersonalData = serviceProcessesPersonalData;
            this.ServiceIsSubjectOfGDPR = serviceIsSubjectOfGDPR;
        }
        //TODO provide controller

    }
}
