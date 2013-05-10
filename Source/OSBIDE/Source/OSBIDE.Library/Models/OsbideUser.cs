using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OSBIDE.Library.Models
{
    [Flags]
    public enum SystemRole : int
    {
        Student = 1,
        TA = 2,
        Instructor = 4
    }

    [Serializable]
    [DataContract]
    public class OsbideUser : INotifyPropertyChanged, IModelBuilderExtender
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private int _id;
        private string _email;
        private string _firstName = "";
        private string _lastName = "";
        private int _schoolId;
        private int _institutionId = -1;
        private byte[] _profileImage;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [Key]
        [DataMember]
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your email address.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        [DataMember]
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First Name")]
        [DataMember]
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value.Trim();
                OnPropertyChanged("FirstName");
            }
        }



        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your last name.")]
        [Display(Name = "Last Name")]
        [DataMember]
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value.Trim();
                OnPropertyChanged("LastName");
            }
        }

        /// <summary>
        /// SchoolId references the name of the school or institution that the user belongs to.  For example,
        /// if a student attended Washington State University, the SchoolId would match that of Washington State University.
        /// I know this is confusing :(
        /// </summary>
        [Display(Name = "School / Institution")]
        [Required(ErrorMessage = "Please select your school or institution.")]
        [DataMember]
        public int SchoolId
        {
            get
            {
                return _schoolId;
            }
            set
            {
                _schoolId = value;
                OnPropertyChanged("SchoolId");
            }
        }

        [ForeignKey("SchoolId")]
        public virtual School SchoolObj { get; set; }

        /// <summary>
        /// InstitutionId references the user's ID number at their particular institution.  For example,
        /// If a student attended WSU and their school ID was 123456, their InstitutionId would be "123456."  
        /// I know this is confusing :(
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your school or institution ID.")]
        [Display(Name = "School / Institution ID")]
        [DataMember]
        public int InstitutionId
        {
            get
            {
                return _institutionId;
            }
            set
            {
                _institutionId = value;
                OnPropertyChanged("InstitutionId");
            }
        }

        [Column(TypeName = "image")]
        [DataMember]
        public byte[] ProfileImage
        {
            get
            {
                return _profileImage;
            }
            set
            {
                _profileImage = value;
                OnPropertyChanged("ProfileImage");
            }
        }

        /// <summary>
        /// The numeric representation of the user's role within OSBIDE.  
        /// Use <see cref="Role"/> instead.
        /// </summary>
        [DataMember]
        [Required]
        public int RoleValue { get; protected set; }

        /// <summary>
        /// The user's role within the OSBIDE system
        /// </summary>
        [NotMapped]
        public SystemRole Role
        {
            get
            {
                return (SystemRole)RoleValue;
            }
            set
            {
                RoleValue = (int)value;
                OnPropertyChanged("Role");
            }
        }

        private DateTime _lastVsActivity;

        /// <summary>
        /// Tracks when the user's Visual Studio client was last active.
        /// Users can only access the web portion of OSBIDE if they have an
        /// active connection through Visual Studio.
        /// </summary>
        [DataMember]
        [Required]
        public DateTime LastVsActivity
        {
            get
            {
                return _lastVsActivity;
            }
            set
            {
                _lastVsActivity = value;
                OnPropertyChanged("LastVsActivity");
            }
        }

        

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

        /// <summary>
        /// Returns the user's full name in "First Last" format
        /// </summary>
        [NotMapped]
        public string FirstAndLastName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }

        [IgnoreDataMember]
        public virtual IList<EventLogSubscription> LogSubscriptions { get; set; }

        public virtual UserScore Score { get; set; }

        public void SetProfileImage(Bitmap bmp)
        {
            MemoryStream stream = new MemoryStream();
            bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Position = 0;
            ProfileImage = stream.ToArray();
        }

        public Bitmap GetProfileImage()
        {
            MemoryStream stream = new MemoryStream(ProfileImage);
            Bitmap bmp = new Bitmap(stream);
            return bmp;
        }

        public static OsbideUser GenericUser()
        {
            return new OsbideUser()
            {
                FirstName = "Ann",
                LastName = "Onymous",
                InstitutionId = -1,
                ProfileImage = new byte[0]
            };
        }

        public OsbideUser()
        {
            LogSubscriptions = new List<EventLogSubscription>();
            Role = SystemRole.Student;
            LastVsActivity = DateTime.Now;
        }

        public OsbideUser(OsbideUser copyUser)
            : this()
        {
            Role = copyUser.Role;
            Id = copyUser.Id;
            FirstName = copyUser.FirstName;
            LastName = copyUser.LastName;
            InstitutionId = copyUser.InstitutionId;
            ProfileImage = copyUser.ProfileImage;
            Email = copyUser.Email;
            SchoolId = copyUser.SchoolId;
            LastVsActivity = copyUser.LastVsActivity;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OsbideUser>()
                .HasRequired(u => u.SchoolObj)
                .WithMany()
                .WillCascadeOnDelete(true);
        }
    }
}
