using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using Watson.ORM;
using Watson.ORM.Core;

namespace Test
{
    class Program
    {
        static string _DbType = null;
        static string _Filename = null;
        static string _Username = null;
        static string _Password = null;

        static DatabaseSettings _Settings = null;
        static WatsonORM _Orm = null;

        static void Main(string[] args)
        {
            try
            {
                #region Setup

                Console.Write("DB type [sqlserver|mysql|postgresql|sqlite]: ");
                _DbType = Console.ReadLine();
                if (String.IsNullOrEmpty(_DbType)) return;
                _DbType = _DbType.ToLower();

                if (_DbType.Equals("sqlserver") || _DbType.Equals("mysql") || _DbType.Equals("postgresql"))
                {
                    Console.Write("User: ");
                    _Username = Console.ReadLine();

                    Console.Write("Password: ");
                    _Password = Console.ReadLine();

                    switch (_DbType)
                    {
                        case "sqlserver":
                            _Settings = new DatabaseSettings(DbTypes.SqlServer, "localhost", 1433, _Username, _Password, "test");
                            break;
                        case "mysql":
                            _Settings = new DatabaseSettings(DbTypes.Mysql, "localhost", 3306, _Username, _Password, "test");
                            break;
                        case "postgresql":
                            _Settings = new DatabaseSettings(DbTypes.Postgresql, "localhost", 5432, _Username, _Password, "test");
                            break;
                        default:
                            return;
                    }
                }
                else if (_DbType.Equals("sqlite"))
                {
                    Console.Write("Filename: ");
                    _Filename = Console.ReadLine();
                    if (String.IsNullOrEmpty(_Filename)) return;

                    _Settings = new DatabaseSettings(_Filename);
                }
                else
                {
                    Console.WriteLine("Invalid database type.");
                    return;
                }
                
                _Orm = new WatsonORM(_Settings);

                _Orm.InitializeDatabase();

                DebugSettings debug = new DebugSettings();
                debug.DatabaseQueries = true;
                debug.DatabaseResults = true;

                _Orm.Logger = Logger;
                _Orm.Debug = debug;

                _Orm.InitializeTable(typeof(Person));
                _Orm.TruncateTable(typeof(Person));

                #endregion

                #region Create-and-Store-Records

                Person p = new Person
                {
                    FirstName = "Joel",
                    LastName = "Christner",

                };

                Person p1 = new Person("Abraham", "Lincoln", Convert.ToDateTime("1/1/1980"), null, 42, null, "initial notes p1", PersonType.Human, null, false);
                Person p2 = new Person("Ronald", "Reagan", Convert.ToDateTime("2/2/1981"), Convert.ToDateTime("3/3/1982"), 43, 43, "initial notes p2", PersonType.Cat, PersonType.Cat, true);
                Person p3 = new Person("George", "Bush", Convert.ToDateTime("3/3/1982"), null, 44, null, "initial notes p3", PersonType.Dog, PersonType.Dog, false);
                Person p4 = new Person("Barack", "Obama", Convert.ToDateTime("4/4/1983"), Convert.ToDateTime("5/5/1983"), 45, null, "initial notes p4", PersonType.Human, null, true);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p1");
                p1 = _Orm.Insert<Person>(p1);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p2");
                p2 = _Orm.Insert<Person>(p2);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p3");
                p3 = _Orm.Insert<Person>(p3);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p4");
                p4 = _Orm.Insert<Person>(p4);

                #endregion

                #region Select

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many by column name");
                DbExpression eSelect1 = new DbExpression("id", DbOperators.GreaterThan, 0);
                List<Person> selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect1);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine("  | " + curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many by property name");
                DbExpression eSelect2 = new DbExpression(
                    _Orm.GetColumnName<Person>(nameof(Person.FirstName)),
                    DbOperators.Equals,
                    "Abraham");
                List<Person> selectedList2 = _Orm.SelectMany<Person>(null, null, eSelect2);
                Console.WriteLine("| Retrieved: " + selectedList2.Count + " records");
                foreach (Person curr in selectedList2) Console.WriteLine("  | " + curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by ID");
                Person pSelected = _Orm.SelectByPrimaryKey<Person>(3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting first by column name");
                DbExpression eSelect3 = new DbExpression("id", DbOperators.Equals, 4);
                pSelected = _Orm.SelectFirst<Person>(eSelect3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                #endregion 

                #region Update-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p1");
                p1.Notes = "updated notes p1";
                p1 = _Orm.Update<Person>(p1);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p2");
                p2.Notes = "updated notes p2";
                p2 = _Orm.Update<Person>(p2);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p3");
                p3.Notes = "updated notes p3";
                p3 = _Orm.Update<Person>(p3);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p4");
                p4.Notes = "updated notes p4";
                p4 = _Orm.Update<Person>(p4);

                #endregion

                #region Update-Many-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating many records");
                Dictionary<string, object> updateVals = new Dictionary<string, object>();
                updateVals.Add(_Orm.GetColumnName<Person>("Notes"), "Updated during update many!");
                _Orm.UpdateMany<Person>(eSelect1, updateVals);

                #endregion

                #region Select

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many, test 1");
                selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect1);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine("  | " + curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by ID");
                pSelected = _Orm.SelectByPrimaryKey<Person>(3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting first");
                pSelected = _Orm.SelectFirst<Person>(eSelect2);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting between, test 1");
                DbExpression eSelect4 = DbExpression.Between("id", new List<object> { 2, 4 });
                selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect4);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine("  | " + curr.ToString());

                #endregion

                #region Delete-Records
                 
                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Deleting p1");
                _Orm.Delete<Person>(p1);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Deleting p2");
                _Orm.DeleteByPrimaryKey<Person>(2);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Deleting p3 and p4");
                DbExpression eDelete = new DbExpression("id", DbOperators.GreaterThan, 2);
                _Orm.DeleteMany<Person>(eDelete);
                 
                #endregion
            }
            catch (Exception e)
            {    
                // Get stack trace for the exception with source file information
                var st = new StackTrace(e, true);
                // Get the top stack frame
                var frame = st.GetFrame(0);
                // Get the line number from the stack frame
                var line = frame.GetFileLineNumber();
                Console.WriteLine("Stack trace:" + Environment.NewLine + SerializeJson(st, true));
                Console.WriteLine("Stack frame: " + Environment.NewLine + SerializeJson(st, true));
                Console.WriteLine("Line number: " + line);
                Console.WriteLine("Exception: " + Environment.NewLine + SerializeJson(e, true));
            }

            Console.WriteLine("");
            Console.WriteLine("Press ENTER to exit");
            Console.ReadLine();
        }

        static void Logger(string msg)
        {
            Console.WriteLine(msg);
        }

        static string SerializeJson(object obj, bool pretty)
        {
            if (obj == null) return null;
            string json;

            if (pretty)
            {
                json = JsonConvert.SerializeObject(
                  obj,
                  Newtonsoft.Json.Formatting.Indented,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                  });
            }
            else
            {
                json = JsonConvert.SerializeObject(obj,
                  new JsonSerializerSettings
                  {
                      NullValueHandling = NullValueHandling.Ignore,
                      DateTimeZoneHandling = DateTimeZoneHandling.Utc
                  });
            }

            return json;
        }
    }
}
