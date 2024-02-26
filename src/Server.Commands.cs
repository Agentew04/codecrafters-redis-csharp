using codecrafters_redis.src.RESP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace codecrafters_redis.src; 
public static partial class Server {
    
    private static async Task PSyncCommand(NetworkStream stream, List<string> args) {
        string replid = args[1];
        long offset = long.Parse(args[2]);

        RespToken response = new SimpleStringToken() {
            Value = $"FULLRESYNC {masterReplId} 0"
        };
        await stream.WriteAsync(response);

        response = new FileToken() {
            Content = Rdb.EmptyFile,
            Length = Rdb.EmptyFile.Length
        };
        await stream.WriteAsync(response);
    }

    private static async Task ReplConfCommand(NetworkStream stream, List<string> args) {
        SimpleStringToken response = new() {
            Value = "OK"
        };
        await stream.WriteAsync(response);
    }

    private static async Task InfoCommand(NetworkStream stream, List<string> args) {
        StringBuilder sb = new();

        if (args[1] != "replication") {
            return;
        }
        sb.Append("# Replication\r\n");
        sb.Append($"role:{(isMaster ? "master" : "slave")}\r\n");
        sb.Append("connected_slaves:0\r\n");
        sb.Append($"master_replid:{masterReplId}\r\n");
        sb.Append($"master_repl_offset:{masterReplOffset}\r\n");

        BulkStringToken response = new() {
            Length = sb.Length,
            Value = sb.ToString()
        };
        await stream.WriteAsync(response);
    }

    private static async Task GetCommand(NetworkStream stream, List<string> args) {
        string key = args[1];
        await Console.Out.WriteLineAsync($"GET {key}");
        (string value, DateTime? expiry) = _data[key];
        RespToken response;

        await Console.Out.WriteLineAsync($"Value: {value}; Expiry: {(expiry.HasValue ? expiry.Value : "null")}");

        await Console.Out.WriteLineAsync($"NowTicks: {DateTime.Now.Ticks} ExpiryTicks: {(expiry.HasValue ? expiry.Value.Ticks : "null")} " +
            $"Diff: {(expiry.HasValue ? expiry.Value.Ticks : 0) - DateTime.Now.Ticks}");

        // does not have expiry, get data
        if (!expiry.HasValue) {
            await Console.Out.WriteLineAsync("Expiry not found. Retrieving data");
            response = new BulkStringToken() {
                Length = value.Length,
                Value = value
            };
        }
        // has expiry, is not expired, get data
        else if ((expiry.Value.Ticks - DateTime.Now.Ticks > 0)) {
            await Console.Out.WriteLineAsync("Expiry found. Not expired yet. retrieving data");
            response = new BulkStringToken() {
                Length = value.Length,
                Value = value
            };
        }
        // has expiry, is expired, set null bulk string
        else {
            await Console.Out.WriteLineAsync("Expiry found. The diff was negative, was expired. issuing nullbulk");
            response = new NullBulkString();
        }

        await stream.WriteAsync(response);
        await Console.Out.WriteLineAsync("Sent data from get");
    }

    private static async Task SetCommand(NetworkStream stream, List<string> args) {
        string key = args[1];
        string value = args[2];
        string? px = args.Count > 3 ? args[3] : null;
        string? pxMsStr = args.Count > 4 ? args[4] : null;
        await Console.Out.WriteLineAsync($"SET {key}: {value}");

        if (px is null || pxMsStr is null) {
            await Console.Out.WriteLineAsync($"No expiry data found(px: {px ?? "null"}; pxMsStr: {pxMsStr ?? "null"})");
            _data[key] = (value, null);
        } else {
            await Console.Out.WriteLineAsync($"Expiry data found(px: {px}; pxMsStr: {pxMsStr})");
            int pxMs = int.Parse(pxMsStr);
            _data[key] = (value, DateTime.Now.AddMilliseconds(pxMs));
        }
        await Console.Out.WriteLineAsync("Data has been set! Responding");
        SimpleStringToken response = new() {
            Value = "OK"
        };
        await stream.WriteAsync(response);
        await Console.Out.WriteLineAsync("Sent ok from set");
    }

    private static async Task EchoCommand(NetworkStream stream, List<string> args) {
        string echoContentToken = args[1];
        BulkStringToken response = new() {
            Length = echoContentToken.Length,
            Value = echoContentToken
        };
        await stream.WriteAsync(response);
    }

    private static async Task PingCommand(NetworkStream stream) {
        SimpleStringToken response = new() {
            Value = "PONG"
        };
        await stream.WriteAsync(response);
        await Console.Out.WriteLineAsync($"Sent pong");
    }
}
