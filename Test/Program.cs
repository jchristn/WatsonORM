using System;
using System.Collections.Generic;
using System.IO;
using Watson.ORM;

namespace Test
{
    class Program
    {
        static DatabaseSettings _Settings = null;
        static WatsonORM _Orm = null;

        static void Main(string[] args)
        {
            try
            {
                #region Setup

                if (File.Exists("./WatsonORM.db")) File.Delete("./WatsonORM.db");

                _Settings = new DatabaseSettings("./WatsonORM.db");
                _Orm = new WatsonORM(_Settings);
                _Orm.Logger = Logger;
                
                _Orm.InitializeDatabase();
                 
                _Orm.Database.Logger = Logger;
                _Orm.Database.LogQueries = true;
                _Orm.Database.LogResults = true; 

                _Orm.InitializeTable(typeof(Person));

                #endregion

                #region Create-and-Store-Records

                Person p = new Person
                {
                    FirstName = "Joel",
                    LastName = "Christner",

                };

                Person p1 = new Person("Abraham", "Lincoln", Convert.ToDateTime("1/1/1980"), "initial notes p1");
                Person p2 = new Person("Ronald", "Reagan", Convert.ToDateTime("2/2/1981"), "initial notes p2");
                Person p3 = new Person("George", "Bush", Convert.ToDateTime("3/3/1982"), "initial notes p3");
                Person p4 = new Person("Barack", "Obama", Convert.ToDateTime("4/4/1983"), "initial notes p4");

                Console.WriteLine("| Creating p1");
                p1 = _Orm.Insert<Person>(p1);

                Console.WriteLine("| Creating p2");
                p2 = _Orm.Insert<Person>(p2);

                Console.WriteLine("| Creating p3");
                p3 = _Orm.Insert<Person>(p3);

                Console.WriteLine("| Creating p4");
                p4 = _Orm.Insert<Person>(p4);

                #endregion

                #region Select

                Console.WriteLine("| Selecting many by column name");
                DbExpression eSelect1 = new DbExpression("id", DbOperators.GreaterThan, 0);
                List<Person> selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect1);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine("  | " + curr.ToString());

                Console.WriteLine("| Selecting many by property name");
                DbExpression eSelect2 = new DbExpression(
                    _Orm.GetColumnName<Person>(nameof(Person.FirstName)),
                    DbOperators.Equals,
                    "Abraham");
                List<Person> selectedList2 = _Orm.SelectMany<Person>(null, null, eSelect2);
                Console.WriteLine("| Retrieved: " + selectedList2.Count + " records");
                foreach (Person curr in selectedList2) Console.WriteLine("  | " + curr.ToString());

                Console.WriteLine("| Selecting by ID");
                Person pSelected = _Orm.SelectById<Person>(3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                Console.WriteLine("| Selecting first by column name");
                DbExpression eSelect3 = new DbExpression("id", DbOperators.Equals, 4);
                pSelected = _Orm.SelectFirst<Person>(eSelect3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                #endregion

                #region Update-Records

                Console.WriteLine("| Updating p1");
                p1.Notes = "updated notes p1";
                p1 = _Orm.Update<Person>(p1);

                Console.WriteLine("| Updating p2");
                p2.Notes = "updated notes p2";
                p2 = _Orm.Update<Person>(p2);

                Console.WriteLine("| Updating p3");
                p3.Notes = "updated notes p3";
                p3 = _Orm.Update<Person>(p3);

                Console.WriteLine("| Updating p4");
                p4.Notes = "updated notes p4";
                p4 = _Orm.Update<Person>(p4);

                #endregion

                #region Select

                Console.WriteLine("| Selecting many, test 1");
                selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect1);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine("  | " + curr.ToString());

                Console.WriteLine("| Selecting by ID");
                pSelected = _Orm.SelectById<Person>(3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                Console.WriteLine("| Selecting first");
                pSelected = _Orm.SelectFirst<Person>(eSelect2);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                #endregion

                #region Delete-Records

                // delete p1
                Console.WriteLine("| Deleting p1");
                _Orm.Delete<Person>(p1);

                // delete p2
                Console.WriteLine("| Deleting p2");
                _Orm.DeleteById<Person>(2);

                // delete p3 and p4
                Console.WriteLine("| Deleting p3 and p4");
                DbExpression eDelete = new DbExpression("id", DbOperators.GreaterThan, 2);
                _Orm.DeleteByFilter<Person>(eDelete);
                 
                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + Environment.NewLine + e.ToString());
            }

            Console.WriteLine("");
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        static void Logger(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
