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
using System.Windows.Shapes;
using System.Data.SQLite;

namespace metro
{
    /// <summary>
    /// Interaction logic for EditStation.xaml
    /// </summary>
    public partial class EditStation : Window
    {
        MainWindow mw;
        Station station;
        List<Route> routes;
        string sql;
        SQLiteCommand cmdQ;
        SQLiteDataReader reader;
        public EditStation()
        {
            InitializeComponent();
            station = new Station();
            mw = MainWindow.main_window;
            txtStation.ItemsSource = mw.auto_complete_station_names;
            txtTo.ItemsSource = mw.auto_complete_station_names;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // save station
            if (station.id == -1)
            {
                // new station
                sql = String.Format("insert into stations (name, pos_x, pos_y, route_count) values(\"{0}\", {1}, {2}, {3});select last_insert_rowid()",
                    txtStation.Text, Int32.Parse(txtX.Text), Int32.Parse(txtY.Text), lstRoutes.Items.Count);
                cmdQ = new SQLiteCommand(sql, mw.conn);
                station.id = Int32.Parse(cmdQ.ExecuteScalar().ToString());
                foreach (Route r in routes)
                {
                    sql = String.Format("insert into routes (\"from\", \"to\", line, length) values({0}, {1}, {2}, {3})",
                        station.id, r.to, r.line, r.length);
                    cmdQ = new SQLiteCommand(sql, mw.conn);
                    cmdQ.ExecuteNonQuery();
                }
                mw.initData();
                this.btnQuery_clk();

            }
            else
            {
                // update old station
                MessageBox.Show("This station is not saved yet.");
            }
            mw.initData();
            this.btnQuery_clk();
        }

        public String readStationName(int stationId)
        {
            // read station name
            String sql2 = "select name from stations where id=" + stationId.ToString();
            SQLiteCommand cmdQ2 = new SQLiteCommand(sql2, mw.conn);
            SQLiteDataReader reader2 = cmdQ2.ExecuteReader();
            reader2.Read();
            return reader2.GetString(0);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Copyright (C) 2017 Sykie Chen \nhttp://coder17.com");
        }

        private void lstRoutes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstRoutes.Items.Count > 0)
            {
                txtLine.Text = routes[lstRoutes.SelectedIndex].line.ToString();
                txtLength.Text = routes[lstRoutes.SelectedIndex].length.ToString();
                txtTo.Text = readStationName(routes[lstRoutes.SelectedIndex].to);
            }
            
        }

        private void btnQuery_clk(object sender = null, RoutedEventArgs e = null)
        {
            station = new Station();
            routes = new List<Route>();
            sql = "select * from stations where name=\"" + txtStation.Text + "\"";
            cmdQ = new SQLiteCommand(sql, mw.conn);
            reader = cmdQ.ExecuteReader();
            if (reader.Read())
            {
                // existing station
                txtStation.Foreground = Brushes.Black;
                station.id = reader.GetInt32(0);
                station.name = txtStation.Text;
                station.pos_x = reader.GetInt32(2);
                station.pos_y = reader.GetInt32(3);
                station.route_count = reader.GetInt32(4);
                // display paras
                txtX.Text = station.pos_x.ToString();
                txtY.Text = station.pos_y.ToString();

                // query routes
                lstRoutes.Items.Clear();
                sql = "select * from routes where \"from\"=\"" + station.id.ToString() + "\" or \"to\"=\"" + station.id.ToString() + "\"";
                cmdQ = new SQLiteCommand(sql, mw.conn);
                reader = cmdQ.ExecuteReader();
                while (reader.Read())
                {
                    Route r = new Route();
                    r.id = reader.GetInt32(0);
                    if (reader.GetInt32(1) == station.id) r.to = reader.GetInt32(2);
                    else r.to = reader.GetInt32(1);
                    r.from = station.id;
                    r.length = reader.GetInt32(4);
                    r.line = reader.GetInt32(3);
                    routes.Add(r);
                    lstRoutes.Items.Add(readStationName(r.to));
                }


            }
            else
            {
                // new station
                txtStation.Foreground = Brushes.Red;
                station.name = txtStation.Text;
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // remove station
            if (station.id != -1)
            {
                if(MessageBox.Show("Do you really want to delete station "
                    + readStationName(station.id) + "?"
                    ,"Confirm",MessageBoxButton.OKCancel)==MessageBoxResult.OK)
                {
                    int rm_s, rm_r;
                    sql = "delete from stations where id=" + station.id.ToString();
                    cmdQ = new SQLiteCommand(sql, mw.conn);
                    rm_s = cmdQ.ExecuteNonQuery();
                    sql = "delete from routes where \"from\"=\"" + station.id.ToString() + "\" or \"to\"=\"" + station.id.ToString() + "\"";
                    cmdQ = new SQLiteCommand(sql, mw.conn);
                    rm_r = cmdQ.ExecuteNonQuery();
                    MessageBox.Show("Deleted " + rm_s.ToString() + " stations and " + rm_r.ToString() + " routes.");
                    mw.initData();
                    station.id = -1;
                }
            }
            else
            {
                MessageBox.Show("This station is not saved yet.");
            }
        }

    }
}
