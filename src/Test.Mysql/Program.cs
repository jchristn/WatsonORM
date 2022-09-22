using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DatabaseWrapper.Core;
using ExpressionTree;
using Newtonsoft.Json;
using Watson.ORM.Core;
using Watson.ORM.Mysql;

namespace Test.Mysql
{
    class Program
    {
        static string _Username = null;
        static string _Password = null;
        static byte[] _FileBytes = File.ReadAllBytes("./headshot.png");
        static DatabaseSettings _Settings = null;
        static WatsonORM _Orm = null;

        static void Main(string[] args)
        {
            try
            {
                #region Setup

                Console.Write("User: ");
                _Username = Console.ReadLine();

                Console.Write("Password: ");
                _Password = Console.ReadLine();

                _Settings = new DatabaseSettings(DbTypes.Mysql, "localhost", 3306, _Username, _Password, "test");

                _Orm = new WatsonORM(_Settings);
                _Orm.Settings.Debug.Logger = Logger;
                _Orm.Settings.Debug.EnableForQueries = true;
                _Orm.Settings.Debug.EnableForResults = true;

                _Orm.InitializeDatabase();
                _Orm.InitializeTable(typeof(Person));
                _Orm.TruncateTable(typeof(Person));
                Console.WriteLine("Using table: " + _Orm.GetTableName(typeof(Person)));

                #endregion

                #region Create-and-Store-Records

                DateTimeOffset localTime = new DateTimeOffset(Convert.ToDateTime("1/1/2021"));
                Person p1 = new Person("Abraham", "Lincoln", Convert.ToDateTime("1/1/1980"), null, localTime, null, 42, null, "initial notes p1", PersonType.Human, null, false, _FileBytes);
                Person p2 = new Person("Ronald", "Reagan", Convert.ToDateTime("2/2/1981"), Convert.ToDateTime("3/3/1982"), localTime, localTime, 43, 43, "initial notes p2", PersonType.Cat, PersonType.Cat, true, _FileBytes);
                Person p3 = new Person("George", "Bush", Convert.ToDateTime("3/3/1982"), null, localTime, null, 44, null, "initial notes p3", PersonType.Dog, PersonType.Dog, false, _FileBytes);
                Person p4 = new Person("Barack", "Obama", Convert.ToDateTime("4/4/1983"), Convert.ToDateTime("5/5/1983"), localTime, localTime, 45, null, "initial notes p4", PersonType.Human, null, true, _FileBytes);

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

                #region Insert-Multiple-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p5 through p8");
                Person p5 = new Person("Jason", "Christner", Convert.ToDateTime("4/21/2020"), null, localTime, null, 1, null, "initial notes p5", PersonType.Human, null, false, _FileBytes);
                Person p6 = new Person("Maria", "Sanchez", Convert.ToDateTime("10/10/1982"), Convert.ToDateTime("10/10/1982"), localTime, localTime, 38, null, "initial notes p6", PersonType.Cat, PersonType.Cat, true, _FileBytes);
                Person p7 = new Person("Eddie", "Van Halen", Convert.ToDateTime("3/3/1982"), null, localTime, null, 44, null, "initial notes p7", PersonType.Dog, PersonType.Dog, false, _FileBytes);
                Person p8 = new Person("Steve", "Vai", Convert.ToDateTime("4/4/1983"), Convert.ToDateTime("5/5/1983"), localTime, localTime, 45, null, "initial notes p8", PersonType.Human, null, true, _FileBytes);
                List<Person> people = new List<Person> { p5, p6, p7, p8 };
                _Orm.InsertMultiple<Person>(people);

                #endregion

                #region Exists-Count-Sum

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Expr existsExpression = new Expr(_Orm.GetColumnName<Person>(nameof(Person.Id)), OperatorEnum.GreaterThan, 0);
                Console.WriteLine("| Checking existence of records: " + _Orm.Exists<Person>(existsExpression));

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Expr countExpression = new Expr(_Orm.GetColumnName<Person>(nameof(Person.Id)), OperatorEnum.GreaterThan, 2);
                Console.WriteLine("| Checking count of records: " + _Orm.Count<Person>(countExpression));

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Checking sum of ages: " + _Orm.Sum<Person>(_Orm.GetColumnName<Person>(nameof(Person.Age)), existsExpression));

                #endregion

                #region Select

                Console.WriteLine("| Selecting all records");
                List<Person> all = _Orm.SelectMany<Person>();
                Console.WriteLine("| Retrieved: " + all.Count + " records");
                foreach (Person curr in all) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many by column name");
                Expr eSelect1 = new Expr("id", OperatorEnum.GreaterThan, 0);
                List<Person> selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect1);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many by property name");
                Expr eSelect2 = new Expr(
                    _Orm.GetColumnName<Person>(nameof(Person.FirstName)),
                    OperatorEnum.Equals,
                    "Abraham");
                List<Person> selectedList2 = _Orm.SelectMany<Person>(null, null, eSelect2);
                Console.WriteLine("| Retrieved: " + selectedList2.Count + " records");
                foreach (Person curr in selectedList2) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by ID");
                Person pSelected = _Orm.SelectByPrimaryKey<Person>(3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting first by column name");
                Expr eSelect3 = new Expr("id", OperatorEnum.Equals, 4);
                pSelected = _Orm.SelectFirst<Person>(eSelect3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                #endregion

                #region Update-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p1");
                p1.Notes = "updated notes p1";
                p1.NullableType = null;
                p1 = _Orm.Update<Person>(p1);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p2");
                p2.Notes = "updated notes p2";
                p2.NullableType = null;
                p2 = _Orm.Update<Person>(p2);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p3");
                p3.Notes = "updated notes p3";
                p3.NullableType = null;
                p3 = _Orm.Update<Person>(p3);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p4");
                p4.Notes = "updated notes p4";
                p4.NullableType = null;
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
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

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
                Expr eSelect4 = Expr.Between("id", new List<object> { 2, 4 });
                selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect4);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by persontype");
                Expr eSelect5 = new Expr("persontype", OperatorEnum.Equals, PersonType.Dog);
                selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect5);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting handsome people");
                Expr eSelect6 = new Expr("ishandsome", OperatorEnum.Equals, true);
                selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect6);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by reverse ID order");
                Expr eSelect7 = new Expr("id", OperatorEnum.GreaterThan, 0);
                ResultOrder[] resultOrder = new ResultOrder[1];
                resultOrder[0] = new ResultOrder("id", OrderDirection.Descending);
                selectedList1 = _Orm.SelectMany<Person>(null, null, eSelect7, resultOrder);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                #endregion

                #region Exception

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Catching exception and displaying query");

                try
                {
                    _Orm.Query("SELECT * FROM person (((");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                    Console.WriteLine("Query    : " + e.Data["Query"]);
                }

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
                Expr eDelete = new Expr("id", OperatorEnum.GreaterThan, 2);
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
