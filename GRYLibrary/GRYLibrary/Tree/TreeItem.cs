
namespace GRYLibrary.Core.Tree
{
    public abstract class TreeItem<ContainedItemType>
    {
        public abstract void Accept(ITreeItemVisitor visitor);
        public abstract T Accept<T>(ITreeItemVisitor<T> visitor);
    }
    public interface ITreeItemVisitor
    {
        void Handle<ContainedItemType>(Containee<ContainedItemType> containee);
        void Handle<ContainedItemType>(Container<ContainedItemType> containee);
    }
    public interface ITreeItemVisitor<T>
    {
        T Handle<ContainedItemType>(Containee<ContainedItemType> containee);
        T Handle<ContainedItemType>(Container<ContainedItemType> containee);
    }
}