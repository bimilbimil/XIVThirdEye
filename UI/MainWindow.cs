using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using XIVThirdEye.Core;
using XIVThirdEye.Services;
using ImGui = Dalamud.Bindings.ImGui.ImGui;

namespace XIVThirdEye.UI
{
    public class MainWindow : Window
    {
        private readonly Configuration _config;
        private readonly IObjectTable _objectTable;
        private readonly IClientState _clientState;
        private readonly IPluginLog _log;
        private readonly IChatGui _chat;

        private string _searchFilter = string.Empty;

        public MainWindow(Configuration config, IObjectTable objectTable, IClientState clientState, IPluginLog log, IChatGui chat)
            : base("Third Eye##thirdeye", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
        {
            _config = config;
            _objectTable = objectTable;
            _clientState = clientState;
            _log = log;
            _chat = chat;
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(560, 300),
                MaximumSize = new Vector2(900, 700),
            };
        }

        public override void Draw()
        {
            if (!_clientState.IsLoggedIn)
            {
                ImGui.TextUnformatted("Not logged in.");
                return;
            }

            ImGui.SetNextItemWidth(200);
            ImGui.InputText("##search", ref _searchFilter, 64);
            ImGui.SameLine();
            ImGui.TextUnformatted("Filter");

            ImGui.Separator();

            var players = GetNearbyPlayers();
            ImGui.TextUnformatted($"{players.Count} player(s) in instance");
            ImGui.Spacing();

            var tableFlags = ImGuiTableFlags.Borders
                           | ImGuiTableFlags.RowBg
                           | ImGuiTableFlags.ScrollY
                           | ImGuiTableFlags.SizingStretchProp;

            var tableHeight = ImGui.GetContentRegionAvail().Y;

            if (!ImGui.BeginTable("##players", 5, tableFlags, new Vector2(0, tableHeight)))
                return;

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn("Name",    ImGuiTableColumnFlags.None, 3f);
            ImGui.TableSetupColumn("World",   ImGuiTableColumnFlags.None, 2f);
            ImGui.TableSetupColumn("Job",     ImGuiTableColumnFlags.None, 1f);
            ImGui.TableSetupColumn("Lv",      ImGuiTableColumnFlags.None, 0.6f);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.None, 3.5f);
            ImGui.TableHeadersRow();

            foreach (var pc in players)
            {
                var name  = pc.Name.TextValue;
                var world = pc.HomeWorld.Value.Name.ToString();

                if (!string.IsNullOrEmpty(_searchFilter) &&
                    !name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                var job   = pc.ClassJob.Value.Abbreviation.ToString();
                var level = pc.Level.ToString();

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0); ImGui.TextUnformatted(name);
                ImGui.TableSetColumnIndex(1); ImGui.TextUnformatted(world);
                ImGui.TableSetColumnIndex(2); ImGui.TextUnformatted(job);
                ImGui.TableSetColumnIndex(3); ImGui.TextUnformatted(level);
                ImGui.TableSetColumnIndex(4);

                ImGui.PushID((int)pc.EntityId);

                if (ImGui.SmallButton("Examine"))
                {
                    Examine(pc);
                    _chat.Print($"[ThirdEye] Examining {name}");
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("Plate"))
                {
                    OpenAdventurePlate(pc);
                    _chat.Print($"[ThirdEye] Opening adventure plate for {name}");
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("Copy"))
                {
                    ImGui.SetClipboardText($"{name}@{world}");
                    _chat.Print($"[ThirdEye] Copied: {name}@{world}");
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("Invite"))
                {
                    ChatHelper.Send($"/invite {name}");
                    _chat.Print($"[ThirdEye] Party invite sent to {name}");
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("Friend"))
                {
                    ChatHelper.Send($"/friend {name}");
                    _chat.Print($"[ThirdEye] Friend request sent to {name}");
                }

                ImGui.SameLine();
                if (ImGui.SmallButton("Mute"))
                {
                    AddToMuteList(pc);
                    _chat.Print($"[ThirdEye] Muted {name}");
                }

                ImGui.PopID();
            }

            ImGui.EndTable();
        }

        private List<IPlayerCharacter> GetNearbyPlayers()
        {
            var localId = (_objectTable[0] as IPlayerCharacter)?.EntityId ?? 0;
            var result = new List<IPlayerCharacter>();

            foreach (var obj in _objectTable)
            {
                if (obj is IPlayerCharacter pc && pc.EntityId != localId)
                    result.Add(pc);
            }

            result.Sort((a, b) => string.Compare(a.Name.TextValue, b.Name.TextValue, StringComparison.Ordinal));
            return result;
        }

        private static unsafe void Examine(IPlayerCharacter pc)
        {
            var agent = AgentInspect.Instance();
            if (agent == null) return;
            agent->ExamineCharacter(pc.EntityId, false);
        }

        private static unsafe void OpenAdventurePlate(IPlayerCharacter pc)
        {
            var agent = AgentCharaCard.Instance();
            if (agent == null) return;
            agent->OpenCharaCard((GameObject*)pc.Address);
        }

        private static unsafe void AddToMuteList(IPlayerCharacter pc)
        {
            var uiModule = UIModule.Instance();
            if (uiModule == null) return;
            var uiDataModule = uiModule->GetUiDataModule();
            if (uiDataModule == null) return;
            var contentId = ((BattleChara*)pc.Address)->ContentId;
            var worldId   = (ushort)pc.HomeWorld.RowId;
            uiDataModule->Mutelist.Add(contentId, pc.Name.TextValue, worldId);
            uiDataModule->SaveFile(false);
        }

        public void Dispose() { }
    }
}
