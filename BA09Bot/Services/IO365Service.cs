using System.Collections.Generic;
using System.Threading.Tasks;
using BA09Bot.Models;

namespace BA09Bot.Services
{
    public interface IO365Service
    {
        Task<List<PlannerTask>> GetTasks();
    }
}