using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Xml.Schema;

namespace cs_grid
{
    internal class utilities
    {
        static int[] magic_numbers = {3, 6, 12, 24, 48, 96, 192, 384, 768, 1536, 3072};
        public static int thread_limit = 5;
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public static byte[] ServerTranslation(string arg)
        {
            return Encoding.UTF8.GetBytes(arg);
        }

        public static int[] ParseBoard(string arg) {
            arg = arg.Substring(1);
            int[] ints = new int[25];
            int count = 0;
            string num = "";
            for (int i = 0; i < arg.Length; i++) {
                if (char.IsDigit(arg[i]))
                {
                    num += arg[i];
                }
                else {
                    if (!string.Equals(num, "")){
                        ints[count] = Int32.Parse(num);
                        num = "";
                        count += 1;
                    }
                }
            }
            return ints;
        }
        public static float board_eval(Board _board)
        {
            int total = 0;
            for (int i = 0; i < _board.coordinates.Length; i++)
            {
                if (magic_numbers.Contains(_board.board_array[i]))
                {
                    for (int j = 0; j < _board.coordinates[i].neighbors.Count; j++)
                    {
                        if (_board.board_array[i] == _board.board_array[_board.coordinates[i].neighbors[j]])
                        {
                            total += _board.board_array[i];
                        }
                    }
                }

            }
            return total;
        }

        public static (float, int[]) move_search(Board starting_board, int search_depth, int thread_count)
        {
            float base_score = board_eval(starting_board);
            if (search_depth < 1 || starting_board.moves.Count < 1)
            {
                return (base_score, new int[2]);
            }

            float best_score = -9999999f;
            int[] best_move = starting_board.moves[0];

            float score = 0f;
            Board generated_board;
            float score_average = 0f;

            if (thread_count > 0)
            {

                Thread[] threads = new Thread[thread_count];
                List<int[]> generated_moves = starting_board.moves;
                float[] run_results = new float[starting_board.moves.Count];
                List<int[]> new_moves = new List<int[]>();
                for (int i = 0; i < 4; i++)
                {
                    if (i >= 1 && generated_moves.Count > 3)
                    {
                        new_moves = new List<int[]>();
                        for (int x = 0; x < generated_moves.Count; x++)
                        {
                            if (run_results[x] > score_average)
                            {
                                new_moves.Add(generated_moves[x]);
                            }
                        }
                        generated_moves = new_moves;
                        run_results = new float[generated_moves.Count];
                        score_average = 0f;
                    }
                    for (int j = 0; j < generated_moves.Count; j++)
                    {
                        for (int k = 0; k < threads.Length; k++)
                        {
                            threads[k] = new Thread(() =>
                            {
                                generated_board = starting_board.board_move(starting_board.moves.IndexOf(generated_moves[j]));
                                var result = move_search(generated_board, search_depth - 1, 0);
                                run_results[j] += result.Item1;
                            });
                            threads[k].Start();
                        }
                        for (int k = 0; k < threads.Length; k++)
                        {
                            threads[k].Join();
                        }

                    }
                    score_average = run_results.Sum() / run_results.Length;
                }

                for (int i = 0; i < generated_moves.Count; i++)
                {
                    if (run_results[i] > best_score)
                    {
                        best_score = run_results[i];
                        best_move = generated_moves[i];
                    }
                }
                return (best_score + base_score, best_move);
            }

            else
            {
                for (int i = 0; i < starting_board.moves.Count; i++)
                {
                    generated_board = starting_board.board_move(i);
                    var result = move_search(generated_board, search_depth - 1, 0);
                    score = result.Item1;
                    if (score > best_score)
                    {
                        best_move = starting_board.moves[i];
                        best_score = score;
                    }
                }
            }


            return (best_score + base_score, best_move);
        }

        /*
        public static (float, int[]) move_search(Board starting_board, int search_depth, bool run_averages) {
            float base_score = board_eval(starting_board);
            if (search_depth < 1 || starting_board.moves.Count < 1) { 
                return (base_score, new int[2]);
            }

            float best_score = -9999999f;
            int[] best_move = starting_board.moves[0];

            float score = 0f;
            Board generated_board;
            float score_average = 0f;

            if (run_averages)
            {
                
                Thread[] threads = new Thread[10];
                int[][] generated_moves = starting_board.moves.ToArray();
                float[] run_results = new float[starting_board.moves.Count];
                List<int[]> new_moves = new List<int[]>();
                for (int i = 0; i < 4; i++) {
                    if (i >= 1 && generated_moves.Length > 3) {
                        new_moves = new List<int[]>();
                        for (int x = 0; x < generated_moves.Length; x++) {
                            if (run_results[x] > score_average) {
                                new_moves.Add(generated_moves[x]);
                            }
                        }
                        generated_moves = new_moves.ToArray();
                        run_results = new float[generated_moves.Length];
                        score_average = 0f;
                    }
                    for (int j = 0; j < generated_moves.Length; j++){
                        for (int k = 0; k < threads.Length; k++) {
                            threads[k] = new Thread(() =>
                            {
                                generated_board = starting_board.board_move(starting_board.moves.IndexOf(generated_moves[j]));
                                var result = move_search(generated_board, search_depth - 1, false);
                                run_results[j] += result.Item1;
                            });
                            threads[k].Start();
                        }
                        for (int k = 0; k < threads.Length; k++) {
                            threads[k].Join();
                        }
                            
                    }
                    score_average = run_results.Sum()/run_results.Length;
                }

                for (int i = 0; i < generated_moves.Length; i++) {
                    if (run_results[i] > best_score) { 
                        best_score = run_results[i];
                        best_move = generated_moves[i];
                    }
                }
                return (best_score + base_score, best_move);
            }

            else { 
                for (int i = 0; i < starting_board.moves.Count; i++)
                {
                    generated_board = starting_board.board_move(i);
                    var result = move_search(generated_board, search_depth - 1, false);
                    score = result.Item1;
                    if (score > best_score)
                    {
                        best_move = starting_board.moves[i];
                        best_score = score;
                    }
                }
            }


            return (best_score + base_score, best_move);
        }
        */

    }
}
