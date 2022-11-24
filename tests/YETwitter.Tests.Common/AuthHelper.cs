namespace YETwitter.Tests.Common
{
    public class AuthHelper
    {
        // TODO make dynamic token
        const string token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoidGVzdF91c3JfZWJmN2IyZjFjODJkNGQyMDhjMTMyZDU1Y2FlYzNmMGYiLCJqdGkiOiI4YTdhYTJhYS0wYTc3LTRiZDUtOTFkYi0yNjhhOTNiYzEyM2MiLCJleHAiOjE2ODM4Mzk5MzMsImlzcyI6Imh0dHA6Ly9sb2NhbGhvc3Q6NTAwMCIsImF1ZCI6Imh0dHA6Ly9sb2NhbGhvc3Q6NDIwMCJ9.nM7I5jyGMJXPsGuuh-_dc2IqGVO0vwu7EdP86OtUwWk";

        public static string GetJwtToken()
        {
            return token;
        }

    }
}