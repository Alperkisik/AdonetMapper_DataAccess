using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using static AdonetMapper_DataAccess.AdonetMapper;

namespace AdonetMapper_DataAccess
{
    /// <summary>
    /// <para>Using Abstract protected version. That's the way you can restrict the class users to prevent unwanted usage of this class </para>
    /// <br>Another usecase public class AdonetDataProvider</br>
    /// <br>[CallerMemberName] this structure allows developers to name their function names excatly the same as Stored Procedured Functions names.</br>
    /// <br>For Example: [dbo].[Select_Students] returns Student collection. May call your function name IEnumerable{Student} Select_Students then inside function using <seealso cref="MultipleRowAsync"/> function to get data.</br>
    /// </summary>
    public abstract class AdonetDataProvider
    {
        readonly string _connectionString;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ConnectionString"></param>
        public AdonetDataProvider(string ConnectionString)
        {
            _connectionString = ConnectionString;
        }

        private string GeneratedSpQuery_FromList(string StoredProcedure, List<SqlParameter> Parameters)
        {
            StringBuilder QueryBuilder = new StringBuilder();

            QueryBuilder.Append(StoredProcedure);

            if (Parameters != null)
            {
                foreach (var parameter in Parameters)
                {
                    if (parameter.DbType == DbType.Int16 || parameter.DbType == DbType.Int32 || parameter.DbType == DbType.Int64)
                        QueryBuilder.Append($" {parameter.ParameterName}={parameter.Value},");
                    else
                        QueryBuilder.Append($" {parameter.ParameterName}='{parameter.Value}',");
                }
            }

            if (QueryBuilder[QueryBuilder.Length - 1] == ',') QueryBuilder.Remove(QueryBuilder.Length - 1, 1);

            return QueryBuilder.ToString();
        }
        private string GeneratedSpQuery_FromObject(string StoredProcedure, object Parameters)
        {
            StringBuilder QueryBuilder = new StringBuilder();

            QueryBuilder.Append(StoredProcedure);

            if (Parameters is null) return QueryBuilder.ToString();

            var properties = Parameters.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object propertyValue = property.GetValue(Parameters);
                if (propertyValue is null) continue;

                if (propertyValue.GetType() == typeof(int))
                    QueryBuilder.Append($" @{property.Name}={propertyValue},");
                else
                    QueryBuilder.Append($" @{property.Name}='{propertyValue}',");
            }

            if (QueryBuilder[QueryBuilder.Length - 1] == ',') QueryBuilder.Remove(QueryBuilder.Length - 1, 1);

            return QueryBuilder.ToString();
        }


        private string StoredProcedureMethod(string schema, string spMethod)
        {
            return $"[{schema}].[{spMethod}]";
        }

        protected string BaseConnectionString => _connectionString;

        protected async Task<TEntityDto> SingleRowAsync<TEntityDto>(object parameters, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            TEntityDto expectedData;

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                expectedData = await connection.SingleRowAsync<TEntityDto>(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            };

            return expectedData;
        }

        protected async Task<IEnumerable<TEntityDto>> MultipleRowAsync<TEntityDto>(object? parameters = null, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            IEnumerable<TEntityDto> expectedData;

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                expectedData = await connection.MultipleRowAsync<TEntityDto>(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            };

            return expectedData;
        }

        protected async Task<int> SingleResult_CountReturnAsync(object? parameters = null, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            object count = default;

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                count = await connection.ExecuteScalarAsync(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            };

            return Convert.ToInt32(count);
        }

        protected async Task<int> InsertOrUpdate_IDReturnAsync(object parameters, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            if (parameters == null) throw new ArgumentNullException("parameters");

            object IdResult = default;

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                IdResult = await connection.ExecuteScalarAsync(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            };

            return Convert.ToInt32(IdResult); ;
        }


        protected TEntityDto SingleRow<TEntityDto>(object parameters, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            TEntityDto expectedData;

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                expectedData = connection.SingleRow<TEntityDto>(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            };

            return expectedData;
        }

        protected IEnumerable<TEntityDto> MultipleRow<TEntityDto>(object? parameters = null, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            IEnumerable<TEntityDto> expectedData;

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                expectedData = connection.MultipleRow<TEntityDto>(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            };

            return expectedData;
        }

        protected int SingleResult_CountReturn(object? parameters = null, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            object count = default;

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                count = connection.ExecuteScalar(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            };

            return Convert.ToInt32(count);
        }

        protected int InsertOrUpdate_IDReturn(object parameters, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            if (parameters == null) throw new ArgumentNullException("parameters");

            object IdResult;

            using (SqlConnection connection = new SqlConnection(BaseConnectionString))
            {
                try
                {
                    IdResult = connection.ExecuteScalar(
                        Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                        Parameters: parameters,
                        commandType: CommandType.StoredProcedure);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    throw;
                }

            };

            return Convert.ToInt32(IdResult);
        }


        protected DataSet DatasetResult(object? parameters = null, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            var dataSet = new DataSet();

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                dataSet = connection.DataAdapterFill(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            }

            return dataSet;
        }

        protected DataTable DataAsDataTable(object? parameters = null, string tableName = "default", string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            var DataTable = new DataTable(tableName);

            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                DataTable = connection.DataAdapterFill(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure).Tables[0];
            }

            return DataTable;
        }

        protected IEnumerable<DataTable> DataAsMultipleDataTable(object? parameters = null, List<string>? tableNames = null, string schema = "dbo", [CallerMemberName] string storedProcedureMethodName = "")
        {
            var dataTables = new List<DataTable>();
            var DataSetResult = new DataSet();



            using (DbConnection connection = new SqlConnection(BaseConnectionString))
            {
                DataSetResult = connection.DataAdapterFill(
                    Query: StoredProcedureMethod(schema, storedProcedureMethodName),
                    Parameters: parameters,
                    commandType: CommandType.StoredProcedure);
            }



            var TableCount = DataSetResult.Tables.Count;
            var tableNameCountCheck = tableNames != null && tableNames.Count == TableCount;
            var TableNumber = 1;
            for (int i = 0; i < TableCount; i++)
            {
                TableNumber++;

                if (tableNameCountCheck) DataSetResult.Tables[i].TableName = tableNames[i];
                else DataSetResult.Tables[i].TableName = $"Table_{TableNumber}";

                dataTables.Add(DataSetResult.Tables[i]);
            }

            return dataTables;
        }
    }
}