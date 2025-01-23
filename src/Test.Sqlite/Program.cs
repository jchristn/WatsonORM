using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DatabaseWrapper.Core;
using ExpressionTree;
using Newtonsoft.Json;
using Watson.ORM.Core;
using Watson.ORM.Sqlite;

namespace Test.Sqlite
{
    class Program
    { 
        static string _Filename = null;
        static byte[] _FileBytes = File.ReadAllBytes("./headshot.png");
        static DatabaseSettings _Settings = null;
        static WatsonORM _ORM = null;

        static void Main(string[] args)
        {
            try
            {
                #region Setup
                 
                Console.Write("Filename: ");
                _Filename = Console.ReadLine();
                if (String.IsNullOrEmpty(_Filename)) return;

                _Settings = new DatabaseSettings(_Filename); 
                _ORM = new WatsonORM(_Settings);
                _ORM.Settings.Debug.Logger = Logger;
                _ORM.Settings.Debug.EnableForQueries = true;
                _ORM.Settings.Debug.EnableForResults = true;

                _ORM.InitializeDatabase();
                _ORM.InitializeTable(typeof(Person));
                _ORM.TruncateTable(typeof(Person));
                Console.WriteLine("Using table: " + _ORM.GetTableName(typeof(Person)));

                #endregion

                #region Sanitize-Data

                string[] attacks = {
                    "' OR '1'='1",
                    "'; DROP TABLE Users; --",
                    "' UNION SELECT username, password FROM Users--",
                    "' OR 1=1--",
                    "admin' --",
                    "'; EXEC xp_cmdshell 'net user';--",
                    "' OR 'x'='x",
                    "1 OR 1=1",
                    "1; SELECT * FROM Users",
                    "' OR id IS NOT NULL OR id = '",
                    "username' AND 1=0 UNION ALL SELECT 'admin', '81dc9bdb52d04dc20036dbd8313ed055'--",
                    "' OR '1'='1' /*",
                    "' UNION ALL SELECT NULL, NULL, NULL, CONCAT(username,':',password) FROM Users--",
                    "' AND (SELECT * FROM (SELECT(SLEEP(5)))bAKL) AND 'vRxe'='vRxe",
                    "'; WAITFOR DELAY '0:0:5'--",
                    "The quick brown fox jumped over the lazy dog"
                };

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Sanitizing input strings");
                foreach (string attack in attacks)
                    Console.WriteLine("  | " + attack + " | Sanitized: " + _ORM.Sanitize(attack));

                #endregion

                #region Create-and-Store-Records

                DateTimeOffset localTime = new DateTimeOffset(Convert.ToDateTime("1/1/2021"));
                Person p1 = new Person("Abraham", "Lincoln", Convert.ToDateTime("1/1/1980"), null, localTime, null, 42, null, "initial notes p1", PersonType.Human, null, false, _FileBytes, Guid.NewGuid(), null);
                Person p2 = new Person("Ronald", "Reagan", Convert.ToDateTime("2/2/1981"), Convert.ToDateTime("3/3/1982"), localTime, localTime, 43, 43, "initial notes p2", PersonType.Cat, PersonType.Cat, true, _FileBytes, Guid.NewGuid(), Guid.NewGuid());
                Person p3 = new Person("George", "Bush", Convert.ToDateTime("3/3/1982"), null, localTime, null, 44, null, "initial notes p3", PersonType.Dog, PersonType.Dog, false, _FileBytes, Guid.NewGuid(), null);
                Person p4 = new Person("Barack", "Obama", Convert.ToDateTime("4/4/1983"), Convert.ToDateTime("5/5/1983"), localTime, localTime, 45, null, "initial notes p4", PersonType.Human, null, true, _FileBytes, Guid.NewGuid(), Guid.NewGuid());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p1");
                p1 = _ORM.Insert<Person>(p1);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p2");
                p2 = _ORM.Insert<Person>(p2);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p3");
                p3 = _ORM.Insert<Person>(p3);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p4");
                p4 = _ORM.Insert<Person>(p4);

                #endregion

                #region Insert-Multiple-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p5 through p8");
                Person p5 = new Person("Jason", "Christner", Convert.ToDateTime("4/21/2020"), null, localTime, null, 1, null, "initial notes p5", PersonType.Human, null, false, _FileBytes, Guid.NewGuid(), null);
                Person p6 = new Person("Maria", "Sanchez", Convert.ToDateTime("10/10/1982"), Convert.ToDateTime("10/10/1982"), localTime, localTime, 38, null, "initial notes p6", PersonType.Cat, PersonType.Cat, true, _FileBytes, Guid.NewGuid(), Guid.NewGuid());
                Person p7 = new Person("Eddie", "Van Halen", Convert.ToDateTime("3/3/1982"), null, localTime, null, 44, null, "initial notes p7", PersonType.Dog, PersonType.Dog, false, _FileBytes, Guid.NewGuid(), null);
                Person p8 = new Person("Steve", "Vai", Convert.ToDateTime("4/4/1983"), Convert.ToDateTime("5/5/1983"), localTime, localTime, 45, null, "initial notes p8", PersonType.Human, null, true, _FileBytes, Guid.NewGuid(), Guid.NewGuid());
                List<Person> people = new List<Person> { p5, p6, p7, p8 };
                _ORM.InsertMultiple<Person>(people);

                #endregion

                #region Insert-with-Special-Characters

                Person p9 = new Person("TestШЋЖ", "PersonŠĆŽ", Convert.ToDateTime("4/4/1983"), Convert.ToDateTime("5/5/1983"), localTime, localTime, 45, null, "initial notes p9", PersonType.Human, null, true, _FileBytes, Guid.NewGuid(), null);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Creating p9");
                p9 = _ORM.Insert<Person>(p9);

                #endregion

                #region Select

                Console.WriteLine("| Selecting all records");
                List<Person> all = _ORM.SelectMany<Person>();
                Console.WriteLine("| Retrieved: " + all.Count + " records");
                foreach (Person curr in all) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by GUID for p7: " + p7.GUID.ToString());
                Expr eSelectGuid = new Expr("guid", OperatorEnum.Equals, p7.GUID);
                Person selected1 = _ORM.SelectFirst<Person>(eSelectGuid);
                Console.WriteLine("| Retrieved: " + Environment.NewLine + selected1.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many by column name");
                Expr eSelect1 = new Expr("id", OperatorEnum.GreaterThan, 0);
                List<Person> selectedList1 = _ORM.SelectMany<Person>(null, null, eSelect1);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many by property name");
                Expr eSelect2 = new Expr(
                    _ORM.GetColumnName<Person>(nameof(Person.FirstName)),
                    OperatorEnum.Equals,
                    "Abraham");
                List<Person> selectedList2 = _ORM.SelectMany<Person>(null, null, eSelect2);
                Console.WriteLine("| Retrieved: " + selectedList2.Count + " records");
                foreach (Person curr in selectedList2) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by ID");
                Person pSelected = _ORM.SelectByPrimaryKey<Person>(3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting first by column name");
                Expr eSelect3 = new Expr("id", OperatorEnum.Equals, 4);
                pSelected = _ORM.SelectFirst<Person>(eSelect3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                #endregion

                #region Update-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p1");
                p1.Notes = "updated notes p1";
                p1.NullableType = null;
                p1 = _ORM.Update<Person>(p1);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p2");
                p2.Notes = "updated notes p2";
                p2.NullableType = null;
                p2 = _ORM.Update<Person>(p2);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p3");
                p3.Notes = "updated notes p3";
                p3.NullableType = null;
                p3 = _ORM.Update<Person>(p3);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating p4");
                p4.Notes = "updated notes p4";
                p4.NullableType = null;
                p4 = _ORM.Update<Person>(p4);

                #endregion

                #region Update-Many-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Updating many records");
                Dictionary<string, object> updateVals = new Dictionary<string, object>();
                updateVals.Add(_ORM.GetColumnName<Person>("Notes"), "Updated during update many!");
                _ORM.UpdateMany<Person>(eSelect1, updateVals);

                #endregion

                #region Select

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting many, test 1");
                selectedList1 = _ORM.SelectMany<Person>(null, null, eSelect1);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by ID");
                pSelected = _ORM.SelectByPrimaryKey<Person>(3);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting first");
                pSelected = _ORM.SelectFirst<Person>(eSelect2);
                Console.WriteLine("| Selected: " + pSelected.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting between, test 1");
                Expr eSelect4 = Expr.Between("id", new List<object> { 2, 4 });
                selectedList1 = _ORM.SelectMany<Person>(null, null, eSelect4);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by persontype");
                Expr eSelect5 = new Expr("persontype", OperatorEnum.Equals, PersonType.Dog);
                selectedList1 = _ORM.SelectMany<Person>(null, null, eSelect5);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting handsome people");
                Expr eSelect6 = new Expr("ishandsome", OperatorEnum.Equals, true);
                selectedList1 = _ORM.SelectMany<Person>(null, null, eSelect6);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Selecting by reverse ID order");
                Expr eSelect7 = new Expr("id", OperatorEnum.GreaterThan, 0);
                ResultOrder[] resultOrder = new ResultOrder[1];
                resultOrder[0] = new ResultOrder("id", OrderDirectionEnum.Descending);
                selectedList1 = _ORM.SelectMany<Person>(null, null, eSelect7, resultOrder);
                Console.WriteLine("| Retrieved: " + selectedList1.Count + " records");
                foreach (Person curr in selectedList1) Console.WriteLine(curr.ToString());

                #endregion

                #region Exception

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Catching exception and displaying query");

                try
                {
                    _ORM.Query("SELECT * FROM person (((");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                    Console.WriteLine("Query    : " + e.Data["Query"]);
                }

                #endregion

                #region Pause

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("Press ENTER to delete records");
                Console.ReadLine();

                #endregion

                #region Delete-Records

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Deleting p1");
                _ORM.Delete<Person>(p1);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Deleting p2");
                _ORM.DeleteByPrimaryKey<Person>(2);

                for (int i = 0; i < 8; i++) Console.WriteLine("");
                Console.WriteLine("| Deleting records");
                Expr eDelete = new Expr("id", OperatorEnum.GreaterThan, 2);
                _ORM.DeleteMany<Person>(eDelete);

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
