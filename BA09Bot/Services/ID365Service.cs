using System.Collections.Generic;
using System.Threading.Tasks;
using BA09Bot.Models;

namespace BA09Bot.Services
{
    public interface ID365Service
    {
        Task CreateDailyReport(string report);
        Task CreateIoTCommand(string operation, string alertId, string deviceId);
        Task<List<Appointment>> GetAppointmentsForToday();
        Task<List<Product>> GetProducts(string query);
        Task<List<CrmTask>> GetTasks();
        Task UpdateUserInfoForBot();
    }
}