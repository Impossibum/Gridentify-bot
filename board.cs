using Microsoft.AspNetCore.Mvc.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cs_grid
{
    internal class Board
    {
        public Coordinate[] coordinates { get; }
        public int[] board_array { get; }

        public List<int[]> moves { get; }

        public int max_move_length { get; }

        public Board(int[] _board_array, int move_length) {
            max_move_length = move_length;
            board_array = _board_array;
            //board_filler(_board_array);
            coordinates = new Coordinate[board_array.Length];
            for (int i = 0; i < board_array.Length; i++) {
                coordinates[i] = new Coordinate(i, board_array[i]);
            }
            moves = new List<int[]>();
            generate_legal_moves();
        }

        private void board_filler(int[] arr) {
            var r = new Random();
            for(int i  = 0; i < arr.Length; i++)
            {
                if (arr[i] == -1) {
                    arr[i] = r.Next(1, 4);
                }
            }
        }
        public Board board_move(int move_index)
        {
            var copy = board_array.ToArray();
            
            var r = new Random();
            int move_val = board_array[moves[move_index][0]];
            for (int i = 0; i < moves[move_index].Length-1; i++) {
                copy[moves[move_index][i]] = r.Next(1, 4);
            }
            copy[moves[move_index].Last()] = move_val * moves[move_index].Length;
            return new Board(copy, max_move_length);
        }

        private void generate_legal_moves() { 
            for(int i = 0; i < board_array.Length; i++) {
                moves.AddRange(generate_moves_from_coord(coordinates[i], new int[0], max_move_length));
            }
        }

        private List<int[]> generate_moves_from_coord(Coordinate coord, int[] move_array, int depth) {
            List<int[]> local_moves = new List<int[]>();
            List<int[]> potential_moves = new List<int[]>();

            if (depth < 1 || (move_array.Length > 0 && coord.value != coordinates[move_array[0]].value) || move_array.Contains(coord.index)) {
                return local_moves;
            }
            int[] move = new int[1];
            if (move_array.Length > 0)
            {
                move = new int[move_array.Length + 1];
                Array.Copy(move_array, move, move_array.Length);
                move[move_array.Length] = coord.index;
            }
            else {
                move[0] = coord.index;
            }
            
            if (move.Length > 1)
            {
                local_moves.Add(move);
            }
            

            for (int i = 0; i < coord.neighbors.Count; i++)
            {
                potential_moves = generate_moves_from_coord(coordinates[coord.neighbors[i]], move, depth -1);
                if (potential_moves.Count > 0)
                {
                    for (int j = 0; j < potential_moves.Count; j++)
                    {
                        if (potential_moves[j].Length > 1)
                        {
                            local_moves.Add(potential_moves[j]);
                        }
                    }
                }
                
            }
            return local_moves;
        } 
    }
}
