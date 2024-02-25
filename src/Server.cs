using System.Net;
using System.Net.Sockets;
using System.Text;

const int PORT = 6379;
const string PING_RESPONSE = "+PONG\r\n";

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, PORT);
server.Start();
TcpClient client = await server.AcceptTcpClientAsync(); // wait for client

var ns = client.GetStream();
StreamReader sr = new(ns, Encoding.ASCII);
StreamWriter sw = new(ns, Encoding.ASCII);

var request = sr.ReadToEnd(); // read the client's request

foreach(var req in request.Split("\r\n")) {
    sw.WriteLine(PING_RESPONSE);
}
