using AmethystScreen.Areas.Identity.Data;
using AmethystScreen.Data;
using Microsoft.EntityFrameworkCore;

namespace AmethystScreen.Services
{
    public class Roles
    {
        public int Level { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;

        public void SetRoleItem(int level, string name, string id)
        {
            Level = level;
            Name = name;
            Id = id;
        }
    }

    public class RolesService
    {
        private readonly UserDbContext _context;
        private readonly Roles[] _roles;

        public RolesService(UserDbContext context)
        {
            _context = context;

            _roles = new Roles[4];

            for (int i = 0; i < _roles.Length; i++)
            {
                _roles[i] = new Roles();
            }

            SetRoles();
        }

        private void SetRoles()
        {
            var userRole = _context.Roles.FirstOrDefault(r => r.Name == "User");
            var moderatorRole = _context.Roles.FirstOrDefault(r => r.Name == "Moderator");
            var adminRole = _context.Roles.FirstOrDefault(r => r.Name == "Admin");
            var superUserRole = _context.Roles.FirstOrDefault(r => r.Name == "SuperUser");

            if (userRole != null) _roles[0].SetRoleItem(0, "User", userRole.Id);
            if (moderatorRole != null) _roles[1].SetRoleItem(1, "Moderator", moderatorRole.Id);
            if (adminRole != null) _roles[2].SetRoleItem(2, "Admin", adminRole.Id);
            if (superUserRole != null) _roles[3].SetRoleItem(3, "SuperUser", superUserRole.Id);
        }

        public async Task<List<User>> GetUsersAsync(string? accessorUserId)
        {
            var authUserRoleId = _context.UserRoles.FirstOrDefault(u  => u.UserId == accessorUserId)?.RoleId;
            var accessorRole = _roles.FirstOrDefault(r => r.Id == authUserRoleId);
            if (accessorRole == null) return new List<User>();

            int accessLevel = accessorRole.Level;

            List<User> users = new List<User>();

            var userList = await _context.Users.ToListAsync();
            foreach (var user in userList)
            {
                var userRoleId = await _context.UserRoles
                    .Where(r => r.UserId == user.Id)
                    .Select(r => r.RoleId)
                    .FirstOrDefaultAsync();

                if (userRoleId == null) continue;

                var userRole = _roles.FirstOrDefault(r => r.Id == userRoleId);
                if (userRole != null && userRole.Level < accessLevel)
                {
                    users.Add(user);
                }
            }

            return users;
        }

    }

}
