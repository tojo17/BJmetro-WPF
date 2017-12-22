using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Controls;

namespace metro
{
    class Station
    {
        public int id;
        public string name;
        public int pos_x;
        public int pos_y;
        public int route_count;

        public Ellipse map_dot;
        public TextBlock map_label;

        public Station()
        {
            id = -1;
            map_dot = new Ellipse();
            map_label = new TextBlock();
        }
    }
}
