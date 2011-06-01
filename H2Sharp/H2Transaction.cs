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

namespace System.Data.H2
{
    public sealed class H2Transaction : DbTransaction
    {
        H2Connection connection;
        internal H2Transaction(H2Connection connection)
        {
            this.connection = connection;
        }

        public new H2Connection Connection { get { return connection; } }
        public override IsolationLevel IsolationLevel
        {
            get { return H2Helper.GetAdoTransactionLevel(connection.connection.getTransactionIsolation()); }
        }

        protected override DbConnection DbConnection
        {
            get { return connection; }
        }

        public override void Commit()
        {
            try
            {
                connection.connection.commit();
                connection.transaction = null;
            }
            catch (org.h2.jdbc.JdbcSQLException ex)
            {
                throw new H2Exception(ex);
            }
        }
        public override void Rollback()
        {
            try
            {
                connection.connection.rollback();
                connection.transaction = null;
            }
            catch (org.h2.jdbc.JdbcSQLException ex)
            {
                throw new H2Exception(ex);
            }
        }
    }
}