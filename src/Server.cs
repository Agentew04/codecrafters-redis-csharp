using codecrafters_redis.src.RESP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace codecrafters_redis.src;

public static partial class Server {

    public static async Task Main(string[] args) {
        ParseFlags(args);
        Setup();
        
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

            if(bytesRead == 0) {
                break;
            }
            byte[] request = buffer[..bytesRead];

            var reqToken = RespToken.Parse(request, out _);

            if(reqToken is not ArrayToken arrayToken) {
                await Console.Out.WriteLineAsync("req is not array");
                continue;
            }

            var args = FlattenArgs(arrayToken);

            args = args.Select(a => a.ToLower()).ToList();
            

            var cmd = args[0];
            await Console.Out.WriteLineAsync($"Command: {cmd}");
            if (cmd == "ping") {
                await PingCommand(stream);
            } else if(cmd == "echo") {
                await EchoCommand(stream, args);
            } else if(cmd == "set") {
                await SetCommand(stream, args);
                await SendCommandToReplicas(request);
            } else if(cmd == "get") {
                await GetCommand(stream, args);
                await SendCommandToReplicas(request);
            } else if (cmd == "info") {
                await InfoCommand(stream, args);
            }else if(cmd == "replconf") {
                await ReplConfCommand(stream, args);
            }else if(cmd == "psync") {
                await PSyncCommand(stream, args);
            } else {
                await Console.Out.WriteLineAsync("Command not found");
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

    private static async Task Setup() {
        if (isMaster) {
            masterReplId = Convert.ToHexString(RandomNumberGenerator.GetBytes(20));
            masterReplOffset = 0;
        } else { // replica
            await HandshakeMaster();
        }
    }

    private static async Task HandshakeMaster() {
        TcpClient tcpClient = new TcpClient(masterHost, masterPort);
        using NetworkStream stream = tcpClient.GetStream();

        // ping
        ArrayToken request = new();
        request.Tokens.Add(BulkStringToken.FromString("PING"));
        request.Count = 1;
        await stream.WriteAsync(request);

        // replconf 1
        request = new();
        request.Tokens.Add(BulkStringToken.FromString("REPLCONF"));
        request.Tokens.Add(BulkStringToken.FromString("listening-port"));
        request.Tokens.Add(BulkStringToken.FromString(port.ToString()));
        request.Count = 3;
        await stream.WriteAsync(request);

        // replconf 2
        request = new();
        request.Tokens.Add(BulkStringToken.FromString("REPLCONF"));
        request.Tokens.Add(BulkStringToken.FromString("capa"));
        request.Tokens.Add(BulkStringToken.FromString("psync2"));
        request.Count = 3;
        await stream.WriteAsync(request);

        // psync
        request = new();
        request.Tokens.Add(BulkStringToken.FromString("PSYNC"));
        request.Tokens.Add(BulkStringToken.FromString("?"));
        request.Tokens.Add(BulkStringToken.FromString("-1"));
        request.Count = 3;
        await stream.WriteAsync(request);
    }

    private static async Task SendCommandToReplicas(byte[] args) {
        foreach (var replica in replicaStreams) {
            await replica.WriteAsync(args);
        }
    }
}