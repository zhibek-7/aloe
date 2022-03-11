using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Xml.Linq;
using System.IO;
using System.Xml;
using exportXml.Model;
using Newtonsoft.Json;

namespace exportXml
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int i = 0;
        
        string _xmlFile = "\\test1.xml";
        public MainWindow()
        {
            InitializeComponent();
            loadGrid();
        }

        SqlConnection con = new SqlConnection(@"Data Source=ZHIBEKLATOP;Initial Catalog=farm;Persist Security Info=True;User ID=sa;Password=123456");

        public void readData() {
            name_txt.Clear();
            count_txt.Clear();
            mnn_txt.Clear();
            price_txt.Clear();
            search_txt.Clear();
        
        }

        public void loadGrid() {
            SqlCommand cmd = new SqlCommand("select * from Farm", con);
            DataTable dt = new DataTable();
            con.Open();
            SqlDataReader sdr = cmd.ExecuteReader();
            dt.Load(sdr);
            con.Close();
            datagrid.ItemsSource = dt.DefaultView;
        }

        private void ReadBtn_Click(object sender, RoutedEventArgs e)
        {
            readData();
        }

        public bool isValid() {
            if (name_txt.Text==string.Empty) {
                MessageBox.Show("Name is requiered","Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        private void InsertBtn_Click(object sender, RoutedEventArgs e)
        {
            try {
                if (isValid())
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO Farm VALUES (@Name, @Price, @Count, @Mnn)", con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Name", name_txt.Text);
                    cmd.Parameters.AddWithValue("@Price", price_txt.Text);
                    cmd.Parameters.AddWithValue("@Count", count_txt.Text);
                    cmd.Parameters.AddWithValue("@Mnn", mnn_txt.Text);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                    loadGrid();
                    MessageBox.Show("Successfully", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                    readData();
                }
            } catch (SqlException ex ){
                MessageBox.Show(ex.Message);
            }

        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            con.Open();
            SqlCommand cmd = new SqlCommand("DELETE FROM Farm where Id="+search_txt.Text+ " ", con );
            try
            {
                cmd.ExecuteNonQuery();
                MessageBox.Show("Record has been deleted", "Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                con.Close();
                readData();
                loadGrid();
                con.Close();
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Not Deleted" + ex.Message);
            }
            finally {
                con.Close();
            }

        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {
            con.Open();
            SqlCommand cmd = new SqlCommand("update Farm set Name='"+ name_txt.Text+"', Price='"+price_txt.Text+ "', Count='"+count_txt.Text+"', Mnn='"+mnn_txt.Text+"' WHERE Id = '"+search_txt.Text+"' ", con);
            try {
                cmd.ExecuteNonQuery();
                MessageBox.Show("Record has been updated successfully", "Updated", MessageBoxButton.OK, MessageBoxImage.Information);

            } catch (SqlException ex) {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                con.Close();
                readData();
                loadGrid();
            }
         


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            i++;
            XDocument xDoc = XDocument.Load(_xmlFile);
            List<farmData> fmList = new List<farmData>();
            fmList = xDoc.Descendants("LS").Select(
                item => new farmData
                {
                    Id=i,
                    Name = item.Element("DATA").Element("NAME").Value ,
                    Price = item.Element("DATA").Element("PRICE").Value,
                    Count = item.Element("DATA").Element("COUNT").Value,
                    Mnn=item.Element("MNN").Value
                }
                ).ToList();


            try
            {
                foreach (farmData fd in fmList)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO Farm VALUES (@Name, @Price, @Count, @Mnn)", con);
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.AddWithValue("@Name", fd.Name);
                    cmd.Parameters.AddWithValue("@Price", fd.Price);
                    cmd.Parameters.AddWithValue("@Count", fd.Count.Substring(0, fd.Count.LastIndexOf('.') ));
                    cmd.Parameters.AddWithValue("@Mnn",fd.Mnn);
                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();
                   
                }
                loadGrid();
                MessageBox.Show("Successfully", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                readData();
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
            }


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            try {

                SqlCommand cmd = new SqlCommand("select * from Farm", con);
                con.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    List<farmData> frdList = new List<farmData>();
                    while (reader.Read())
                    {
                        farmData frd = new farmData();

                        frd.Id = reader.GetInt32(0);
                        frd.Name = reader.GetValue(1).ToString();
                        frd.Price = reader.GetValue(2).ToString();
                        frd.Count = reader.GetValue(3).ToString();
                        frd.Mnn = reader.GetValue(4).ToString();

                        frdList.Add(frd);

                    }
                    string json = JsonConvert.SerializeObject(frdList);
                    using (StreamWriter sw = new StreamWriter(File.Create("D:\\aloe\\file.json")))
                    {
                        sw.Write(json);
                    }

                }
                con.Close();

                MessageBox.Show("Successfully", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
            } catch(SqlException ex) {
                MessageBox.Show(ex.Message);
            }





        }
    }
}
