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

using System.Collections.Generic;
using System.Text;
using java.sql;

using DbCommand = System.Data.Common.DbCommand;
using DbConnection = System.Data.Common.DbConnection;
using DbDataReader = System.Data.Common.DbDataReader;
using DbParameter = System.Data.Common.DbParameter;
using DbParameterCollection = System.Data.Common.DbParameterCollection;
using DbTransaction = System.Data.Common.DbTransaction;

namespace System.Data.H2
{
    public sealed class H2Command : DbCommand
    {
        #region sub classes
        class PreparedTemplate
        {
            private string oldSql;
            private string trueSql;
            private int[] mapping;
            public PreparedTemplate(string oldSql, string trueSql, int[] mapping)
            {
                this.oldSql = oldSql;
                this.trueSql = trueSql;
                this.mapping = mapping;
            }
            public string OldSql
            {
                get { return oldSql; }
            }
            public string TrueSql
            {
                get { return trueSql; }
            }
            public int[] Mapping
            {
                get { return mapping; }
            }
        } 
        #endregion

        #region static
        static Dictionary<string, PreparedTemplate> templates = new Dictionary<string, PreparedTemplate>();
        static object syncRoot = new object();

        private static int[] CreateRange(int length)
        {
            int[] result = new int[length];
            for (int index = 0; index < length; ++index)
            {
                result[index] = index;
            }
            return result;
        }
        #endregion

        #region fields
        H2Connection connection;
        CommandType commandType;
        string commandText;
        int commandTimeout = 30;
        bool timeoutSet;
        bool designTimeVisible;
        H2ParameterCollection collection;
        PreparedStatement statement;
        PreparedTemplate template;
        UpdateRowSource updatedRowSource; 
        bool disableNamedParameters;
        #endregion

        #region constructors
        public H2Command()
            : this(null, null, null)
        { }
        public H2Command(H2Connection connection)
            : this(null, connection, null)
        { }
        public H2Command(string commandText)
            : this(commandText, null, null)
        { }
        public H2Command(string commandText, H2Connection connection)
            : this(commandText, connection, null)
        { }
        public H2Command(string commandText, H2Connection connection, H2Transaction transaction)
        {
            this.commandText = commandText;
            this.connection = connection;
            this.collection = new H2ParameterCollection();
            this.updatedRowSource = UpdateRowSource.None;
        } 
        #endregion

        #region properties
        public new H2Connection Connection
        {
            get { return connection; }
            set { connection = value; }
        }
        public new H2ParameterCollection Parameters
        {
            get { return collection; }
        }
        public new H2Transaction Transaction
        {
            get
            {
                if (connection == null) { return null; }
                return connection.transaction;
            }
            set
            {
                if (value == null) { throw new ArgumentNullException("value"); }
                this.connection = value.Connection;
            }
        }

        protected override DbConnection DbConnection
        {
            get
            {
                return connection;
            }
            set
            {
                connection = (H2Connection)value;
            }
        }
        protected override DbParameterCollection DbParameterCollection
        {
            get { return collection; }
        }
        protected override DbTransaction DbTransaction
        {
            get
            {
                return Transaction;
            }
            set
            {
                this.Transaction = (H2Transaction)value;
            }
        }

        public override string CommandText
        {
            get
            {
                return commandText;
            }
            set
            {
                commandText = value;
            }
        }
        public override int CommandTimeout
        {
            get
            {
                return commandTimeout;
            }
            set
            {
                timeoutSet = true;
                commandTimeout = value;
            }
        }
        public override CommandType CommandType
        {
            get { return commandType; }
            set { commandType = value; }
        }
        public override bool DesignTimeVisible
        {
            get
            {
                return designTimeVisible;
            }
            set
            {
                designTimeVisible = value;
            }
        }
        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return updatedRowSource;
            }
            set
            {
                updatedRowSource = value;
            }
        }
        /// <summary>
        /// This is here if you are having problems with Named Parameters. it turns them off.
        /// if you have to set this to true inform me at (http://groups.google.com/group/H2Sharp)
        /// </summary>
        public bool DisableNamedParameters
        {
            get { return disableNamedParameters; }
            set { disableNamedParameters = value; }
        }
        private bool IsNamed
        {
            get
            {
                if (disableNamedParameters) { return false; }
                bool inQuote = false;
                for (int index = 0; index < commandText.Length; ++index)
                {
                    char c = commandText[index];
                    if (!inQuote && c == '@')
                    {
                        return true;
                    }
                    else if (c == '\'')
                    {
                        inQuote = !inQuote;
                    }
                }
                return false;
            }
        }
        #endregion

        #region methods
        private void CheckConnection()
        {
            if (connection == null) { throw new H2Exception("DbConnection must be set."); }
            connection.CheckIsOpen();
        }
        private PreparedTemplate CreateNameTemplate()
        {
            List<int> list = new List<int>();
            StringBuilder command = new StringBuilder();
            StringBuilder name = new StringBuilder();
            bool inQuote = false;
            for (int index = 0; index < commandText.Length; ++index)
            {
                char c = commandText[index];
                if (name.Length == 0)
                {
                    if (!inQuote && c == '@')
                    {
                        name.Append(c);
                    }
                    else
                    {
                        if (c == '\'')
                        {
                            inQuote = !inQuote;
                        }
                        command.Append(c);
                    }
                }
                else
                {
                    if (char.IsLetterOrDigit(c) || c == '_')
                    {
                        name.Append(c);
                    }
                    else
                    {
                        command.Append('?');
                        command.Append(c);
                        string paramName = name.ToString();
                        name.Length = 0;
                        int paramIndex = collection.FindIndex(delegate(H2Parameter p) { return p.ParameterName == paramName; });
                        if (paramIndex == -1) { throw new H2Exception(string.Format("Missing Parameter: {0}", paramName)); }
                        list.Add(paramIndex);
                    }
                }
            }
            return new PreparedTemplate(commandText, command.ToString(), list.ToArray());
        }
        private PreparedTemplate CreateIndexTemplate()
        {
            int count = 0;
            int index = -1;
            while ((index = commandText.IndexOf('?', index + 1)) != -1)
            {
                count++;
            }
            return new PreparedTemplate(commandText, commandText, CreateRange(count));
        }
        private void CreateStatement()
        {
            if (statement != null)
            {
                statement.close();
            }
            try
            {
                statement = connection.connection.prepareStatement(template.TrueSql);
            }
            catch (org.h2.jdbc.JdbcSQLException ex)
            {
                throw new H2Exception(ex);
            }
            if (timeoutSet)
            {
                statement.setQueryTimeout(commandTimeout);
            }
        }

       
        private void EnsureStatment()
        {
            if (commandText == null) { throw new InvalidOperationException("must set CommandText"); }
            if (template == null || template.OldSql != commandText)
            {
                lock (syncRoot)
                {
                    if (!templates.TryGetValue(commandText, out template))
                    {
                        if (IsNamed)
                        {
                            template = CreateNameTemplate();
                        }
                        else
                        {
                            template = CreateIndexTemplate();
                        }
                        templates.Add(commandText, template);
                    }
                }
                CreateStatement();
            }
            else
            {
                statement.clearParameters();
            }
            for (int index = 0; index < template.Mapping.Length; ++index)
            {
                collection[template.Mapping[index]].SetStatement(index + 1, statement);
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            return new H2Parameter();
        }
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            return ExecuteReader(behavior);
        }

        public new H2Parameter CreateParameter()
        {
            return new H2Parameter();
        }
        public new H2DataReader ExecuteReader()
        {
            return ExecuteReader(CommandBehavior.Default);
        }
        public new H2DataReader ExecuteReader(CommandBehavior behavior)
        {
            if (behavior != CommandBehavior.Default) { throw new NotSupportedException("Only CommandBehavior Default is supported for now."); }
            CheckConnection();
            EnsureStatment();
            try
            {
                return new H2DataReader(statement.executeQuery());
            }
            catch(org.h2.jdbc.JdbcSQLException ex)
            {
                throw new H2Exception(ex);
            }
        }
        public override void Cancel()
        {
            CheckConnection();
            if (statement != null)
            {
                try
                {
                    statement.cancel();
                }
                catch (org.h2.jdbc.JdbcSQLException ex)
                {
                    throw new H2Exception(ex);
                }
            }
        }
        public override int ExecuteNonQuery()
        {
            CheckConnection();
            EnsureStatment();
            try
            {
                return statement.executeUpdate();
            }
            catch (org.h2.jdbc.JdbcSQLException ex)
            {
                throw new H2Exception(ex);
            }
        }
        public override object ExecuteScalar()
        {
            CheckConnection();
            EnsureStatment();
            object result = null;
            try
            {
                ResultSet set = statement.executeQuery();
                try
                {
                    if (set.next())
                    {
                        result = set.getObject(1);
                        result = H2Helper.ConvertToDotNet(result);
                    }
                }
                finally
                {
                    set.close();
                }
                return result;
            }
            catch (org.h2.jdbc.JdbcSQLException ex)
            {
                throw new H2Exception(ex);
            }
        }
        public override void Prepare()
        {
            CheckConnection();
            EnsureStatment();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (statement != null)
                {
                    statement.close();
                    statement = null;
                }
            }
        } 
        #endregion
    }
}