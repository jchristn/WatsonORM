using System;
using System.Collections.Generic;
using System.Text;
using Watson.ORM.Core;
 
namespace Test.NoColumnNames
{
    [Table("person")]
    public class Person
    {
        [Column(true, DataTypes.Int, false)]
        public int Id { get; set; } = 0;

        [Column(DataTypes.Nvarchar, 64, false)]
        public string FirstName { get; set; } = null;

        [Column(DataTypes.Nvarchar, 64, false)]
        public string LastName { get; set; } = null;

        [Column(DataTypes.DateTime, false)]
        public DateTime Birthdate { get; set; } = DateTime.Now.ToUniversalTime();

        [Column(DataTypes.DateTime, true)]
        public DateTime? NullableBirthdate { get; set; } = null;

        [Column(DataTypes.DateTimeOffset, false)]
        public DateTimeOffset LocalTime { get; set; } = new DateTimeOffset(DateTime.Now.ToUniversalTime());

        [Column(DataTypes.DateTimeOffset, true)]
        public DateTimeOffset? NullableLocalTime { get; set; } = null;

        [Column(DataTypes.Int, false)]
        public int Age { get; set; } = 42;

        [Column(DataTypes.Int, true)]
        public int? NullableAge { get; set; } = null;

        [Column(DataTypes.Nvarchar, 256, true)]
        public string Notes { get; set; } = null;

        [Column(DataTypes.Nvarchar, 8, false)]
        public PersonType Type { get; set; } = PersonType.Human;

        [Column(DataTypes.Nvarchar, 8, true)]
        public PersonType? NullableType { get; set; } = null;

        [Column(DataTypes.Boolean, false)]
        public bool IsHandsome { get; set; } = true;

        [Column(DataTypes.Blob, true)]
        public byte[] Picture { get; set; } = null;

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
                "   Picture     : " + (Picture != null ? Picture.Length + " bytes" : "(null)");
        } 
    }
}
