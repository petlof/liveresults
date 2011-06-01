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
using System.Threading;
using System.Data.Common;
using System.Collections.Generic;
using java.sql;
using org.h2.jdbcx;

namespace System.Data.H2
{
    /// <summary>
    /// Pools connections so that applications that opens and closes connections
    /// rapidly will not suffer.
    /// </summary>
    /// <remarks>It does not use the connection pool built into H2.</remarks>
    public sealed class H2ConnectionPool : IDisposable
    {
        #region fields
        object syncRoot;
        string connectionString;
        string userName;
        string password;
        bool inTimeout;
        bool isDisposed;
        int maxConnections;
        int currentCount;
        int connectionTimeout;
        ManualResetEvent waitHandle;
        Queue<Connection> avaliable; 
        #endregion
        #region constructors
        /// <summary>
        /// Creates a new H2ConnectionPool Instance.
        /// </summary>
        /// <param name="url">connection string</param>
        public H2ConnectionPool(string connectionString) : this(connectionString, null, null) { }
        /// <summary>
        /// Creates a new H2ConnectionPool Instance.
        /// </summary>
        /// <param name="url">connection string</param>
        /// <param name="userName">username for the database</param>
        /// <param name="password">password for the database</param>
        public H2ConnectionPool(string connectionString, string userName, string password)
        {
            this.syncRoot = new object();
            this.connectionTimeout = 2000;
            this.connectionString = connectionString;
            this.userName = userName;
            this.password = password;
            this.avaliable = new Queue<Connection>();
            this.waitHandle = new ManualResetEvent(false);
        } 
        #endregion
        #region properties
        /// <summary>
        /// The maximum number of connections that this pool will have open at the same time.
        /// </summary>
        public int MaxConnections
        {
            get { return maxConnections; }
            set { maxConnections = value; }
        }
        /// <summary>
        /// The amount of time after all connections are no longer in use that a connection 
        /// gets closed, repeating until all connections are closed.
        /// </summary>
        public int ConnectionTimeout
        {
            get { return connectionTimeout; }
            set { connectionTimeout = value; }
        } 
        #endregion
        #region methods
        internal Connection GetConnection()
        {
            return GetConnection(userName, password);
        }
        internal Connection GetConnection(string userName, string password)
        {
            lock (syncRoot)
            {
                if (isDisposed) { throw new ObjectDisposedException(GetType().Name); }
                waitHandle.Set();
                if (avaliable.Count > 0)
                {
                    return avaliable.Dequeue();
                }
                else
                {
                    if (currentCount < maxConnections)
                    {
                        Connection connection = DriverManager.getConnection(connectionString, userName, password);
                        currentCount++;
                        return connection;
                    }
                    else
                    {
                        Monitor.Wait(syncRoot);
                        if (avaliable.Count > 0)
                        {
                            return avaliable.Dequeue();
                        }
                        else
                        {
                            throw new ObjectDisposedException(GetType().Name);
                        }
                    }
                }
            }
        }
        internal void Enqueue(Connection connection)
        {
            lock (syncRoot)
            {
                if (isDisposed)
                {
                    connection.close();
                    currentCount--;
                    return;
                }
                connection.clearWarnings();
                avaliable.Enqueue(connection);
                Monitor.Pulse(syncRoot);
                RegisterTimout();
            }
        }
        void RegisterTimout()
        {
            if (!inTimeout &&
                avaliable.Count == currentCount &&
                currentCount > 0)
            {
                inTimeout = true;
                ThreadPool.RegisterWaitForSingleObject(waitHandle, TimeoutCallback, null, connectionTimeout, true);
            }
        }
        void TimeoutCallback(object state, bool timedout)
        {
            lock (syncRoot)
            {
                inTimeout = false;
                if (!timedout || isDisposed) { return; }
                if (avaliable.Count > 0)
                {
                    Connection connection = avaliable.Dequeue();
                    connection.close();
                    currentCount--;
                    RegisterTimout();
                }
            }
        }
        /// <summary>
        /// Gets a connection that will open from this pool.
        /// </summary>
        /// <returns>A H2Connection</returns>
        public H2Connection CreateConnection()
        {
            return new H2Connection(this);
        }
        /// <summary>
        /// Closes the pool and all its connections.
        /// </summary>
        public void Dispose()
        {
            lock (syncRoot)
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    Monitor.PulseAll(syncRoot);
                    foreach (Connection con in avaliable)
                    {
                        con.close();
                        currentCount--;
                    }
                    avaliable.Clear();
                    waitHandle.Set();
                    waitHandle.Close();
                    userName = null;
                    password = null;
                }
            }
        } 
        #endregion
    }
}