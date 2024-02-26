using codecrafters_redis.src.RESP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Text.RegularExpressions;

namespace codecrafters_redis.src;

public static class Server {

    private static readonly Dictionary<string, (string value, DateTime? expiry)> _data = new();

    const string CRLF = "\r\n";
    const int PORT = 6379;
    const string PING_RESPONSE = "+PONG\r\n";

    public static async Task Main() {
        Console.WriteLine("Logs from your program will appear here!");

        TcpListener server = new TcpListener(IPAddress.Any, PORT);
        server.Start();

        while (true) {
            await Console.Out.WriteLineAsync("waiting new client");
            TcpClient client = server.AcceptTcpClient(); // blockng
            await Console.Out.WriteLineAsync("new conn received. handling");
#pragma warning disable CS4014 // Como esta chamada não é esperada, a execução do método atual continua antes de a chamada ser concluída
            HandleClient(client);
#pragma warning restore CS4014 // Como esta chamada não é esperada, a execução do método atual continua antes de a chamada ser concluída
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

    private static async Task InfoCommand(NetworkStream stream, List<string> args) {

    }

    private static async Task GetCommand(NetworkStream stream, List<string> args) {
        string key = args[1];
        await Console.Out.WriteLineAsync($"GET {key}");
        (string value, DateTime? expiry) = _data[key];
        RespToken response;

        await Console.Out.WriteLineAsync($"Value: {value}; Expiry: {(expiry.HasValue ? expiry.Value : "null")}");

        // does not have expiry, get data
        if(expiry is null) {
            response = new BulkStringToken() {
                Length = value.Length,
                Value = value
            };
        }
        // has expiry, is not expired, get data
        else if(expiry <= DateTime.Now) {
            response = new BulkStringToken() {
                Length = value.Length,
                Value = value
            };
        }
        // has expiry, is expired, set null bulk string
        else {
            response = new NullBulkString();
        }
        
        byte[] responseBytes = Encoding.UTF8.GetBytes(response.ToRESP());
        await stream.WriteAsync(responseBytes);
        await Console.Out.WriteLineAsync("Sent ok from set");
    }

    private static async Task SetCommand(NetworkStream stream, List<string> args) {
        string key = args[1];
        string value = args[2];
        string? px = args.Count > 3 ? args[3] : null;
        string? pxMsStr = args.Count > 4 ? args[4] : null;
        await Console.Out.WriteLineAsync($"SET {key}: {value}");

        if(px is null || pxMsStr is null) {
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
        byte[] responseBytes = Encoding.UTF8.GetBytes(response.ToRESP());
        await stream.WriteAsync(responseBytes);
        await Console.Out.WriteLineAsync("Sent ok from set");
    }

    private static async Task EchoCommand(NetworkStream stream, List<string> args) {
        string echoContentToken = args[1];
        BulkStringToken response = new() {
            Length = echoContentToken.Length,
            Value = echoContentToken
        };
        byte[] responseBytes = Encoding.UTF8.GetBytes(response.ToRESP());
        await stream.WriteAsync(responseBytes);
    }

    private static async Task PingCommand(NetworkStream stream) {
        byte[] response = Encoding.UTF8.GetBytes(PING_RESPONSE);
        await stream.WriteAsync(response, 0, response.Length);
        await Console.Out.WriteLineAsync($"Sent: {PING_RESPONSE}");
    }
}