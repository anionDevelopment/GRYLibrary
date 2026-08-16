namespace GRYLibrary.Tests.TestData.TestTypes.ComplexDataStructure2
{
    public class OtherType : ModelType
    {
        #region Overhead
        public override bool Equals(object @object)
        {
            return base.Equals(@object);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public override string ToString()
        {
            return base.ToString();
        }
        #endregion
    }
}