namespace GRYLibrary.Core.Tree
{
    public class Containee<ContainedItemType> : TreeItem<ContainedItemType>
    {
        public Containee()
        {
            this.Item = default;
        }
        public Containee(ContainedItemType item)
        {
            this.Item = item;
        }
        public ContainedItemType Item { get; set; }
        public override void Accept(ITreeItemVisitor visitor)
        {
            visitor.Handle(this);
        }

        public override T Accept<T>(ITreeItemVisitor<T> visitor)
        {
            return visitor.Handle(this);
        }
    }
}