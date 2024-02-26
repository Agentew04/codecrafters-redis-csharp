using codecrafters_redis.src.RESP;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace codecrafters_redis.src;

public static class Server {
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

            if(reqToken is ArrayToken arrayToken) {
                var cmd = arrayToken.Tokens[0].ToRESP();
                await Console.Out.WriteLineAsync($"Command: {cmd}");
                if (cmd == "ping") {
                    byte[] response = Encoding.UTF8.GetBytes(PING_RESPONSE);
                    await stream.WriteAsync(response, 0, response.Length);
                    await Console.Out.WriteLineAsync($"Sent: {PING_RESPONSE}");
                }else if(cmd == "echo") {
                    BulkStringToken echoContentToken = (BulkStringToken)arrayToken.Tokens[1];
                    byte[] response = Encoding.UTF8.GetBytes(echoContentToken.Value);
                    await stream.WriteAsync(response);
                    await Console.Out.WriteLineAsync($"Sent: {echoContentToken.Value}");
                }
            } else {
                await Console.Out.WriteLineAsync("req is not array");
            }
        }while(bytesRead > 0);
        client.Close();
    }

}