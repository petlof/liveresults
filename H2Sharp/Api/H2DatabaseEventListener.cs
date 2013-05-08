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

using System;

namespace System.Data.H2.Api
{
    [Serializable]
    public enum ProgressState
    {
        /// <summary>
        /// This state is used when scanning the data or index file.
        /// </summary>
        ScanFile = 0,
        /// <summary>
        /// This state is used when re-creating an index.
        /// </summary>
        CreateIndex = 1,
        /// <summary>
        /// This state is used when re-applying the transaction log or rolling back uncommitted transactions.
        /// </summary>
        Recover = 2,
        /// <summary>
        /// This state is used during the BACKUP command.
        /// </summary>
        BackupFile = 3
    }

    /// <summary>
    /// A class that implements this interface can get notified about exceptions 
    /// and other events. A database event listener can be registered when 
    /// connecting to a database. Example database URL: 
    /// jdbc:h2:test;DATABASE_EVENT_LISTENER='com.acme.DbListener'
    /// </summary>
    public abstract class H2DatabaseEventListener : org.h2.api.DatabaseEventListener
    {
        void org.h2.api.DatabaseEventListener.closingDatabase()
        {
            OnClosingDatabase();
        }
        /// <summary>
        /// This method is called before the database is closed normally. It is safe
        /// to connect to the database and execute statements at this point, however
        /// the connection must be closed before the method returns.
        /// </summary>
        protected abstract void OnClosingDatabase();

        /*void org.h2.api.DatabaseEventListener.diskSpaceIsLow()
        {
            OnDiskSpaceIsLow();
        }*/

        /// <summary>
        /// This method is called if the disk space is very low.
        /// One strategy is to inform the user and wait for it to clean up disk space.
        /// Another strategy is to send an email to the administrator in this method and
        /// then throw a SQLException. The database should not be accessed from
        /// within this method (even to close it).
        /// </summary>
        /// <param name="stillAvailable">the estimated space that is still available, in bytes</param>
        /// <remarks>throw SQLException if the operation should be cancelled</remarks>
        protected abstract void OnDiskSpaceIsLow();

        void org.h2.api.DatabaseEventListener.exceptionThrown(java.sql.SQLException e, String sql)
        {
            OnExceptionThrown(new H2Exception(e), sql);
        }
        /// <summary>
        /// This method is called if an exception occurred during database recovery
        /// </summary>
        /// <param name="e">the exception</param>
        /// <param name="sql">the SQL statement</param>
        protected abstract void OnExceptionThrown(H2Exception e, String sql);

        void org.h2.api.DatabaseEventListener.init(String url)
        {
            OnInit(url);
        }
        /// <summary>
        /// This method is called just after creating the object.
        /// This is done when opening the database if the listener 
        /// is specified  in the database URL, but may be later if
        /// the listener is set at  runtime with the SET SQL statement.
        /// </summary>
        /// <param name="url">the database URL</param>
        protected abstract void OnInit(String url);

        void org.h2.api.DatabaseEventListener.opened()
        {
            OnOpened();
        }
        /// <summary>
        /// This method is called after the database has been opened.
        /// It is safe to connect to the database and execute statements at this point.
        /// </summary>
        protected abstract void OnOpened();

        void org.h2.api.DatabaseEventListener.setProgress(int state, String name, int x, int max)
        {
            OnSetProgress((ProgressState)state, name, x, max);
        }

        /// <summary>
        /// This method is called for long running events, such as recovering,
        /// scanning a file or building an index.
        /// </summary>
        /// <param name="state">the state</param>
        /// <param name="name">the object name</param>
        /// <param name="x">the current position</param>
        /// <param name="max">the highest value</param>
        protected abstract void OnSetProgress(ProgressState state, String name, int x, int max);

    }
}