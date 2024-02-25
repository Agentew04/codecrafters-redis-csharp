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

            //if (request == "PING\r\n") {
            byte[] response = Encoding.UTF8.GetBytes(PING_RESPONSE);
            await stream.WriteAsync(response, 0, response.Length);
            await Console.Out.WriteLineAsync($"Sent: {PING_RESPONSE}");
            //}
        }while(bytesRead > 0);
        client.Close();
    }
}