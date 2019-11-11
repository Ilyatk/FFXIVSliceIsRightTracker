//!CompilerOption:AddRef:SlimDx.dll

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ff14bot;
using ff14bot.AClasses;
using ff14bot.Helpers;
using ff14bot.Managers;
using ff14bot.Objects;
using SliceIsRightTracker.Overlay;
using Color = System.Drawing.Color;
using DrawingContext = SliceIsRightTracker.Overlay.DrawingContext;
using System.Windows.Media;

namespace SliceIsRightTracker
{
    public class Plugin : BotPlugin
    {
        private RenderForm _renderForm;

        public override void OnPulse()
        {
        }

        public override void OnInitialize()
        {
            //Check if rivatuner is running, if rivatuner is running and isn't blocked from attaching to rebornbuddy it can cause RB to crash
            var rivaTunerRunning = Process.GetProcessesByName("RTSS").Any();
            if (rivaTunerRunning)
            {
                Logging.Write(Colors.Red, @"Rivatuner has been detected running on this machine. If rebornbuddy crashes, add rebornbuddy.exe to rivatuner and disable ""On-Screen Display support""");
            }
        }

        public override void OnShutdown()
        {
            Task.Run(OnDisableAsync);
        }

        public override void OnEnabled()
        {
            Task.Factory.StartNew(RunRenderForm, TaskCreationOptions.LongRunning);
        }

        private void RunRenderForm()
        {
            OverlayManager.Drawing += Drawing;

            IntPtr targetWindow = Core.Memory.Process.MainWindowHandle;
            _renderForm = new RenderForm(targetWindow);

            Application.Run(_renderForm);
        }

        public override void OnDisabled()
        {
            Task.Run(OnDisableAsync);
        }

        private async Task OnDisableAsync()
        {
            OverlayManager.Drawing -= Drawing;

            if (_renderForm == null)
                return;

            await _renderForm.ShutdownAsync();
        }

        public override string Name => "Slice Is Right";
        public override string Description => "Plugin draw incoming damage in gate 'Slice is Right'.";

        public override string Author => "Ilya";

        public override Version Version => new Version(1, 0, 0);

        public override bool WantButton => false;

        private void DrawCircleAttack(DrawingContext ctx, GameObject obj)
        {
            ctx.DrawCircleWithPoint(obj.Location, obj.Heading, 11.0f, Color.FromArgb(100, Color.Red), Color.FromArgb(100, Color.Red));
        }

        private void DrawSideAttack(DrawingContext ctx, GameObject obj)
        {
            ctx.DrawSideAttackAgroLine(obj.Location, obj.Heading, 5.0f, 50.0f, Color.FromArgb(100, Color.Blue));
        }

        private void DrawSliceAttack(DrawingContext ctx, GameObject obj)
        {
            ctx.DrawAgroLine(obj.Location, obj.Heading + (float)Math.PI / 2, 5.0f, 100.0f, Color.FromArgb(100, Color.Yellow), Color.FromArgb(100, Color.Yellow));
        }

        private void Drawing(DrawingContext ctx)
        {
            if (QuestLogManager.InCutscene)
                return;

            //Gameobject list is threadstatic, always need to pulse otherwise new objects wont show up
            GameObjectManager.Update();

            var debug = false;
            if (debug)
            {
                var obj = Core.Me;
                DrawCircleAttack(ctx, obj);
                DrawSideAttack(ctx, obj);
                DrawSliceAttack(ctx, obj);
            }

            if (debug && Core.Me.CurrentTarget != null)
            {
                var obj = Core.Me.CurrentTarget;
                DrawCircleAttack(ctx, obj);
                DrawSideAttack(ctx, obj);
                DrawSliceAttack(ctx, obj);
            }

            foreach (GameObject obj in GameObjectManager.GameObjects)
            {
                if (obj.NpcId == 2010779)
                {
                    // Gate "The Slise is Right" Circle attack
                    DrawCircleAttack(ctx, obj);
                    continue;
                }

                if (obj.NpcId == 2010778)
                {
                    // Gate "The Slise is Right" Side attack
                    DrawSideAttack(ctx, obj);
                    continue;
                }

                if (obj.NpcId == 2010777)
                {
                    // Gate "The Slise is Right" Slice(frontal?) attack
                    DrawSliceAttack(ctx, obj);
                    continue;
                }
            }
        }
    }
}
