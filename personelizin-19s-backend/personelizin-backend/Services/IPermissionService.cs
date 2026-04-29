using System.Threading.Tasks;

namespace personelizin_backend.Services
{
    public interface IPermissionService
    {
        Task<bool> ApproveAsync(int id);
        Task<bool> RejectAsync(int id);
    }
}