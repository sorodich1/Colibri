namespace Colibri.ConnectNetwork.Services.Abstract
{
    public interface ITcpConnectService
    {
        void Connect(string host, int port);
        void Send(string message);
        string Receive();
        void Disconnect();
    }
}
