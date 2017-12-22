using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;

namespace metro
{
    class Route
    {
        public int id;
        public int from;
        public int to;
        public int line;
        public int length;
        public Line map_line;
        public Brush color;
        public Route()
        {
            id = -1;
            map_line = new Line();
        }
    }
}
