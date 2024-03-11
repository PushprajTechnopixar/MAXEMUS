using MaxemusAPI.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MaxemusAPI.IRepository
{
    public interface ITwilioManager
    {
        Task SendMessage(string body, string to);
        Task<TwilioVerificationResult> StartVerificationAsync(string phoneNumber, string channel);

        Task<TwilioVerificationResult> CheckVerificationAsync(string phoneNumber, string code);
    }
}
