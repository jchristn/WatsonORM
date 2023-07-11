using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM.Core;
 
namespace Test.Async
{
    [Table("person")]
    public class Person
    {
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; } = 0;

        [Column("firstname", false, DataTypes.Nvarchar, 64, false)]
        public string FirstName { get; set; } = null;

        [Column("lastname", false, DataTypes.Nvarchar, 64, false)]
        public string LastName { get; set; } = null;

        [Column("birthdate", false, DataTypes.DateTime, false)]
        public DateTime Birthdate { get; set; } = DateTime.Now.ToUniversalTime();

        [Column("nullablebirthdate", false, DataTypes.DateTime, true)]
        public DateTime? NullableBirthdate { get; set; } = null;

        [Column("localtime", false, DataTypes.DateTimeOffset, false)]
        public DateTimeOffset LocalTime { get; set; } = new DateTimeOffset(DateTime.Now.ToUniversalTime());

        [Column("nullablelocaltime", false, DataTypes.DateTimeOffset, true)]
        public DateTimeOffset? NullableLocalTime { get; set; } = null;

        [Column("age", false, DataTypes.Int, false)]
        public int Age { get; set; } = 42;

        [Column("nullableage", false, DataTypes.Int, true)]
        public int? NullableAge { get; set; } = null;

        [Column("notes", false, DataTypes.Nvarchar, 256, true)]
        public string Notes { get; set; } = null;

        [Column("persontype", false, DataTypes.Nvarchar, 8, false)]
        public PersonType Type { get; set; } = PersonType.Human;

        [Column("nullablepersontype", false, DataTypes.Nvarchar, 8, true)]
        public PersonType? NullableType { get; set; } = null;

        [Column("ishandsome", false, DataTypes.Boolean, false)]
        public bool IsHandsome { get; set; } = true;

        [Column("picture", false, DataTypes.Blob, true)]
        public byte[] Picture { get; set; } = null;

        [Column("guid", false, DataTypes.Guid, 36, true)]
        public Guid GUID { get; set; } = Guid.NewGuid();

        public Person()
        {

        }

        public Person(
            string first,
            string last,
            DateTime birthDate,
            DateTime? nullableBirthDate,
            DateTimeOffset localTime,
            DateTimeOffset? nullableLocalTime,
            int age,
            int? nullableAge,
            string notes,
            PersonType personType,
            PersonType? nullablePersonType,
            bool isHandsome,
            byte[] picture)
        {
            FirstName = first;
            LastName = last;
            Birthdate = birthDate;
            NullableBirthdate = nullableBirthDate;
            LocalTime = localTime;
            NullableLocalTime = nullableLocalTime;
            Age = age;
            NullableAge = nullableAge;
            Notes = notes;
            Type = personType;
            NullableType = nullablePersonType;
            IsHandsome = isHandsome;
            Picture = picture;
        }

        public override string ToString()
        {
            return
                "---" + Environment.NewLine +
                "ID " + Id + Environment.NewLine +
                "   Name        : " + FirstName + " " + LastName + Environment.NewLine +
                "   Birthdate   : " + Birthdate.ToString() + " nullable " + (NullableBirthdate != null ? NullableBirthdate.ToString() : "(null)") + Environment.NewLine +
                "   Local time  : " + LocalTime.ToString() + " nullable " + (NullableLocalTime != null ? NullableLocalTime.ToString() : "(null)") + Environment.NewLine +
                "   Age         : " + Age + " nullable " + (NullableAge != null ? NullableAge.ToString() : "(null)") + Environment.NewLine +
                "   Type        : " + Type.ToString() + " nullable " + NullableType + Environment.NewLine +
                "   Notes       : " + Notes + Environment.NewLine +
                "   Handsome    : " + IsHandsome + Environment.NewLine +
                "   Picture     : " + (Picture != null ? Picture.Length + " bytes" : "(null)") + Environment.NewLine +
                "   GUID        : " + GUID.ToString();
        }
    }
}
