namespace Peasys
{
    /// <summary>
    /// Abstracts the concept of response in the case of a query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaResponse
    {
        public readonly bool HasSucceeded;
        public readonly string ReturnedSQLMessage;
        public readonly string ReturnedSQLState;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        protected internal PeaResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState)
        {
            HasSucceeded = hasSucceeded;
            ReturnedSQLMessage = returnedSQLMessage;
            ReturnedSQLState = returnedSQLState;
        }
    }

    /// <summary>
    /// Represents the concept of response in the case of a CREATE query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaCreateResponse : PeaResponse
    {
        public readonly string DatabaseName;
        public readonly string IndexName;
        public readonly Dictionary<string, ColumnInfo>? TableSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaCreateResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        /// <param name="databaseName">Name of the database if the SQL create query creates a new database.</param>
        /// <param name="indexName">Name of the index if the SQL create query creates a new index.</param>
        /// <param name="tableSchema">Schema of the table if the SQL create query creates a new table. 
        ///     The Schema is a Dictionary with columns' name as key and a <see cref="ColumnInfo"/> object as value.</param>
        protected internal PeaCreateResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState, string databaseName, string indexName, Dictionary<string, ColumnInfo>? tableSchema)
            : base(hasSucceeded, returnedSQLMessage, returnedSQLState)
        {
            DatabaseName = databaseName;
            IndexName = indexName;
            TableSchema = tableSchema;
        }
    }

    /// <summary>
    /// Represents the concept of response in the case of an ALTER query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaAlterResponse : PeaResponse
    {
        public readonly Dictionary<string, ColumnInfo>? TableSchema;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaAlterResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        /// <param name="tableSchema">Schema of the updated table that has been modified by the SQL ALTER query. 
        ///     The Schema is a Dictionary with columns' name as key and a <see cref="ColumnInfo"/> object as value.</param>
        protected internal PeaAlterResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState, Dictionary<string, ColumnInfo>? tableSchema) : base(hasSucceeded, returnedSQLMessage, returnedSQLState)
        {
            TableSchema = tableSchema;
        }
    }

    /// <summary>
    /// Represents the concept of response in the case of a DROP query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaDropResponse : PeaResponse
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PeaDropResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        protected internal PeaDropResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState) : base(hasSucceeded, returnedSQLMessage, returnedSQLState) { }
    }

    /// <summary>
    /// Represents the concept of response in the case of a SELECT query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaSelectResponse : PeaResponse
    {
        public readonly Dictionary<string, List<dynamic>> Result;
        public readonly int RowCount;
        public readonly string[] ColumnsName;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaSelectResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        /// <param name="result">Results of the query in the form of an Dictionary where the columns' name are the key and the values are the elements of this column in the SQL table.</param>
        /// <param name="rowCount">Represents the number of rows that have been retreived by the query.</param>
        /// <param name="columnsName">Array representing the name of the columns in the order of the SELECT query.</param>
        protected internal PeaSelectResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState, Dictionary<string, List<dynamic>> result, int rowCount, string[] columnsName)
            : base(hasSucceeded, returnedSQLMessage, returnedSQLState)
        {
            Result = result;
            RowCount = rowCount;
            ColumnsName = columnsName;
        }
    }

    /// <summary>
    /// Represents the concept of response in the case of an UPDATE query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaUpdateResponse : PeaResponse
    {
        public readonly int RowCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaUpdateResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        /// <param name="rowCount">Represents the number of rows that have been updated by the query.</param>
        protected internal PeaUpdateResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState, int rowCount)
            : base(hasSucceeded, returnedSQLMessage, returnedSQLState)
        {
            RowCount = rowCount;
        }
    }

    /// <summary>
    /// Represents the concept of response in the case of a DELETE query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaDeleteResponse : PeaResponse
    {
        public readonly int RowCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaDeleteResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        /// <param name="rowCount">Represents the number of rows that have been deleted by the query.</param>
        protected internal PeaDeleteResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState, int rowCount)
            : base(hasSucceeded, returnedSQLMessage, returnedSQLState)
        {
            RowCount = rowCount;
        }
    }

    /// <summary>
    /// Represents the concept of response in the case of an INSERT query executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaInsertResponse : PeaResponse
    {
        public readonly int RowCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaInsertResponse"/> class.
        /// </summary>
        /// <param name="hasSucceeded">Boolean set to true if the query has correctly been executed. Set to true if the SQL state is 00000.</param>
        /// <param name="returnedSQLMessage">SQL message return from the execution of the query.</param>
        /// <param name="returnedSQLState">SQL state return from the execution of the query.</param>
        /// <param name="rowCount">Represents the number of rows that have been inserted by the query.</param>
        protected internal PeaInsertResponse(bool hasSucceeded, string returnedSQLMessage, string returnedSQLState, int rowCount)
            : base(hasSucceeded, returnedSQLMessage, returnedSQLState) => RowCount = rowCount;
    }

    /// <summary>
    /// Represents the concept of response in the case of an OS/400 command executed on the database of an AS/400 server by a <see cref="PeaClient"/> object.
    /// </summary>
    public class PeaCommandResponse
    {
        public readonly List<string> Warnings;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeaCommandResponse"/> class.
        /// </summary>
        /// <param name="HasSucceeded">Boolean set to true if the command has correctly been executed meaning that no CPFxxxx was return. Still, description messages can 
        /// be return along with CP*xxxx.</param>
        /// <param name="Errors">List of warnings that results from the command execution. Errors are of the form : CP*xxxx Description of the warning.</param>
        public PeaCommandResponse(List<string> Warnings)
        {
            this.Warnings = Warnings;
        }
    }

    /// <summary>
    /// Represents the concept of metadata for a column of an SQL table on the AS/400 server.
    /// </summary>
    public class ColumnInfo
    {
        public readonly string ColumnName;
        public readonly int OrdinalPosition;
        public readonly string DataType;
        public readonly int Length;
        public readonly int NumericScale;
        public readonly string IsNullable;
        public readonly string IsUpdatable;
        public readonly int NumericPrecision;

        /// <summary>
        /// Initializes a new instance of the <see cref="ColumnInfo"/> class.
        /// </summary>
        /// <param name="columnName">Name of the column.</param> 
        /// <param name="ordinalPosition">Ordinal position of the column.</param>
        /// <param name="dataType">DB2 Type of the data contain in the column.</param>
        /// <param name="length">Length of the data contain in the column.</param>
        /// <param name="numericScale">Scale of the data contain in the column if numeric type.</param>
        /// <param name="isNullable">Y/N depending on the nullability of the field.</param>
        /// <param name="isUpdatable">Y/N depending on the updaptability of the field.</param>
        /// <param name="numericPrecision">Precision of the data contain in the column if numeric type.</param>
        protected internal ColumnInfo(string columnName, int ordinalPosition, string dataType, int length, int numericScale, string isNullable,
            string isUpdatable, int numericPrecision)
        {
            ColumnName = columnName;
            OrdinalPosition = ordinalPosition;
            DataType = dataType;
            Length = length;
            NumericScale = numericScale;
            IsNullable = isNullable;
            IsUpdatable = isUpdatable;
            NumericPrecision = numericPrecision;
        }
    }
}
