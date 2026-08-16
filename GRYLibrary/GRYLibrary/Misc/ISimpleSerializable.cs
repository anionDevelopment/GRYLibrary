namespace GRYLibrary.Core.Misc
{
    public interface ISimpleSerializable
    {
        public void DeserializeFromString(string content);
        public string SerializeToString();
    }
}
