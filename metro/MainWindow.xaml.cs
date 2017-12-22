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
using System.Data.SQLite;

namespace metro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow main_window;
        bool map_drag = false;
        double map_prev_X;
        double map_prev_Y;
        public SQLiteConnection conn;
        Dictionary<int, Station> d_stations;
        Dictionary<int, Route> d_routes;
        public List<string> auto_complete_station_names;
        List<string> auto_complete_attracton_names;
        Graph graph;
        string sql;
        SQLiteCommand cmdQ;
        SQLiteDataReader reader;
        Dictionary<int, Brush> colors;

        public MainWindow()
        {
            InitializeComponent();
            main_window = this;
        }

        public void initData()
        {
            
            graph = new Graph();
            d_stations = new Dictionary<int, Station>();
            d_routes = new Dictionary<int, Route>();
            colors = new Dictionary<int, Brush>();
            auto_complete_station_names = new List<string>();
            auto_complete_attracton_names = new List<string>();

            // init combo box
            cbo_line_op1.Items.Add("1st");
            cbo_line_op1.Items.Add("2nd");
            cbo_line_op1.Items.Add("3rd");
            cbo_line_op1.SelectedIndex = 0;
            cbo_line_op2.Items.Add("shortest");
            cbo_line_op2.Items.Add("less transfer");
            cbo_line_op2.SelectedIndex = 0;

            // read line colors
            sql = "select * from colors";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            while (reader.Read())
            {
                int line = reader.GetInt32(0);
                byte r = (byte)reader.GetInt32(1);
                byte g = (byte)reader.GetInt32(2);
                byte b = (byte)reader.GetInt32(3);
                Brush br = new SolidColorBrush(Color.FromRgb(r, g, b));
                colors.Add(line, br);
            }

            // read stations
            sql = "select * from stations";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            map.Children.Clear();
            while (reader.Read())
            {
                // counting graph add node
                graph.addNode(reader.GetInt32(0));

                Station s = new Station();
                s.id = reader.GetInt32(0);
                s.name = reader.GetString(1);
                s.pos_x = reader.GetInt32(2);
                s.pos_y = reader.GetInt32(3);
                s.route_count = reader.GetInt32(4);
                s.map_dot.StrokeThickness = 2;
                s.map_dot.Stroke = Brushes.Black;
                s.map_dot.Fill = Brushes.White;
                s.map_dot.Tag = s.id;
                // if transfer station, bigger circle
                s.map_dot.Width = (s.route_count > 2) ? 15 : 8;
                s.map_dot.Height = s.map_dot.Width;
                // set position on map
                Canvas.SetLeft(s.map_dot, s.pos_x - s.map_dot.Width / 2);
                Canvas.SetTop(s.map_dot, s.pos_y - s.map_dot.Width / 2);
                Canvas.SetZIndex(s.map_dot, 2);
                // add to map
                map.Children.Add(s.map_dot);
                // add station event handler
                s.map_dot.MouseEnter += new MouseEventHandler(this.station_Enter);
                s.map_dot.MouseLeave += new MouseEventHandler(this.station_Leave);
                s.map_dot.MouseRightButtonUp += new MouseButtonEventHandler(this.station_MouseRightButtonUp);
                s.map_dot.MouseLeftButtonUp += new MouseButtonEventHandler(this.station_MouseLeftButtonUp);
                // station label
                s.map_label.Text = s.name;
                s.map_label.Foreground = Brushes.Black;
                // if name too long, smaller text
                s.map_label.FontSize = s.map_label.Text.Length > 3 ? 10 : 12;
                // add to map, above circles
                Canvas.SetLeft(s.map_label, s.pos_x + 2);
                Canvas.SetTop(s.map_label, s.pos_y + 2);
                Canvas.SetZIndex(s.map_label, 3);
                map.Children.Add(s.map_label);
                d_stations.Add(s.id, s);

                // add to auto complete
                auto_complete_station_names.Add(s.name);
            }

            // read routes
            sql = "select * from routes";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            while (reader.Read())
            {
                graph.addEdge(reader.GetInt32(0), reader.GetInt32(1),
                    reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4));

                Route route = new Route();
                route.id = reader.GetInt32(0);
                route.from = reader.GetInt32(1);
                route.to = reader.GetInt32(2);
                route.line = reader.GetInt32(3);
                route.length = reader.GetInt32(4);
                route.color = colors[route.line];
                route.map_line.StrokeThickness = 6;
                route.map_line.Stroke = route.color;
                route.map_line.X1 = Canvas.GetLeft(d_stations[route.from].map_dot) + d_stations[route.from].map_dot.Width / 2;
                route.map_line.Y1 = Canvas.GetTop(d_stations[route.from].map_dot) + d_stations[route.from].map_dot.Width / 2;
                route.map_line.X2 = Canvas.GetLeft(d_stations[route.to].map_dot) + d_stations[route.to].map_dot.Width / 2;
                route.map_line.Y2 = Canvas.GetTop(d_stations[route.to].map_dot) + d_stations[route.to].map_dot.Width / 2;
                Canvas.SetZIndex(route.map_line, 1);
                map.Children.Add(route.map_line);
                d_routes.Add(route.id, route);
            }

            // read attractions
            sql = "select name from attractions";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            while (reader.Read())
            {
                auto_complete_attracton_names.Add(reader.GetString(0));
            }


            // init auto compelete
            txtStart.ItemsSource = auto_complete_station_names;
            txtEnd.ItemsSource = auto_complete_station_names;
            txtAttraction.ItemsSource = auto_complete_attracton_names;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string dbPath = "Data Source =" + Environment.CurrentDirectory + "/metro.db";
            conn = new SQLiteConnection(dbPath);
            conn.Open();
            
            initData();

        }

        private void map_Unloaded(object sender, RoutedEventArgs e)
        {
            conn.Close();
        }

        private void map_Reset()
        {
            foreach (Route r in d_routes.Values)
            {
                r.map_line.Stroke = r.color;
                r.map_line.StrokeThickness = 6;
            }
            lstStations.Items.Clear();
            lstStations.Visibility = Visibility.Hidden;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            map_Reset();
            int _from, _to;

            if (txtStart.Text.Length == 0 || txtEnd.Text.Length == 0)
            {
                MessageBox.Show("Query string can not be empty!");
            }

            // convert station name to id via DB
            sql = "select id from stations where name=\"" + txtStart.Text + "\"";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            if (reader.Read())
            {
                _to = reader.GetInt32(0);
            }
            else
            {
                MessageBox.Show(txtStart.Text + " not found!");
                return;
            }

            sql = "select id from stations where name=\"" + txtEnd.Text + "\"";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            if (reader.Read())
            {
                _from = reader.GetInt32(0);
            }
            else
            {
                MessageBox.Show(txtEnd.Text + " not found!");
                return;
            }

            /*Dictionary<int, int> res = graph.dijkstra(_from);

            int[] r = graph.dijM(_from);
            //for (int i=1;i<=graph.nodes.Count;i++){
            //    MessageBox.Show(names[i].Text + r[i].ToString());
            //}
            foreach (Line l in routes.Values)
            {
                // l.Stroke = Brushes.Gray;
                l.StrokeThickness = 6;
            }
            int _line_num = graph.edges[new NodePair(_to, r[_to])].line;
            long _length = 0;
            lstStations.Items.Add("Start at: " + names[_to].Text);
            while (r[_to] != 0)
            {

                //MessageBox.Show(names[_to].Text);
                NodePair np = new NodePair(_to, r[_to]);
                routes[graph.edges[np].routeId].Stroke = Brushes.Green;
                routes[graph.edges[np].routeId].StrokeThickness = 10;

                if (graph.edges[np].line != _line_num)
                {
                    lstStations.Items.Add("↓ Line " + _line_num + ": " + _length + " km");
                    lstStations.Items.Add("Transfer at: " + names[_to].Text);
                    _length = 0;
                }
                _length += graph.edges[np].length;
                //lstStations.Items.Add(_length);
                _line_num = graph.edges[np].line;

                _to = r[_to];
            }



            routes[graph.edges[new NodePair(_to, _from)].routeId].Stroke = Brushes.Green;
            routes[graph.edges[new NodePair(_to, _from)].routeId].StrokeThickness = 10;
            _length += graph.edges[new NodePair(_to, _from)].length;
            lstStations.Items.Add("↓ Line " + _line_num + ": " + _length + " km");
            lstStations.Items.Add("Arrive at: " + names[_from].Text);

            lstStations.Visibility = Visibility.Visible;
            lstStations.Height = lstStations.Items.Count * (lstStations.FontSize + 10);
            //foreach (KeyValuePair<int, int> i in res)
            //{
            //    MessageBox.Show(names[i.Key].Text + i.Value.ToString());
            //}
            */

        }

        private void btnSwitch_Click(object sender, RoutedEventArgs e)
        {
            String tmp = tmp = txtStart.Text;
            txtStart.Text = txtEnd.Text;
            txtEnd.Text = tmp;
            map_Reset();
        }

        private void map_holder_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            map_sc.ScaleX *= 1 + e.Delta * 0.001;
            if (map_sc.ScaleX > 10) map_sc.ScaleX = 10;
            if (map_sc.ScaleX < 0.1) map_sc.ScaleX = 0.1;
            map_sc.ScaleY *= 1 + e.Delta * 0.001;
            if (map_sc.ScaleY > 10) map_sc.ScaleY = 10;
            if (map_sc.ScaleY < 0.1) map_sc.ScaleY = 0.1;
            map_sc.CenterX = e.GetPosition(map).X;
            map_sc.CenterY = e.GetPosition(map).Y;
            //map_tr.X = -e.GetPosition(map).X;
            //map_tr.Y = -e.GetPosition(map).Y;
            //MessageBox.Show(e.GetPosition(map).X.ToString());


            //map.LayoutTransform = map_tg;
            //map.UpdateLayout();
        }

        private void map_holder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            map_drag = true;
            map_prev_X = e.GetPosition(map).X;
            map_prev_Y = e.GetPosition(map).Y;
        }

        private void map_holder_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            map_drag = false;
        }

        private void map_holder_MouseMove(object sender, MouseEventArgs e)
        {
            if (map_drag)
            {
                map_tr.X += (e.GetPosition(map).X - map_prev_X) * map_sc.ScaleX;
                map_tr.Y += (e.GetPosition(map).Y - map_prev_Y) * map_sc.ScaleY;
                map_prev_X = e.GetPosition(map).X;
                map_prev_Y = e.GetPosition(map).Y;
            }
        }

        private void map_holder_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            map_Reset();
        }

        private void station_Enter(object sender, MouseEventArgs e)
        {
            (sender as Ellipse).Fill = Brushes.OrangeRed;

            sql = "select * from attractions where station_id=\"" + (sender as Ellipse).Tag + "\"";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            if (reader.Read())
            {
                lbl_att_name.Content = reader.GetString(2);
                txt_lbl_detail.Text = reader.GetString(3);
                panel_attraction.Visibility = Visibility.Visible;
            }

        }
        private void station_Leave(object sender, MouseEventArgs e)
        {
            (sender as Ellipse).Fill = Brushes.White;
            panel_attraction.Visibility = Visibility.Hidden;
        }

        private void station_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            main_tab.SelectedIndex = 0;
            txtEnd.Text = d_stations[(int)(sender as Ellipse).Tag].name;
        }

        private void station_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            main_tab.SelectedIndex = 0;
            txtStart.Text = d_stations[(int)(sender as Ellipse).Tag].name;
        }

        private void btnQueryAttraction_Click(object sender, RoutedEventArgs e)
        {
            sql = "select * from attractions where name=\"" + txtAttraction.Text + "\"";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            if (reader.Read())
            {
                lbl_att_name.Content = reader.GetString(2);
                txt_lbl_detail.Text = reader.GetString(3);
                panel_attraction.Visibility = Visibility.Visible;
                d_stations[reader.GetInt32(1)].map_dot.Fill = Brushes.OrangeRed;
                lblAttStation.Content = "Station: "
                    + d_stations[reader.GetInt32(1)].name
                    + "\nX: " + d_stations[reader.GetInt32(1)].pos_x
                    + ", Y: " + d_stations[reader.GetInt32(1)].pos_y;
            }
            else
            {
                MessageBox.Show(txtAttraction.Text + "not exist.");
                lblAttStation.Content = "";
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            EditStation dlgEdit = new EditStation();
            dlgEdit.Show();
        }
    }
}
