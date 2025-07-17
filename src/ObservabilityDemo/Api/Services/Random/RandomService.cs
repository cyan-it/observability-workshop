namespace Api.Services.Random
{
    public class RandomService : IRandomService
    {
        private readonly System.Random _random = new();
        public int Next(int min, int max)
        {
            // Simulate an error in 10% of the cases
            if (_random.NextDouble() < 0.1)
            {
                throw new Exception("Internal Server Error");
            }

            int result = _random.Next(min, max);
            return result;
        }


        public double NextDouble()
        {
            double result = _random.NextDouble();
            return result;
        }
    }
}