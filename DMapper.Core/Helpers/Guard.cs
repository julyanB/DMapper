namespace DMapper.Helpers
{
    public class Guard
    {
        public static void IsNotNull(object? value, string paramName)
        {
            if (value is null)
            {
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null.");
            }
        }

    }
}
