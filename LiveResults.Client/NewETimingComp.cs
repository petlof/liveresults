using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.H2;
using System.IO;
using System.Xml.Serialization;
using LiveResults.Model;

namespace LiveResults.Client
{
    public partial class NewETimingComp : Form
    {
        public NewETimingComp()
        {
            InitializeComponent();
            comboBox1.DataSource = new string[] { "eTiming Access database", "SQL-Server" };
            comboBox1.SelectedIndex = 0;
            txtUser.Text = "emit";
            txtPw.Text   = "time";
            txtPort.Text = "1433";
            txtETimingDb.Text = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            txtSleepTime.Text = "10";
            chkOneLineRelayRes.Checked = false;
            RetreiveSettings();
            chkDeleteEmmaIDs.Checked = false;
            
        }

        [Serializable]
        public struct Setting
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
        private void RetreiveSettings()
        {
            try
            {
                if (File.Exists(GetSettingsFile()))
                {

                    using (var ms = new MemoryStream(File.ReadAllBytes(GetSettingsFile())))
                    {
                        List<Setting> setts = new List<Setting>();

                        var serializer = new XmlSerializer(setts.GetType());
                        setts = serializer.Deserialize(ms) as List<Setting>;
                        if (setts != null)
                        {
                        }

                        applyControlValues(Controls, setts);
                    }

                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, ee.Message);
            }
        }

        private void applyControlValues(Control.ControlCollection controls, List<Setting> setts)
        {
            foreach (Control c in controls)
            {
                if (c is TextBox)
                {
                    string val = setts.Where(x => x.Key == c.Name).Select(x => x.Value).FirstOrDefault();
                    (c as TextBox).Text = val;
                }
                if (c is ComboBox)
                {
                    string val = setts.Where(x => x.Key == c.Name).Select(x => x.Value).FirstOrDefault();
                    (c as ComboBox).SelectedItem = val;
                    
                }
                if (c is CheckBox)
                {
                    string val = setts.Where(x => x.Key == c.Name).Select(x => x.Value).FirstOrDefault();
                    if (val != null)
                    {
                        (c as CheckBox).Checked = val == "True";
                    }
                }

                applyControlValues(c.Controls, setts);
            }
        }

        private void StoreSettings()
        {
            List<Setting> setts = new List<Setting>();
            extractControlValues(Controls, setts);

            var serializer = new XmlSerializer(setts.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(ms, setts);
                var data = new byte[ms.Length];
                ms.Seek(0, SeekOrigin.Begin);
                ms.Read(data, 0, data.Length);

                if (!Directory.Exists(Path.GetDirectoryName(GetSettingsFile())))
                    Directory.CreateDirectory(Path.GetDirectoryName(GetSettingsFile()));

                File.WriteAllBytes(GetSettingsFile(), data);
            }


        }

        private static string GetSettingsFile()
        {
            return Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LiveResults.Client"),
                "ETimingfrm.setts");
        }

        private void extractControlValues(Control.ControlCollection controls, List<Setting> setts)
        {
            foreach (Control c in controls)
            {
                if (c is TextBox)
                {
                    setts.Add(new Setting
                    {
                        Key = (c as TextBox).Name,
                        Value = (c as TextBox).Text
                    });
                }
                if (c is ComboBox)
                {
                    setts.Add(new Setting
                    {
                        Key = (c as ComboBox).Name,
                        Value = (c as ComboBox).SelectedValue as string
                    });
                }
                if (c is CheckBox)
                {
                    setts.Add(new Setting
                    {
                        Key = (c as CheckBox).Name,
                        Value = (c as CheckBox).Checked.ToString()
                    });
                }

                extractControlValues(c.Controls, setts);
            }
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboBox1.SelectedIndex)
            {
                case 0:
                    txtHost.Enabled = txtPort.Enabled = txtUser.Enabled = txtPw.Enabled = false;
                    panel1.Visible = true;
                    break;
                case 1:
                    txtHost.Enabled = txtPort.Enabled = txtUser.Enabled = txtPw.Enabled = true;
                    panel1.Visible = false;
                    break;
            }
        }

        private void wizardPage2_ShowFromNext(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                wizard1.NextTo(wizardPage5);
            StoreSettings();
            /*Try Connect*/
            IDbConnection conn = null;
            try
            {
                if (comboBox1.SelectedIndex != 0)
                {
                    conn = GetDBConnection();
                    if (conn != null)
                    {
                        conn.Open();
                        lstDB.DataSource = null;

                        string[] databases = GetDatabases(conn);
                        lstDB.DataSource = databases;
                    }
                }
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
                    return new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + txtETimingDb.Text + ";");
                case 1:
                    return new OleDbConnection("Provider=SQL Server Native Client 11.0;Data Source=" + txtHost.Text + "; Port=" + txtPort + "; UID=" + txtUser.Text + ";PWD=" + txtPw.Text + (schema != null ? "; Database=" + schema : ""));
            }
            return null;
        }

      
        private class ETimingComp
        {
            public string Name;
            public int Id;
            public List<int> eTimingId;
            public string Organizer;
            public DateTime CompDate;

            public override string ToString()
            {
                return Name;
            }
        }

        ETimingComp cmp = new ETimingComp();

        private void wizardPage5_ShowFromNext(object sender, EventArgs e)
        {


            IDbConnection conn = null;

            try
            {
                conn = GetDBConnection(lstDB.SelectedItem as string);
                conn.Open();
                IDbCommand cmd = conn.CreateCommand();

                cmd.CommandText = "SELECT id, name, organizator, firststart FROM arr";
                IDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    cmp.Id = Convert.ToInt32(reader["id"].ToString());
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmp.Organizer = Convert.ToString(reader["organizator"]).Trim();
                    cmp.CompDate = Convert.ToDateTime(reader[("firststart")]);
                }
                reader.Close();

                cmd.CommandText = "SELECT id, status from Name";
                reader = cmd.ExecuteReader();
                var eTimingId = new List<int>();
                while (reader.Read())
                {
                    string status = reader["status"] as string;
                    if (!(status == "C")) // Do not add runner with status "avmeldt"
                        eTimingId.Add(Convert.ToInt32(reader["id"].ToString()));
                }
                reader.Close();
                cmd.Dispose();

                cmp.eTimingId = eTimingId;
            }
            catch (Exception ee)
            {
                MessageBox.Show(this, ee.Message);
            }
            finally
            {
                if (conn != null)
                    conn.Close();

                txtCompName.Text = cmp.Name;
                txtOrgName.Text = cmp.Organizer;
                txtCompDate.Text = Convert.ToString(cmp.CompDate);
            }
        }

        private void wizardPage5_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            StoreSettings();

            FrmETimingMonitor monForm = new FrmETimingMonitor();
            this.Hide();

            bool MSSQL = false;
            if (comboBox1.SelectedIndex == 1) MSSQL = true;
            
            ETimingParser pars = new ETimingParser(GetDBConnection(lstDB.SelectedItem as string),
                    (Convert.ToInt32(txtSleepTime.Text)), chkCreateRadioControls.Checked, chkOneLineRelayRes.Checked, MSSQL);

            monForm.SetParser(pars as IExternalSystemResultParser);
            monForm.CompetitionID = Convert.ToInt32(txtCompID.Text);
            monForm.Organizer = cmp.Organizer;
            monForm.CompDate  = cmp.CompDate;
            monForm.eTimingId = cmp.eTimingId;
            monForm.deleteEmmaIDs = chkDeleteEmmaIDs.Checked;
            monForm.ShowDialog(this);
              
        }


        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "eTiming databases (*.mdb)|*.mdb";
            ofd.FileName = txtETimingDb.Text;
            ofd.CustomPlaces.Add(new FileDialogCustomPlace(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)));
            ofd.InitialDirectory = Directory.Exists(txtETimingDb.Text) ? txtETimingDb.Text : Path.GetDirectoryName(txtETimingDb.Text); 
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                txtETimingDb.Text = ofd.FileName;
            }
        }

        private void wizardPage2_ShowFromBack(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                wizard1.NextTo(wizardPage1);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}