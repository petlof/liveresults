#region MIT License
/*
 * Copyright © 2008 Jonathan Mark Porter.
 * H2Sharp is a wrapper for the H2 Database Engine. http://h2sharp.googlecode.com
 * 
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion
using java.sql;

namespace System.Data.H2
{
    static class H2Helper
    {
        public static int GetTypeCode(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    return Types.VARCHAR;
                case DbType.AnsiStringFixedLength:
                    return Types.CHAR;
                case DbType.Binary:
                    return Types.BINARY;
                case DbType.Boolean:
                    return Types.BOOLEAN;
                case DbType.Byte:
                    return Types.TINYINT;
                case DbType.Currency:
                    throw new NotSupportedException();
                case DbType.Date:
                    return Types.DATE;
                case DbType.DateTime:
                    return Types.TIMESTAMP;
                case DbType.DateTime2:
                    return Types.TIMESTAMP;
                case DbType.DateTimeOffset:
                    return Types.TIMESTAMP;
                case DbType.Decimal:
                    return Types.DECIMAL;
                case DbType.Double:
                    return Types.DOUBLE;
                case DbType.Guid:
                    throw new NotSupportedException();
                case DbType.Int16:
                    return Types.SMALLINT;
                case DbType.Int32:
                    return Types.INTEGER;
                case DbType.Int64:
                    return Types.BIGINT;
                case DbType.Object:
                    return Types.JAVA_OBJECT;
                case DbType.SByte:
                    return Types.TINYINT;
                case DbType.Single:
                    return Types.FLOAT;
                case DbType.String:
                    return Types.NVARCHAR;
                case DbType.StringFixedLength:
                    return Types.NCHAR;
                case DbType.Time:
                    return Types.TIME;
                case DbType.UInt16:
                    throw new NotSupportedException();
                case DbType.UInt32:
                    throw new NotSupportedException();
                case DbType.UInt64:
                    throw new NotSupportedException();
                case DbType.VarNumeric:
                    return Types.ARRAY;
                case DbType.Xml:
                    throw new NotSupportedException();
                default :
                    throw new ArgumentOutOfRangeException("dbType");
            }
        }

        public static IsolationLevel GetAdoTransactionLevel(int level)
        {
            switch (level)
            {
                case java.sql.Connection.__Fields.TRANSACTION_NONE:
                    return IsolationLevel.Unspecified;
                case java.sql.Connection.__Fields.TRANSACTION_READ_COMMITTED:
                    return IsolationLevel.ReadCommitted;
                case java.sql.Connection.__Fields.TRANSACTION_READ_UNCOMMITTED:
                    return IsolationLevel.ReadUncommitted;
                case java.sql.Connection.__Fields.TRANSACTION_REPEATABLE_READ:
                    return IsolationLevel.RepeatableRead;
                case java.sql.Connection.__Fields.TRANSACTION_SERIALIZABLE:
                    return IsolationLevel.Serializable;
                default:
                    throw new NotSupportedException("unsupported transaction level");
            }
        }
        public static int GetJdbcTransactionLevel(IsolationLevel level)
        {
            switch (level)
            {
                case IsolationLevel.Unspecified:
                    return java.sql.Connection.__Fields.TRANSACTION_NONE;
                case IsolationLevel.ReadCommitted:
                    return java.sql.Connection.__Fields.TRANSACTION_READ_COMMITTED;
                case IsolationLevel.ReadUncommitted:
                    return java.sql.Connection.__Fields.TRANSACTION_READ_UNCOMMITTED;
                case IsolationLevel.RepeatableRead:
                    return java.sql.Connection.__Fields.TRANSACTION_REPEATABLE_READ;
                case IsolationLevel.Serializable:
                    return java.sql.Connection.__Fields.TRANSACTION_SERIALIZABLE;
                default:
                    throw new NotSupportedException("unsupported transaction level");
            }
        }

        public static object ConvertToDotNet(object result)
        {
            if (result is java.lang.Integer)
            {
                result = ((java.lang.Integer)result).intValue();
            }
            else if (result is java.lang.Long)
            {
                result = ((java.lang.Long)result).longValue();
            }
            else if (result is java.lang.Short)
            {
                result = ((java.lang.Short)result).shortValue();
            }
            else if (result is java.lang.Number)
            {
                result = ((java.lang.Number)result).intValue();
            }
            return result;
        }
        public static object ConvertToJava(object result)
        {
            if (result is int)
            {
                result = new java.lang.Integer((int)result);
            }
            else if (result is long)
            {
                result = new java.lang.Long((long)result);
            }
            else if (result is short)
            {
                result = new java.lang.Short((short)result);
            }
            return result;
        }

        public static Type GetType(int typeCode)
        {
            switch (typeCode)
            {
                case 0:
                    return typeof(DBNull);
                case -2:
                case 2004:
                case -4:
                case -3:
                    return typeof(byte[]);
                case -1:
                case 12:
                    return typeof(string);
                case 4:
                    return typeof(int);
                case 3:
                    return typeof(decimal);
                case 1:
                    return typeof(char);
                case 6:
                    return typeof(float);
                case 8:
                    return typeof(double);
                case -5:
                    return typeof(long);
                case 5:
                    return typeof(short);
                default:
                    return null;
            }
        }
    }
}