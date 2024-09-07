using NStack;

using P3D.Legacy.Server.Abstractions;

using System;
using System.Collections;
using System.Collections.Generic;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Utils;

internal sealed class PlayerListDataSource : IListDataSource
{
    public List<IPlayer> Players { get; }

    public int Count => Players.Count;

    public int Length { get; }

    public PlayerListDataSource(List<IPlayer> itemList)
    {
        Players = itemList;
        Length = GetMaxLengthItem();
    }

    public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
    {
        container.Move(col, line);
        RenderUstr(driver, Players[item].Name, col, line, width, start);
    }

    public bool IsMarked(int item) => false;
    public void SetMark(int item, bool value) { }

    private int GetMaxLengthItem()
    {
        if (Players.Count == 0)
        {
            return 0;
        }

        var maxLength = 0;
        for (var i = 0; i < Players.Count; i++)
        {
            var l = Players[i].Name.Length;
            if (l > maxLength)
            {
                maxLength = l;
            }
        }

        return maxLength;
    }

    // A slightly adapted method from: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
    private static void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width, int start = 0)
    {
        var used = 0;
        var index = start;
        while (index < ustr.Length)
        {
            var (rune, size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
            var count = Rune.ColumnWidth(rune);
            if (used + count >= width) break;
            driver.AddRune(rune);
            used += count;
            index += size;
        }

        while (used < width)
        {
            driver.AddRune(' ');
            used++;
        }
    }

    public IList ToList() => Players;
}