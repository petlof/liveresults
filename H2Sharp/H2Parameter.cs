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

using System.Data.Common;
using java.sql;

namespace System.Data.H2
{

    public sealed class H2Parameter : DbParameter
    {
        ParameterDirection direction = ParameterDirection.Input;
        bool isNullable;
        bool isTypeSet;
        string parameterName;
        int size;
        object value;
        object javaValue;
        DbType dbType = DbType.Object;
        int javaType;
        string sourceColumn;
        bool sourceColumnNullMapping;
        DataRowVersion sourceVersion = DataRowVersion.Current;

        public H2Parameter() { }
        public H2Parameter(string parameterName)
        {
            this.parameterName = parameterName;
        }
        public H2Parameter(string parameterName, object value)
        {
            this.parameterName = parameterName;
            this.Value = value;
        }
        public H2Parameter(object value)
        {
            this.Value = value;
        }

        public H2Parameter(string name, DbType dataType)
        {
            this.parameterName = name;
            this.DbType = dbType;
        }
        public H2Parameter(string name, DbType dataType, int size)
        {
            this.parameterName = name;
            this.DbType = dbType;
            this.size = size;
        }
        public H2Parameter(string name, DbType dataType, int size, string sourceColumn)
        {
            this.parameterName = name;
            this.DbType = dbType;
            this.size = size;
            this.sourceColumn = sourceColumn;
        }
        public H2Parameter(
                     string name,
                     DbType dbType,
                     int size,
                     ParameterDirection direction,
                     Boolean isNullable,
                     Byte precision,
                     Byte scale,
                     string sourceColumn,
                     DataRowVersion sourceVersion,
                     object value)
        {
            this.parameterName = name;
            this.DbType = dbType;
            this.size = size;
            this.direction = direction;
            this.isNullable = isNullable;
            this.sourceColumn = sourceColumn;
            this.sourceVersion = sourceVersion;
            this.Value = value;
        }


        public override DbType DbType
        {
            get { return dbType; }
            set
            {
                isTypeSet = true;
                dbType = value;
                javaType = H2Helper.GetTypeCode(value);
            }
        }
        public override ParameterDirection Direction
        {
            get { return direction; }
            set
            {
                if (value != ParameterDirection.Input) { throw new NotSupportedException(); }
                direction = value;
            }
        }
        public override bool IsNullable
        {
            get { return isNullable; }
            set { isNullable = value; }
        }
        public override string ParameterName
        {
            get { return parameterName; }
            set { this.parameterName = value; }
        }
        public override int Size
        {
            get { return size; }
            set { size = value; }
        }
        public override string SourceColumn
        {
            get
            {
                return sourceColumn;
            }
            set
            {
                sourceColumn = value;
            }
        }
        public override bool SourceColumnNullMapping
        {
            get
            {
                return sourceColumnNullMapping;
            }
            set
            {
                sourceColumnNullMapping = value;
            }
        }
        public override DataRowVersion SourceVersion
        {
            get
            {
                return sourceVersion;
            }
            set
            {
                sourceVersion = value;
            }
        }
        public override object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                this.javaValue = H2Helper.ConvertToJava(value);
            }
        }

        public override void ResetDbType()
        {
            dbType = DbType.Object;
            isTypeSet = false;
        }
        internal void SetStatement(int ordnal, PreparedStatement statement)
        {
            if (isTypeSet)
            {
                statement.setObject(ordnal, javaValue, javaType);
            }
            else
            {
                statement.setObject(ordnal, javaValue);
            }
        }
    }
}