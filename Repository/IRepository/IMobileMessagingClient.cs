using System.Threading.Tasks;

namespace MaxemusAPI.Firebase
{
    public interface IMobileMessagingClient
    {
        Task<string> SendNotificationAsync(string token, string title, string body);
    }
}
