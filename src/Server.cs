using codecrafters_redis.src.RESP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;

namespace codecrafters_redis.src;

public static partial class Server {

    private static int port;
    private static readonly Dictionary<string, (string value, DateTime? expiry)> _data = new();
    private static bool isMaster = true;
    private static string masterHost;
    private static int masterPort;
    

    public static async Task Main(string[] args) {
        ParseFlags(args);
        
        TcpListener server = new TcpListener(IPAddress.Any, port);
        server.Start();

        while (true) {
            await Console.Out.WriteLineAsync("waiting new client");
            TcpClient client = server.AcceptTcpClient(); // blocking
            await Console.Out.WriteLineAsync("new conn received. handling");
            HandleClient(client);
        }
    }

    public static async Task HandleClient(TcpClient client) {
        using NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];

        int bytesRead;
        do {
            bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            string request = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            await Console.Out.WriteLineAsync($"Received: {request}");

            if (request.Length == 0) {
                await Console.Out.WriteLineAsync("end conn");
                break;
            }
            request = request.ToLower();

            var reqToken = RespToken.Parse(request, out _);

            if(reqToken is not ArrayToken arrayToken) {
                await Console.Out.WriteLineAsync("req is not array");
                continue;
            }

            var args = FlattenArgs(arrayToken);

            args.ForEach(a => Console.Out.Write($"Arg: {a}; "));
            await Console.Out.WriteLineAsync();


            var cmd = args[0];
            await Console.Out.WriteLineAsync($"Command: {cmd}");
            if (cmd == "ping") {
                await PingCommand(stream);
            } else if(cmd == "echo") {
                await EchoCommand(stream, args);
            } else if(cmd == "set") {
                await SetCommand(stream, args);
            } else if(cmd == "get") {
                await GetCommand(stream, args);
            } else if (cmd == "info") {
                await InfoCommand(stream, args);
            }
        } while(bytesRead > 0);
        client.Close();
    }

    private static List<string> FlattenArgs(ArrayToken arrayToken) {
        return arrayToken.Tokens
            .Where(t => t is BulkStringToken)
            .Cast<BulkStringToken>()
            .Select(t => t.Value)
            .ToList();
    }

    private static void ParseFlags(string[] args) {

        port = 6379; // default value
        int portTagIndex = Array.IndexOf(args, "--port");
        if (portTagIndex != -1) {
            try {
                port = int.Parse(args[portTagIndex + 1]);
            } catch {
                port = 6379;
            }
        }

        isMaster = true; // default values
        masterHost = "";
        masterPort = 0;
        int replicaofTagIndex = Array.IndexOf(args, "--replicaof");
        if (replicaofTagIndex != -1) {
            try {
                isMaster = false;
                masterHost = args[replicaofTagIndex + 1];
                masterPort = int.Parse(args[replicaofTagIndex + 2]);
            } catch {
                isMaster = true;
            }
        }
    }
}