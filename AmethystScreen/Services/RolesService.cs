using AmethystScreen.Areas.Identity.Data;
using AmethystScreen.Data;
using AmethystScreen.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
        private readonly UserDbContext _userContext;
        private readonly AppDbContext _appContext;
        private readonly Roles[] _roles;

        public RolesService(UserDbContext userContext, AppDbContext appContext)
        {
            _userContext = userContext;
            _appContext = appContext;

            _roles = new Roles[4];

            for (int i = 0; i < _roles.Length; i++)
            {
                _roles[i] = new Roles();
            }

            SetRoles();
        }

        private void SetRoles()
        {
            var userRole = _userContext.Roles.FirstOrDefault(r => r.Name == "User");
            var moderatorRole = _userContext.Roles.FirstOrDefault(r => r.Name == "Moderator");
            var adminRole = _userContext.Roles.FirstOrDefault(r => r.Name == "Admin");
            var superUserRole = _userContext.Roles.FirstOrDefault(r => r.Name == "SuperUser");

            if (userRole != null) _roles[0].SetRoleItem(0, "User", userRole.Id);
            if (moderatorRole != null) _roles[1].SetRoleItem(1, "Moderator", moderatorRole.Id);
            if (adminRole != null) _roles[2].SetRoleItem(2, "Admin", adminRole.Id);
            if (superUserRole != null) _roles[3].SetRoleItem(3, "SuperUser", superUserRole.Id);
        }

        public async Task<List<User>> GetAllUsersAsync(string? accessorUserId)
        {
            var authUserRoleId = _userContext.UserRoles.FirstOrDefault(u => u.UserId == accessorUserId)?.RoleId;
            var accessorRole = _roles.FirstOrDefault(r => r.Id == authUserRoleId);
            if (accessorRole == null) return new List<User>();
            int accessLevel = accessorRole.Level;

            List<User> users = new List<User>();
            var userList = await _userContext.Users.ToListAsync();
            foreach (var user in userList)
            {
                var userRoleId = await _userContext.UserRoles
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

        public async Task<List<ReportedContent>> GetReportedContentAsync(string? accessorUserId)
        {
            var authUserRoleId = _userContext.UserRoles.FirstOrDefault(u => u.UserId == accessorUserId)?.RoleId;
            var accessorRole = _roles.FirstOrDefault(r => r.Id == authUserRoleId);
            if (accessorRole == null) return new List<ReportedContent>();
            int accessLevel = accessorRole.Level;

            List<ReportedContent> reports = new List<ReportedContent>();
            var reportListPending = await _appContext.ReportedContent.Where(r => r.ReportStatus == ReportedContent.Status.Pending).ToListAsync();
            var reportListUnderReview = await _appContext.ReportedContent.Where(r => r.ReportStatus == ReportedContent.Status.UnderReview).ToListAsync();
            var reportListApproved = await _appContext.ReportedContent.Where(r => r.ReportStatus == ReportedContent.Status.Approved).ToListAsync();
            var reportListRejected = await _appContext.ReportedContent.Where(r => r.ReportStatus == ReportedContent.Status.Rejected).ToListAsync();

            switch(accessLevel)
            {
                case 0:
                    // user role, reject access
                    break;
                case 1:
                    reports.AddRange(reportListPending);
                    break;
                case 2:
                    reports.AddRange(reportListUnderReview);
                    break;
                case 3:
                    reports.AddRange(reportListPending);
                    reports.AddRange(reportListUnderReview);
                    reports.AddRange(reportListApproved);
                    reports.AddRange(reportListRejected);
                    break;
                default:
                    break;
            }

            return reports;
        }

        public async Task<List<Feedback>> GetFeedbackAsync()
        {
            return await _appContext.Feedbacks.ToListAsync();
        }

        public async Task<List<ContentActivity>> GetRecentActivityAsync()
        {
            return await _appContext.ContentActivities.ToListAsync();
        }
    }

}
