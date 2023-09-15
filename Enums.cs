using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace fro_mod
{
    public static class Enums
    {
        public static string[] BodyParts = new string[]
        {
            "Pelvis",
            "Spine",
            "Spine1",
            "Spine2",
            "Head",
            "Neck",
            "Left arm",
            "Left forearm",
            "Left hand",
            "Right arm",
            "Right forearm",
            "Right hand",
            "Left upleg",
            "Left leg",
            "Left foot",
            "Right upleg",
            "Right leg",
            "Right foot"
        };

        public static string[] States = new string[] {
            "Disabled",
            "Riding",
            "Setup",
            "BeginPop",
            "Pop",
            "InAir",
            "Release",
            "Impact",
            "Powerslide",
            "Manual",
            "Grinding",
            "EnterCoping",
            "ExitCoping",
            "Grabs",
            "Bailed",
            "Pushing",
            "Braking"
        };

        public static string[] StatesReal = new string[] {
            "Riding",
            "Setup",
            "BeginPop",
            "Pop",
            "InAir",
            "Release",
            "Impact",
            "Powerslide",
            "Manual",
            "Grinding",
            "EnterCoping",
            "ExitCoping",
            "Grabs",
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
            "BsLosi"
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

        public static string[] special_patreons = new string[]
        {
            "doobiedoober",
            "Eric Mcgrady",
            "Slabs",
            "Marcel Mink",
            "helio",
            "Kyle Gherman",
            "Max Crowe",
            "Nick Morocco",
            "Euan",
            "J'vonte Johnson",
            "Foolie Surfin",
            "Alex Tagg",
            "Jeffery Depriest",
            "Nati Adams",
            "Don Panda",
            "heartsick",
            "dustin fuston",
            "Dali",
            "Nick Duncan",
            "V I E",
            "Nathaniel Gardner",
            "Corey Populus",
            "countinsequence",
            "Justin S Reynolds",
            "Jake DeFazio",
            "Seth Bates",
            "Christian Joel",
            "Zaheer",
            "Blake Young",
            "kevin hardy",
            "Alexander",
            "renzoh",
            "kevin4mayor",
            "Drippy QTip",
            "Malcolm hazley",
            "fleep",
            "Cruzzy",
            "ClutchClubz YT",
            "alex",
            "Gabriel Rosa",
            "Bradford McMahan",
            "Bob Gnarly",
            "Babyfacedbull",
            "Self destruct",
            "Mathew Boynton",
            "James Healy",
            "Mitchell",
            "Jordan Byrd",
            "Lochie Axon",
            "Benny Nixx",
            "Mr Karl Havelock",
            "PsycoOG",
            "thomas medina",
            "Austen",
            "Anthony Beheler",
            "DoomBoy",
            "Brandon",
            "Aaron Eaddy",
            "Ruckus Roke",
            "Mataytopotato",
            "Rob Gene",
            "Sawzy",
            "phoenix Arvo",
            "Lucian",
            "z e",
            "Justin",
            "Edward Harris",
            "kalani weir",
            "az6ly",
            "seasons",
        };

        public static Vector3[] OriginalRotations = new Vector3[]
        {
            new Vector3(1.3f, 336.2f, 274.9f),
            new Vector3(1.6f, 2.3f, 347.5f),
            new Vector3(2.1f, 5.4f, 350.2f),
            new Vector3(4.6f, 1.1f, 347.4f),
            new Vector3(18.1f, 6.1f, 15.1f),
            new Vector3(19.0f, 359.3f, 341.5f),
            new Vector3(11.9f, 301.0f, 12.8f),
            new Vector3(353.9f, 14.5f, 41.2f),
            new Vector3(13.9f, 353.7f, 352.1f),
            new Vector3(337.9f, 53.3f, 9.9f),
            new Vector3(1.8f, 352.0f, 47.9f),
            new Vector3(355.7f, 357.6f, 0.7f),
            new Vector3(354.2f, 169.8f, 329.3f),
            new Vector3(1.0f, 1.4f, 55.9f),
            new Vector3(1.1f, 9.8f, 338.9f),
            new Vector3(4.0f, 182.7f, 342.3f),
            new Vector3(0.1f, 0.2f, 58.5f),
            new Vector3(359.0f, 352.3f, 331.8f)
        };
    }
}