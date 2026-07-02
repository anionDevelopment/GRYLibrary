using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace GRYLibrary.Core.APIServer.CommonDBTypes
{
    [PrimaryKey(nameof(Id))]
    public class Role : IEquatable<Role>
    {
        public string Id { get; set; }
        public string Name { get; set; } = null;
        public HashSet<Role> DirectlyInheritedRoles { get; set; } = new HashSet<Role>();
        public ISet<Role> GetAllInheritedRoles()
        {
            ISet<Role> result = new HashSet<Role>();
            foreach (Role inheritedRole in this.DirectlyInheritedRoles)
            {
                result.Add(inheritedRole);
                result.UnionWith(inheritedRole.GetAllInheritedRoles());
            }
            return result;
        }
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Role);
        }

        public bool Equals(Role other)
        {
            return other is not null &&
                   this.Id == other.Id;
        }

        public override int GetHashCode()
        {
            HashCode hash = new HashCode();
            hash.Add(this.Id);
            return hash.ToHashCode();
        }
    }
}
