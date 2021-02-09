using System;
using System.Collections.Generic;
using System.Text; 
using Watson.ORM;
using Watson.ORM.Core;
 
namespace Test.Sqlite
{
    [Table("person")]
    public class Person
    {
        [Column("id", true, DataTypes.Int, false)]
        public int Id { get; set; }

        [Column("firstname", false, DataTypes.Nvarchar, 64, false)]
        public string FirstName { get; set; }

        [Column("lastname", false, DataTypes.Nvarchar, 64, false)]
        public string LastName { get; set; }

        [Column("birthdate", false, DataTypes.DateTime, false)]
        public DateTime Birthdate { get; set; }
        
        [Column("nullablebirthdate", false, DataTypes.DateTime, true)]
        public DateTime? NullableBirthdate { get; set; }

        [Column("age", false, DataTypes.Int, false)]
        public int Age { get; set; }

        [Column("nullableage", false, DataTypes.Int, true)]
        public int? NullableAge { get; set; }

        [Column("notes", false, DataTypes.Nvarchar, 256, true)]
        public string Notes { get; set; }

        [Column("persontype", false, DataTypes.Nvarchar, 8, false)]
        public PersonType Type { get; set; }

        [Column("nullablepersontype", false, DataTypes.Nvarchar, 8, true)]
        public PersonType? NullableType { get; set; }

        [Column("ishandsome", false, DataTypes.Boolean, false)]
        public bool IsHandsome { get; set; }

        public Person()
        {

        }

        public Person(
            string first, 
            string last, 
            DateTime birthDate, 
            DateTime? nullableBirthDate,
            int age,
            int? nullableAge,
            string notes, 
            PersonType personType, 
            PersonType? nullablePersonType,
            bool isHandsome)
        {
            FirstName = first;
            LastName = last;
            Birthdate = birthDate;
            NullableBirthdate = nullableBirthDate;
            Age = age;
            NullableAge = nullableAge;
            Notes = notes;
            Type = personType;
            NullableType = nullablePersonType;
            IsHandsome = isHandsome;
        }

        public override string ToString()
        {
            return
                "---" + Environment.NewLine +
                "ID " + Id + Environment.NewLine +
                "   Name        : " + FirstName + " " + LastName + Environment.NewLine +
                "   Birthdate   : " + Birthdate.ToString() + " nullable " + (NullableBirthdate != null ? NullableBirthdate.ToString() : "(null)") + Environment.NewLine +
                "   Age         : " + Age + " nullable " + (NullableAge != null ? NullableAge.ToString() : "(null)") + Environment.NewLine +
                "   Type        : " + Type.ToString() + " nullable " + NullableType + Environment.NewLine +
                "   Notes       : " + Notes + Environment.NewLine +
                "   Handsome    : " + IsHandsome;
        } 
    }
}
