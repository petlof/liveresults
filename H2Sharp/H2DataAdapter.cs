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
    public sealed class H2DataAdapter : DbDataAdapter, IDbDataAdapter
    {
        static private readonly object EventRowUpdated = new object();
        static private readonly object EventRowUpdating = new object();

        public event EventHandler<H2RowUpdatingEventArgs> RowUpdating
        {
            add { Events.AddHandler(EventRowUpdating, value); }
            remove { Events.RemoveHandler(EventRowUpdating, value); }
        }
        public event EventHandler<H2RowUpdatedEventArgs> RowUpdated
        {
            add { Events.AddHandler(EventRowUpdated, value); }
            remove { Events.RemoveHandler(EventRowUpdated, value); }
        }

        private H2Command selectCommand;
        private H2Command insertCommand;
        private H2Command updateCommand;
        private H2Command deleteCommand;

        public H2DataAdapter()
        {
        }
        public H2DataAdapter(H2Command selectCommand)
        {
            this.selectCommand = selectCommand;
        }
        public H2DataAdapter(string selectCommandText, string selectConnectionString)
            : this(selectCommandText, new H2Connection(selectConnectionString))
        { }
        public H2DataAdapter(string selectCommandText, H2Connection selectConnection)
        {
            this.selectCommand = selectConnection.CreateCommand();
            this.selectCommand.CommandText = selectCommandText;
        }


        public new H2Command SelectCommand
        {
            get { return selectCommand; }
            set { selectCommand = value; }
        }
        IDbCommand IDbDataAdapter.SelectCommand
        {
            get { return selectCommand; }
            set { selectCommand = (H2Command)value; }
        }
        public new H2Command InsertCommand
        {
            get { return insertCommand; }
            set { insertCommand = value; }
        }
        IDbCommand IDbDataAdapter.InsertCommand
        {
            get { return insertCommand; }
            set { insertCommand = (H2Command)value; }
        }
        public new H2Command UpdateCommand
        {
            get { return updateCommand; }
            set { updateCommand = value; }
        }
        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get { return updateCommand; }
            set { updateCommand = (H2Command)value; }
        }
        public new H2Command DeleteCommand
        {
            get { return deleteCommand; }
            set { deleteCommand = value; }
        }
        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get { return deleteCommand; }
            set { deleteCommand = (H2Command)value; }
        }

        override protected RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new H2RowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }
        override protected RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping)
        {
            return new H2RowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }
        override protected void OnRowUpdating(RowUpdatingEventArgs value)
        {
            EventHandler<H2RowUpdatingEventArgs> handler = (EventHandler<H2RowUpdatingEventArgs>)Events[EventRowUpdating];
            if (null != handler)
            {
                handler(this, (H2RowUpdatingEventArgs)value);
            }
        }
        override protected void OnRowUpdated(RowUpdatedEventArgs value)
        {
            EventHandler<H2RowUpdatedEventArgs> handler = (EventHandler<H2RowUpdatedEventArgs>)Events[EventRowUpdated];
            if (null != handler)
            {
                handler(this, (H2RowUpdatedEventArgs)value);
            }
        }
    }
}