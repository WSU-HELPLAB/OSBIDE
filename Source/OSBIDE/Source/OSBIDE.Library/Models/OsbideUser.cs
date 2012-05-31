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

        private string _firstName = "";

        [Required]
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

        private string _lastName = "";

        [Required]
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

        public int OsbleId { get; set; }

        /// <summary>
        /// Returns the User's full name in "Last, First" format.
        /// </summary>
        [NotMapped]
        public string FullName
        {
            get
            {
                return string.Format("{0}, {1}", LastName, FirstName);
            }
        }

        public static OsbideUser GenericUser()
        {
            return new OsbideUser()
            {
                FirstName = "Ann",
                LastName = "Onymous",
                InstitutionId = "0000",
                OsbleId = 0
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
            OsbleId = copyUser.OsbleId;
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

            //reset current user property after a save
            _currentUser = null;
        }

        private static OsbideUser _currentUser;
        public static OsbideUser CurrentUser
        {
            get
            {
                if (_currentUser == null)
                {
                    _currentUser = ReadUserFromFile(StringConstants.UserDataPath);
                }
                return _currentUser;
            }
        }
    }
}
