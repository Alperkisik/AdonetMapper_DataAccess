# **$\textcolor{#7209B7}{\textsf{AdonetMapper DataAccess Class Library Based on Stored Procedure}}$**
- AdonetMapper is a custom basic ORM Basic 'Dapper' like Adonet class library targeting for Stored Procedure based Database transactions.
- <span style="color:red;">**Aim of this project**</span> => Gain Access data from Stored Procedure Methods with Adonet transactions and organizing them with related Tables and Dto's in c# class libraries for easy organized use cases.
- Base organized use case code pattern =>  "dataAccess.{Your Table name or Dto Name}.{Related Stored Procedure Method Name}"
- Use case is almost the same with 'Dapper'. It has 3 layered Backend structure for .

## **$\textcolor{#7209B7}{\textsf{4 Layered DataAccess Backend Pattern}}$**

### **$\textcolor{#4895EF}{\textsf{First Layer - AdonetMapper.cs Layer}}$**
- This is a 'Dapper' like Adonet extension class for stored procedure database transactions. 
- Includes no raw query transactions because of bussiness rule indicates "only usage of stored procedure methods" but raw query transactions can be addded. 
- It extends adonet classes (such as DbConnection, SqlCommand, SqlDataAdapter etc) to get datas then map datas into generic objects provided by outside.

### **$\textcolor{#4895EF}{\textsf{Second Layer - AdonetDataProvider.cs Layer}}$**
- This Provider class have to include AdonetMapper.cs to extend DbConnection or SqlConnection classes to gain access data from Database and Stored Procedure Methods with custom bussiness rules.
- AdonetDataProvider methods gets caller method names (like [CallerMemberName] string storedProcedureMethodName = ""). In this way you can name caller methods as Stored Procedure Method Names in your database.
- Some use cases you may pass the stored procedure method name by manual that is why it is an optional parameter.


### **$\textcolor{#4895EF}{\textsf{Third Layer - Repository.cs Layer}}$**
- This layer presents Stored Procedure Methods related with Dto Models. 
- For example, create a repository layer based on that in this case repository named "StudentRepository" or "Student_SPPM" (SPPM => Stored Procedure Method Library) inherited from AdonetDataProvider , if database has Student table and Stored Procedure Methods related with Student table like Select_Students ( returns Student records based on parameters ). 
- Then add Stored Procedure Methods as C# methods like Ienumerable<Student> Select_Students(object parameters) "Select_Students name represents Stored Procedure Name" then inside the method call base methods comes from AdonetDataProvider.
- This layer is also be used as validating your parameters (like in Bussiness Layers) or converting parameters to proper types for AdonetDataProvider methods.
- Only one thing is certainly required that is validating parameters somewhere before calling AdonetDataProvider methods because Stored Procedure Method has parameters with limits and accepts only with same named parameters.

### **$\textcolor{#4895EF}{\textsf{Fourth Layer - DataAccess.cs Layer}}$**
- Newable DataAccess.cs presents Repositories or SPPM classes to gain Access data from other classes. 
- Pattern => "dataAccess.{Your Table name or Dto Name}.{Related Stored Procedure Method Name}".
- Aim of this pattern is organizing your Stored Procedure Methods with related Tables or Dto classes.

- $\textcolor{#3f37C9}{\textsf{Example Code UseCase =>}}$
```csharp
DataAccess storedProcedureMethodLibrary = new DataAccess(connectionString);

var data = storedProcedureMethodLibrary.Student.Select_Student(parameters);

foreach (var item in data)
{
    Console.log(item.Name);
}
```
