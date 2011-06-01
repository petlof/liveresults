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

namespace System.Data.H2.Api
{

    [Serializable,Flags]
    public enum TriggerTypes
    {
        /// <summary>
        /// The trigger is called for Insert statements.
        /// </summary>
        Insert = 1,
        /// <summary>
        /// The trigger is called for Update statements.
        /// </summary>
        Update = 2,
        /// <summary>
        /// The trigger is called for Delete statements.
        /// </summary>
        Delete = 4,
    }

    /// <summary>
    /// A class that implements this abstract class can be used as a trigger.
    /// </summary>
    public abstract class H2Trigger : org.h2.api.Trigger
    {
        void org.h2.api.Trigger.fire(java.sql.Connection conn, object[] oldRow, object[] newRow)
        {
            H2Connection connection = new H2Connection(conn);
            OnFire(connection, oldRow, newRow);
        }

        /// <summary>
        /// This method is called for each triggered action.
        /// </summary>
        /// <param name="connection"> a connection to the database</param>
        /// <param name="oldRow">the old row, or null if no old row is available (for INSERT)</param>
        /// <param name="newRow">the new row, or null if no new row is available (for DELETE)</param>
        /// <remarks>throw SQLException if the operation must be undone</remarks>
        protected abstract void OnFire(H2Connection connection, object[] oldRow, object[] newRow);

        void org.h2.api.Trigger.init(java.sql.Connection conn, String schemaName, String triggerName, String tableName, bool before, int type)
        {
            H2Connection connection = new H2Connection(conn);
            OnInit(connection, schemaName, triggerName, tableName, before, (TriggerTypes)type);
        }
        /// <summary>
        /// This method is called by the database engine once when initializing the trigger.
        /// </summary>
        /// <param name="connection">a connection to the database</param>
        /// <param name="schemaName">the name of the schema</param>
        /// <param name="triggerName">the name of the trigger used in the CREATE TRIGGER statement</param>
        /// <param name="tableName">the name of the table</param>
        /// <param name="before">whether the fire method is called before or after the operation is performed</param>
        /// <param name="type">the operation type: INSERT, UPDATE, or DELETE</param>
        protected abstract void OnInit(H2Connection connection, String schemaName, String triggerName, String tableName, bool before, TriggerTypes type);

        #region Trigger Members

        public void close()
        {
        }

        public void remove()
        {
        }

        #endregion
    }

}