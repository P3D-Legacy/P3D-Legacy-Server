using Microsoft.Extensions.Configuration;

using Terminal.Gui;

namespace P3D.Legacy.Server.GUI.Views
{
    public sealed class SettingsTabView : View
    {
        public SettingsTabView(IConfiguration configuration)
        {


            Width = Dim.Fill();
            Height = Dim.Fill();

            //var generalView = new FrameView("General") { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            //Add(generalView);
        }
    }
}