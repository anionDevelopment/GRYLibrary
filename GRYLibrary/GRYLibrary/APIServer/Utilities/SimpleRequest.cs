namespace GRYLibrary.Core.APIServer.Utilities
{
    public class SimpleRequest
    {
        public string HTTPMethod { get; set; }
        public string Route { get; set; }
        public string Query { get; set; }
        public byte[] Body { get; set; }
    }
}
