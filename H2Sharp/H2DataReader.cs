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
using System.Globalization;

namespace System.Data.H2
{


    public sealed class H2DataReader : DbDataReader
    {
        private static int ConvertOrdnal(int ordinal)
        {
            if (ordinal == int.MaxValue) { throw new H2Exception("invalid ordinal"); }
            return ordinal+1;
        }

        private ResultSet set;
        private ResultSetMetaData meta;

        internal H2DataReader(ResultSet set)
        {
            this.set = set;
        }


        private ResultSetMetaData Meta
        {
            get
            {
                if (meta == null)
                {
                    meta = set.getMetaData();
                }
                return meta;
            }
        }
        public override bool IsDBNull(int ordinal)
        {
            return set.getObject(ConvertOrdnal(ordinal)) == null;
        }
        public override bool NextResult()
        {
            throw new NotImplementedException();
        }
        public override int RecordsAffected
        {
            get { throw new NotImplementedException(); }
        }
        public override bool HasRows
        {
            get { throw new NotImplementedException(); }
        }
        public override bool IsClosed
        {
            get { return set.isClosed(); }
        }
        public override object this[string name]
        {
            get { return set.getObject(name); }
        }
        public override object this[int ordinal]
        {
            get { return set.getObject(ConvertOrdnal(ordinal)); }
        }
        public override int Depth
        {
            get { return Meta.getColumnCount(); }
        }
        public override int FieldCount
        {
            get { return Meta.getColumnCount(); }
        }


        public override bool GetBoolean(int ordinal)
        {
            return set.getBoolean(ConvertOrdnal(ordinal));
        }
        public override byte GetByte(int ordinal)
        {
            return set.getByte(ConvertOrdnal(ordinal));
        }
        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            Byte[] rv = set.getBytes(ConvertOrdnal(ordinal));
            Array.Copy(rv, dataOffset, buffer, bufferOffset, length);
            return length;
        }
        public override char GetChar(int ordinal)
        {
            throw new NotImplementedException();
        }
        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            throw new NotImplementedException();
        }
        public override string GetDataTypeName(int ordinal)
        {
            throw new NotImplementedException();
        }
        public override DateTime GetDateTime(int ordinal)
        {
            throw new NotImplementedException();
        }
        public override decimal GetDecimal(int ordinal)
        {
            throw new NotImplementedException();
        }
        public override double GetDouble(int ordinal)
        {
            return set.getDouble(ConvertOrdnal(ordinal));
        }
        public override System.Collections.IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        
        public override Type GetFieldType(int ordinal)
        {

            object temp = set.getObject(ConvertOrdnal(ordinal));
            if (temp == null)
            {
                int typeCode = Meta.getColumnType(ConvertOrdnal(ordinal));
                Type rv = H2Helper.GetType(typeCode);
                if (rv != null)
                {
                    return rv;
                }
                string type = Meta.getColumnTypeName(ConvertOrdnal(ordinal));
                throw new NotImplementedException(type);
            }
            Type result = temp.GetType();
            if (result == typeof(java.lang.Integer))
            {
                result = typeof(int);
            }
            else if (result == typeof(java.lang.Long))
            {
                result = typeof(long);
            }
            else if (result == typeof(java.lang.Short))
            {
                result = typeof(short);
            }
            else if (result == typeof(java.lang.String))
            {
                result = typeof(String);
            }
            return result;
        }
        public override float GetFloat(int ordinal)
        {
            return set.getFloat(ConvertOrdnal(ordinal));
        }
        public override Guid GetGuid(int ordinal)
        {
            throw new NotImplementedException();
        }
        public override short GetInt16(int ordinal)
        {
            return set.getShort(ConvertOrdnal(ordinal));
        }
        public override int GetInt32(int ordinal)
        {
            return set.getInt(ConvertOrdnal(ordinal));
        }
        public override long GetInt64(int ordinal)
        {
            return set.getLong(ConvertOrdnal(ordinal));
        }
        public override string GetName(int ordinal)
        {
            return Meta.getColumnName(ConvertOrdnal(ordinal));
        }
        public override int GetOrdinal(string name)
        {
            for (int index = 0; index < Meta.getColumnCount(); ++index)
            {
                if (string.Compare(Meta.getColumnName(ConvertOrdnal(index)), name, true) == 0)
                {
                    return index;
                }
            }
            return -1;
        }
        public override DataTable GetSchemaTable()
        {
            throw new NotImplementedException();
        }
        public override string GetString(int ordinal)
        {
            return set.getString(ConvertOrdnal(ordinal));
        }
        public override object GetValue(int ordinal)
        {
            object result = set.getObject(ConvertOrdnal(ordinal));
            result = H2Helper.ConvertToDotNet(result);
            return result;
        }
        public override int GetValues(object[] values)
        {
            if (values == null) { throw new ArgumentNullException("values"); }
            for (int index = 0; index < values.Length; ++index)
            {
                values[index] = GetValue(index);
            }
            return values.Length;
        }
        public override bool Read()
        {
            return set.next();
        }
        public override void Close()
        {
            set.close();
        }

    }
}