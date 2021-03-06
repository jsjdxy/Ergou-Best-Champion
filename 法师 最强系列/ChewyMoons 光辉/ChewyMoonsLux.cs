﻿#region

using System;
using System.Drawing;
using LeagueSharp;
using LeagueSharp.Common;

#endregion

namespace ChewyMoonsLux
{
    internal class ChewyMoonsLux
    {
        public static Menu Menu;
        public static Orbwalking.Orbwalker Orbwalker;
        public static Spell Q, W, E, R;
        public static bool PacketCast = false;

        public static bool Debug
        {
            get { return Menu.Item("debug").GetValue<bool>(); }
        }

        public static void OnGameLoad(EventArgs args)
        {
            if (ObjectManager.Player.BaseSkinName != "Lux")
            {
                return;
            }

            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1100);
            R = new Spell(SpellSlot.R, 3340);

            // Refine skillshots
            Q.SetSkillshot(0.25f, 80f, 1200f, false, SkillshotType.SkillshotLine); // to get collision objects
            W.SetSkillshot(0.25f, 150f, 1200f, false, SkillshotType.SkillshotLine);
            E.SetSkillshot(0.25f, 275f, 1300f, false, SkillshotType.SkillshotCircle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SkillshotType.SkillshotLine);

            // Setup Main Menu
            SetupMenu();

            Drawing.OnDraw += OnDraw;
            AntiGapcloser.OnEnemyGapcloser += QGapCloser.OnEnemyGapCloser;
            Game.OnGameUpdate += LuxCombo.OnGameUpdate;
            GameObject.OnCreate += LuxCombo.OnGameObjectCreate;
            GameObject.OnDelete += LuxCombo.OnGameObjectDelete;

            Utilities.PrintChat("Loaded.");
        }

        private static void OnDraw(EventArgs args)
        {
            var drawQ = Menu.Item("drawQ").GetValue<bool>();
            var drawW = Menu.Item("drawW").GetValue<bool>();
            var drawE = Menu.Item("drawE").GetValue<bool>();
            var drawR = Menu.Item("drawR").GetValue<bool>();

            var qColor = Menu.Item("qColor").GetValue<Circle>().Color;
            var wColor = Menu.Item("wColor").GetValue<Circle>().Color;
            var eColor = Menu.Item("eColor").GetValue<Circle>().Color;
            var rColor = Menu.Item("rColor").GetValue<Circle>().Color;

            var position = ObjectManager.Player.Position;

            if (drawQ)
            {
                Render.Circle.DrawCircle(position, Q.Range, qColor);
            }

            if (drawW)
            {
                Render.Circle.DrawCircle(position, W.Range, wColor);
            }

            if (drawE)
            {
                Render.Circle.DrawCircle(position, E.Range, eColor);
            }

            if (drawR)
            {
                Render.Circle.DrawCircle(position, R.Range, rColor);
            }
        }

        private static void SetupMenu()
        {
            Menu = new Menu("[Chewy's 光辉]", "cmLux", true);

            // Target Selector
            var tsMenu = new Menu("[Chewy's Lux] - 目标选择", "cmLuxTs");
            TargetSelector.AddToMenu(tsMenu);
            Menu.AddSubMenu(tsMenu);

            // Orbwalker
            var orbwalkerMenu = new Menu("[Chewy's Lux] - 走砍", "cmLuxOrbwalker");
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);
            Menu.AddSubMenu(orbwalkerMenu);

            // Combo settings
            var comboMenu = new Menu("[Chewy's Lux] - 连招", "cmLuxCombo");
            comboMenu.AddItem(new MenuItem("useQ", "使用 Q").SetValue(true));
            comboMenu.AddItem(new MenuItem("useW", "使用 W").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "使用 E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "使用 R").SetValue(true));
            comboMenu.AddItem(new MenuItem("onlyRIfKill", "使用 R 可击杀").SetValue(false));
            comboMenu.AddItem(new MenuItem("useIgnite", "使用 点燃 连招").SetValue(true));
            Menu.AddSubMenu(comboMenu);

            // Harass Settings
            var harassMenu = new Menu("[Chewy's Lux] - 骚扰", "cmLuxHarass");
            harassMenu.AddItem(new MenuItem("useQHarass", "使用 Q").SetValue(true));
            harassMenu.AddItem(new MenuItem("useEHarass", "使用 E").SetValue(true));
            Menu.AddSubMenu(harassMenu);

            // KS / Finisher Settings
            var ksMenu = new Menu("[Chewy's Lux] - 抢人头", "cmLuxKS");
            ksMenu.AddItem(new MenuItem("ultKS", "使用 R").SetValue(true));
            //ksMenu.AddItem(new MenuItem("recallExploitKS", "KS enemies recalling").SetValue(true));
            Menu.AddSubMenu(ksMenu);

            // Items
            var itemsMenu = new Menu("[Chewy's Lux] - 物品", "cmLuxItems");
            itemsMenu.AddItem(new MenuItem("useDFG", "使用 冥火").SetValue(true));
            Menu.AddSubMenu(itemsMenu);

            //Drawing
            var drawingMenu = new Menu("[Chewy's Lux] - 范围", "cmLuxDrawing");
            drawingMenu.AddItem(new MenuItem("drawQ", "范围 Q").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawW", "范围 W").SetValue(false));
            drawingMenu.AddItem(new MenuItem("drawE", "范围 E").SetValue(true));
            drawingMenu.AddItem(new MenuItem("drawR", "范围 R").SetValue(true));
            drawingMenu.AddItem(new MenuItem("qColor", "Q 范围颜色").SetValue(new Circle(true, Color.Gray)));
            drawingMenu.AddItem(new MenuItem("wColor", "W 范围颜色").SetValue(new Circle(true, Color.Gray)));
            drawingMenu.AddItem(new MenuItem("eColor", "E 范围颜色").SetValue(new Circle(true, Color.Gray)));
            drawingMenu.AddItem(new MenuItem("rColor", "R 范围颜色").SetValue(new Circle(true, Color.Gray)));
            Menu.AddSubMenu(drawingMenu);

            // Misc
            var miscMenu = new Menu("[Chewy's Lux] - 杂项", "cmLuxMisc");
            miscMenu.AddItem(new MenuItem("antiGapCloserQ", "防止 突进").SetValue(true));
            miscMenu.AddItem(new MenuItem("packetCast", "技能 范围").SetValue(false));
            miscMenu.AddItem(
                new MenuItem("autoShield", "自动盟友给盾").SetValue(new KeyBind('c', KeyBindType.Toggle)));
            miscMenu.AddItem(new MenuItem("autoShieldPercent", "盟友给盾 HP%").SetValue(new Slider(20)));
            miscMenu.AddItem(new MenuItem("debug", "调试").SetValue(false));
            Menu.AddSubMenu(miscMenu);

            // Combo / Harass
            //Menu.AddItem(new MenuItem("combo", "Combo!").SetValue(new KeyBind(32, KeyBindType.Press)));
            //Menu.AddItem(new MenuItem("harass", "Harass!").SetValue(new KeyBind('v', KeyBindType.Press)));

            // Finalize
            Menu.AddToMainMenu();
        }
    }
}