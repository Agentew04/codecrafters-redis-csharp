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
        await Console.Out.WriteLineAsync("PSYNC");

        RespToken response = new SimpleStringToken() {
            Value = $"FULLRESYNC {masterReplId} 0"
        };
        await stream.WriteAsync(response);

        // send RDB FILE
        response = new FileToken() {
            Content = Rdb.EmptyFile,
            Length = Rdb.EmptyFile.Length
        };
        await stream.WriteAsync(response);
    }

    private static async Task ReplConfCommand(NetworkStream stream, List<string> args) {
        await Console.Out.WriteLineAsync("REPLCONF");
        SimpleStringToken response = new() {
            Value = "OK"
        };
        await stream.WriteAsync(response);

        // register the replica in memory
        if (!replicaStreams.Contains(stream)) {
            replicaStreams.Add(stream);
        }
    }

    private static async Task InfoCommand(NetworkStream stream, List<string> args) {
        StringBuilder sb = new();

        await Console.Out.WriteLineAsync("INFO");

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

        // does not have expiry, get data
        if (!expiry.HasValue) {
            response = new BulkStringToken() {
                Length = value.Length,
                Value = value
            };
        }
        // has expiry, is not expired, get data
        else if ((expiry.Value.Ticks - DateTime.Now.Ticks > 0)) {
            response = new BulkStringToken() {
                Length = value.Length,
                Value = value
            };
        }
        // has expiry, is expired, set null bulk string
        else {
            response = new NullBulkString();
        }

        if (isMaster) {
            await stream.WriteAsync(response);
        }
    }

    private static async Task SetCommand(NetworkStream stream, List<string> args) {
        string key = args[1];
        string value = args[2];
        string? px = args.Count > 3 ? args[3] : null;
        string? pxMsStr = args.Count > 4 ? args[4] : null;
        await Console.Out.WriteLineAsync($"SET {key}: {value}");

        if (px is null || pxMsStr is null) {
            _data[key] = (value, null);
        } else {
            int pxMs = int.Parse(pxMsStr);
            _data[key] = (value, DateTime.Now.AddMilliseconds(pxMs));
        }
        SimpleStringToken response = new() {
            Value = "OK"
        };

        if (isMaster) {
            await stream.WriteAsync(response);
        }
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
        await Console.Out.WriteLineAsync($"PING");
        SimpleStringToken response = new() {
            Value = "PONG"
        };
        await stream.WriteAsync(response);
    }
}
