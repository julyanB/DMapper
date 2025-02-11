namespace DMapper.Core.Helpers;

public class ErrorHelper
{
    
    // ErrorHelper.HandleError
    public static void HandleError(Action action)
    {
        try
        {
            action();
        }
        catch
        {
        }
    }
}