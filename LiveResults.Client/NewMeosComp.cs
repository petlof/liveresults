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
    public partial class NewMeosComp : Form
    {
        public NewMeosComp()
        {
            InitializeComponent();
            txtUser.Text = "live";
            txtPw.Text = "live";
            txtPort.Text = "3306";
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
                "meosfrm.setts");
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


        private IDbConnection GetDBConnection()
        {
            return GetDBConnection("meosmain");
        }

        private IDbConnection GetDBConnection(string schema)
        {

            return new MySqlConnector.MySqlConnection("Server=" + txtHost.Text + ";User Id=" + txtUser.Text + ";Port=" + txtPort.Text + ";Password=" + txtPw.Text + (schema != null ? ";Initial Catalog=" + schema : "") + ";charset=utf8;ConnectionTimeout=30");
        }

        private void wizardPage3_ShowFromNext(object sender, EventArgs e)
        {
            StoreSettings();
            /*Try Connect*/
            IDbConnection conn = null;
            try
            {

                conn = GetDBConnection();
                conn.Open();

                IDbCommand cmd = conn.CreateCommand();
                cmbMeosComp.Items.Clear();

                cmd.CommandText = "SELECT Id, Name, Date, ZeroTime, NameId FROM oevent WHERE Removed=0";

                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    MeosComp cmp = new MeosComp();
                    cmp.Id = Convert.ToInt32(reader["Id"].ToString());
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmp.Date = Convert.ToString(reader["date"]);
                    cmp.NameId = Convert.ToString(reader["NameId"]);
                    cmbMeosComp.Items.Add(cmp);
                }
                reader.Close();
                cmd.Dispose();

                if (cmbMeosComp.Items.Count > 0)
                    cmbMeosComp.SelectedIndex = 0;


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

        private static void TryApplyReadRights(IDbCommand cmd)
        {
            cmd.CommandText = @"GRANT SELECT on Version to live;
GRANT SELECT on EVENTS to live;
GRANT SELECT ON EVENTRACES to live;
GRANT SELECT ON RESULTS to live;
GRANT SELECT ON PERSONS to live;
GRANT SELECT ON Organisations to live;
GRANT SELECT ON eventclasses to live;
GRANT SELECT ON entries to live;
GRANT SELECT ON raceclasses to live;
GRANT SELECT ON splittimes to live;
GRANT SELECT ON SplitTimeControls to live;
GRANT SELECT ON Controls to live;";
            cmd.ExecuteNonQuery();
        }
        private class MeosComp
        {
            public string Name;
            public int Id;
            public string Date;
            public string NameId;

            public override string ToString()
            {
                return Date + " - " + Name;
            }
        }

        
        private void wizardPage5_CloseFromNext(object sender, Gui.Wizard.PageEventArgs e)
        {
            StoreSettings();
            //start
            FrmMonitor monForm = new FrmMonitor();
            this.Hide();
            MeOsParser pars = new MeOsParser(GetDBConnection((cmbMeosComp.SelectedItem as MeosComp).NameId), chkCreateRadioControls.Checked, chkIsRelay.Checked);
            monForm.SetParser(pars as IExternalSystemResultParser);
            monForm.CompetitionID = Convert.ToInt32(txtCompID.Text);
            monForm.ShowDialog(this);
        }

       
    }
}