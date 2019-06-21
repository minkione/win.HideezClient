using HideezSafe.Models;
using System.Threading.Tasks;

namespace HideezSafe.Modules.HotkeyManager
{
    public interface IHotkeyManager
	{
        bool Enabled { get; set; }

        Task<string> GetHotkeyForAction(UserAction action);

        Task<bool> IsFreeHotkey(UserAction action, string hotkey);

        Task<bool> IsUniqueHotkey(UserAction action, string hotkey);


    }
}