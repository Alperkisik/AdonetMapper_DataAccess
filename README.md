# **AdonetMapper DataAccess Class Library**

AdonetMapper is a custom basic ORM Basic 'Dapper' like Adonet class library targeting for Stored Procedure based Database transactions.
Use case is almost the same.

## BackEnd Layout Based on 3 Layers

## First Layer - AdonetDataProvider.cs Layer
This Provider class have to include AdonetMapper.cs to extend DbConnection or SqlConnection classes to access data from Database Stored Procedure Methods with custom bussiness rules.
AdonetDataProvider methods gets caller method names (like [CallerMemberName] string storedProcedureMethodName = ""). In this way you can name caller methods as Stored Procedure Method Names in your database. Some use cases you may pass the stored procedure method name by manual that is why it is an optional parameter.


## Second Layer - Repository.cs Layer
This layer presents Stored Procedure Methods related with Dto Models. For example, if database has Student table and Stored Procedure Methods related with Student table like Select_Students ( returns Student records based on parameters ), create a repository layer based on that in this case repository named "StudentRepository" or "Student_SPPM" (SPPM => Stored Procedure Method Library) inherited from AdonetDataProvider. Then add Stored Procedure Methods as C# methods like Ienumerable<Student> Select_Students(object parameters) then inside the method call base methods comes from AdonetDataProvider.
This layer is also be used as validating your parameters (like in Bussiness Layers) or converting parameters to proper types for AdonetDataProvider methods. Only one thing is certainly required that is validating parameters somewhere before calling AdonetDataProvider methods because Stored Procedure Method has parameters with limits and accepts only with same named parameters.

#Third Layer - DataAccess.cs Layer
Newable DataAccess.cs presents Repositories or SPPM classes to gain Access data from other classes. 
Pattern => "dataAccess.{Your Table name or Dto Name}.{Related Stored Procedure Method Name}".
Aim of this pattern is organizing your Stored Procedure Methods with related Tables or Dto classes.

Example Code UseCase =>
```csharp
DataAccess storedProcedureMethodLibrary = new DataAccess(connectionString);

var data = storedProcedureMethodLibrary.Student.Select_Student(parameters);

foreach (var item in data)
{
    Console.log(item.Name);
}
```
