using System.Data.Common;
using System.Data;
using System.Reflection;
using System.Data.SqlClient;

namespace AdonetMapper_DataAccess
{
    public static class AdonetMapper
    {
        static TEntity GetInstance<TEntity>()
        {
            TEntity? instance = (TEntity?)Activator.CreateInstance(typeof(TEntity), false);

            return instance ?? default!;
        }

        private static void SetProperty_NestedEntity(string compoundProperty, object target, object value)
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic;

            string[] bits = compoundProperty.Split('.');
            for (int i = 0; i < bits.Length - 1; i++)
            {
                PropertyInfo propertyToGet = target.GetType().GetProperty(bits[i], bindingFlags);
                target = propertyToGet.GetValue(target, null);
            }
            PropertyInfo propertyToSet = target.GetType().GetProperty(bits.Last(), bindingFlags);

            if (value != DBNull.Value) propertyToSet.SetValue(target, value, null);
        }

        public static void Add(this List<SqlParameter> parameters, string name, object value)
        {
            if (name[0] != '@') name = $"@{name}";

            parameters.Add(new SqlParameter(name, value));
        }

        private static bool ParametersTypeValidation(Type type)
        {
            if (type == typeof(Dictionary<string, object>)) return true;
            if (type == typeof(List<object>)) return true;
            if (type.IsClass && !type.IsInterface && !type.IsAbstract) return true;
            if (type == typeof(object)) return true;

            return false;
        }

        #region SqlDataReader Extensions

        private static List<TSystemEntity> ReadSystemDataCollection<TSystemEntity>(SqlDataReader reader)
        {
            var data = new List<TSystemEntity>();

            while (reader.Read())
            {
                data.Add((TSystemEntity)Convert.ChangeType(reader[0], typeof(TSystemEntity)));
            }

            return data;
        }

        public static object ReadSingleData(this SqlDataReader reader)
        {
            if (!reader.Read()) return default;

            return reader[0];
        }

        public static T ReadSingle<T>(this SqlDataReader reader)
        {
            if (!reader.Read()) return default;

            var T_TypeName = typeof(T).FullName!;

            //if T is a system object (string,int,datetime etc. ) only means that there is a system object
            if (T_TypeName.Contains("System")) return (T)Convert.ChangeType(reader[0], typeof(T));

            T dataObject = GetInstance<T>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Contains("."))
                {
                    SetProperty_NestedEntity(reader.GetName(i), dataObject, reader[i]);
                    continue;
                }

                var property = dataObject.GetType().GetProperty(reader.GetName(i));
                if (property == null) continue;

                if (reader[i] != DBNull.Value) property.SetValue(dataObject, reader[i]);
            }

            return dataObject;
        }

        public static IEnumerable<T> ReadList<T>(this SqlDataReader reader)
        {
            var expectedDataCollection = new List<T>();

            var T_TypeName = typeof(T).FullName!;

            //if T is a system object (string,int,datetime etc. ) only means that there is a system object collection with 1 column and multiple row (array).
            if (T_TypeName.Contains("System")) return ReadSystemDataCollection<T>(reader);


            while (reader.Read())
            {
                T dataObject = GetInstance<T>()!;

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    //if data format id,name,address.name,address.location etc. that means there are relational entities inside T. For example T is a Person(comes form Persons table) and Person has a Address(Address comes from Addresses table).
                    if (reader.GetName(i).Contains('.'))
                    {
                        SetProperty_NestedEntity(reader.GetName(i), dataObject, reader[i]);
                        continue;
                    }

                    var property = dataObject.GetType().GetProperty(reader.GetName(i));
                    if (property == null) continue;

                    if (reader[i] != DBNull.Value) property.SetValue(dataObject, reader[i]);
                }

                expectedDataCollection.Add(dataObject);
            }

            return expectedDataCollection;
        }

        #endregion SqlDataReader Methods

        #region DbDataReader Extensions

        public static object ReadSingleData(this DbDataReader reader)
        {
            if (!reader.Read()) return default;

            return reader[0];
        }

        public static T ReadSingleRow<T>(this DbDataReader reader)
        {
            if (!reader.Read()) return default;

            if (typeof(T).FullName.Contains("System")) return (T)Convert.ChangeType(reader[0], typeof(T));

            T dataObject = GetInstance<T>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Contains("."))
                {
                    SetProperty_NestedEntity(reader.GetName(i), dataObject, reader[i]);
                    continue;
                }

                var property = dataObject.GetType().GetProperty(reader.GetName(i));
                if (property == null) continue;

                if (reader[i] != DBNull.Value) property.SetValue(dataObject, reader[i]);
            }

            return dataObject;
        }

        public static IEnumerable<T> ReadMultipleRow<T>(this DbDataReader reader)
        {
            var data = new List<T>();

            if (typeof(T).FullName.Contains("System"))
            {
                while (reader.Read())
                {
                    data.Add((T)Convert.ChangeType(reader[0], typeof(T)));
                }

                return data;
            }

            while (reader.Read())
            {
                T dataObject = GetInstance<T>();

                for (int i = 0; i < reader.FieldCount; i++)
                {
                    if (reader.GetName(i).Contains("."))
                    {
                        SetProperty_NestedEntity(reader.GetName(i), dataObject, reader[i]);
                        continue;
                    }

                    var property = dataObject.GetType().GetProperty(reader.GetName(i));
                    if (property == null) continue;

                    if (reader[i] != DBNull.Value) property.SetValue(dataObject, reader[i]);
                }

                data.Add(dataObject);
            }

            return data;
        }

        #endregion

        #region DataTable Extensions

        public static IEnumerable<T> Read<T>(this DataTable dataTable)
        {
            var data = new List<T>();

            if (typeof(T).FullName.Contains("System"))
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    data.Add((T)Convert.ChangeType(row[0], typeof(T)));
                }

                return data;
            }

            foreach (DataRow row in dataTable.Rows)
            {
                T dataObject = GetInstance<T>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    if (column.ColumnName.Contains("."))
                    {
                        SetProperty_NestedEntity(column.ColumnName, dataObject, row[column.ColumnName]);
                        continue;
                    }

                    var property = dataObject.GetType().GetProperty(column.ColumnName);
                    if (property == null) continue;

                    var value = row[column.ColumnName];
                    if (value == DBNull.Value) continue;

                    property.SetValue(dataObject, value);
                }

                data.Add(dataObject);
            }

            return data;
        }

        public static List<string> Read(this DataTable dataTable)
        {
            var data = new List<string>();
            string line;

            foreach (DataRow row in dataTable.Rows)
            {
                line = "";

                foreach (DataColumn column in dataTable.Columns)
                {
                    line += $"{column.ColumnName}={row[column.ColumnName]},";
                }

                line = line.Remove(line.Length - 1);

                data.Add(line);
            }

            return data;
        }

        #endregion

        #region DbCommand Extensions

        private static void AddParameter(this DbCommand dbCommand, string parameterName, object? value = default)
        {
            DbParameter dbParameter = dbCommand.CreateParameter();

            if (parameterName[0] != '@') parameterName = $"@{parameterName}";

            dbParameter.ParameterName = parameterName;
            dbParameter.Value = value;

            dbCommand.Parameters.Add(dbParameter);
        }

        private static void AddParametersFromDictionary(this DbCommand dbCommand, Dictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                dbCommand.AddParameter(parameter.Key, parameter.Value);
            }
        }

        private static void AddParametersFromList(this DbCommand dbCommand, List<object> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (parameter is null) continue;

                var properties = parameters.GetType().GetProperties();
                foreach (var property in properties)
                {
                    if (property is null) continue;

                    dbCommand.AddParameter(property.Name, property.GetValue(parameter));
                }
            }
        }

        private static void AddParametersFromObject(this DbCommand dbCommand, object parameters)
        {
            var properties = parameters.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                if (property is null) continue;

                dbCommand.AddParameter(property.Name, property.GetValue(parameters));
            }
        }

        private static void AddParameters(this DbCommand dbCommand, object parameters)
        {
            //if (!ParametersTypeValidation(parameters)) throw new Exception("Type Error. object parameters type can be only dictionary<string,object> or list<object> or object");

            var type = parameters.GetType();

            if (type == typeof(Dictionary<string, object>)) dbCommand.AddParametersFromDictionary(parameters as Dictionary<string, object>);
            if (type == typeof(List<object>)) dbCommand.AddParametersFromList(parameters as List<object>);

            dbCommand.AddParametersFromObject(parameters);
        }

        #endregion

        #region DbConnection Extensions

        public static object SingleData(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            object expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = dbCommand.ExecuteReader(CommandBehavior.SingleResult))
                {
                    expectedData = dbDataReader.ReadSingleData();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static TEntityDto SingleRow<TEntityDto>(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            TEntityDto expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = dbCommand.ExecuteReader(CommandBehavior.SingleRow))
                {
                    expectedData = dbDataReader.ReadSingleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static IEnumerable<TEntityDto> MultipleRow<TEntityDto>(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            IEnumerable<TEntityDto> expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    expectedData = dbDataReader.ReadMultipleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static object ExecuteScalar(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            object expectedData;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                expectedData = dbCommand.ExecuteScalar();
            }

            dbConnection.Close();

            return expectedData;
        }

        public static bool ExecuteNonQuery(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            dbConnection.Close();

            return true;
        }

        public static DataSet DataAdapterFill(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            DataSet dataSet = new DataSet();

            dbConnection.Open();

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataAdapter adapter = new SqlDataAdapter((SqlCommand)dbCommand))
                {
                    adapter.Fill(dataSet);
                }
            }

            dbConnection.Close();

            return dataSet;
        }

        public static async Task<object> SingleDataAsync(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            object expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleResult))
                {
                    expectedData = dbDataReader.ReadSingleData();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<TEntityDto> SingleRowAsync<TEntityDto>(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            TEntityDto expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    expectedData = dbDataReader.ReadSingleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<IEnumerable<TEntityDto>> MultipleRowAsync<TEntityDto>(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            IEnumerable<TEntityDto> expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync())
                {
                    expectedData = dbDataReader.ReadMultipleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<object> ExecuteScalarAsync(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            object expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                expectedData = await dbCommand.ExecuteScalarAsync();
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<bool> ExecuteNonQueryAsync(this DbConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                try
                {
                    await dbCommand.ExecuteNonQueryAsync();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            dbConnection.Close();

            return true;
        }

        #endregion

        #region SqlConnection Extensions

        public static object SingleData(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            object expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = dbCommand.ExecuteReader(CommandBehavior.SingleResult))
                {
                    expectedData = dbDataReader.ReadSingleData();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static TEntityDto SingleRow<TEntityDto>(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            TEntityDto expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = dbCommand.ExecuteReader(CommandBehavior.SingleRow))
                {
                    expectedData = dbDataReader.ReadSingleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static IEnumerable<TEntityDto> MultipleRow<TEntityDto>(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            IEnumerable<TEntityDto> expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = dbCommand.ExecuteReader())
                {
                    expectedData = dbDataReader.ReadMultipleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static object ExecuteScalar(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            object expectedData = default;

            using (SqlCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                expectedData = dbCommand.ExecuteScalar();
            }

            dbConnection.Close();

            return expectedData;
        }

        public static bool ExecuteNonQuery(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            dbConnection.Open();

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                try
                {
                    dbCommand.ExecuteNonQuery();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            dbConnection.Close();

            return true;
        }

        public static async Task<object> SingleDataAsync(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            object expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleResult))
                {
                    expectedData = dbDataReader.ReadSingleData();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<TEntityDto> SingleRowAsync<TEntityDto>(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            TEntityDto expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync(CommandBehavior.SingleRow))
                {
                    expectedData = dbDataReader.ReadSingleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<IEnumerable<TEntityDto>> MultipleRowAsync<TEntityDto>(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            IEnumerable<TEntityDto> expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                using (DbDataReader dbDataReader = await dbCommand.ExecuteReaderAsync())
                {
                    expectedData = dbDataReader.ReadMultipleRow<TEntityDto>();
                };
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<object> ExecuteScalarAsync(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            object expectedData = default;

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                expectedData = await dbCommand.ExecuteScalarAsync();
            }

            dbConnection.Close();

            return expectedData;
        }

        public static async Task<bool> ExecuteNonQueryAsync(this SqlConnection dbConnection, string Query, object? Parameters = null, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(Query)) throw new Exception("Query cannot be null or empty.");

            await dbConnection.OpenAsync();

            using (DbCommand dbCommand = dbConnection.CreateCommand())
            {
                dbCommand.CommandText = Query;
                dbCommand.CommandType = commandType;

                if (Parameters != null) dbCommand.AddParameters(Parameters);

                try
                {
                    await dbCommand.ExecuteNonQueryAsync();
                }
                catch (Exception)
                {
                    return false;
                }
            }

            dbConnection.Close();

            return true;
        }

        #endregion
    }
}
