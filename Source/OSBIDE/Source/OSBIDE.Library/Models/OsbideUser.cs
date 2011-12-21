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
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string InstitutionId { get; set; }

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
