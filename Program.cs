using System;
using System.Threading;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using cs_grid;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Diagnostics;

Console.Title = "grid client";

Console.WriteLine("What name should be used for the leaderboard?");
string name = Console.ReadLine();
Console.WriteLine($"Greetings {name}, how deep will we be searching today? (max of 4 suggested)");
int depth = Int32.Parse(Console.ReadLine());
Console.WriteLine("How many threads should be spawned for move search? (5-20 suggested)");
int thread_count = Int32.Parse(Console.ReadLine());
Console.WriteLine("What should be the maximum length move allowed? (3 is likely ideal)");
int move_length = Int32.Parse(Console.ReadLine());
Console.WriteLine("Enter 1 for verbose mode, 0 for quiet.");
bool verbose = Console.ReadLine() == "1";
int total_score = 0;
Console.WriteLine("Game Started!");
int highest_score = 0;
int count = 1;
int move_count = 0;
Stopwatch sw = Stopwatch.StartNew();
while (true)
{
    int[] board_array;
    Board gameboard;
    CancellationTokenSource source = new CancellationTokenSource();
    CancellationToken token = source.Token;
    var serialized = JsonConvert.SerializeObject(name);
    using var ws = new ClientWebSocket();
    byte[] byte_name = utilities.ServerTranslation(serialized);
    await ws.ConnectAsync(new Uri("wss://server.lucasholten.com:21212"), token);
    await ws.SendAsync(byte_name, WebSocketMessageType.Text, true, token);
    byte[] buf = new byte[1056];
    int[] move = new int[2];
    int score = 0;
    float val = 0f;
    

    while (ws.State == WebSocketState.Open)
    {
        sw.Restart();
        move_count++;
        serialized = "";
        //Console.WriteLine("awaiting info");
        var result = await ws.ReceiveAsync(buf, token);
        string board = Encoding.UTF8.GetString(buf);
       // Console.WriteLine($"Received {board}");
        board_array = utilities.ParseBoard(board);
        gameboard = new Board(board_array, move_length);
        if (gameboard.moves.Count > 0)
        {
            var search_result = utilities.move_search(gameboard, depth, thread_count);
            val = search_result.Item1;
            move = search_result.Item2;
            //Console.WriteLine(val.ToString());
            //Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
            serialized = JsonConvert.SerializeObject(move);
            byte_name = utilities.ServerTranslation(serialized);
            score += board_array[move[0]] * move.Length;
            if (verbose) {
                Console.WriteLine($"Sending move #{move_count.ToString()} took {sw.ElapsedMilliseconds/1000f} seconds to execute.");
            }
            await ws.SendAsync(byte_name, WebSocketMessageType.Text, true, token);
        }
        else
        {
            //Console.WriteLine(board);
            total_score += score;
            if(score > highest_score) {
                highest_score = score;
            }
            Console.WriteLine("Game " + count.ToString() + " score: " + score.ToString() +", avg: " + (total_score/count).ToString() + $", High score: {highest_score.ToString()}");
            // d2 with -1 placeholders: Game 99 ended with score: 154 and a rolling avg of 290
            // d2 with randomized values: Game 99 ended with score: 688 and a rolling avg of 400
            // d2 with randomized and avg eval: Game 99 ended with score: 184 and a rolling avg of 173
            move_count = 0;
            break;
        }
        Array.Clear(buf);
    }
    count += 1;
}