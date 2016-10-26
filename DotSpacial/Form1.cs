using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DotSpatial.Data;
using DotSpatial.Topology;
using DotSpatial.Topology.Utilities;
using DotSpatial.Projections;
using Npgsql;
using System.IO;
using OSGeo.FDO;
using OSGeo.FDO.ClientServices;

namespace DotSpacialTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // define the feature type for this file
                FeatureSet fs = new FeatureSet(FeatureType.Polygon);

                // Add Some Columns
                fs.DataTable.Columns.Add(new DataColumn("ID", typeof(int)));
                fs.DataTable.Columns.Add(new DataColumn("Text", typeof(string)));

                // create a geometry (square polygon)
                List<Coordinate> vertices = new List<Coordinate>();

                vertices.Add(new Coordinate(0, 0));
                vertices.Add(new Coordinate(0, 100));
                vertices.Add(new Coordinate(100, 100));
                vertices.Add(new Coordinate(100, 0));

                Polygon geom = new Polygon(vertices);

                // add the geometry to the featureset. 
                IFeature feature = fs.AddFeature(geom);
 
                feature.DataRow.BeginEdit();
                feature.DataRow["ID"] = 1;
                feature.DataRow["Text"] = "Hello World";
                feature.DataRow.EndEdit();

                //========================================================================//
                vertices.Clear();
                vertices.Add(new Coordinate(0, 0));          
                vertices.Add(new Coordinate(0, 100));
                vertices.Add(new Coordinate(100, 100));
                vertices.Add(new Coordinate(100, 0));

                geom = new Polygon(vertices);
                feature = fs.AddFeature(geom);

                feature.DataRow.BeginEdit();
                feature.DataRow["ID"] = 2;
                feature.DataRow["Text"] = "My Text";
                feature.DataRow.EndEdit();

                // save the feature set
                fs.SaveAs("F:\\Test_value\\test.shp", true);

                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                IFeatureSet fs = FeatureSet.Open("F:\\Test_value\\hitbgt_backup.shp");
                fs.FillAttributes();  

                //update attribute value
                //fs.DataTable.Rows[0]["Text"] = "New value1";
                //fs.DataTable.Rows[1]["Text"] = "New value2";

                DataTable dt = fs.DataTable;

                dataGridView1.DataSource = dt;

                IFeature feature = fs.GetFeature(1);



                List<Coordinate> vertices = new List<Coordinate>();

                vertices.Add(new Coordinate(0, 0));
                vertices.Add(new Coordinate(50, 100));
                vertices.Add(new Coordinate(0, 100));
                vertices.Add(new Coordinate(100, 100));
                vertices.Add(new Coordinate(100, 0));

                Polygon geom = new Polygon(vertices);
                

                //fs.Features[0] = feature.Buffer(10);
                //fs.Features[1].BasicGeometry = geom;

                dataGridView1.DataSource = fs.DataTable;

                MessageBox.Show(fs.Features[1].BasicGeometry.ToString());

                //fs.SaveAs("F:\\Test_value\\result.shp", true);

                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCreateShapeFromDB_Click(object sender, EventArgs e)
        {
            IFeature feature = new Feature();
            FeatureSet fs = new FeatureSet(FeatureType.Polygon);

            DataTable dt = new DataTable();

            DbHandeler dbHandelerObj = new DbHandeler();
            string query = "Select st_asBinary(the_geom) as geom, * from hitbgt";

            try
            {
                dt = dbHandelerObj.GetDataTable(query);

                //add column to the featureset datatable
                foreach (DataColumn dc in dt.Columns)
                {
                    if (dc.ColumnName != "the_geom" && dc.ColumnName != "geom")
                    {                   
                        fs.DataTable.Columns.Add(dc.ColumnName, dc.DataType);
                    }
                }

                //create feature from hitbgt along with attribute value
                foreach (DataRow dr in dt.Rows)
                {              
                    Byte[] data = (Byte[])dr["geom"];
                    WkbReader wkbReader = new WkbReader();
                    IGeometry geometry = wkbReader.Read(data);

                    feature = fs.AddFeature(geometry);
   
                    feature.DataRow.BeginEdit();                    
                    foreach (DataColumn dc in fs.DataTable.Columns)
                    {
                        feature.DataRow[dc.ColumnName] = dr[dc.ColumnName];
                    }
                    feature.DataRow.EndEdit();                  
                }

                dataGridView1.DataSource = fs.DataTable;

                //create the shapefile after adding all features to the featureset
                fs.SaveAs("F:\\Test_value\\hitbgt_backup.shp", true);
                fs.Close();

                MessageBox.Show("hitbgt_backup.shp successfully created.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CompareWithShape(DataTable dt)
        {
            try
            {
                IFeatureSet fs = FeatureSet.Open("F:\\Test_value\\hitbgt\\v_hitbgt_lev1.shp");
                fs.FillAttributes();

                WkbReader reader = new WkbReader();
                IGeometry dbGeometry = null;
                IGeometry shapeGeometry = null;
                string notEqualCadID = string.Empty;
                int rowCount = fs.NumRows();

                MessageBox.Show(fs.DataTable.Rows[28]["geotxtlin"].ToString());
                //----------------------------------------------
                //compare whole geometry
                for (int i = 0; i < rowCount; i++)
                {
                    dbGeometry = reader.Read((Byte[])dt.Rows[i]["geom"]);
                    shapeGeometry = fs.Features[i].BasicGeometry as IGeometry;

                    if (!dbGeometry.Equals(shapeGeometry))
                    {
                        notEqualCadID += dt.Rows[i]["cadid"].ToString() + "  ";
                    }
                }

                if (notEqualCadID != string.Empty)
                {
                    MessageBox.Show(notEqualCadID + " is not equal");
                }
                else
                {
                    MessageBox.Show("All geometry are equal");
                }

                //----------------------------------------------------
                //Compare geometry for the specific cadid.
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["cadid"].ToString() == "12345")
                    {
                        dbGeometry = reader.Read((Byte[])dr["geom"]);
                        break;
                    }
                }
                foreach (IFeature ff in fs.Features)
                {
                    if (ff.DataRow["cadid"].ToString() == "12345")
                    {
                        shapeGeometry = ff.BasicGeometry as IGeometry;
                        break;
                    }
                }

                bool equal = dbGeometry.Equals(shapeGeometry);
                if (equal)
                {
                    MessageBox.Show("The geometry are equal for the provided cadid.");
                }

                //compare all attributes
                dt.Columns.Remove("geom");
                dt.Columns.Remove("the_geom");             

                dataGridView1.DataSource = fs.DataTable;

                equal = IsEqual(dt, fs.DataTable);
                if (equal)
                {
                    MessageBox.Show("All attributes are same");
                }
                else
                {
                    MessageBox.Show("All attributes are not same");
                }

                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool IsEqual(DataTable t1, DataTable t2)
        {
            // If the number of rows is different, no need to compare the data
            if (t1.Rows.Count != t2.Rows.Count)
                return false;

            if (t1.Columns.Count != t2.Columns.Count)
                return false;

            for (int i = 0; i < t1.Rows.Count; i++)
            {
                foreach (DataColumn col in t1.Columns)
                {
                    if (!Equals(t1.Rows[i][col.ColumnName].ToString(), t2.Rows[i][col.ColumnName].ToString()))
                        //MessageBox.Show("Hello");
                        return false;
                }
            }

            return true;
        }
            
        private void btnTest_Click(object sender, EventArgs e)
        {
            try
            {
                string str = "Hawar IT.";
                byte[] data = Encoding.UTF8.GetBytes(str);

                string insertString = "Insert into test_1 values(@Id, @value)";

                List<NpgsqlParameter> parameter = new List<NpgsqlParameter>();

                parameter.Add(new NpgsqlParameter("@Id", 2));
                parameter.Add(new NpgsqlParameter("@value", data));

                DbHandeler dbHandelerObj = new DbHandeler();
                dbHandelerObj.InsertData(insertString, parameter);

                MessageBox.Show("Inserted");           
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnShow_Click(object sender, EventArgs e)
        {
            DbHandeler dbHandelerObj = new DbHandeler();
            DataTable dt = new DataTable();
            dt = dbHandelerObj.GetDataTable("select * from test_1");

            byte[] data = (byte[])dt.Rows[0]["value"];

            string str = Encoding.ASCII.GetString(data);

            MessageBox.Show(str);
        }


        private void btnFdoTest_Click(object sender, EventArgs e)
        {

            try
            {
                //FDO Provider Registry
                IProviderRegistry FDORegistry = (IProviderRegistry)FeatureAccessManager.GetProviderRegistry();
                ProviderCollection providers = FDORegistry.GetProviders();
                string s = "Registered FDO Providers: \n";
                foreach (Provider p in providers)
                {
                    s += "# " + p.DisplayName + "\n";
                }
                MessageBox.Show(s, "Registered FDO providers", MessageBoxButtons.OK, MessageBoxIcon.Information);

                DbConnection dbConnection = new DbConnection();
                IConnectionManager connManager = FeatureAccessManager.GetConnectionManager();
                OSGeo.FDO.Connections.IConnection conn = connManager.CreateConnection("OSGeo.PostgreSQL.3.9");
                conn.ConnectionString = dbConnection.GetConnectionString();
                
                OSGeo.FDO.Connections.ConnectionState connState = conn.Open();



                MessageBox.Show("Done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCompare_Click(object sender, EventArgs e)
        {
            DbHandeler dbHandeler = new DbHandeler();
            DataTable dt = new DataTable();
            string query = "Select st_asBinary(the_geom) as geom, * from hitbgt";

            dt = dbHandeler.GetDataTable(query);

            CompareWithShape(dt);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string cadId = "cadid='12345'";
            try
            {
                IFeatureSet fs = FeatureSet.Open("F:\\Test_value\\hitbgt_backup.shp");
                fs.FillAttributes();

                List<IFeature> fList = fs.SelectByAttribute(cadId);
                fs.Features.Remove(fList[0]);

                dataGridView1.DataSource = fs.DataTable;

                fs.SaveAs("F:\\Test_value\\hitbgt_backup.shp", true);
                fs.Close();

                MessageBox.Show("Row deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "select st_asbinary(the_geom) as geom from hitbgt where cadid='111etiLRxs'";
                DbHandeler dbHandeler = new DbHandeler();
                DataTable dt = dbHandeler.GetDataTable(query);
                WkbReader reader = new WkbReader();
                IGeometry geometry = reader.Read((byte[])dt.Rows[0]["geom"]);

                //----------------------------------------------------
                IFeatureSet fs = FeatureSet.Open("D:\\Test_value\\hitbgt.shp");
                fs.FillAttributes();

                IFeature feature = fs.AddFeature(geometry);

                feature.DataRow.BeginEdit();
                feature.DataRow["cadid"] = "123456";
                feature.DataRow["blk"] = "New value";
                feature.DataRow["area"] = "1";
                feature.DataRow.EndEdit();

                dataGridView1.DataSource = fs.DataTable;

                fs.SaveAs("D:\\Test_value\\hitbgt.shp", true);
                fs.Close();
                MessageBox.Show("Row inserted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string cadId = "cadid='12345'";
            try
            {
                IFeatureSet fs = FeatureSet.Open("F:\\Test_value\\hitbgt_backup.shp");
                fs.FillAttributes();

                List<int> index = fs.SelectIndexByAttribute(cadId);
                fs.Features[index[0]].DataRow["blk"] = "Updated value";
                fs.UpdateExtent();

                dataGridView1.DataSource = fs.DataTable;

                //fs.SaveAs("F:\\Test_value\\hitbgt_backup.shp", true);
                fs.Close();
                MessageBox.Show("Updated successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }
        
    }
}
