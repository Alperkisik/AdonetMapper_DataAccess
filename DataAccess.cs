using AdonetMapper_DataAccess.Repository;

namespace AdonetMapper_DataAccess
{
    public class DataAccess
    {
        readonly StudentRepository _studentRepository;

        public DataAccess(string ConnectionString)
        {
            _studentRepository = new StudentRepository(ConnectionString);
        }

        /// <summary>
        /// Use Case ->
        ///     <code>Students.Select_Student()</code> or any other method to get Ienumerable{Student} data
        /// </summary>
        public StudentRepository Students => _studentRepository;
    }
}
