using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace xlperimental_mod
{
    public static class EnumHelper
    {
        public static string[] States = new string[] {
            "Disabled",
            "Riding",
            "WallRiding",
            "Setup",
            "BeginPop",
            "Pop",
            "InAir",
            "Release",
            "Impact",
            "Powerslide",
            "Primoslide",
            "Casperslide",
            "Manual",
            "Grinding",
            "EnterCoping",
            "ExitCoping",
            "Grabs",
            "Footplant",
            "Bailed",
            "Pushing",
            "Braking"
        };

        public static string[] StatesReal = new string[] {
            "Riding",
            "WallRiding",
            "Setup",
            "BeginPop",
            "Pop",
            "InAir",
            "Release",
            "Impact",
            "Powerslide",
            "Primoslide",
            "Casperslide",
            "Manual",
            "Grinding",
            "EnterCoping",
            "ExitCoping",
            "Grabs",
            "Footplant",
            "Bailed",
            "Pushing",
            "Braking"
        };

        public static string[] Stances = new string[] {
            "Fakie",
            "Switch"
        };

        public static string[] StancesCustomizer = new string[] {
            "Regular",
            "Fakie",
            "Nollie",
            "Switch"
        };

        public static string[] GrindType = new string[] {
            "BsFiftyFifty",
            "FsFiftyFifty",
            "FsFiveO",
            "BsFiveO",
            "BsNoseGrind",
            "FsNoseGrind",
            "BsCrook",
            "FsOverCrook",
            "FsCrook",
            "BsOverCrook",
            "FsNoseSlide",
            "BsNoseSlide",
            "FsNoseBluntSlide",
            "BsNoseBluntSlide",
            "BsBoardSlide",
            "FsBoardSlide",
            "FsLipSlide",
            "BsLipSlide",
            "FsTailSlide",
            "BsBluntSlide",
            "BsTailSlide",
            "FsBluntSlide",
            "BsFeeble",
            "FsSmith",
            "FsFeeble",
            "BsSmith",
            "BsSuski",
            "FsSuski",
            "BsSalad",
            "FsSalad",
            "BsWilly",
            "FsWilly",
            "FsLosi",
            "BsLosi",
            "BsDarkSlide",
            "FsDarkSlide",
        };

        public static string[] tricks_customizer = new string[] {
            "Ollie",
            "Kickflip",
            "Heelflip"
        };

        public static string[] input_types = new string[] {
            "Both sticks to the front",
            "Both sticks to the back",
            "Left stick to the front",
            "Right stick to the front",
            "Left stick to the back",
            "Right stick to the back",
            "Both sticks to the outside",
            "Both sticks to the inside",
            "Left stick to the left",
            "Left stick to the right",
            "Right stick to the left",
            "Right stick to the right",
            "Both sticks to the left",
            "Both sticks to the right"
        };

        public static string[] Keyframe_States = new string[] {
            "Head",
            "Left Hand",
            "Right Hand"
        };

        public static string[] Animations = new string[] {
            "Waving",
            "Celebrating",
            "Clapping"
        };
    }
}
