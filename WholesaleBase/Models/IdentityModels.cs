using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace WholesaleBase.Models
{
    // В профиль пользователя можно добавить дополнительные данные, если указать больше свойств для класса ApplicationUser. Подробности см. на странице https://go.microsoft.com/fwlink/?LinkID=317594.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Обратите внимание, что authenticationType должен совпадать с типом, определенным в CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            var warehouseIdClaim = Claims.FirstOrDefault(c => c.ClaimType == "WarehouseId");
            if (warehouseIdClaim != null)
                userIdentity.AddClaim(new Claim(warehouseIdClaim.ClaimType, warehouseIdClaim.ClaimValue));
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection")
        {
        }
        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new AppDbInitializer());
        }
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
    public class AppDbInitializer : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context));

            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            // создаем две роли
            var role1 = new IdentityRole { Name = "boss" };
            var role2 = new IdentityRole { Name = "manager" };
            var role3 = new IdentityRole { Name = "storekeeper" };

            // добавляем роли в бд
            roleManager.Create(role1);
            roleManager.Create(role2);
            roleManager.Create(role3);

            // создаем пользователей
            var boss = new ApplicationUser { Email = "boss@mail.ru", UserName = "boss@mail.ru" };
            string password = "Qwerty11";
            var result = userManager.Create(boss, password);
            var manager = new ApplicationUser { Email = "manager@mail.ru", UserName = "manager@mail.ru" };
            password = "Qwerty11";
            result = userManager.Create(manager, password);
            // если создание пользователя прошло успешно
            if (result.Succeeded)
            {
                // добавляем для пользователя роль
                userManager.AddToRole(boss.Id, role1.Name);
                userManager.AddToRole(manager.Id, role2.Name);
            }

            base.Seed(context);
        }
    }
}