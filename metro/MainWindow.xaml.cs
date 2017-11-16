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
        SQLiteConnection conn;
        Dictionary<int, Ellipse> stations;
        Dictionary<int, Line> routes;
        Dictionary<int, TextBlock> names;
        List<String> lNames;
        Graph graph;
        string sql;
        SQLiteCommand cmdQ;
        SQLiteDataReader reader;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //SQLiteConnection conn = null;
            string dbPath = "Data Source =" + Environment.CurrentDirectory + "/metro.db";
            conn = new SQLiteConnection(dbPath);
            
            conn.Open();
            graph = new Graph();
            lNames = new List<string>();

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
                station.StrokeThickness = 2;
                station.Stroke = Brushes.Black;
                station.Fill = Brushes.White;
                station.Width = (reader.GetInt32(4) > 2) ? 15 : 8;
                station.Height = station.Width;
                Canvas.SetLeft(station, reader.GetInt32(2) - station.Width / 2);
                Canvas.SetTop(station, reader.GetInt32(3) - station.Width / 2);
                Canvas.SetZIndex(station, 2);
                map.Children.Add(station);
                stations.Add(reader.GetInt32(0), station);

                TextBlock sName = new TextBlock();
                sName.Text = reader.GetString(1);
                sName.Foreground = Brushes.Black;
                sName.FontSize = 12;
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

            while (reader.Read()){
                graph.addEdge(reader.GetInt32(0), reader.GetInt32(1), 
                    reader.GetInt32(2), reader.GetInt32(3), reader.GetInt32(4));

                Line route = new Line();
                route.StrokeThickness = 6;
                route.Fill = (reader.GetInt32(3) == 1) ? Brushes.Red : Brushes.Blue; //back up color
                route.Stroke = (reader.GetInt32(3) == 1) ? Brushes.Red : Brushes.Blue;
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
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            int _from, _to;

            sql = "select id from stations where name=\"" + txtStart.Text + "\"";
            cmdQ = new SQLiteCommand(sql, conn);
            reader = cmdQ.ExecuteReader();
            if (reader.Read())
            {
                _from = reader.GetInt32(0);
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
                _to = reader.GetInt32(0);
            }
            else
            {
                MessageBox.Show(txtEnd.Text + " not found!");
                return;
            }
            
            Dictionary<int,int> res = graph.dijkstra(_from);

            int[] r = graph.dijM(_from);
            //for (int i=1;i<=graph.nodes.Count;i++){
            //    MessageBox.Show(names[i].Text + r[i].ToString());
            //}
            foreach (Line l in routes.Values)
            {
                l.Stroke = Brushes.Gray;
                l.StrokeThickness = 6;
            }
            while (r[_to]!=0)
            {

                    //MessageBox.Show(names[_to].Text);
                    routes[graph.edges[new NodePair(_to, r[_to])].routeId].Stroke = Brushes.Green;
                    routes[graph.edges[new NodePair(_to, r[_to])].routeId].StrokeThickness = 10;
                    _to = r[_to];
            }

            routes[graph.edges[new NodePair(_to, _from)].routeId].Stroke = Brushes.Green;
            routes[graph.edges[new NodePair(_to, _from)].routeId].StrokeThickness = 10;

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


    }
}
