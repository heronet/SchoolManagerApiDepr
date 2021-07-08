namespace SchoolManagerApi.Security
{
    public class Permissions
    {
        public static class Products // Add, Modify, Delete for StoreKeeper, Readonly for teachers
        {
            public const string Read = "products.read";
            public const string Add = "products.add";
            public const string Modify = "products.modify";
            public const string Delete = "products.delete";
        }
        public static class RolesManagement // Only accessible for Admins
        {
            public const string Access = "roles.access";
        }
    }
}