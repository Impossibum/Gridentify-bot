using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs_grid
{
    internal class Coordinate
    {   
        public int value { get; }
        public int index { get; }
        public int coord_x { get; }
        public int coord_y { get; }
        public List<int> neighbors { get; }
        
        public Coordinate(int Index, int val)
        {
            index = Index;
            value = val;
            coord_y = (int)index / 5;
            coord_x = index - coord_y * 5;
            neighbors = new List<int>();
            if (coord_x < 4) { 
                neighbors.Add(index +1);
            }
            if (coord_x > 0)
            {
                neighbors.Add(index - 1);
            }
            if (coord_y < 4)
            {
                neighbors.Add(index + 5);
            }
            if (coord_y > 0)
            {
                neighbors.Add(index - 5);
            }
        }
    }
}
