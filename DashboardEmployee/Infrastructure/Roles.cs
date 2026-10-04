namespace DashboardEmployee.Infrastructure
{
    /// <summary>Role names in one place, so a typo becomes a compile error instead of a silent 403.</summary>
    public class Roles
    {    
        public const string Admin = "Admin";
        public const string Viewer = "Viewer";

        public static readonly string[] All = [Admin, Viewer];
    }
}
