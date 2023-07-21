using System.Text.RegularExpressions;

namespace Smort_api.Handlers
{
    public static class RegexHandler
    {
        public static bool IsValidEmail(string email)
        {
            string pattern = @"[\w._-]+@[\w]+[\.][\w]{2,}";

            Match m = Regex.Match(email, pattern);

            return m.Success;
        }
    }
}
