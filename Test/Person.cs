using System;
using System.Collections.Generic;
using System.Text; 
using Watson.ORM;

namespace Test
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
        
        [Column("notes", false, DataTypes.Nvarchar, true)]
        public string Notes { get; set; }

        [Column("persontype", false, DataTypes.Nvarchar, 8, false)]
        public PersonType Type { get; set; }

        [Column("ishandsome", false, DataTypes.Boolean, false)]
        public bool IsHandsome { get; set; }

        public Person()
        {

        }

        public Person(string first, string last, DateTime birthDate, string notes, PersonType personType, bool isHandsome)
        {
            FirstName = first;
            LastName = last;
            Birthdate = birthDate;
            Notes = notes;
            Type = personType;
            IsHandsome = isHandsome;
        }

        public override string ToString()
        {
            return "ID " + Id + " " + FirstName + " " + LastName + " Birthdate " + Birthdate.ToString() + " [" + Type.ToString() + "] Notes " + Notes + " Type " + Type.ToString() + " Handsome? " + IsHandsome;
        } 
    }
}
