using Microsoft.EntityFrameworkCore;
using personelizin_backend.Data;
using personelizin_backend.Models;

namespace personelizin_backend.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;

        public PermissionService(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<bool> ApproveAsync(int id)
        {
            var request = await _context.Leaves.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return false;

            if (request.Status == "Pending" && request.CurrentApproverId == 1)
            {
                request.Status = "ManagerApproved";
                request.CurrentApproverId = 2;
            }
            else if (request.Status == "ManagerApproved" && request.CurrentApproverId == 2)
            {
                request.Status = "Approved";
                request.CurrentApproverId = 0;
                if (!string.IsNullOrEmpty(request.User?.Email))
                {
                    await _emailService.SendEmailAsync(request.User.Email, "Ýzin Onayý", "Ýzniniz onaylandý.");
                }
            }
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectAsync(int id)
        {
            var request = await _context.Leaves.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
            if (request == null) return false;

            request.Status = "Rejected";
            request.CurrentApproverId = 0;
            if (!string.IsNullOrEmpty(request.User?.Email))
            {
                await _emailService.SendEmailAsync(request.User.Email, "Ýzin Reddi", "Talebiniz reddedildi.");
            }
            return await _context.SaveChangesAsync() > 0;
        }
    }
}