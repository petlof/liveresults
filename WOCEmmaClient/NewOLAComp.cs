using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.H2;

namespace WOCEmmaClient
{
    public partial class NewOLAComp : Form
    {
        public NewOLAComp()
        {
            InitializeComponent();
            comboBox1.DataSource = new string[] { "OLA 5.0 Internal", "OLA4 Internal Mysql", "Mysql-Server", "SQL-Server" };
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    txtHost.Enabled = txtPort.Enabled = txtUser.Enabled = txtPw.Enabled = false;
                    break;
                case 1:
                    txtHost.Enabled = txtPort.Enabled = txtUser.Enabled = txtPw.Enabled = true;
                    txtHost.Text = "localhost";
                    txtPort.Text = "3307";
                    txtUser.Text = "live";
                    txtPw.Text = "live";
                    break;
                case 2:
                    txtHost.Enabled = txtPort.Enabled = txtUser.Enabled = txtPw.Enabled = true;
                    txtPort.Text = "3309";
                    break;
                case 3:
                    txtHost.Enabled = txtPort.Enabled = txtUser.Enabled = txtPw.Enabled = true;
                    txtPort.Text = "1433";
                    break;
            }
        }

        private void wizardPage2_ShowFromBack(object sender, EventArgs e)
        {
            /*Try Connect*/
            IDbConnection conn = null;
            try
            {
            conn = GetDBConnection();
            conn.Open();
            lstDB.DataSource = null;
            
                string[] databases = GetDatabases(conn);
                lstDB.DataSource = databases;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, ee.Message);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private string[] GetDatabases(IDbConnection conn)
        {
            List<string> dbs = new List<string>();
            if (conn is OleDbConnection)
            {
                OleDbConnection oleConn = conn as OleDbConnection;
                DataTable schemas = oleConn.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null);
                //dataGridView1.DataSource = schemas;
                foreach (DataRow r in schemas.Rows)
                {
                    dbs.Add(r["CATALOG_NAME"] as string);
                }
            }
            else if (conn is MySql.Data.MySqlClient.MySqlConnection)
            {
                IDbCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SHOW DATABASES";
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string db = reader[0] as string;
                    if (db != "mysql" && db != "information_schema")
                    {
                        dbs.Add(db);
                    }
                }
                reader.Close();
            }
            return dbs.ToArray();
        }

        private IDbConnection GetDBConnection()
        {
            return GetDBConnection(null);
        }

        private IDbConnection GetDBConnection(string schema)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    return new H2Connection("jdbc:h2://" + txtOlaDb.Text + ";AUTO_SERVER=TRUE", "live", "live");
                case 1:
                case 2:
                    return new MySql.Data.MySqlClient.MySqlConnection("Server=" + txtHost.Text + ";User Id=" + txtUser.Text + ";Port=" + txtPort.Text + ";Password=" + txtPw.Text + (schema != null ? ";Initial Catalog=" + schema : ""));
                case 3:
                    return new OleDbConnection("Provider=SQLOLEDB.1;Persist Security Info=False;User ID=" + txtUser.Text + ";Password=" + txtPw.Text + ";Data Source=" + txtHost.Text + (schema != null ? ";Initial Catalog=" + schema : ""));
            }
            return null;
        }

        private void wizardPage3_ShowFromNext(object sender, EventArgs e)
        {
            /*Try Connect*/
            IDbConnection conn = null;
            try
            {
                conn = GetDBConnection(lstDB.SelectedItem as string);
                conn.Open();

                IDbCommand cmd = conn.CreateCommand();
                cmbOLAComp.Items.Clear();
                cmd.CommandText = "select eventid, name from Events";
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    OlaComp cmp = new OlaComp();
                    cmp.Id = Convert.ToInt32(reader["eventid"].ToString());
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmbOLAComp.Items.Add(cmp);
                }
                reader.Close();
                cmd.Dispose();

                if (cmbOLAComp.Items.Count > 0)
                    cmbOLAComp.SelectedIndex = 0;

            }
            catch (Exception ee)
            {
                MessageBox.Show(this, ee.Message);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }
        private class OlaComp
        {
            public string Name;
            public int Id;

            public override string ToString()
            {
                return Name;
            }
        }

        private void wizardPage4_ShowFromNext(object sender, EventArgs e)
        {
            /*Try Connect*/
            IDbConnection conn = null;
            try
            {
                conn = GetDBConnection(lstDB.SelectedItem as string);
                conn.Open();

                IDbCommand cmd = conn.CreateCommand();
                cmbOLAEtapp.Items.Clear();
                cmd.CommandText = "select eventraceid, name from EventRaces where eventid = " + ((OlaComp)cmbOLAComp.SelectedItem).Id;
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    OlaComp cmp = new OlaComp();
                    cmp.Id = Convert.ToInt32(reader["eventraceid"].ToString());
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmbOLAEtapp.Items.Add(cmp);
                }
                reader.Close();
                cmd.Dispose();

                if (cmbOLAEtapp.Items.Count > 0)
                    cmbOLAEtapp.SelectedIndex = 0;

            }
            catch (Exception ee)
            {
                MessageBox.Show(this, ee.Message);
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        private void wizardPage5_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            //start
            FrmMonitor monForm = new FrmMonitor();
            this.Hide();
            OlaParser pars = new OlaParser(GetDBConnection(lstDB.SelectedItem as string), (cmbOLAComp.SelectedItem as OlaComp).Id, (cmbOLAEtapp.SelectedItem as OlaComp).Id);
            monForm.SetParser(pars as IExternalSystemResultParser);
            monForm.CompetitionID = Convert.ToInt32(txtCompID.Text);
            monForm.ShowDialog(this);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "OLA5 databases (*.db)|*.db";
            ofd.FileName = txtOlaDb.Text;
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                txtOlaDb.Text = ofd.FileName;
            }
        }
    }
}