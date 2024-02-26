using codecrafters_redis.src.RESP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
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

            // print array items for debug
            await Console.Out.WriteAsync($"Array Items: ");
            arrayToken.Tokens.Where(t => t is BulkStringToken)
                .Cast<BulkStringToken>()
                .ToList()
                .ForEach(t => Console.Write(t.Value+"; "));
            Console.WriteLine();

            var cmd = ((BulkStringToken)arrayToken.Tokens[0]).Value;
            await Console.Out.WriteLineAsync($"Command: {cmd}");
            if (cmd == "ping") {
                await PingCommand(stream);
            } else if(cmd == "echo") {
                await EchoCommand(stream, arrayToken);
            }else if(cmd == "set") {
                await SetCommand(stream, arrayToken);
            } else if(cmd == "get") {
                await GetCommand(stream, arrayToken);
            }
        } while(bytesRead > 0);
        client.Close();
    }

    private static async Task GetCommand(NetworkStream stream, ArrayToken arrayToken) {
        string key = ((BulkStringToken)arrayToken.Tokens[1]).Value;
        await Console.Out.WriteLineAsync($"GET {key}");
        (string value, DateTime? expiry) = _data[key];
        BulkStringToken response;

        if(expiry is not null && expiry < DateTime.Now) {
            response = new() {
                Length = 0,
                Value = ""
            };
        } else {
            response = new() {
                Length = value.Length,
                Value = value
            };
        }
        byte[] responseBytes = Encoding.UTF8.GetBytes(response.ToRESP());
        await stream.WriteAsync(responseBytes);
        await Console.Out.WriteLineAsync("Sent ok from set");
    }

    private static async Task SetCommand(NetworkStream stream, ArrayToken arrayToken) {
        string key = ((BulkStringToken)arrayToken.Tokens[1]).Value;
        string value = ((BulkStringToken)arrayToken.Tokens[2]).Value;
        string? px = (arrayToken.Tokens[3] as BulkStringToken)?.Value;
        string? pxMsStr = (arrayToken.Tokens[3] as BulkStringToken)?.Value;
        await Console.Out.WriteLineAsync($"SET {key}: {value}");

        if(px is null || pxMsStr is null) {
            _data[key] = (value, null);
        } else {
            int pxMs = int.Parse(pxMsStr);
            _data[key] = (value, DateTime.Now.AddMilliseconds(pxMs));
        }
        SimpleStringToken response = new() {
            Value = "OK"
        };
        byte[] responseBytes = Encoding.UTF8.GetBytes(response.ToRESP());
        await stream.WriteAsync(responseBytes);
        await Console.Out.WriteLineAsync("Sent ok from set");
    }

    private static async Task EchoCommand(NetworkStream stream, ArrayToken arrayToken) {
        BulkStringToken echoContentToken = (BulkStringToken)arrayToken.Tokens[1];
        byte[] response = Encoding.UTF8.GetBytes(echoContentToken.ToRESP());
        await stream.WriteAsync(response);
        await Console.Out.WriteLineAsync($"Sent: {echoContentToken.Value}");
    }

    private static async Task PingCommand(NetworkStream stream) {
        byte[] response = Encoding.UTF8.GetBytes(PING_RESPONSE);
        await stream.WriteAsync(response, 0, response.Length);
        await Console.Out.WriteLineAsync($"Sent: {PING_RESPONSE}");
    }
}