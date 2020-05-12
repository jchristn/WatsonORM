# WatsonORM

[![NuGet Version](https://img.shields.io/nuget/v/WatsonORM.svg?style=flat)](https://www.nuget.org/packages/WatsonORM/) [![NuGet](https://img.shields.io/nuget/dt/WatsonORM.svg)](https://www.nuget.org/packages/WatsonORM) 

## Description

WatsonORM is a lightweight and easy to use object-relational mapper (ORM) in C# for .NET Core built on top of DatabaseWrapper.  Watson.ORM supports Microsoft SQL Server, MySQL, PostgreSQL, and Sqlite databases.

Core features:

- Annotate classes and automatically create database tables
- Quickly create, read, update, or delete database records using your own objects
- Reduce time-to-production and time spent building scaffolding code
- Programmatic table creation and removal

For a sample app exercising this library, refer to the ```Test``` project contained within the solution.

## New in v1.0.0

- Initial release
 
## Simple Example

This example uses ```Sqlite```.  For ```MsSql```, ```MySql```, or ```PgSql```, you must make sure the database exists.  Tables will be automatically created in this example.  Refer to the ```Test``` project for a complete example.
```
using Watson.ORM;

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
...
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
Person selected = orm.SelectById<Person>(1); 

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

// Update
inserted.FirstName = "Jason";
Person updated = orm.Update<Person>(inserted);

// Delete
orm.Delete<Person>(updated); 
```
 
## Pagination with SelectMany

```SelectMany``` can be paginated by using the method with signature ```(int? indexStart, int? maxResults, DbExpression expr)```.  ```indexStart``` is the number of records to skip, and ```maxResults``` is the number of records to retrieve.  

Paginated results are always ordered by the primary key column value in ascending order, i.e. ```ORDER BY id ASC``` in the ```Person``` example above.
  
## Version history

Refer to CHANGELOG.md.
