namespace AmazonReviewsCRM.Security
{
    public static class RolePerms
    {
        public static readonly Dictionary<string, List<string>> Permissions =
    new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
{
    { "System Administrator", new List<string>
        {
            "Dashboard", "games", "trends", "reviewers",
            "Alerts", "Reports", "model", "admin",
            "UserManagement", "SentimentPage", "ProfileSettings"
        }
    },
    { "Data Analyst", new List<string>
        {
            "Dashboard", "games", "trends",
            "Reports", "SentimentPage", "ProfileSettings"
        }
    },
    { "Marketing Manager", new List<string> { "Dashboard", "games", "GameDetails", "reviewers", "Alerts", "Reports", "ProfileSettings" } },
    { "Game Developer", new List<string> { "Dashboard", "games", "trends", "ProfileSettings" } },
    { "Customer Support", new List<string> { "Dashboard", "Alerts", "ProfileSettings" } },
    { "Stakeholder", new List<string> { "Dashboard", "Reports", "games", "ProfileSettings" } }
    };

        
        public static bool HasAccess(string role, string menuKey)
        {
            if (string.IsNullOrEmpty(role)) return false;
            if (!Permissions.ContainsKey(role)) return false;
            return Permissions[role].Contains(menuKey);
        }
    }
}