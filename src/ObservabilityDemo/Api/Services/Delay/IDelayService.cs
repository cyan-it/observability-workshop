namespace Api.Services.Delay;

public interface IDelayService
{
    Task Delay(int milliseconds);
}