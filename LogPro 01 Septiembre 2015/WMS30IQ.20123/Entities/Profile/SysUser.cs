// File:    Users.cs
// Author:  jairomurillo
// Created: miércoles, 27 de agosto de 2008 15:05:17
// Purpose: Definition of Class Users

using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using Entities.Master;
using Entities.Trace;

namespace Entities.Profile
{
    [DataContract(Namespace = "Entities.Profile", IsReference = true)] //)] //, IsReference=true)]
    public class SysUser : Auditing
    {
        [DataMember]
        public virtual Int32 UserID { get; set; }
        [DataMember]
        public virtual String UserName { get; set; }
        [DataMember]
        public virtual String Password { get; set; }
        [DataMember]
        public virtual String Domain { get; set; }
        [DataMember]
        public virtual String FirstName { get; set; }
        [DataMember]
        public virtual String LastName { get; set; }
        [DataMember]
        public virtual String Phone { get; set; }
        [DataMember]
        public virtual String Email { get; set; }
        [DataMember]
        public virtual Boolean? IsSuperUser { get; set; }
        [DataMember]
        public virtual Boolean? IsMultiCompany { get; set; }
        [DataMember]
        public virtual String LastSession { get; set; }
        [DataMember]
        public virtual DateTime? LastLogin { get; set; }
        [DataMember]
        public virtual String DecryptPass { get; set; }

        [DataMember]
        public virtual String FullDesc
        {
            get { return this.UserName + ", " + this.FirstName + " " + (string.IsNullOrEmpty(LastName) ? "" : LastName) ; }
            set { }
        }


        [DataMember]
        public virtual IList<UserByRol> UserRols { get; set; }


        public override Boolean Equals(object obj)
        {
            if ((obj == null) || (obj.GetType() != this.GetType())) return false;
            SysUser castObj = (SysUser)obj;
            return (castObj != null) &&
                (this.UserID == castObj.UserID);
        }

        public override Int32 GetHashCode()
        {
            return 9 * 3 * this.UserID.GetHashCode();
        }
    }
}
