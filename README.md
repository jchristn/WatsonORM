![alt tag](https://github.com/jchristn/watsonorm/blob/master/assets/watson.ico)

# WatsonORM

| Library | Version | Downloads |
|---|---|---|
| WatsonORM (all supported database types) | [![NuGet Version](https://img.shields.io/nuget/v/WatsonORM.svg?style=flat)](https://www.nuget.org/packages/WatsonORM/)  | [![NuGet](https://img.shields.io/nuget/dt/WatsonORM.svg)](https://www.nuget.org/packages/WatsonORM) |
| WatsonORM.Mysql | [![NuGet Version](https://img.shields.io/nuget/v/WatsonORM.Mysql.svg?style=flat)](https://www.nuget.org/packages/WatsonORM.Mysql/)  | [![NuGet](https://img.shields.io/nuget/dt/WatsonORM.Mysql.svg)](https://www.nuget.org/packages/WatsonORM.Mysql) |
| WatsonORM.Postgresql | [![NuGet Version](https://img.shields.io/nuget/v/WatsonORM.Postgresql.svg?style=flat)](https://www.nuget.org/packages/WatsonORM.Postgresql/)  | [![NuGet](https://img.shields.io/nuget/dt/WatsonORM.Postgresql.svg)](https://www.nuget.org/packages/WatsonORM.Postgresql) |
| WatsonORM.Sqlite | [![NuGet Version](https://img.shields.io/nuget/v/WatsonORM.Sqlite.svg?style=flat)](https://www.nuget.org/packages/WatsonORM.Sqlite/)  | [![NuGet](https://img.shields.io/nuget/dt/WatsonORM.Sqlite.svg)](https://www.nuget.org/packages/WatsonORM.Sqlite) |
| WatsonORM.SqlServer | [![NuGet Version](https://img.shields.io/nuget/v/WatsonORM.SqlServer.svg?style=flat)](https://www.nuget.org/packages/WatsonORM.SqlServer/)  | [![NuGet](https://img.shields.io/nuget/dt/WatsonORM.SqlServer.svg)](https://www.nuget.org/packages/WatsonORM.SqlServer) |
| WatsonORM.Core | [![NuGet Version](https://img.shields.io/nuget/v/WatsonORM.Core.svg?style=flat)](https://www.nuget.org/packages/WatsonORM.Core/)  | [![NuGet](https://img.shields.io/nuget/dt/WatsonORM.Core.svg)](https://www.nuget.org/packages/WatsonORM.Core) |
 
## Description

WatsonORM is a lightweight and easy to use object-relational mapper (ORM) in C# for .NET Core built on top of DatabaseWrapper.  WatsonORM supports Microsoft SQL Server, Mysql, PostgreSQL, and Sqlite databases.  

Core features:

- Annotate classes and automatically create database tables
- Quickly create, read, update, or delete database records using your own objects
- Reduce time-to-production and time spent building scaffolding code
- Programmatic table creation and removal

For a sample app exercising this library, refer to the ```Test``` project contained within the solution.

## New in v1.3.0

- Dependency update
- New APIs: Exists, Count, Sum
- Select with ordering
- Better support for DateTimeOffset

## Simple Example

This example uses ```Sqlite```.  For ```SqlServer```, ```Mysql```, or ```Postgresql```, you must make sure the database exists.  Tables will be automatically created in this example.  Refer to the ```Test``` project for a complete example.
```csharp
using Watson.ORM;
using Watson.ORM.Core;

// Apply attributes to your class
[Table("person")]
public class Person
{
  [Column("id", true, DataTypes.Int, false)]
  public int Id { get; set; }

  [Column("firstname", false, DataTypes.Nvarchar, 64, false)]
  public string FirstName { get; set; }

  // Parameter-less constructor is required
  public Person()
  {
  }
}

// Initialize
DatabaseSettings settings = new DatabaseSettings("./WatsonORM.db");
WatsonORM orm = new WatsonORM(settings);
orm.InitializeDatabase();
orm.InitializeTable(typeof(Person));

// Insert 
Person person = new Person { FirstName = "Joel" };
Person inserted = orm.Insert<Person>(person);

// Select
Person selected = orm.SelectByPrimaryKey<Person>(1); 

// Select many by column name
DbExpression e1 = new DbExpression("id", DbOperators.GreaterThan, 0);
List<Person> people = orm.SelectMany<Person>(e1);

// Select many by property
DbExpression e2 = new DbExpression(
  orm.GetColumnName<Person>(nameof(Person.Id)),
  DbOperators.GreaterThan,
  0);
people = orm.SelectMany<Person>(e2);

// Select many by property with pagination
// Retrieve 50 records starting at record number 10
people = orm.SelectMany<Person>(10, 50, e2);

// Select many with descending order
DbResultOrder[] resultOrder = new DbResultOrder[1];
resultOrder[0] = new DbResultOrder("id", DbOrderDirection.Descending);
people = orm.SelectMany<Person>(null, null, e2, resultOrder);

// Update
inserted.FirstName = "Jason";
Person updated = orm.Update<Person>(inserted);

// Delete
orm.Delete<Person>(updated); 
```
 
## Pagination with SelectMany

```SelectMany``` can be paginated by using the method with either signature ```(int? indexStart, int? maxResults, DbExpression expr)``` or ```(int? indexStart, int? maxResults, DbExpression expr, DbResultOrder[] resultOrder)```.  ```indexStart``` is the number of records to skip, and ```maxResults``` is the number of records to retrieve.  

Paginated results are always ordered by the primary key column value in ascending order, i.e. ```ORDER BY id ASC``` in the ```Person``` example above.

## Using Sqlite

Sqlite may not work out of the box with .NET Framework. In order to use Sqlite with .NET Framework, you'll need to manually copy the runtimes folder into your project output directory. This directory is automatically created when building for .NET Core. To get this folder, build the Test.Sqlite project and navigate to the bin/debug/netcoreapp* directory. Then copy the runtimes folder into the project output directory of your .NET Framework application.

## Using SQL Server

In order to use pagination with SQL Server, the ```SelectMany``` method containing the ```DbResultOrder[] resultOrder``` parameter must be used.

## Using MySQL

While the ```DateTimeOffset``` type can be used in objects, with MySQL the offset is not persisted.  It is recommended that you store UTC timestamps using the ```DateTime``` type instead.

## Version history

Refer to CHANGELOG.md.
