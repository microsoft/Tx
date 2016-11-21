namespace Tx.Network.Snmp
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Class to represent ObjectIdentifier Asn.1 type.
    /// </summary>
    public struct ObjectIdentifier : IComparable<ObjectIdentifier>
    {
        /// <summary>
        /// The oid array of readonly uint
        /// </summary>
        public readonly ReadOnlyCollection<uint> Oids;

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            if (this.Oids == null)
            {
                return string.Empty;
            }

            return string.Join(".", this.Oids);
        }

        /// <summary>
        /// Determines whether parameters is suboid.
        /// </summary>
        /// <param name="otherOid">The oid to be compared.</param>
        /// <returns>boolean value true otherOid is suboid of this ObjectIdentifier</returns>
        public bool IsSubOid(ObjectIdentifier otherOid)
        {
            if (this.Oids == null && otherOid.Oids == null)
            {
                return true;
            }

            if (this.Oids == null && otherOid.Oids != null)
            {
                return false;
            }

            if (this.Oids != null && otherOid.Oids == null)
            {
                return false;
            }

            if (otherOid.Oids.Count > this.Oids.Count)
            {
                return false;
            }

            for (int i = 0; i < otherOid.Oids.Count; i++)
            {
                if(otherOid.Oids[i] != this.Oids[i])
                {
                    return false;
                }
            }
           
            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifier"/> struct.
        /// </summary>
        /// <param name="oids">The oids.</param>
        internal ObjectIdentifier(uint[] oids)
        {
            this.Oids = new ReadOnlyCollection<uint>(oids);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifier"/> struct.
        /// </summary>
        /// <param name="oids">The list oids.</param>
        public ObjectIdentifier(IList<uint> oids)
        {
            if (oids == null || oids.Count == 0)
            {
                throw new ArgumentNullException("oids");
            }

            uint[] newoids = new uint[oids.Count];
            oids.CopyTo(newoids, 0);
            this.Oids = new ReadOnlyCollection<uint>(newoids);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectIdentifier"/> struct.
        /// </summary>
        /// <param name="oids">The oids.</param>
        public ObjectIdentifier(string oids)
        {
            if(string.IsNullOrWhiteSpace(oids))
            {
                throw new ArgumentNullException("oids");
            }

            var oidArray = new uint[25];
            int count = 0;
            uint val = 0;
            for (int i = 0; i < oids.Length; i++)
            {
                if (oids[i] != '.')
                {
                    uint currentVal = oids[i] - 48u;
                    if (currentVal > 9)
                    {
                        throw new InvalidCastException("Input not an ObjectIdentifier string");
                    }

                    val = (val * 10) + currentVal;
                }
                else
                {
                    if (oidArray.Length <= count)
                    {
                        Array.Resize(ref oidArray, oidArray.Length * 2);
                    }

                    oidArray[count++] = val;
                    val = 0;
                }
            }

            Array.Resize(ref oidArray, count + 1);
            oidArray[count] = val;

            this.Oids = new ReadOnlyCollection<uint>(oidArray);
        }

        /// <summary>
        /// Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other" /> parameter.Zero This object is equal to <paramref name="other" />. Greater than zero This object is greater than <paramref name="other" />.
        /// </returns>
        public int CompareTo(ObjectIdentifier other)
        {
            if(other.Oids ==null && this.Oids == null)
            {
                return 0;
            }

            if (other.Oids == null )
            {
                return 1;
            }

            if (this.Oids == null)
            {
                return -1;
            }

            int length = other.Oids.Count - this.Oids.Count;
            if(length != 0)
            {
                return length;
            }

            for(int i = 0; i < this.Oids.Count; i++)
            {
                if (other.Oids[i] != this.Oids[i])
                {
                    return (other.Oids[i] > this.Oids[i]) ? 
                        (int)(other.Oids[i] - this.Oids[i]) : 
                        -1 * (int)(this.Oids[i] - other.Oids[i]);
                }
            }

            return 0;
        }

        /// <summary>
        /// Chaecks for equality of current object with another.
        /// </summary>
        /// <param name="anotherObj">Another object.</param>
        /// <returns>Boolean true is equal</returns>
        public override bool Equals(object anotherObj)
        {
            if (typeof(ObjectIdentifier) != anotherObj.GetType())
            {
                return false;
            }
            ObjectIdentifier otherObj = (ObjectIdentifier)anotherObj;
            if (this.Oids == null && otherObj.Oids == null)
            {
                return true;
            }

            if (this.Oids == null && otherObj.Oids != null)
            {
                return false;
            }

            if (this.Oids != null && otherObj.Oids == null)
            {
                return false;
            }

            for (int i = 0; i < this.Oids.Count; i++)
            {
                int length = (int)(otherObj.Oids[i] - this.Oids[i]);
                if (length != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            int val = 0;
            int j = 0;
            int[] primes = new int[] { 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97 };
            for (int i = 0; i < this.Oids.Count; i++)
            {
                if(j == 24)
                {
                    j = 0;
                }

                val += ((int)this.Oids[i] * primes[j++]);
            }

            return val;
        }
    }
}
