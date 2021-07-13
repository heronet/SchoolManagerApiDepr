using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SchoolManagerApi.Security;

namespace SchoolManagerApi.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddAuthorizationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Security.Policies.Store.ManageStorePolicy,
                    builder => builder
                        .RequireClaim(CustomClaimTypes.Permission, Permissions.Products.Read)
                        .RequireClaim(CustomClaimTypes.Permission, Permissions.Products.Add)
                        .RequireClaim(CustomClaimTypes.Permission, Permissions.Products.Modify)
                        .RequireClaim(CustomClaimTypes.Permission, Permissions.Products.Delete)
                );
                options.AddPolicy(Security.Policies.Store.AccessStorePolicy,
                    builder => builder
                        .RequireClaim(CustomClaimTypes.Permission, Permissions.Products.Read)
                );
                options.AddPolicy(Security.Policies.Store.OrderFromStorePolicy,
                    builder => builder
                        .RequireClaim(CustomClaimTypes.Permission, Permissions.Products.Order)
                );
                options.AddPolicy(Security.Policies.RolesManagement.ManageRolesPolicy,
                    builder => builder
                        .RequireClaim(CustomClaimTypes.Permission, Permissions.RolesManagement.Access)
                );
            });
            return services;
        }
    }
}