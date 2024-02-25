using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CodeCrafters.Redis;

public class Server {
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
            HandleClient(client);
        }
    }

    public static async Task HandleClient(TcpClient client) {
        using NetworkStream stream = client.GetStream();

        using StreamReader sr = new(stream, Encoding.ASCII);
        using StreamWriter sw = new(stream, Encoding.ASCII);

        while (true) {
            var input = await sr.ReadToEndAsync();
            await Console.Out.WriteLineAsync("input "+input);
            if (input.Length == 0) {
                await Console.Out.WriteLineAsync("finish conn");
                break;
            }

            await Console.Out.WriteLineAsync("sending pong");
            await sw.WriteAsync(PING_RESPONSE);
            sw.Flush();
        }

        client.Dispose();
    }
}