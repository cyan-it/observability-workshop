using System.Diagnostics;

namespace Api.Services.Delay
{
    public class DelayService : IDelayService
    {
        public async Task Delay(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }
    }
}