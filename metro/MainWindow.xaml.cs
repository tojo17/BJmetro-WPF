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
        bool map_drag = false;
        double map_prev_X;
        double map_prev_Y;
        SQLiteConnection conn;
        Dictionary<int, Ellipse> stations;
        Dictionary<int, Line> routes;
        Dictionary<int, TextBlock> names;
        List<String> lNames;
        Graph graph;
        string sql;
        SQLiteCommand cmdQ;
        SQLiteDataReader reader;
        Dictionary<int, Brush> colors;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string dbPath = "Data Source =" + Environment.CurrentDirectory + "/metro.db";
            conn = new SQLiteConnection(dbPath);

            conn.Open();
            graph = new Graph();
            lNames = new List<string>();

            sql = "select * from colors";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();

            colors = new Dictionary<int, Brush>();

            while (reader.Read())
            {
                int line = reader.GetInt32(0);
                byte r = (byte)reader.GetInt32(1);
                byte g = (byte)reader.GetInt32(2);
                byte b = (byte)reader.GetInt32(3);
                Brush br = new SolidColorBrush(Color.FromRgb(r, g, b));
                colors.Add(line, br);
            }

            sql = "select * from stations";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();

            map.Children.Clear();
            stations = new Dictionary<int, Ellipse>();
            names = new Dictionary<int, TextBlock>();

            while (reader.Read())
            {
                graph.addNode(reader.GetInt32(0));

                Ellipse station = new Ellipse();
                station.Tag = reader.GetInt32(0); //id
                station.StrokeThickness = 2;
                station.Stroke = Brushes.Black;
                station.Fill = Brushes.White;
                station.Width = (reader.GetInt32(4) > 2) ? 15 : 8;
                station.Height = station.Width;
                Canvas.SetLeft(station, reader.GetInt32(2) - station.Width / 2);
                Canvas.SetTop(station, reader.GetInt32(3) - station.Width / 2);
                Canvas.SetZIndex(station, 2);
                map.Children.Add(station);
                // add station event handler
                station.MouseEnter += new MouseEventHandler(this.station_Enter);
                station.MouseLeave += new MouseEventHandler(this.station_Leave);
                station.MouseRightButtonUp += new MouseButtonEventHandler(this.station_MouseRightButtonUp);
                station.MouseLeftButtonUp += new MouseButtonEventHandler(this.station_MouseLeftButtonUp);
                stations.Add(reader.GetInt32(0), station);

                TextBlock sName = new TextBlock();
                sName.Text = reader.GetString(1);
                sName.Foreground = Brushes.Black;
                sName.FontSize = sName.Text.Length > 3 ? 10 : 12;
                Canvas.SetLeft(sName, reader.GetInt32(2) + 2);
                Canvas.SetTop(sName, reader.GetInt32(3) + 2);
                Canvas.SetZIndex(sName, 3);
                map.Children.Add(sName);
                names.Add(reader.GetInt32(0), sName);
                lNames.Add(sName.Text);
            }

            sql = "select * from routes";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();

            routes = new Dictionary<int, Line>();

            while (reader.Read())
            {
                graph.addEdge(reader.GetInt32(0), reader.GetInt32(1),
                    reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4));

                Line route = new Line();
                route.StrokeThickness = 6;

                route.Fill = colors[reader.GetInt32(3)]; //back up color
                route.Stroke = colors[reader.GetInt32(3)];
                route.X1 = Canvas.GetLeft(stations[reader.GetInt32(1)]) + stations[reader.GetInt32(1)].Width / 2;
                route.Y1 = Canvas.GetTop(stations[reader.GetInt32(1)]) + stations[reader.GetInt32(1)].Width / 2;
                route.X2 = Canvas.GetLeft(stations[reader.GetInt32(2)]) + stations[reader.GetInt32(2)].Width / 2;
                route.Y2 = Canvas.GetTop(stations[reader.GetInt32(2)]) + stations[reader.GetInt32(2)].Width / 2;
                Canvas.SetZIndex(route, 1);
                map.Children.Add(route);
                routes.Add(reader.GetInt32(0), route);
            }

            //init auto compelete
            txtStart.ItemsSource = lNames;
            txtEnd.ItemsSource = lNames;

        }

        private void map_Unloaded(object sender, RoutedEventArgs e)
        {
            conn.Close();
        }

        private void map_Reset()
        {
            foreach (Line l in routes.Values)
            {
                l.Stroke = l.Fill;
                l.StrokeThickness = 6;
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

            Dictionary<int, int> res = graph.dijkstra(_from);

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
        }
        private void station_Leave(object sender, MouseEventArgs e)
        {
            (sender as Ellipse).Fill = Brushes.White;
        }

        private void station_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtEnd.Text = names[(int)(sender as Ellipse).Tag].Text;
        }

        private void station_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            txtStart.Text = names[(int)(sender as Ellipse).Tag].Text;
        }
    }
}
