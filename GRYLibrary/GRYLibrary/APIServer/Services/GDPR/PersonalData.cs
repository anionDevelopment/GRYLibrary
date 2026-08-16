using GRYLibrary.Core.Misc;

namespace GRYLibrary.Core.APIServer.Services.GDPR
{
    public record PersonalData
    {
        public string PersonIdentifier { get; set; }
        public string InformationName { get; set; }
        public string InformationValue { get; set; }
        public GRYDateTime InformationCollectionDateTime { get; set; }
        public string DataCollectionReason { get; set; }
        public GRYDateTime EarliestPossibleDeleteDateTime { get; set; }
        public string ReasonForEarliestPossibleDeleteDateTime { get; set; }

    }
}
