using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class OsbideUser
    {
        [Key]
        public int Id { get; set; }

        [Required]
        private string _firstName = "";
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value.Trim();
            }
        }

        [Required]
        private string _lastName = "";
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value.Trim();
            }
        }

        [Required]
        private string _institutionId = "";
        public string InstitutionId
        {
            get
            {
                return _institutionId;
            }
            set
            {
                _institutionId = value.Trim();
            }
        }

        public static OsbideUser GenericUser()
        {
            return new OsbideUser()
            {
                FirstName = "Ann",
                LastName = "Onymous",
                InstitutionId = "0000"
            };
        }

        public OsbideUser()
        {
        }

        public OsbideUser(OsbideUser copyUser)
        {
            Id = copyUser.Id;
            FirstName = copyUser.FirstName;
            LastName = copyUser.LastName;
            InstitutionId = copyUser.InstitutionId;
        }

        public static OsbideUser ReadUserFromFile(string filePath)
        {
            OsbideUser savedUser = new OsbideUser();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new OsbideDeserializationBinder();

            if (File.Exists(filePath))
            {
                FileStream data = File.Open(filePath, FileMode.Open);
                try
                {
                    savedUser = (OsbideUser)formatter.Deserialize(data);
                }
                catch (Exception ex)
                {
                }
                data.Close();
            }
            return savedUser;
        }

        public static void SaveUserToFile(OsbideUser user, string filePath)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream file = File.Open(filePath, FileMode.Create);
            formatter.Serialize(file, user);
            file.Close();
        }

    }
}
