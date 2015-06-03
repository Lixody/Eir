using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PHPAnalysis.Utils;

namespace PHPAnalysis.Analysis.CFG.Taint
{
    public class StoredVulnInfo : IMergeable<StoredVulnInfo>, IEquatable<StoredVulnInfo>
    {
        public string StorageOrigin { get; set; }
        public string StorageName { get; set; }
        public TaintSets Taint { get; set; }
        public IsItInYet ICantFeelIt { get; set; }

        public bool StorageEquals(StoredVulnInfo other)
        {
            if (other == null)
                return false;

            return StorageOrigin == other.StorageOrigin && StorageName == other.StorageName;
        }

        public StoredVulnInfo()
        {
            Taint = new TaintSets().ClearTaint();
        }

        public StoredVulnInfo(string tableName, int startLine)
        {
            StorageName = tableName;
        }

        public StoredVulnInfo Merge(StoredVulnInfo other)
        {
            if (StorageName == null && other.StorageName != null)
            {
                return new StoredVulnInfo()
                {
                    StorageOrigin = other.StorageOrigin,
                    StorageName = other.StorageName,
                    Taint = this.Taint.Merge(other.Taint),
                    ICantFeelIt = other.ICantFeelIt
                };
            }
            return new StoredVulnInfo() { 
                StorageOrigin = this.StorageOrigin,
                StorageName = this.StorageName, 
                Taint = this.Taint.Merge(other.Taint),
                ICantFeelIt = this.ICantFeelIt
            };
        }

        public bool Equals(StoredVulnInfo other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return Equals(Taint, other.Taint) && string.Equals(StorageName, other.StorageName) 
                                              && string.Equals(StorageOrigin, other.StorageOrigin);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((StoredVulnInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Taint != null ? Taint.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (StorageName != null ? StorageName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public enum IsItInYet
    {
        Unknown = 0,
        YesItsGoingIn = 1,
        NoImPullingOut = 2,
    }
}
