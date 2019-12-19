using HideezClient.Models;
using System.Threading.Tasks;

namespace HideezClient.Modules.HotkeyManager
{
    interface IHotkeyManager
	{
        bool Enabled { get; set; }

        Task<string> GetHotkeyForAction(UserAction action);

        Task<bool> IsFreeHotkey(UserAction action, string hotkey);

        Task<bool> IsUniqueHotkey(UserAction action, string hotkey);


    }
}