using System.Collections.Generic;

namespace GRYLibrary.Core.Tree
{
    public class Container<ContainedItemType> : TreeItem<ContainedItemType>
    {
        public List<TreeItem<ContainedItemType>> Children { get; set; }
        public Container()
        {
            this.Children = [];
        }
        public IList<TreeItem<ContainedItemType>> GetDirectChildren()
        {
            return new List<TreeItem<ContainedItemType>>(this.Children);
        }

        public IEnumerable<TreeItem<ContainedItemType>> GetDirectAndTransitiveChildren()
        {
            List<TreeItem<ContainedItemType>> result = [.. this.Children];
            foreach (TreeItem<ContainedItemType> child in this.Children)
            {
                if (child is Container<ContainedItemType> container)
                {
                    result.AddRange(container.GetDirectAndTransitiveChildren());
                }
            }
            return result;
        }
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