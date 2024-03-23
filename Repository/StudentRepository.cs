using AdonetMapper_DataAccess.Entity;
using AdonetMapper_DataAccess.Utility;

namespace AdonetMapper_DataAccess.Repository
{
    /// <summary>
    /// This is where Stored Procedure Methods represents as C# Methods to Easy Access and Document Stored Procedure Methods in C# Class Libraries
    /// </summary>
    public class StudentRepository : AdonetDataProvider
    {
        public StudentRepository(string ConnectionString) : base(ConnectionString)
        {
        }

        //Select_Student is a Stored Procedure Method Name Related with Student Table
        public async Task<IEnumerable<Student>> Select_Student()
        {
            return await MultipleRowAsync<Student>();
        }

        //Select_Student is a Stored Procedure Method Name Related with Student Table
        public async Task<IEnumerable<Student>> Select_Student(Student model)
        {
            //May want to use different validation approach here or somewhere else

            var validationResult = Utilities.Validation(model);
            if (validationResult.IsFailure) throw new ArgumentException(validationResult.Error);

            return await MultipleRowAsync<Student>(
                parameters: Utilities.SqlParameters_FromObject(model),
                schema: "dbo");
        }
    }
}
