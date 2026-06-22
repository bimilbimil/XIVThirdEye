using System;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace XIVThirdEye.Services
{
    public static class ChatHelper
    {
        public static unsafe void Send(string command)
        {
            var uiModule = UIModule.Instance();
            if (uiModule == null) return;

            var str = Utf8String.FromString(command);
            try
            {
                uiModule->ProcessChatBoxEntry(str, IntPtr.Zero, false);
            }
            finally
            {
                str->Dtor(true);
            }
        }
    }
}
