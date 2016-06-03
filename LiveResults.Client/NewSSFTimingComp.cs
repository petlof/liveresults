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

namespace LiveResults.Client
{
    public partial class NewSSFTimingComp : Form
    {
        public NewSSFTimingComp()
        {
            InitializeComponent();
            txtUser.Text = "sa";
            txtPw.Text = "ssftiming";
            txtPort.Text = "1433";
            RetreiveSettings();
           

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
                "ssftimingfrm.setts");
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


        private void wizardPage2_ShowFromBack(object sender, EventArgs e)
        {
            StoreSettings();
            /*Try Connect*/
            IDbConnection conn = null;
            try
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

            OleDbConnection oleConn = conn as OleDbConnection;
            DataTable schemas = oleConn.GetOleDbSchemaTable(OleDbSchemaGuid.Catalogs, null);
            //dataGridView1.DataSource = schemas;
            foreach (DataRow r in schemas.Rows)
            {
                dbs.Add(r["CATALOG_NAME"] as string);
            }

            return dbs.ToArray();
        }

        private IDbConnection GetDBConnection()
        {
            return GetDBConnection(null);
        }

        private IDbConnection GetDBConnection(string schema)
        {

            return
                new OleDbConnection("Provider=SQLOLEDB.1;Persist Security Info=False;User ID=" + txtUser.Text + ";Password=" + txtPw.Text + ";Data Source=" +
                                    txtHost.Text + "," + txtPort.Text + (schema != null ? ";Initial Catalog=" + schema : ""));

        }

        private void wizardPage3_ShowFromNext(object sender, EventArgs e)
        {
            StoreSettings();
            /*Try Connect*/
            IDbConnection conn = null;
            try
            {
                conn = GetDBConnection(lstDB.SelectedItem as string);
                conn.Open();

                IDbCommand cmd = conn.CreateCommand();
                cmbOLAComp.Items.Clear();

                cmd.CommandText = "select raceId, RaceName, raceDate from dbRaces";


                    IDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        OlaComp cmp = new OlaComp();
                        cmp.Id = Convert.ToInt32(reader["raceId"].ToString());
                        cmp.Name = Convert.ToString(reader["raceName"]) + " [" + Convert.ToDateTime(reader["raceDate"]).ToString("yyyy-MM-dd") + "]";
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

        

        private void wizardPage5_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            StoreSettings();
            //start
            FrmMonitor monForm = new FrmMonitor();
            this.Hide();
            SSFTimingParser pars = new SSFTimingParser(GetDBConnection(lstDB.SelectedItem as string), 
                (cmbOLAComp.SelectedItem as OlaComp).Id, chkCreateRadioControls.Checked);
            monForm.SetParser(pars as IExternalSystemResultParser);
            monForm.CompetitionID = Convert.ToInt32(txtCompID.Text);
            monForm.ShowDialog(this);
        }
    }
}