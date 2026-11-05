using AutoRetainerAPI.Configuration;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Utility;
using ECommons.ChatMethods;
using ECommons.ExcelServices;
using ECommons.EzContextMenu;
using ECommons.Interop;
using Lumina.Excel.Sheets;
using UIColor = ECommons.ChatMethods.UIColor;

namespace AutoRetainer.Internal;

internal unsafe class ContextMenuManager
{
    private SeString Prefix = new SeStringBuilder().AddUiForeground(" ", 539).Build();

    public ContextMenuManager()
    {
        ContextMenuPrefixRemover.Initialize();
        Svc.ContextMenu.OnMenuOpened += ContextMenu_OnMenuOpened;
    }

    private void ContextMenu_OnMenuOpened(IMenuOpenedArgs args)
    {
        if(Data == null) return;
        if(!Data.GetIMSettings().IMEnableContextMenu) return;
        if(args.MenuType == ContextMenuType.Inventory && args.Target is MenuTargetInventory inv && inv.TargetItem != null)
        {
            if(ItemUtil.GetBaseId(inv.TargetItem.Value.ItemId) is { ItemId: > 0 and var id, Kind: not ItemKind.EventItem })
            {
                if(Data.GetIMSettings(true).IMProtectList.Contains(id))
                {
                    args.AddMenuItem(new MenuItem()
                    {
                        Name = new SeStringBuilder().Append(Prefix).AddText("= 物品已受保护 =").Build(),
                        OnClicked = (a) =>
                        {
                            if(IsKeyPressed([LimitedKeys.LeftControlKey, LimitedKeys.RightControlKey]) && IsKeyPressed([LimitedKeys.RightShiftKey, LimitedKeys.LeftShiftKey]))
                            {
                                var t = $"已将 {ExcelItemHelper.GetName(id)} 从保护列表移除";
                                Notify.Success(t);
                                ChatPrinter.Red("[AutoRetainer] " + t);
                                Data.GetIMSettings(true).IMProtectList.Remove(id);
                            }
                            else
                            {
                                Notify.Error($"按住 CTRL+SHIFT 并点击，才能取消物品保护");
                            }
                        }
                    }.RemovePrefix());
                }
                else
                {
                    var data = Svc.Data.GetExcelSheet<Item>().GetRow(id);
                    if(Data.GetIMSettings(true).IMAutoVendorSoft.Contains(id))
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- 从自由探索委托出售列表移除", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                Data.GetIMSettings(true).IMAutoVendorSoft.Remove(id);
                                Notify.Info($"已将 {ExcelItemHelper.GetName(id)} 从自由探索委托出售列表移除");
                            }
                        }.RemovePrefix());
                    }
                    else if(data.PriceLow > 0)
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ 添加到自由探索委托出售列表", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                if(Data.GetIMSettings(true).AddItemToList(IMListKind.SoftSell, id, out var error))
                                {
                                    Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 添加到自由探索委托出售列表");
                                }
                                else
                                {
                                    Notify.Error(error);
                                }
                            }
                        }.RemovePrefix());
                    }

                    if(Data.GetIMSettings(true).IMAutoVendorHard.Contains(id))
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- 从无条件出售列表移除", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                Data.GetIMSettings(true).IMAutoVendorHard.Remove(id);
                                Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 从无条件出售列表移除");
                            }
                        }.RemovePrefix());
                    }
                    else if(data.PriceLow > 0)
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ 添加到无条件出售列表", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                if(Data.GetIMSettings(true).AddItemToList(IMListKind.HardSell, id, out var error))
                                {
                                    Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 添加到无条件出售列表");
                                }
                                else
                                {
                                    Notify.Error(error);
                                }
                            }
                        }.RemovePrefix());
                    }

                    if(Data.GetIMSettings(true).IMDiscardList.Contains(id))
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("- 从丢弃列表移除", (ushort)UIColor.Orange).Build(),
                            OnClicked = (a) =>
                            {
                                Data.GetIMSettings(true).IMDiscardList.Remove(id);
                                Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 从丢弃列表移除");
                            }
                        }.RemovePrefix());
                    }
                    else
                    {
                        args.AddMenuItem(new MenuItem()
                        {
                            Name = new SeStringBuilder().Append(Prefix).AddUiForeground("+ 添加到丢弃列表", (ushort)UIColor.Yellow).Build(),
                            OnClicked = (a) =>
                            {
                                if(Data.GetIMSettings(true).AddItemToList(IMListKind.Discard, id, out var error))
                                {
                                    Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 添加到丢弃列表");
                                }
                                else
                                {
                                    Notify.Error(error);
                                }
                            }
                        }.RemovePrefix());
                    }

                    args.AddMenuItem(new MenuItem()
                    {
                        Name = new SeStringBuilder().Append(Prefix).AddText("保护物品，避免自动处理").Build(),
                        OnClicked = (a) =>
                        {
                            if(Data.GetIMSettings(true).AddItemToList(IMListKind.Protect, id, out var error))
                            {
                                Notify.Success($"已将 {ExcelItemHelper.GetName(id)} 添加到保护列表");
                            }
                            else
                            {
                                Notify.Error(error);
                            }
                        }
                    }.RemovePrefix());
                }
            }
        }
    }

    public void Dispose()
    {
        Svc.ContextMenu.OnMenuOpened -= ContextMenu_OnMenuOpened;
    }
}
