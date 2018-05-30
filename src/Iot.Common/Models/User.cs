using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace Iot.Common
{ 
    [DataContract]
    public class User
    {
        static User() { EntityRegistry.RegisterEntity(Names.EntitiesDictionaryName, "user", new User().GetType()); }

        public User()
        {
            this.Id = FnvHash.GetUniqueId();
        }

        public User(string firstName, string lastName, string username, string password = null )
        {
            this.Id = FnvHash.GetUniqueId();
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Username = username;
            this.Password = password;
        }

        [DataMember]
        public string   Id { get; set; }
        [DataMember]
        public string   FirstName { get; set; }
        [DataMember]
        public string   LastName { get; set; }
        [DataMember]
        public string   Username { get; set; }
        [DataMember]
        public bool     PasswordCreated { get; set; }
        [DataMember]
        public string   Password { get; set; }
        public byte[]   PasswordHash { get; set; }
        public byte[]   PasswordSalt { get; set; }
    }
}
