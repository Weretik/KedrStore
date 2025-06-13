namespace Domain.Identity.Constants
{
    /// <summary>
    /// Константы для детальных разрешений в приложении
    /// </summary>
    public static class AppPermissions
    {
        // Разрешения для управления пользователями
        public const string UsersRead = "Users.Read";
        public const string UsersCreate = "Users.Create";
        public const string UsersUpdate = "Users.Update";
        public const string UsersDelete = "Users.Delete";

        // Разрешения для управления товарами
        public const string ProductsRead = "Products.Read";
        public const string ProductsCreate = "Products.Create";
        public const string ProductsUpdate = "Products.Update";
        public const string ProductsDelete = "Products.Delete";

        // Разрешения для управления заказами
        public const string OrdersRead = "Orders.Read";
        public const string OrdersCreate = "Orders.Create";
        public const string OrdersUpdate = "Orders.Update";
        public const string OrdersDelete = "Orders.Delete";
    }
}
