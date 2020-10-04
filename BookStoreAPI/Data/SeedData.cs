using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAPI.Data
{
    public class SeedData
    {
        public async static Task Seed(UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            await SeedRoles(roleManager);
            await SeedUsers(userManager);
        }

        private async static Task SeedUsers(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@bookstore.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "admin@bookstore.com",
                    Email = "admin@bookstore.com"
                };

                var result = await userManager.CreateAsync(user, "Pass.word1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "admin");
                }
            }

            if (await userManager.FindByEmailAsync("cust@bookstore.com") == null)
            {
                var user = new IdentityUser
                {
                    UserName = "cust@bookstore.com",
                    Email = "cust@bookstore.com"
                };

                var result = await userManager.CreateAsync(user, "Pass.word1");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, "user");
                }
            }
        }

        private async static Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            if (!await roleManager.RoleExistsAsync("admin"))
            {
                var role = new IdentityRole
                {
                    Name = "admin"
                };
                await roleManager.CreateAsync(role);
            }

            if (!await roleManager.RoleExistsAsync("user"))
            {
                var role = new IdentityRole
                {
                    Name = "user"
                };
                await roleManager.CreateAsync(role);
            }
        }
    }
}
