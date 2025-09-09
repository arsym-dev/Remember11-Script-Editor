/*
 * Issues:
 * Automatically create archive
 *  Randomly spits out gibberish then dies
 *  Text on choices is misaligned severely
 *  I think the way I made TokenIf create an IntGoto token is messing things up
 *      Nope, something else is causing it. Look for commands that happen before it screws up.
 */
    
    
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using R11_Script_Editor.FileTypes;

namespace R11_Script_Editor.Tokens
{
    static class Tokenizer
    {
        static byte[] data;
        static byte[] data_switch_block;
        static public List<Token> Tokens;
        static string filename;
        static bool _changed_file;
        static public bool ChangedFile
        {
            get { return _changed_file; }
            set {
                _changed_file = value;
                var x = ((MainWindow)Application.Current.MainWindow);
                if (!value && x.Title.EndsWith("*"))
                    x.Title = x.Title.Substring(0, x.Title.Length - 2);
                else if (value && !x.Title.EndsWith("*"))
                    x.Title += " *";
            }
        }
        static int pos;
        static int posEndOfInstruction;
        static int posStringBlock;
        static bool searchEndOfFile = false;
        static public Grid Grid = ((MainWindow)Application.Current.MainWindow).GuiArea;
        static public ListBox TokensList = ((MainWindow)Application.Current.MainWindow).ListView1;
        static public ListBox EntriesList = ((MainWindow)Application.Current.MainWindow).listviewEntries;
        static public MenuItem MenuViewDescription = ((MainWindow)Application.Current.MainWindow).MenuViewDescription;
        static public MenuItem MenuViewLabel = ((MainWindow)Application.Current.MainWindow).MenuViewLabel;
        static public Dictionary<int, string> MemoryNames = new Dictionary<int, string>()
        {
            {0x6000, "Program Register 0"},
            {0x6001, "Program Register 1"},
            {0x6002, "Program Register 2"},
            {0x6003, "Program Register 3"},
            {0x6004, "Temporary Register 0"},
            {0x6005, "Temporary Register 1"},
            {0x6006, "Temporary Register 2"},
            {0x6007, "Temporary Register 3"},
            {0x6008, "~Mode"},
            {0x6009, "~Choice"},
            {0x600a, "~Month"},
            {0x600b, "~Day"},
            {0x600c, "~Weekday"},
            {0x600d, "~Hours"},
            {0x600e, "~Minutes"},
            {0x600f, "~G00"},
            {0x6010, "~First Choice"},
            {0x6011, "~Mayuzumi Stress Level - Biolocal"},
            {0x6012, "~Kokoro Physical Strength Value"},
            {0x6013, "~Meal"},
            {0x6014, "~i(Satoru Chapter Calculation Register)"},
            {0x6015, "~Score"},
            {0x6016, "~Random Number"},
            {0x6017, "MACLOCAL Register 00"},
            {0x6018, "MACLOCAL Register 01"},
            {0x6019, "MACLOCAL Register 02"},
            {0x601a, "MACLOCAL Register 03"},
            {0x601b, "MACLOCAL Register 04"},
            {0x601c, "MACLOCAL Register 05"},
            {0x601d, "MACLOCAL Register 06"},
            {0x601e, "MACLOCAL Register 07"},
            {0x601f, "MACLOCAL Register 08"},
            {0x6020, "MACLOCAL Register 09"},
            {0x6021, "MACLOCAL Register 10"},
            {0x6022, "MACLOCAL Register 11"},
            {0x6023, "MACLOCAL Register 12"},
            {0x6024, "MACLOCAL Register 13"},
            {0x6025, "MACLOCAL Register 14"},
            {0x6026, "MACLOCAL Register 15"},
            {0x6027, "~Memo Illustration - Biolocal"},
            {0x6028, "MACLOCAL Register 16"},
            {0x6029, "MACLOCAL Register 17"},
            {0x602a, "~Chapter"},
            {0x602b, "~Satoru Epilogue Criteria"},
            {0x602c, "~Shortcut Unlocked Criteria 0"},
            {0x602d, "~Shortcut Unlocked Criteria 1"},
            {0x602e, "~Previous Play Discrimination"},
            {0x602f, "MACLOCAL Register 18"},
            {0x6030, "MACLOCAL Register 19"},
            {0x6031, "MACLOCAL Register 20"},
            {0x6032, "MACLOCAL Register 21" },

            {0x8000, "~Start Game"},
            {0x8001, "~Debug Mode"},
            {0x8002, "~Unused 00"},
            {0x8003, "~Unused 01"},
            {0x8004, "~Unused 02"},
            {0x8005, "~Unused 03"},
            {0x8006, "~Unused 04"},
            {0x8007, "~Unused 05"},
            {0x8008, "~Album Unlocked"},
            {0x8009, "~Music Unlocked"},
            {0x800a, "~Clear List Unlocked"},
            {0x800b, "~Shortcuts Unlocked"},
            {0x800c, "~Unused 06"},
            {0x800d, "~Unused 07"},
            {0x800e, "~Unused 08"},
            {0x800f, "~Unused 09"},
            {0x8010, "~All Endings Unlocked"},
            {0x8011, "~All Titles Unlocked"},
            {0x8012, "~Album Fully Unlocked"},
            {0x8013, "~All Music Unlocked"},
            {0x8014, "~All Messages Unlocked"},
            {0x8015, "~All Choices Unlocked"},
            {0x8016, "~Play Time Fully Unlocked"},
            {0x8017, "~TIPS Fully Unlocked"},
            {0x8018, "~Unused 10"},
            {0x8019, "~Unused 11"},
            {0x801a, "~Unused 12"},
            {0x801b, "~Unused 13"},
            {0x801c, "~Unused 14"},
            {0x801d, "~Unused 15"},
            {0x801e, "~Unused 16"},
            {0x801f, "~Maximum Choices"},
            {0x8020, "~Kokoro Chapter - Clear"},
            {0x8021, "~Didn't Take the Sleeping Pills BAD - Clear"},
            {0x8022, "~Drank Water BAD - Clear"},
            {0x8023, "~Mayuzumi's Madness A BAD - Clear"},
            {0x8024, "~Incident at SPHIA BAD - Clear"},
            {0x8025, "~Killed by Enomoto BAD - Clear"},
            {0x8026, "~Killed by Enomoto2 BAD - Clear"},
            {0x8027, "~Mayuzumi's Madness B Bad - Clear"},
            {0x8028, "~Yomogi's Madness BAD - Clear"},
            {0x8029, "~Meal at SPHIA BAD - Clear"},
            {0x802a, "~Mayuzumi's Madness C BAD - Clear"},
            {0x802b, "~Incident at Mount Akakura 1 BAD - Clear"},
            {0x802c, "~Incident at Mount Akakura 2 BAD - Clear"},
            {0x802d, "~Death from Cold at the Crash Site 1 BAD - Clear"},
            {0x802e, "~Death from Cold at the Crash Site 2 BAD - Clear"},
            {0x802f, "~Yuni Dies From Cold BAD - Clear"},
            {0x8030, "~Avalanche 1 BAD - Clear"},
            {0x8031, "~Avalanche 2 BAD - Clear"},
            {0x8032, "~Avalanche 3 BAD - Clear"},
            {0x8033, "~Dream of the Butterfly 2 - Clear"},
            {0x8034, "~Satoru Good End - Clear"},
            {0x8035, "~Satoru Bad End A - Clear"},
            {0x8036, "~Didn't Take the Sleeping Pills BAD - Clear"},
            {0x8037, "~Drank Water BAD - Clear"},
            {0x8038, "~Mayuzumi's Madness A BAD - Clear"},
            {0x8039, "~Incident at SPHIA BAD - Clear"},
            {0x803a, "~Killed by Enomoto BAD - Clear"},
            {0x803b, "~Mayuzumi's Madness B Bad - Clear"},
            {0x803c, "~Yomogi's Madness BAD - Clear"},
            {0x803d, "~Fingers Cut Off BAD - Clear"},
            {0x803e, "~Roll BAD - Clear"},
            {0x803f, "~Nobody at the Cabin BAD - Clear"},
            {0x8040, "~Yuni Dies From Cold BAD - Clear"},
            {0x8041, "~To-Be-Deleted 0"},
            {0x8042, "~To-Be-Deleted 1"},
            {0x8043, "~Tyramine and DMT BAD - Clear"},
            {0x8044, "~Satoru Chapter Epilogue - Clear"},
            {0x8045, "~Kokoro 1st Day Unlocked"},
            {0x8046, "~Kokoro 1st Day - Midday Unlocked"},
            {0x8047, "~Kokoro 2nd Day Unlocked"},
            {0x8048, "~Kokoro 2nd Day - Midday Unlocked"},
            {0x8049, "~Kokoro 3rd Day Unlocked"},
            {0x804a, "~Kokoro 3rd Day - Midday Unlocked"},
            {0x804b, "~Kokoro 4th Day Unlocked"},
            {0x804c, "~Kokoro 4th Day - Midday Unlocked"},
            {0x804d, "~Kokoro 5th Day Unlocked"},
            {0x804e, "~Kokoro 5th Day - Midday Unlocked"},
            {0x804f, "~Kokoro 6th Day Unlocked"},
            {0x8050, "~Kokoro 6th Day - Midday Unlocked"},
            {0x8051, "~Kokoro 7th Day Unlocked"},
            {0x8052, "~Kokoro 7th Day - Midday Unlocked"},
            {0x8053, "~Kokoro Epilogue Unlocked"},
            {0x8054, "~Satoru 1st Day Unlocked"},
            {0x8055, "~Satoru 1st Day - Midday Unlocked"},
            {0x8056, "~Satoru 2nd Day Unlocked"},
            {0x8057, "~Satoru 2nd Day - Midday Unlocked"},
            {0x8058, "~Satoru 3rd Day Unlocked"},
            {0x8059, "~Satoru 3rd Day - Midday Unlocked"},
            {0x805a, "~Satoru 4th Day Unlocked"},
            {0x805b, "~Satoru 4th Day - Midday Unlocked"},
            {0x805c, "~Satoru 5th Day Unlocked"},
            {0x805d, "~Satoru 5th Day - Midday Unlocked"},
            {0x805e, "~Satoru 6th Day Unlocked"},
            {0x805f, "~Satoru 6th Day - Midday Unlocked"},
            {0x8060, "~Satoru 7th Day Unlocked"},
            {0x8061, "~Satoru 7th Day - Midday Unlocked"},
            {0x8062, "~Satoru Epilogue Unlocked"},
            {0x8063, "~Satoru SYSMES Already Read"},
            {0x8064, "~MES100%"},
            {0x8065, "Unlocked 100% Album" },

            {0xa000, "Temporary Flag 00"},
            {0xa001, "Temporary Flag 01"},
            {0xa002, "Temporary Flag 02"},
            {0xa003, "Temporary Flag 03"},
            {0xa004, "Unused 00"},
            {0xa005, "Unused 01"},
            {0xa006, "Unused 02"},
            {0xa007, "Unused 03"},
            {0xa008, "Unused 04"},
            {0xa009, "Unused 05"},
            {0xa00a, "Unused 06"},
            {0xa00b, "Unused 07"},
            {0xa00c, "Unused 08"},
            {0xa00d, "Unused 09"},
            {0xa00e, "Unused 10"},
            {0xa00f, "Unused 11"},
            {0xa010, "~You went to Yuni's room"},
            {0xa011, "~You went outside once"},
            {0xa012, "~You met Inubushi"},
            {0xa013, "~You can't handle rats"},
            {0xa014, "~You told you're Kokoro"},
            {0xa015, "~Who are you?"},
            {0xa016, "~You're bothered by the words \"I missed you\""},
            {0xa017, "~You tasted the hard tack on 1st day"},
            {0xa018, "~You didn't drink the sleeping pill"},
            {0xa019, "~You ate at SPHIA the 3rd and 4th day"},
            {0xa01a, "~You explained the destination to Mayuzumi"},
            {0xa01b, "~You had your fortune told by Yuni"},
            {0xa01c, "~Mayuzumi manages the supplies"},
            {0xa01d, "~You walked along the wall"},
            {0xa01e, "~Incident at SPHIA"},
            {0xa01f, "~You left Yomogi behind"},
            {0xa020, "~You carried Yomogi"},
            {0xa021, "~Yuni's death by hypothermia"},
            {0xa022, "~Death from cold at crash site"},
            {0xa023, "~Yuni's death by hypothermia"},
            {0xa024, "~You swore to protect Hotori"},
            {0xa025, "~You don't know them"},
            {0xa026, "~You asked why you climbed the clock tower"},
            {0xa027, "~You like black coffee"},
            {0xa028, "~You climbed the clock tower"},
            {0xa029, "~You're hungry"},
            {0xa02a, "~Yuni's room"},
            {0xa02b, "~Inubushi's room"},
            {0xa02c, "~You don't know the name of Yomogi and the others"},
            {0xa02d, "~Snowboard"},
            {0xa02e, "~Basket"},
            {0xa02f, "~Yuni 2PL"},
            {0xa030, "~You won against Hotori at basketball"},
            {0xa031, "~You lost against Hotori at basketball"},
            {0xa032, "~You drank coffee (4th day)"},
            {0xa033, "~You want to kill Hotori"},
            {0xa034, "~You want to kill Hotori and think she's the culprit"},
            {0xa035, "~Tobilin is enraged"},
            {0xa036, "~Inubushi and Hotori aren't the same person"},
            {0xa037, "~You notice there are three exchange points"},
            {0xa038, "~You reasoned there are four exchange points"},
            {0xa039, "UV Service Flag 0"},
            {0xa03a, "UV Service Flag 1"},
            {0xa03b, "UV Service Flag 2"},
            {0xa03c, "UV Service Flag 3"},
            {0xa03d, "UV Service Flag 4"},
            {0xa03e, "UV Service Flag 5"},
            {0xa03f, "~The cardboards collapsed - Bio local"},
            {0xa040, "~Yuni's a Twin Theory - Bio local"},
            {0xa041, "~Kokoro manages the rations - Bio local"},
            {0xa042, "~Very worried - Bio local"},
            {0xa043, "~Didn't drink coffee on the second day - Bio local"},
            {0xa044, "~Glad Kokoro's alive - Bio local"},
            {0xa045, "~Didn't speak with Lin about the \"Round and Round\" - Bio local"},
            {0xa046, "~Didn't drink the sleeping pill - Bio BAD local"},
            {0xa047, "~Ate at SPHIA on the third and fourth day - Bio BAD local"},
            {0xa048, "~Killed By Enomoto - Bio BAD local"},
            {0xa049, "~Incident at SPHIA - Bio BAD local"},
            {0xa04a, "~Mayuzumi's madness A - Bio BAD Local"},
            {0xa04b, "~Yomogi's madness - Bio BAD Local"},
            {0xa04c, "~Mayuzumi's madness B - Bio BAD Local"},
            {0xa04d, "~Mayuzumi's madness C - Bio BAD Local"},
            {0xa04e, "~Incident at Mt Akakura - Bio BAD local"},
            {0xa04f, "~Death from cold at the crash site - Bio BAD local"},
            {0xa050, "~Yuni's death from hypothermia - Bio BAD local"},
            {0xa051, "~Kokoro's death on the seventh day A - Bio BAD"},
            {0xa052, "~Kokoro's death on the seventh day B - Bio BAD"},
            {0xa053, "~Satoru Bad End A - Bio BAD Local"},
            {0xa054, "~Drank water BAD - Bio BAD local"},
            {0xa055, "~Fingers cut off BAD - Bio BAD Local"},
            {0xa056, "~Roll BAD - Bio BAD Local"},
            {0xa057, "UV Service Flag 6"},
            {0xa058, "SELF FLAG"},

            {0x000, "~Playthroughs"},
            {0x001, "~Mayuzumi Stress Level - Bio"},
            {0x002, "~Message Illustration - Bio"},
            {0x003, "~The Cardboard Boxes Collapsed - Bio"},
            {0x004, "~Yuni's a Twin Theory - Bio"},
            {0x005, "~Kokoro Manages the Rations - Bio"},
            {0x006, "~Very Worried - Bio"},
            {0x007, "~Didn't Drink Coffee on the Second Day - Bio BAD"},
            {0x008, "~Glad Kokoro's Alive - Bio BAD"},
            {0x009, "~Didn't Speak with Lin About the \"Round and Round\" - Bio BAD"},
            {0x00a, "~Didn't Take the Sleeping Pills - Bio BAD"},
            {0x00b, "~Ate at SPHIA on the Third and Fourth Day - Bio BAD"},
            {0x00c, "~Killed By Enomoto - Bio BAD"},
            {0x00d, "~Incident at SPHIA - Bio BAD"},
            {0x00e, "~Mayuzumi's Madness A - Bio BAD"},
            {0x00f, "~Yomogi's Madness - Bio BAD"},
            {0x010, "~Mayuzumi's Madness B - Bio BAD"},
            {0x011, "~Mayuzumi's Madness C - Bio BAD"},
            {0x012, "~Incident at Mount Akakura - Bio BAD"},
            {0x013, "~Death From Cold at the Crash Site - Bio BAD"},
            {0x014, "~Yuni's Death From Hypothermia - Bio BAD"},
            {0x015, "~Kokoro's Death on the Seventh Day A - Bio BAD"},
            {0x016, "~Kokoro's Death on the Seventh Day B - Bio BAD"},
            {0x017, "~Satoru Bad End A - Bio BAD"},
            {0x018, "~Drank Water BAD - Bio BAD"},
            {0x019, "~Fingers cut off BAD - Bio BAD"},
            {0x01a, "~Roll BAD - Bio BAD"},
            {0x01b, "~ED Count"},
            {0x01c, "~Shortcuts Unlocked Register 0"},
            {0x01d, "~Shortcuts Unlocked Register 1"},
            {0x01e, "~Shortcuts Unlocked Register 2"},
            {0x01f, "~Previous Play Discrimination"}
        };
        static public Dictionary<int, string> SceneTitles = new Dictionary<int, string>(){
            {0x000, "Prologue"},
            {0x001, "Heaven Is Here"},
            {0x002, "Prologue"},
            {0x003, "The α and the ω"},
            {0x004, "Prologue"},
            {0x005, "FALL/DOWN"},
            {0x006, "Kokoro Chapter"},
            {0x007, "In the Bosom of Motherhood"},
            {0x008, "Kokoro Chapter"},
            {0x009, "The Closed-Off Structure"},
            {0x00a, "Kokoro Chapter"},
            {0x00b, "Ethereal \"Girl\""},
            {0x00c, "Kokoro Chapter"},
            {0x00d, "Youth Behavioral Disorders"},
            {0x00e, "Kokoro Chapter"},
            {0x00f, "O Great Sephirot, Our Master"},
            {0x010, "Kokoro Chapter"},
            {0x011, "Liberation of Self"},
            {0x012, "Kokoro Chapter"},
            {0x013, "Personality Development in Youth"},
            {0x014, "Kokoro Chapter"},
            {0x015, "The Animus Within the Anima"},
            {0x016, "Kokoro Chapter"},
            {0x017, "The Inescapable Reality"},
            {0x018, "Kokoro Chapter"},
            {0x019, "Falling into Egocentrism"},
            {0x01a, "Kokoro Chapter"},
            {0x01b, "Heart, Body, and Sedatives"},
            {0x01c, "Kokoro Chapter"},
            {0x01d, "Collective Symptoms of Abnormal Dissociation"},
            {0x01e, "Kokoro Chapter"},
            {0x01f, "Alice in Disorderly Clothes"},
            {0x020, "Kokoro Chapter"},
            {0x021, "Distinctions Between Genders"},
            {0x022, "Kokoro Chapter"},
            {0x023, "Breakfast Is Good for You"},
            {0x024, "Kokoro Chapter"},
            {0x025, "What Is Mutual Understanding?"},
            {0x026, "Kokoro Chapter"},
            {0x027, "Cause and Reason"},
            {0x028, "Kokoro Chapter"},
            {0x029, "Trust and Deception"},
            {0x02a, "Kokoro Chapter"},
            {0x02b, "Alarm!"},
            {0x02c, "Kokoro Chapter"},
            {0x02d, "A Taste of the Primordial Sea"},
            {0x02e, "Kokoro Chapter"},
            {0x02f, "Discord of Collective Consciousness"},
            {0x030, "Kokoro Chapter"},
            {0x031, "Scars Taste Like Paranoia"},
            {0x032, "Kokoro Chapter"},
            {0x033, "The Place Where Bliss Resides"},
            {0x034, "Kokoro Chapter"},
            {0x035, "I'm Here"},
            {0x036, "Kokoro Chapter"},
            {0x037, "False Shadow"},
            {0x038, "Kokoro Chapter"},
            {0x039, "The Desire to Change"},
            {0x03a, "Kokoro Chapter"},
            {0x03b, "Human and Heart"},
            {0x03c, "Kokoro Chapter"},
            {0x03d, "The Meaning of Life, and a Decision"},
            {0x03e, "Kokoro Chapter"},
            {0x03f, "Her Illness"},
            {0x040, "Kokoro Chapter"},
            {0x041, "Do You Believe in \"Fate\"?"},
            {0x042, "Kokoro Chapter"},
            {0x043, "Psychiatric Examination Results"},
            {0x044, "Kokoro Chapter"},
            {0x045, "Whom I Can Rely Upon"},
            {0x046, "Kokoro Chapter"},
            {0x047, "Chainlike Spiral"},
            {0x048, "Kokoro Chapter"},
            {0x049, "Memories and Speculations"},
            {0x04a, "Kokoro Chapter"},
            {0x04b, "New Life"},
            {0x04c, "Kokoro Chapter"},
            {0x04d, "Crucified Emotional Trauma"},
            {0x04e, "Kokoro Chapter"},
            {0x04f, "The Feeling You Believe In"},
            {0x050, "Kokoro Chapter"},
            {0x051, "Artificial Personality Exchange"},
            {0x052, "Kokoro Chapter"},
            {0x053, "The Beginning of Ego Collapse"},
            {0x054, "Kokoro Chapter"},
            {0x055, "Choosing Life"},
            {0x056, "Kokoro Chapter"},
            {0x057, "White World"},
            {0x058, "Kokoro Chapter"},
            {0x059, "Between Anxiety and Collapse"},
            {0x05a, "Kokoro Chapter"},
            {0x05b, "DID Emergence"},
            {0x05c, "Kokoro Chapter"},
            {0x05d, "Abyss of Despair I"},
            {0x05e, "Kokoro Chapter"},
            {0x05f, "A Small Respite"},
            {0x060, "Kokoro Chapter"},
            {0x061, "At the End of Self-Interests..."},
            {0x062, "Kokoro Chapter"},
            {0x063, "At the End of Grudges..."},
            {0x064, "Kokoro Chapter"},
            {0x065, "At the End of Private Plans..."},
            {0x066, "Kokoro Chapter"},
            {0x067, "Being Frightened, Frightened Being"},
            {0x068, "Kokoro Chapter"},
            {0x069, "Self-Defense or False Accusation"},
            {0x06a, "Kokoro Chapter"},
            {0x06b, "Cruel Harmony"},
            {0x06c, "Kokoro Chapter"},
            {0x06d, "Mother and Child"},
            {0x06e, "Kokoro Chapter"},
            {0x06f, "The Hope That Visits in the End"},
            {0x070, "Kokoro Chapter"},
            {0x071, "Endless Mantled Environment"},
            {0x072, "Kokoro Chapter"},
            {0x073, "End of Self-Sacrifice"},
            {0x074, "Kokoro Chapter"},
            {0x075, "Disappearing Light"},
            {0x076, "Kokoro Chapter"},
            {0x077, "While Receiving Blessing"},
            {0x078, "Kokoro Chapter"},
            {0x079, "Bare Minimum of Happiness"},
            {0x07a, "Kokoro Chapter"},
            {0x07b, "Eternal Boy"},
            {0x07c, "Kokoro Chapter"},
            {0x07d, "Man-Made Miracle"},
            {0x07e, "Kokoro Chapter"},
            {0x07f, "Revealed Intentions"},
            {0x080, "Kokoro Chapter"},
            {0x081, "Angel Descending in Darkness"},
            {0x082, "Kokoro Chapter"},
            {0x083, "Struggle Against the End"},
            {0x084, "Kokoro Chapter"},
            {0x085, "Knocking on the Door of Hope"},
            {0x086, "Kokoro Chapter"},
            {0x087, "The Promised Land to Which We Must Return"},
            {0x088, "Kokoro Chapter"},
            {0x089, "Determined Fate"},
            {0x08a, "Kokoro Chapter"},
            {0x08b, "The Shadow That Couldn't Live"},
            {0x08c, "Kokoro Chapter"},
            {0x08d, "God Descends to the New World"},
            {0x08e, "Kokoro Chapter"},
            {0x08f, "Abyss of Despair II"},
            {0x090, "Kokoro Chapter"},
            {0x091, "Within the Cage..."},
            {0x092, "Satoru Chapter"},
            {0x093, "Prenatal"},
            {0x094, "Satoru Chapter"},
            {0x095, "Persona"},
            {0x096, "Satoru Chapter"},
            {0x097, "Animus"},
            {0x098, "Satoru Chapter"},
            {0x099, "Trickster"},
            {0x09a, "Satoru Chapter"},
            {0x09b, "Shadow"},
            {0x09c, "Satoru Chapter"},
            {0x09d, "Great Mother"},
            {0x09e, "Satoru Chapter"},
            {0x09f, "Separation"},
            {0x0a0, "Satoru Chapter"},
            {0x0a1, "Uniform"},
            {0x0a2, "Satoru Chapter"},
            {0x0a3, "Dimly"},
            {0x0a4, "Satoru Chapter"},
            {0x0a5, "Calmness"},
            {0x0a6, "Satoru Chapter"},
            {0x0a7, "Examine"},
            {0x0a8, "Satoru Chapter"},
            {0x0a9, "Break-Time"},
            {0x0aa, "Satoru Chapter"},
            {0x0ab, "Mind-Education"},
            {0x0ac, "Satoru Chapter"},
            {0x0ad, "Transmit"},
            {0x0ae, "Satoru Chapter"},
            {0x0af, "Virgin-Ceiling"},
            {0x0b0, "Satoru Chapter"},
            {0x0b1, "Miserable"},
            {0x0b2, "Satoru Chapter"},
            {0x0b3, "Reflection"},
            {0x0b4, "Satoru Chapter"},
            {0x0b5, "Polyhedron"},
            {0x0b6, "Satoru Chapter"},
            {0x0b7, "Old Wise Man"},
            {0x0b8, "Satoru Chapter"},
            {0x0b9, "Seized"},
            {0x0ba, "Satoru Chapter"},
            {0x0bb, "Receive"},
            {0x0bc, "Satoru Chapter"},
            {0x0bd, "Reflection"},
            {0x0be, "Satoru Chapter"},
            {0x0bf, "Crucified Rat"},
            {0x0c0, "Satoru Chapter"},
            {0x0c1, "Requiem"},
            {0x0c2, "Satoru Chapter"},
            {0x0c3, "Respite"},
            {0x0c4, "Satoru Chapter"},
            {0x0c5, "D-H-A!!"},
            {0x0c6, "Satoru Chapter"},
            {0x0c7, "N-B-A??"},
            {0x0c8, "Satoru Chapter"},
            {0x0c9, "Piece Of Cheese"},
            {0x0ca, "Satoru Chapter"},
            {0x0cb, "Dying"},
            {0x0cc, "Satoru Chapter"},
            {0x0cd, "Dependence"},
            {0x0ce, "Satoru Chapter"},
            {0x0cf, "Fibber?"},
            {0x0d0, "Satoru Chapter"},
            {0x0d1, "Sabbath Time"},
            {0x0d2, "Satoru Chapter"},
            {0x0d3, "Ayahuasca Matter"},
            {0x0d4, "Satoru Chapter"},
            {0x0d5, "Pregnant Girl"},
            {0x0d6, "Satoru Chapter"},
            {0x0d7, "Star's Prince"},
            {0x0d8, "Satoru Chapter"},
            {0x0d9, "Heteronomie"},
            {0x0da, "Satoru Chapter"},
            {0x0db, "Isomorphism"},
            {0x0dc, "Satoru Chapter"},
            {0x0dd, "Liberation"},
            {0x0de, "Satoru Chapter"},
            {0x0df, "TRAUMA"},
            {0x0e0, "Satoru Chapter"},
            {0x0e1, "Nucleotide"},
            {0x0e2, "Satoru Chapter"},
            {0x0e3, "Fitful"},
            {0x0e4, "Satoru Chapter"},
            {0x0e5, "Jesting"},
            {0x0e6, "Satoru Chapter"},
            {0x0e7, "Puer Aeternus"},
            {0x0e8, "Satoru Chapter"},
            {0x0e9, "Awakening"},
            {0x0ea, "Satoru Chapter"},
            {0x0eb, "Transference"},
            {0x0ec, "Satoru Chapter"},
            {0x0ed, "Real"},
            {0x0ee, "Satoru Chapter"},
            {0x0ef, "Trinity"},
            {0x0f0, "Satoru Chapter"},
            {0x0f1, "Quantum Teleportation"},
            {0x0f2, "Satoru Chapter"},
            {0x0f3, "Destruction"},
            {0x0f4, "Satoru Chapter"},
            {0x0f5, "Insanity"},
            {0x0f6, "Satoru Chapter"},
            {0x0f7, "Fatal"},
            {0x0f8, "Satoru Chapter"},
            {0x0f9, "Disclose"},
            {0x0fa, "Satoru Chapter"},
            {0x0fb, "Lethal"},
            {0x0fc, "Satoru Chapter"},
            {0x0fd, "Antidote"},
            {0x0fe, "Satoru Chapter"},
            {0x0ff, "Miracle Child"},
            {0x100, "Satoru Chapter"},
            {0x101, "Recollect"},
            {0x102, "Satoru Chapter"},
            {0x103, "Phobia"},
            {0x104, "Satoru Chapter"},
            {0x105, "Insertion"},
            {0x106, "Satoru Chapter"},
            {0x107, "Promise"},
            {0x108, "Satoru Chapter"},
            {0x109, "Hekate"},
            {0x10a, "Satoru Chapter"},
            {0x10b, "Unmask"},
            {0x10c, "Satoru Chapter"},
            {0x10d, "Another Shadow"},
            {0x10e, "Satoru Chapter"},
            {0x10f, "Rejection"},
            {0x110, "Satoru Chapter"},
            {0x111, "Revenger"},
            {0x112, "Satoru Chapter"},
            {0x113, "Old Wise Woman"},
            {0x114, "Satoru Chapter"},
            {0x115, "Research"},
            {0x116, "Satoru Chapter"},
            {0x117, "Consciousness"},
            {0x118, "Satoru Chapter"},
            {0x119, "Symbolic"},
            {0x11a, "Satoru Chapter"},
            {0x11b, "The Third Area"},
            {0x11c, "Satoru Chapter"},
            {0x11d, "Apoptosis"},
            {0x11e, "Satoru Chapter"},
            {0x11f, "Ambivalence"},
            {0x120, "Satoru Chapter"},
            {0x121, "The End Of World"},
            {0x122, "Satoru Chapter"},
            {0x123, "The End Of Mind"},
            {0x124, "Satoru Chapter"},
            {0x125, "Identification"},
            {0x126, "Satoru Chapter"},
            {0x127, "Anima"},
            {0x128, "Satoru Chapter"},
            {0x129, "Delta"},
            {0x12a, "Satoru Chapter"},
            {0x12b, "Infinite History"},
            {0x12c, "Satoru Chapter"},
            {0x12d, "Never More"},
            {0x12e, "Satoru Chapter"},
            {0x12f, "Ever More"},
            {0x130, "Satoru Chapter"},
            {0x131, "More Remember"},
            {0x132, "Satoru Chapter"},
            {0x133, "Where Is \"Self\"?"},
        };
        static public Dictionary<int, string> EvTitles = new Dictionary<int, string>()
        {
            {0x0, "EV_PR01A"},
            {0x1, "EV_PR02A"},
            {0x2, "EV_PR03A"},
            {0x3, "EV_CO01A"},
            {0x4, "EV_CO01B"},
            {0x5, "EV_CO01C"},
            {0x6, "EV_CO02A"},
            {0x7, "EV_CO02B"},
            {0x8, "EV_CO02C"},
            {0x9, "EV_CO02D"},
            {0xa, "EV_CO03A"},
            {0xb, "EV_CO03B"},
            {0xc, "EV_CO03C"},
            {0xd, "EV_CO04A"},
            {0xe, "EV_CO05A"},
            {0xf, "EV_CO05B"},
            {0x10, "EV_CO05C"},
            {0x11, "EV_CO06A"},
            {0x12, "EV_CO06B"},
            {0x13, "EV_CO07A"},
            {0x14, "EV_CO07B"},
            {0x15, "EV_CO08A"},
            {0x16, "EV_CO09A"},
            {0x17, "EV_CO10A"},
            {0x18, "EV_CO10B"},
            {0x19, "EV_CO11A"},
            {0x1a, "EV_CO12A"},
            {0x1b, "EV_CO13A"},
            {0x1c, "EV_CO13B"},
            {0x1d, "EV_CO13C"},
            {0x1e, "EV_CO14A"},
            {0x1f, "EV_CO14B"},
            {0x20, "EV_CO15A"},
            {0x21, "EV_CO15B"},
            {0x22, "EV_CO16A"},
            {0x23, "EV_CO17A"},
            {0x24, "EV_CO18A"},
            {0x25, "EV_CO19A"},
            {0x26, "EV_CO20A"},
            {0x27, "EV_CO21A"},
            {0x28, "EV_CO22A"},
            {0x29, "EV_CO23A"},
            {0x2a, "EV_CO24A"},
            {0x2b, "EV_CO25A"},
            {0x2c, "EV_CO25B"},
            {0x2d, "EV_CO26A"},
            {0x2e, "EV_CO27A"},
            {0x2f, "EV_CO27B"},
            {0x30, "EV_CO28A"},
            {0x31, "EV_CO29A"},
            {0x32, "EV_CO29B"},
            {0x33, "EV_CO30A"},
            {0x34, "EV_CO30B"},
            {0x35, "EV_CO30C"},
            {0x36, "EV_CO30D"},
            {0x37, "EV_CO31A"},
            {0x38, "EV_CO32A"},
            {0x39, "EV_CO32B"},
            {0x3a, "EV_CO32C"},
            {0x3b, "EV_CO33A"},
            {0x3c, "EV_CO33B"},
            {0x3d, "EV_CO34A"},
            {0x3e, "EV_CO35A"},
            {0x3f, "EV_CO36A"},
            {0x40, "EV_SA01A"},
            {0x41, "EV_SA02A"},
            {0x42, "EV_SA02B"},
            {0x43, "EV_SA02C"},
            {0x44, "EV_SA02D"},
            {0x45, "EV_SA03A"},
            {0x46, "EV_SA03B"},
            {0x47, "EV_SA04A"},
            {0x48, "EV_SA05A"},
            {0x49, "EV_SA06A"},
            {0x4a, "EV_SA06B"},
            {0x4b, "EV_SA06C"},
            {0x4c, "EV_SA06D"},
            {0x4d, "EV_SA07A"},
            {0x4e, "EV_SA08A"},
            {0x4f, "EV_SA09A"},
            {0x50, "EV_SA10A"},
            {0x51, "EV_SA11A"},
            {0x52, "EV_SA12A"},
            {0x53, "EV_SA13A"},
            {0x54, "EV_SA13B"},
            {0x55, "EV_SA14A"},
            {0x56, "EV_SA14B"},
            {0x57, "EV_SA15A"},
            {0x58, "EV_SA15B"},
            {0x59, "EV_SA16A"},
            {0x5a, "EV_SA16B"},
            {0x5b, "EV_SA17A"},
            {0x5c, "EV_SA18A"},
            {0x5d, "EV_SA18B"},
            {0x5e, "EV_SA19A"},
            {0x5f, "EV_SA20A"},
            {0x60, "EV_SA21A"},
            {0x61, "EV_SA22A"},
            {0x62, "EV_SA23A"},
            {0x63, "EV_SA24A"},
            {0x64, "EV_SA24B"},
            {0x65, "POST01"},
            {0x66, "POST02"},
            {0x67, "POST03"},
            {0x68, "POST04"},
            {0x69, "POST05"},
            {0x6a, "REME01"},
            {0x6b, "REME02"},
            {0x6c, "REME03"},
            {0x6d, "REME04"},
            {0x6e, "REME05"},
            {0x6f, "REME06"},
            {0x70, "REME07"},
            {0x71, "REME08"},
            {0x72, "REME09"},
            {0x73, "REME10"},
            {0x74, "REME11"},
            {0x75, "REME12"},
            {0x76, "REME13"},
            {0x77, "REME14"},
            {0x78, "REME15"},
            {0x79, "REME16"},
            {0x7a, "REME17"},
            {0x7b, "REME18"},
            {0x7c, "REME19"},
            {0x7d, "REME20"},
            {0x7e, "REME21"},
            {0x7f, "REME22"},
            {0x80, "REME23"},
            {0x81, "REME24"},
            {0x82, "REME25"},
            {0x83, "REME26"},
            {0x84, "REME27"},
            {0x85, "REME28"},
            {0x86, "REME29"},
            {0x87, "REME30"},
            {0x88, "REME31"},
            {0x89, "REME32"},
            {0x8a, "REME33"},
            {0x8b, "REME34"},
            {0x8c, "REME35"},
            {0x8d, "EV_MV00"},
            {0x8e, "EV_MV01"},
            {0x8f, "EV_MV02"},
            {0x90, "EV_MV03"},
            {0x91, "EV_MV04"},
            {0x92, "EV_MV05"},
            {0x93, "EV_MV06"},
            {0x94, "EV_MV08"},
            {0x95, "EV_MV09"},
            {0x96, "EV_MV12"},
            {0x97, "EV_MV13"},
            {0x98, "EV_MV14"},
            {0x99, "EV_MV15"},
            {0x9a, "EV_MVED1"},
            {0x9b, "EV_MVED2"},
            {0x9c, "EV_MVED3"},
            {0x9d, "EV_MVOP"}
        };
        static public Dictionary<int, string> BgmTitles = new Dictionary<int, string>()
        {
            {0x0, "Chaining"},
            {0x1, "Scheme"},
            {0x2, "Anima"},
            {0x3, "Animus"},
            {0x4, "Persona"},
            {0x5, "Old wise man"},
            {0x6, "Great mother"},
            {0x7, "Shadow"},
            {0x8, "Puer aeternus"},
            {0x9, "Trickster"},
            {0xa, "Self"},
            {0xb, "Communication"},
            {0xc, "Anxiety"},
            {0xd, "Cue"},
            {0xe, "Paranoia"},
            {0xf, "Fear and Insanity"},
            {0x10, "Thanatos"},
            {0x11, "Delusive consciousness"},
            {0x12, "Mantra"},
            {0x13, "Multiple maze"},
            {0x14, "Dreamy lens"},
            {0x15, "Dark Gestalt"},
            {0x16, "Will -Theme-"},
            {0x17, "Catharsis"},
            {0x18, "All or None"},
            {0x19, "Delta Wave"},
            {0x1a, "Heuristic"},
            {0x1b, "Nucleus"},
            {0x1c, "Dreamy lens -Piano-"},
            {0x1d, "Delusive consciousness amb."},
            {0x1e, "All or None -Piano-"},
            {0x1f, "Chaining -Beta-"},
            {0x20, "Scheme -Beta-"},
            {0x21, "Chaining (Unused, Identical to original)"},
            {0x22, "Lamp burning (Unused)"},
            {0x23, "little prophet"},
            {0x24, "Darkness of chaos"},
            {0x25, "----------"}
        };
        static public Dictionary<int, string> FileNames = new Dictionary<int, string>()
        {
            {0x0000, "PR_01"},
            {0x0001, "PR_02"},
            {0x0002, "PR_03"},
            {0x0003, "CO1_01"},
            {0x0004, "CO1_02"},
            {0x0005, "CO1_03"},
            {0x0006, "CO1_04"},
            {0x0007, "CO1_05"},
            {0x0008, "CO1_06"},
            {0x0009, "CO1_07"},
            {0x000a, "CO1_08"},
            {0x000b, "CO1_09"},
            {0x000c, "CO1_10"},
            {0x000d, "CO1_11"},
            {0x000e, "CO1_12"},
            {0x000f, "CO1_13"},
            {0x0010, "CO2_01"},
            {0x0011, "CO2_02"},
            {0x0012, "CO2_03"},
            {0x0013, "CO2_04"},
            {0x0014, "CO2_05"},
            {0x0015, "CO2_06"},
            {0x0016, "CO2_07"},
            {0x0017, "CO2_08"},
            {0x0018, "CO2_09A"},
            {0x0019, "CO2_09B"},
            {0x001a, "CO2_10"},
            {0x001b, "CO2_11"},
            {0x001c, "CO2_12"},
            {0x001d, "CO3_01"},
            {0x001e, "CO3_02"},
            {0x001f, "CO3_03"},
            {0x0020, "CO3_04"},
            {0x0021, "CO3_05"},
            {0x0022, "CO3_06"},
            {0x0023, "CO3_07"},
            {0x0024, "CO3_08"},
            {0x0025, "CO3_09"},
            {0x0026, "CO3_10"},
            {0x0027, "CO4_01"},
            {0x0028, "CO4_02"},
            {0x0029, "CO4_03"},
            {0x002a, "CO4_04"},
            {0x002b, "CO4_05"},
            {0x002c, "CO4_06"},
            {0x002d, "CO4_07"},
            {0x002e, "CO4_08"},
            {0x002f, "CO4_09"},
            {0x0030, "CO5_01"},
            {0x0031, "CO5_02"},
            {0x0032, "CO5_03"},
            {0x0033, "CO5_04"},
            {0x0034, "CO5_05"},
            {0x0035, "CO5_06"},
            {0x0036, "CO5_07"},
            {0x0037, "CO5_08"},
            {0x0038, "CO5_09"},
            {0x0039, "CO5_10"},
            {0x003a, "CO5_11"},
            {0x003b, "CO5_12"},
            {0x003c, "CO5_13"},
            {0x003d, "CO6_01"},
            {0x003e, "CO6_02"},
            {0x003f, "CO6_03"},
            {0x0040, "CO6_04"},
            {0x0041, "CO6_05"},
            {0x0042, "CO7_01"},
            {0x0043, "CO7_02"},
            {0x0044, "CO7_03"},
            {0x0045, "CO7_04"},
            {0x0046, "CO7_05"},
            {0x0047, "CO7_06"},
            {0x0048, "CO7_07"},
            {0x0049, "COEP_01"},
            {0x004a, "SA1_01"},
            {0x004b, "SA1_02"},
            {0x004c, "SA1_03"},
            {0x004d, "SA1_04"},
            {0x004e, "SA1_05"},
            {0x004f, "SA1_06"},
            {0x0050, "SA1_07"},
            {0x0051, "SA1_08"},
            {0x0052, "SA1_09"},
            {0x0053, "SA2_01"},
            {0x0054, "SA2_02"},
            {0x0055, "SA2_03"},
            {0x0056, "SA2_04"},
            {0x0057, "SA2_05"},
            {0x0058, "SA2_06"},
            {0x0059, "SA2_07"},
            {0x005a, "SA2_08"},
            {0x005b, "SA2_09"},
            {0x005c, "SA2_10"},
            {0x005d, "SA2_11"},
            {0x005e, "SA2_12"},
            {0x005f, "SA2_13"},
            {0x0060, "SA2_14"},
            {0x0061, "SA2_15"},
            {0x0062, "SA3_01"},
            {0x0063, "SA3_02"},
            {0x0064, "SA3_03"},
            {0x0065, "SA3_04"},
            {0x0066, "SA3_05"},
            {0x0067, "SA3_06"},
            {0x0068, "SA3_07"},
            {0x0069, "SA3_08"},
            {0x006a, "SA3_09"},
            {0x006b, "SA3_10"},
            {0x006c, "SA3_11"},
            {0x006d, "SA3_12"},
            {0x006e, "SA3_13"},
            {0x006f, "SA3_14"},
            {0x0070, "SA3_15"},
            {0x0071, "SA4_01"},
            {0x0072, "SA4_02"},
            {0x0073, "SA4_03"},
            {0x0074, "SA4_04"},
            {0x0075, "SA4_05"},
            {0x0076, "SA4_06"},
            {0x0077, "SA4_07"},
            {0x0078, "SA4_08"},
            {0x0079, "SA4_09"},
            {0x007a, "SA4_10"},
            {0x007b, "SA4_11"},
            {0x007c, "SA4_12"},
            {0x007d, "SA4_13"},
            {0x007e, "SA5_01"},
            {0x007f, "SA5_02"},
            {0x0080, "SA5_03"},
            {0x0081, "SA5_04"},
            {0x0082, "SA5_05"},
            {0x0083, "SA5_06"},
            {0x0084, "SA5_07"},
            {0x0085, "SA5_08"},
            {0x0086, "SA5_09"},
            {0x0087, "SA6_01"},
            {0x0088, "SA6_02"},
            {0x0089, "SA6_03"},
            {0x008a, "SA6_04"},
            {0x008b, "SA6_05"},
            {0x008c, "SA7_01"},
            {0x008d, "SA7_02"},
            {0x008e, "SA7_03"},
            {0x008f, "SA7_04"},
            {0x0090, "SA7_05"},
            {0x0091, "SA7_06"},
            {0x0092, "SA7_07"},
            {0x0093, "SA7_08"},
            {0x0094, "SA7_09"},
            {0x0095, "SAEP_01"},
            {0x0096, "SAEP_02"},
            {0x0097, "SAEP_03"},
            {0x0098, "SAEP_04"},
            {0x0099, "SAEP_05"},
            {0x009a, "SAEP_06"},
            {0x009b, "SHORTCUT"},
            {0x009c, "SHORTCUT01"},
            {0x009d, "SHORTCUT02"},
            {0x009e, "APPEND"},
            {0x009f, "STARTUP"},
            {0x00a0, "SCR_INIT"},
            {0x00a1, "SCR_SCUT"},
            {0x00a2, "DBG_TEST"},
            {0x00a3, "DBG_MENU"},
            {0x00a4, "DBG_CHR"},
            {0x00a5, "DBG_BG"},
            {0x00a6, "DBG_EV"},
            {0x00a7, "DBG_BGM"},
            {0x00a8, "DBG_SE"},
            {0x00a9, "DBG_MAC"},
            {0x00aa, "DBG_EFT"},
            {0x00ab, "SELF_A_101"},
            {0x00ac, "SELF_A_102"},
            {0x00ad, "SELF_A_103"},

            {0x1000, "BG_00"},
            {0x1001, "BG_01"},
            {0x1002, "BG_02"},
            {0x1003, "BG_03"},
            {0x1004, "BG_99"},
            {0x1005, "BG_A01A"},
            {0x1006, "BG_A01AR"},
            {0x1007, "BG_A01E"},
            {0x1008, "BG_A01N1"},
            {0x1009, "BG_A01N2"},
            {0x100a, "BG_A01NR1"},
            {0x100b, "BG_A01NR2"},
            {0x100c, "BG_A02A1"},
            {0x100d, "BG_A02A2"},
            {0x100e, "BG_A02A3"},
            {0x100f, "BG_A02A4"},
            {0x1010, "BG_A02AR1"},
            {0x1011, "BG_A02AR2"},
            {0x1012, "BG_A02AR3"},
            {0x1013, "BG_A02AR4"},
            {0x1014, "BG_A02E1"},
            {0x1015, "BG_A02E2"},
            {0x1016, "BG_A02N1"},
            {0x1017, "BG_A02N2"},
            {0x1018, "BG_A02N3"},
            {0x1019, "BG_A02N4"},
            {0x101a, "BG_A02E3"},
            {0x101b, "BG_A02E4"},
            {0x101c, "BG_A02N5"},
            {0x101d, "BG_A02N6"},
            {0x101e, "BG_A02N7"},
            {0x101f, "BG_A02N8"},
            {0x1020, "BG_A02NR1"},
            {0x1021, "BG_A02NR2"},
            {0x1022, "BG_A02NR3"},
            {0x1023, "BG_A02NR4"},
            {0x1024, "BG_A02NR5"},
            {0x1025, "BG_A02NR6"},
            {0x1026, "BG_A02NR7"},
            {0x1027, "BG_A02NR8"},
            {0x1028, "BG_A03A1"},
            {0x1029, "BG_A03A2"},
            {0x102a, "BG_A03A3"},
            {0x102b, "BG_A03A4"},
            {0x102c, "BG_A03AR1"},
            {0x102d, "BG_A03AR2"},
            {0x102e, "BG_A03AR3"},
            {0x102f, "BG_A03AR4"},
            {0x1030, "BG_A03E1"},
            {0x1031, "BG_A03E2"},
            {0x1032, "BG_A03E3"},
            {0x1033, "BG_A03E4"},
            {0x1034, "BG_A03N1"},
            {0x1035, "BG_A03N2"},
            {0x1036, "BG_A03N3"},
            {0x1037, "BG_A03N4"},
            {0x1038, "BG_A03N5"},
            {0x1039, "BG_A03N6"},
            {0x103a, "BG_A03N7"},
            {0x103b, "BG_A03N8"},
            {0x103c, "BG_A03NR1"},
            {0x103d, "BG_A03NR2"},
            {0x103e, "BG_A03NR3"},
            {0x103f, "BG_A03NR4"},
            {0x1040, "BG_A03NR5"},
            {0x1041, "BG_A03NR6"},
            {0x1042, "BG_A03NR7"},
            {0x1043, "BG_A03NR8"},
            {0x1044, "BG_A04A"},
            {0x1045, "BG_A04AR"},
            {0x1046, "BG_A04E"},
            {0x1047, "BG_A04N1"},
            {0x1048, "BG_A04N2"},
            {0x1049, "BG_A04NR1"},
            {0x104a, "BG_A04NR2"},
            {0x104b, "BG_A05A1"},
            {0x104c, "BG_A05A2"},
            {0x104d, "BG_A05A3"},
            {0x104e, "BG_A05A4"},
            {0x104f, "BG_A05AR1"},
            {0x1050, "BG_A05AR2"},
            {0x1051, "BG_A05AR3"},
            {0x1052, "BG_A05AR4"},
            {0x1053, "BG_A05E1"},
            {0x1054, "BG_A05E2"},
            {0x1055, "BG_A05E3"},
            {0x1056, "BG_A05E4"},
            {0x1057, "BG_A05N1"},
            {0x1058, "BG_A05N2"},
            {0x1059, "BG_A05N3"},
            {0x105a, "BG_A05N4"},
            {0x105b, "BG_A05N5"},
            {0x105c, "BG_A05N6"},
            {0x105d, "BG_A05N7"},
            {0x105e, "BG_A05N8"},
            {0x105f, "BG_A05NR1"},
            {0x1060, "BG_A05NR2"},
            {0x1061, "BG_A05NR3"},
            {0x1062, "BG_A05NR4"},
            {0x1063, "BG_A05NR5"},
            {0x1064, "BG_A05NR6"},
            {0x1065, "BG_A05NR7"},
            {0x1066, "BG_A05NR8"},
            {0x1067, "BG_A06A1"},
            {0x1068, "BG_A06A2"},
            {0x1069, "BG_A06A3"},
            {0x106a, "BG_A06A4"},
            {0x106b, "BG_A06AR1"},
            {0x106c, "BG_A06AR2"},
            {0x106d, "BG_A06AR3"},
            {0x106e, "BG_A06AR4"},
            {0x106f, "BG_A06E1"},
            {0x1070, "BG_A06E2"},
            {0x1071, "BG_A06E3"},
            {0x1072, "BG_A06E4"},
            {0x1073, "BG_A06N1"},
            {0x1074, "BG_A06N2"},
            {0x1075, "BG_A06N3"},
            {0x1076, "BG_A06N4"},
            {0x1077, "BG_A06N5"},
            {0x1078, "BG_A06N6"},
            {0x1079, "BG_A06N7"},
            {0x107a, "BG_A06N8"},
            {0x107b, "BG_A06NR1"},
            {0x107c, "BG_A06NR2"},
            {0x107d, "BG_A06NR3"},
            {0x107e, "BG_A06NR4"},
            {0x107f, "BG_A06NR5"},
            {0x1080, "BG_A06NR6"},
            {0x1081, "BG_A06NR7"},
            {0x1082, "BG_A06NR8"},
            {0x1083, "BG_A07A"},
            {0x1084, "BG_A07AR"},
            {0x1085, "BG_A07E"},
            {0x1086, "BG_A07N1"},
            {0x1087, "BG_A07N2"},
            {0x1088, "BG_A07NR1"},
            {0x1089, "BG_A07NR2"},
            {0x108a, "BG_A08A1"},
            {0x108b, "BG_A08A2"},
            {0x108c, "BG_A08A3"},
            {0x108d, "BG_A08A4"},
            {0x108e, "BG_A08AR1"},
            {0x108f, "BG_A08AR2"},
            {0x1090, "BG_A08AR3"},
            {0x1091, "BG_A08AR4"},
            {0x1092, "BG_A08E1"},
            {0x1093, "BG_A08E2"},
            {0x1094, "BG_A08E3"},
            {0x1095, "BG_A08E4"},
            {0x1096, "BG_A08N1"},
            {0x1097, "BG_A08N2"},
            {0x1098, "BG_A08N3"},
            {0x1099, "BG_A08N4"},
            {0x109a, "BG_A08N5"},
            {0x109b, "BG_A08N6"},
            {0x109c, "BG_A08N7"},
            {0x109d, "BG_A08N8"},
            {0x109e, "BG_A08NR1"},
            {0x109f, "BG_A08NR2"},
            {0x10a0, "BG_A08NR3"},
            {0x10a1, "BG_A08NR4"},
            {0x10a2, "BG_A08NR5"},
            {0x10a3, "BG_A08NR6"},
            {0x10a4, "BG_A08NR7"},
            {0x10a5, "BG_A08NR8"},
            {0x10a6, "BG_A09N1"},
            {0x10a7, "BG_A09N2"},
            {0x10a8, "BG_A09N3"},
            {0x10a9, "BG_A09N4"},
            {0x10aa, "BG_A09N5"},
            {0x10ab, "BG_A09N6"},
            {0x10ac, "BG_A09N7"},
            {0x10ad, "BG_A09N8"},
            {0x10ae, "BG_A10A"},
            {0x10af, "BG_A10AR"},
            {0x10b0, "BG_A10E"},
            {0x10b1, "BG_A10N"},
            {0x10b2, "BG_A10NR"},
            {0x10b3, "BG_A11A"},
            {0x10b4, "BG_A11AR"},
            {0x10b5, "BG_A11E"},
            {0x10b6, "BG_A11N"},
            {0x10b7, "BG_A11NR"},
            {0x10b8, "BG_A12N1"},
            {0x10b9, "BG_A12N2"},
            {0x10ba, "BG_A12N3"},
            {0x10bb, "BG_A12N4"},
            {0x10bc, "BG_A13A"},
            {0x10bd, "BG_A13AR"},
            {0x10be, "BG_A13E"},
            {0x10bf, "BG_A13N"},
            {0x10c0, "BG_A13NR"},
            {0x10c1, "BG_B01A1"},
            {0x10c2, "BG_B01A2"},
            {0x10c3, "BG_B01AR1"},
            {0x10c4, "BG_B01AR2"},
            {0x10c5, "BG_B01E1"},
            {0x10c6, "BG_B01E2"},
            {0x10c7, "BG_B01N1"},
            {0x10c8, "BG_B01N2"},
            {0x10c9, "BG_B01N3"},
            {0x10ca, "BG_B01N4"},
            {0x10cb, "BG_B01NR1"},
            {0x10cc, "BG_B01NR2"},
            {0x10cd, "BG_B01NR3"},
            {0x10ce, "BG_B01NR4"},
            {0x10cf, "BG_B02A1"},
            {0x10d0, "BG_B02A2"},
            {0x10d1, "BG_B02AR1"},
            {0x10d2, "BG_B02AR2"},
            {0x10d3, "BG_B02E1"},
            {0x10d4, "BG_B02E2"},
            {0x10d5, "BG_B02N1"},
            {0x10d6, "BG_B02N2"},
            {0x10d7, "BG_B02N3"},
            {0x10d8, "BG_B02N4"},
            {0x10d9, "BG_B03A1"},
            {0x10da, "BG_B03A2"},
            {0x10db, "BG_B03AR1"},
            {0x10dc, "BG_B03AR2"},
            {0x10dd, "BG_B03E1"},
            {0x10de, "BG_B03E2"},
            {0x10df, "BG_B03N1"},
            {0x10e0, "BG_B03N2"},
            {0x10e1, "BG_B03N3"},
            {0x10e2, "BG_B03N4"},
            {0x10e3, "BG_B04A1"},
            {0x10e4, "BG_B04A2"},
            {0x10e5, "BG_B04AR1"},
            {0x10e6, "BG_B04AR2"},
            {0x10e7, "BG_B04E1"},
            {0x10e8, "BG_B04E2"},
            {0x10e9, "BG_B04N1"},
            {0x10ea, "BG_B04N2"},
            {0x10eb, "BG_B04N3"},
            {0x10ec, "BG_B04N4"},
            {0x10ed, "BG_B05A1"},
            {0x10ee, "BG_B05A2"},
            {0x10ef, "BG_B05AR1"},
            {0x10f0, "BG_B05AR2"},
            {0x10f1, "BG_B05E1"},
            {0x10f2, "BG_B05E2"},
            {0x10f3, "BG_B05N1"},
            {0x10f4, "BG_B05N2"},
            {0x10f5, "BG_B05NR1"},
            {0x10f6, "BG_B05NR2"},
            {0x10f7, "BG_B06A"},
            {0x10f8, "BG_B06A2"},
            {0x10f9, "BG_B06N1"},
            {0x10fa, "BG_B06N2"},
            {0x10fb, "BG_B07N1"},
            {0x10fc, "BG_B07N2"},
            {0x10fd, "BG_B07N3"},
            {0x10fe, "BG_B07N4"},
            {0x10ff, "BG_B08A"},
            {0x1100, "BG_B08AR"},
            {0x1101, "BG_B08E"},
            {0x1102, "BG_B08N1"},
            {0x1103, "BG_B08N2"},
            {0x1104, "BG_B08NR1"},
            {0x1105, "BG_B08NR2"},
            {0x1106, "BG_B09A"},
            {0x1107, "BG_B09AR"},
            {0x1108, "BG_B09E"},
            {0x1109, "BG_B09N"},
            {0x110a, "BG_B09NR"},
            {0x110b, "BG_B10A"},
            {0x110c, "BG_B10AR"},
            {0x110d, "BG_B10E"},
            {0x110e, "BG_B10N"},
            {0x110f, "BG_B10NR"},
            {0x1110, "BG_B11A"},
            {0x1111, "BG_B11AR"},
            {0x1112, "BG_B11E"},
            {0x1113, "BG_B11N"},
            {0x1114, "BG_B11NR"},
            {0x1115, "BG_B12A"},
            {0x1116, "BG_B12AR"},
            {0x1117, "BG_B12E"},
            {0x1118, "BG_B12N"},
            {0x1119, "BG_B12NR"},
            {0x111a, "BG_B13A"},
            {0x111b, "BG_B13AR"},
            {0x111c, "BG_B13E"},
            {0x111d, "BG_B13N"},
            {0x111e, "BG_B13NR"},
            {0x111f, "BG_B14A"},
            {0x1120, "BG_B14AR"},
            {0x1121, "BG_B14E"},
            {0x1122, "BG_B14N"},
            {0x1123, "BG_B14NR"},
            {0x1124, "BG_B15A1"},
            {0x1125, "BG_B15A2"},
            {0x1126, "BG_B15AR1"},
            {0x1127, "BG_B15AR2"},
            {0x1128, "BG_B15E1"},
            {0x1129, "BG_B15E2"},
            {0x112a, "BG_B15N1"},
            {0x112b, "BG_B15N2"},
            {0x112c, "BG_B15NR1"},
            {0x112d, "BG_B15NR2"},
            {0x112e, "BG_B16A"},
            {0x112f, "BG_B16AR"},
            {0x1130, "BG_B16E"},
            {0x1131, "BG_B16N1"},
            {0x1132, "BG_B16N2"},
            {0x1133, "BG_B16NR1"},
            {0x1134, "BG_B16NR2"},
            {0x1135, "BG_B17A"},
            {0x1136, "BG_B17AR"},
            {0x1137, "BG_B17E"},
            {0x1138, "BG_B17N"},
            {0x1139, "BG_B17NR"},
            {0x113a, "BG_B18N1"},
            {0x113b, "BG_B18N2"},
            {0x113c, "BG_B18N3"},
            {0x113d, "BG_B19N1"},
            {0x113e, "BG_B19N2"},
            {0x113f, "BG_B19N3"},
            {0x1140, "BG_B19N4"},
            {0x1141, "BG_B20N1"},
            {0x1142, "BG_B20N2"},
            {0x1143, "BG_B21N1"},
            {0x1144, "BG_B21N2"},
            {0x1145, "BG_B22A"},
            {0x1146, "BG_B22AR"},
            {0x1147, "BG_B22E"},
            {0x1148, "BG_B22N"},
            {0x1149, "BG_B22NR"},
            {0x114a, "BG_B23A"},
            {0x114b, "BG_B23AR"},
            {0x114c, "BG_B23E"},
            {0x114d, "BG_B23N"},
            {0x114e, "BG_B23NR"},
            {0x114f, "BG_B24A1"},
            {0x1150, "BG_B24A2"},
            {0x1151, "BG_C01A"},
            {0x1152, "BG_C01AR"},
            {0x1153, "BG_C02A1"},
            {0x1154, "BG_C02A2"},
            {0x1155, "BG_C03N1"},
            {0x1156, "BG_C03N2"},
            {0x1157, "BG_C04A"},
            {0x1158, "IT01A"},
            {0x1159, "IT01B"},
            {0x115a, "IT02A"},
            {0x115b, "IT03A"},
            {0x115c, "IT04A"},
            {0x115d, "IT05A"},
            {0x115e, "IT06A"},
            {0x115f, "IT06B"},
            {0x1160, "IT07A"},
            {0x1161, "IT07B"},
            {0x1162, "IT08A"},
            {0x1163, "IT08B"},
            {0x1164, "IT08C"},
            {0x1165, "IT08D"},
            {0x1166, "IT09A"},
            {0x1167, "IT10A"},
            {0x1168, "IT11A"},
            {0x1169, "IT12A"},
            {0x116a, "IT13A"},
            {0x116b, "IT13B"},
            {0x116c, "IT14A"},
            {0x116d, "IT14B"},
            {0x116e, "IT15A"},
            {0x116f, "IT16A"},
            {0x1170, "IT17A"},
            {0x1171, "IT18A"},
            {0x1172, "IT19A"},
            {0x1173, "IT19B"},
            {0x1174, "IT19C"},
            {0x1175, "IT19D"},
            {0x1176, "IT20A"},
            {0x1177, "IT20B"},
            {0x1178, "IT21A"},
            {0x1179, "IT22A"},
            {0x117a, "IT23A"},
            {0x117b, "IT23B"},
            {0x117c, "IT24A"},
            {0x117d, "IT25A"},
            {0x117e, "IT26A"},
            {0x117f, "IT27A"},
            {0x1180, "IT28A"},
            {0x1181, "IT29A"},
            {0x1182, "EBG01A"},
            {0x1183, "EBG01AR1"},
            {0x1184, "EBG01E"},
            {0x1185, "EBG01N1"},
            {0x1186, "EBG01N2"},
            {0x1187, "EBG01NR1"},
            {0x1188, "EBG01NR2"},
            {0x1189, "EBG02A"},
            {0x118a, "EBG03A"},
            {0x118b, "EBG03B"},
            {0x118c, "EBG04A"},
            {0x118d, "EBG04B"},
            {0x118e, "EBG05A"},
            {0x118f, "EBG05AR1"},
            {0x1190, "EBG05AR2"},
            {0x1191, "EBG05E"},
            {0x1192, "EBG05N1"},
            {0x1193, "EBG05N2"},
            {0x1194, "EBG05NR1"},
            {0x1195, "EBG05NR2"},
            {0x1196, "EBG06A"},
            {0x1197, "EBG06B"},
            {0x1198, "EBG06C"},
            {0x1199, "EBG06D"},
            {0x119a, "EBG06E"},
            {0x119b, "EBG06F"},
            {0x119c, "EBG06G"},
            {0x119d, "EBG06H"},
            {0x119e, "EBG06I"},
            {0x119f, "EBG07A"},
            {0x11a0, "EBG08A"},
            {0x11a1, "EBG08B"},
            {0x11a2, "EBG09A"},
            {0x11a3, "EBG10A"},
            {0x11a4, "EBG11A"},
            {0x11a5, "EBG12A"},
            {0x11a6, "EBG13A"},
            {0x11a7, "EBG13B"},
            {0x11a8, "EBG13C"},
            {0x11a9, "EBG13D"},
            {0x11aa, "EBG15A"},
            {0x11ab, "EBG15B"},
            {0x11ac, "EBG15C"},
            {0x11ad, "EBG15D"},
            {0x11ae, "EBG16A"},
            {0x11af, "EBG17A"},
            {0x11b0, "EBG17B"},
            {0x11b1, "EBG17C"},
            {0x11b2, "EBG17D"},
            {0x11b3, "EBG18A"},
            {0x11b4, "EBG18B"},
            {0x11b5, "EBG19A"},
            {0x11b6, "EBG19B"},
            {0x11b7, "EBG19C"},
            {0x11b8, "EBG19D"},
            {0x11b9, "EBG20A"},
            {0x11ba, "EBG20B"},
            {0x11bb, "EBG20C"},
            {0x11bc, "EBG20D"},
            {0x11bd, "EBG20E"},
            {0x11be, "EBG20F"},
            {0x11bf, "EBG21A"},
            {0x11c0, "EBG21B"},
            {0x11c1, "EBG21C"},
            {0x11c2, "EBG21D"},
            {0x11c3, "EBG21E"},
            {0x11c4, "EBG21F"},
            {0x11c5, "EBG22A"},
            {0x11c6, "EBG22B1"},
            {0x11c7, "EBG22B2"},
            {0x11c8, "EBG22C"},
            {0x11c9, "EBG22D"},
            {0x11ca, "EBG22E"},
            {0x11cb, "EV_CO04A1"},
            {0x11cc, "EV_CO05A1"},
            {0x11cd, "EV_CO06A4"},
            {0x11ce, "CAL_A00"},
            {0x11cf, "CAL_A01"},
            {0x11d0, "CAL_A02"},
            {0x11d1, "CAL_A03"},
            {0x11d2, "CAL_A04"},
            {0x11d3, "CAL_A05"},
            {0x11d4, "CAL_A06"},
            {0x11d5, "CAL_A07"},
            {0x11d6, "CAL_B00"},
            {0x11d7, "CAL_B01"},
            {0x11d8, "CAL_B02"},
            {0x11d9, "CAL_B03"},
            {0x11da, "CAL_B04"},
            {0x11db, "CAL_B05"},
            {0x11dc, "CAL_B06"},
            {0x11dd, "CAL_B07"},
            {0x11de, "BEV_CO30A"},
            {0x11df, "BEV_CO33A"},
            {0x11e0, "BEV_SA02A"},
            {0x11e1, "BEV_SA24A"},
            {0x11e2, "BPOST01"},
            {0x11e3, "BPOST02"},
            {0x11e4, "BPOST03"},
            {0x11e5, "BPOST04"},
            {0x11e6, "BPOST05"},

            {0x2000, "EV_PR01A"},
            {0x2001, "EV_PR02A"},
            {0x2002, "EV_PR03A"},
            {0x2003, "EV_CO01A"},
            {0x2004, "EV_CO01B"},
            {0x2005, "EV_CO01C"},
            {0x2006, "EV_CO02A"},
            {0x2007, "EV_CO02B"},
            {0x2008, "EV_CO02C"},
            {0x2009, "EV_CO02D"},
            {0x200a, "EV_CO03A"},
            {0x200b, "EV_CO03B"},
            {0x200c, "EV_CO03C"},
            {0x200d, "EV_CO04A"},
            {0x200e, "EV_CO05A"},
            {0x200f, "EV_CO05B"},
            {0x2010, "EV_CO05C"},
            {0x2011, "EV_CO06A"},
            {0x2012, "EV_CO06B"},
            {0x2013, "EV_CO07A"},
            {0x2014, "EV_CO07B"},
            {0x2015, "EV_CO08A"},
            {0x2016, "EV_CO09A"},
            {0x2017, "EV_CO10A"},
            {0x2018, "EV_CO10B"},
            {0x2019, "EV_CO11A"},
            {0x201a, "EV_CO12A"},
            {0x201b, "EV_CO13A"},
            {0x201c, "EV_CO13B"},
            {0x201d, "EV_CO13C"},
            {0x201e, "EV_CO14A"},
            {0x201f, "EV_CO14B"},
            {0x2020, "EV_CO15A"},
            {0x2021, "EV_CO15B"},
            {0x2022, "EV_CO16A"},
            {0x2023, "EV_CO17A"},
            {0x2024, "EV_CO18A"},
            {0x2025, "EV_CO19A"},
            {0x2026, "EV_CO20A"},
            {0x2027, "EV_CO21A"},
            {0x2028, "EV_CO22A"},
            {0x2029, "EV_CO23A"},
            {0x202a, "EV_CO24A"},
            {0x202b, "EV_CO25A"},
            {0x202c, "EV_CO25B"},
            {0x202d, "EV_CO26A"},
            {0x202e, "EV_CO27A"},
            {0x202f, "EV_CO27B"},
            {0x2030, "EV_CO28A"},
            {0x2031, "EV_CO29A"},
            {0x2032, "EV_CO29B"},
            {0x2033, "EV_CO30A"},
            {0x2034, "EV_CO30B"},
            {0x2035, "EV_CO30C"},
            {0x2036, "EV_CO30D"},
            {0x2037, "EV_CO31A"},
            {0x2038, "EV_CO32A"},
            {0x2039, "EV_CO32B"},
            {0x203a, "EV_CO32C"},
            {0x203b, "EV_CO33A"},
            {0x203c, "EV_CO33B"},
            {0x203d, "EV_CO34A"},
            {0x203e, "EV_CO35A"},
            {0x203f, "EV_CO36A"},
            {0x2040, "EV_SA01A"},
            {0x2041, "EV_SA02A"},
            {0x2042, "EV_SA02B"},
            {0x2043, "EV_SA02C"},
            {0x2044, "EV_SA02D"},
            {0x2045, "EV_SA03A"},
            {0x2046, "EV_SA03B"},
            {0x2047, "EV_SA04A"},
            {0x2048, "EV_SA05A"},
            {0x2049, "EV_SA06A"},
            {0x204a, "EV_SA06B"},
            {0x204b, "EV_SA06C"},
            {0x204c, "EV_SA06D"},
            {0x204d, "EV_SA07A"},
            {0x204e, "EV_SA08A"},
            {0x204f, "EV_SA09A"},
            {0x2050, "EV_SA10A"},
            {0x2051, "EV_SA11A"},
            {0x2052, "EV_SA12A"},
            {0x2053, "EV_SA13A"},
            {0x2054, "EV_SA13B"},
            {0x2055, "EV_SA14A"},
            {0x2056, "EV_SA14B"},
            {0x2057, "EV_SA15A"},
            {0x2058, "EV_SA15B"},
            {0x2059, "EV_SA16A"},
            {0x205a, "EV_SA16B"},
            {0x205b, "EV_SA17A"},
            {0x205c, "EV_SA18A"},
            {0x205d, "EV_SA18B"},
            {0x205e, "EV_SA19A"},
            {0x205f, "EV_SA20A"},
            {0x2060, "EV_SA21A"},
            {0x2061, "EV_SA22A"},
            {0x2062, "EV_SA23A"},
            {0x2063, "EV_SA24A"},
            {0x2064, "EV_SA24B"},
            {0x2065, "POST01"},
            {0x2066, "POST02"},
            {0x2067, "POST03"},
            {0x2068, "POST04"},
            {0x2069, "POST05"},
            {0x206a, "REME01"},
            {0x206b, "REME02"},
            {0x206c, "REME03"},
            {0x206d, "REME04"},
            {0x206e, "REME05"},
            {0x206f, "REME06"},
            {0x2070, "REME07"},
            {0x2071, "REME08"},
            {0x2072, "REME09"},
            {0x2073, "REME10"},
            {0x2074, "REME11"},
            {0x2075, "REME12"},
            {0x2076, "REME13"},
            {0x2077, "REME14"},
            {0x2078, "REME15"},
            {0x2079, "REME16"},
            {0x207a, "REME17"},
            {0x207b, "REME18"},
            {0x207c, "REME19"},
            {0x207d, "REME20"},
            {0x207e, "REME21"},
            {0x207f, "REME22"},
            {0x2080, "REME23"},
            {0x2081, "REME24"},
            {0x2082, "REME25"},
            {0x2083, "REME26"},
            {0x2084, "REME27"},
            {0x2085, "REME28"},
            {0x2086, "REME29"},
            {0x2087, "REME30"},
            {0x2088, "REME31"},
            {0x2089, "REME32"},
            {0x208a, "REME33"},
            {0x208b, "REME34"},
            {0x208c, "REME35"},
            {0x208d, "EV_MV00"},
            {0x208e, "EV_MV01"},
            {0x208f, "EV_MV02"},
            {0x2090, "EV_MV03"},
            {0x2091, "EV_MV04"},
            {0x2092, "EV_MV05"},
            {0x2093, "EV_MV06"},
            {0x2094, "EV_MV08"},
            {0x2095, "EV_MV09"},
            {0x2096, "EV_MV12"},
            {0x2097, "EV_MV13"},
            {0x2098, "EV_MV14"},
            {0x2099, "EV_MV15"},
            {0x209a, "EV_MVED1"},
            {0x209b, "EV_MVED2"},
            {0x209c, "EV_MVED3"},
            {0x209d, "EV_MVOP"},
            {0x209e, "BG_C05N"},

            {0x3000, "CHR_99"},
            {0x3001, "CO01AS"},
            {0x3002, "CO02AS"},
            {0x3003, "CO03AS"},
            {0x3004, "CO04AS"},
            {0x3005, "CO05AS"},
            {0x3006, "CO06AS"},
            {0x3007, "CO07AS"},
            {0x3008, "CO08AS"},
            {0x3009, "CO09AS"},
            {0x300a, "CO10AS"},
            {0x300b, "CO11AS"},
            {0x300c, "CO01AM"},
            {0x300d, "CO02AM"},
            {0x300e, "CO03AM"},
            {0x300f, "CO04AM"},
            {0x3010, "CO05AM"},
            {0x3011, "CO06AM"},
            {0x3012, "CO07AM"},
            {0x3013, "CO08AM"},
            {0x3014, "CO09AM"},
            {0x3015, "CO10AM"},
            {0x3016, "CO11AM"},
            {0x3017, "CO01AL"},
            {0x3018, "CO02AL"},
            {0x3019, "CO03AL"},
            {0x301a, "CO04AL"},
            {0x301b, "CO05AL"},
            {0x301c, "CO06AL"},
            {0x301d, "CO07AL"},
            {0x301e, "CO08AL"},
            {0x301f, "CO09AL"},
            {0x3020, "CO10AL"},
            {0x3021, "CO11AL"},
            {0x3022, "CO01AX"},
            {0x3023, "CO02AX"},
            {0x3024, "CO03AX"},
            {0x3025, "CO04AX"},
            {0x3026, "CO05AX"},
            {0x3027, "CO06AX"},
            {0x3028, "CO07AX"},
            {0x3029, "CO08AX"},
            {0x302a, "CO09AX"},
            {0x302b, "CO10AX"},
            {0x302c, "CO11AX"},
            {0x302d, "CO01BS"},
            {0x302e, "CO02BS"},
            {0x302f, "CO03BS"},
            {0x3030, "CO04BS"},
            {0x3031, "CO05BS"},
            {0x3032, "CO06BS"},
            {0x3033, "CO07BS"},
            {0x3034, "CO08BS"},
            {0x3035, "CO09BS"},
            {0x3036, "CO10BS"},
            {0x3037, "CO11BS"},
            {0x3038, "CO01BM"},
            {0x3039, "CO02BM"},
            {0x303a, "CO03BM"},
            {0x303b, "CO04BM"},
            {0x303c, "CO05BM"},
            {0x303d, "CO06BM"},
            {0x303e, "CO07BM"},
            {0x303f, "CO08BM"},
            {0x3040, "CO09BM"},
            {0x3041, "CO10BM"},
            {0x3042, "CO11BM"},
            {0x3043, "CO01BL"},
            {0x3044, "CO02BL"},
            {0x3045, "CO03BL"},
            {0x3046, "CO04BL"},
            {0x3047, "CO05BL"},
            {0x3048, "CO06BL"},
            {0x3049, "CO07BL"},
            {0x304a, "CO08BL"},
            {0x304b, "CO09BL"},
            {0x304c, "CO10BL"},
            {0x304d, "CO11BL"},
            {0x304e, "CO01BX"},
            {0x304f, "CO02BX"},
            {0x3050, "CO03BX"},
            {0x3051, "CO04BX"},
            {0x3052, "CO05BX"},
            {0x3053, "CO06BX"},
            {0x3054, "CO07BX"},
            {0x3055, "CO08BX"},
            {0x3056, "CO09BX"},
            {0x3057, "CO10BX"},
            {0x3058, "CO11BX"},
            {0x3059, "SA01AS"},
            {0x305a, "SA02AS"},
            {0x305b, "SA03AS"},
            {0x305c, "SA04AS"},
            {0x305d, "SA05AS"},
            {0x305e, "SA06AS"},
            {0x305f, "SA07AS"},
            {0x3060, "SA08AS"},
            {0x3061, "SA09AS"},
            {0x3062, "SA10AS"},
            {0x3063, "SA11AS"},
            {0x3064, "SA01AM"},
            {0x3065, "SA02AM"},
            {0x3066, "SA03AM"},
            {0x3067, "SA04AM"},
            {0x3068, "SA05AM"},
            {0x3069, "SA06AM"},
            {0x306a, "SA07AM"},
            {0x306b, "SA08AM"},
            {0x306c, "SA09AM"},
            {0x306d, "SA10AM"},
            {0x306e, "SA11AM"},
            {0x306f, "SA01AL"},
            {0x3070, "SA02AL"},
            {0x3071, "SA03AL"},
            {0x3072, "SA04AL"},
            {0x3073, "SA05AL"},
            {0x3074, "SA06AL"},
            {0x3075, "SA07AL"},
            {0x3076, "SA08AL"},
            {0x3077, "SA09AL"},
            {0x3078, "SA10AL"},
            {0x3079, "SA11AL"},
            {0x307a, "SA01AX"},
            {0x307b, "SA02AX"},
            {0x307c, "SA03AX"},
            {0x307d, "SA04AX"},
            {0x307e, "SA05AX"},
            {0x307f, "SA06AX"},
            {0x3080, "SA07AX"},
            {0x3081, "SA08AX"},
            {0x3082, "SA09AX"},
            {0x3083, "SA10AX"},
            {0x3084, "SA11AX"},
            {0x3085, "SA01BS"},
            {0x3086, "SA02BS"},
            {0x3087, "SA03BS"},
            {0x3088, "SA04BS"},
            {0x3089, "SA05BS"},
            {0x308a, "SA06BS"},
            {0x308b, "SA07BS"},
            {0x308c, "SA08BS"},
            {0x308d, "SA09BS"},
            {0x308e, "SA10BS"},
            {0x308f, "SA11BS"},
            {0x3090, "SA01BM"},
            {0x3091, "SA02BM"},
            {0x3092, "SA03BM"},
            {0x3093, "SA04BM"},
            {0x3094, "SA05BM"},
            {0x3095, "SA06BM"},
            {0x3096, "SA07BM"},
            {0x3097, "SA08BM"},
            {0x3098, "SA09BM"},
            {0x3099, "SA10BM"},
            {0x309a, "SA11BM"},
            {0x309b, "SA01BL"},
            {0x309c, "SA02BL"},
            {0x309d, "SA03BL"},
            {0x309e, "SA04BL"},
            {0x309f, "SA05BL"},
            {0x30a0, "SA06BL"},
            {0x30a1, "SA07BL"},
            {0x30a2, "SA08BL"},
            {0x30a3, "SA09BL"},
            {0x30a4, "SA10BL"},
            {0x30a5, "SA11BL"},
            {0x30a6, "SA01BX"},
            {0x30a7, "SA02BX"},
            {0x30a8, "SA03BX"},
            {0x30a9, "SA04BX"},
            {0x30aa, "SA05BX"},
            {0x30ab, "SA06BX"},
            {0x30ac, "SA07BX"},
            {0x30ad, "SA08BX"},
            {0x30ae, "SA09BX"},
            {0x30af, "SA10BX"},
            {0x30b0, "SA11BX"},
            {0x30b1, "SA01CS"},
            {0x30b2, "SA02CS"},
            {0x30b3, "SA03CS"},
            {0x30b4, "SA04CS"},
            {0x30b5, "SA05CS"},
            {0x30b6, "SA06CS"},
            {0x30b7, "SA07CS"},
            {0x30b8, "SA08CS"},
            {0x30b9, "SA09CS"},
            {0x30ba, "SA10CS"},
            {0x30bb, "SA11CS"},
            {0x30bc, "SA01CM"},
            {0x30bd, "SA02CM"},
            {0x30be, "SA03CM"},
            {0x30bf, "SA04CM"},
            {0x30c0, "SA05CM"},
            {0x30c1, "SA06CM"},
            {0x30c2, "SA07CM"},
            {0x30c3, "SA08CM"},
            {0x30c4, "SA09CM"},
            {0x30c5, "SA10CM"},
            {0x30c6, "SA11CM"},
            {0x30c7, "SA01CL"},
            {0x30c8, "SA02CL"},
            {0x30c9, "SA03CL"},
            {0x30ca, "SA04CL"},
            {0x30cb, "SA05CL"},
            {0x30cc, "SA06CL"},
            {0x30cd, "SA07CL"},
            {0x30ce, "SA08CL"},
            {0x30cf, "SA09CL"},
            {0x30d0, "SA10CL"},
            {0x30d1, "SA11CL"},
            {0x30d2, "SA01CX"},
            {0x30d3, "SA02CX"},
            {0x30d4, "SA03CX"},
            {0x30d5, "SA04CX"},
            {0x30d6, "SA05CX"},
            {0x30d7, "SA06CX"},
            {0x30d8, "SA07CX"},
            {0x30d9, "SA08CX"},
            {0x30da, "SA09CX"},
            {0x30db, "SA10CX"},
            {0x30dc, "SA11CX"},
            {0x30dd, "MA01AS"},
            {0x30de, "MA02AS"},
            {0x30df, "MA03AS"},
            {0x30e0, "MA04AS"},
            {0x30e1, "MA05AS"},
            {0x30e2, "MA06AS"},
            {0x30e3, "MA07AS"},
            {0x30e4, "MA08AS"},
            {0x30e5, "MA09AS"},
            {0x30e6, "MA10AS"},
            {0x30e7, "MA11AS"},
            {0x30e8, "MA01AM"},
            {0x30e9, "MA02AM"},
            {0x30ea, "MA03AM"},
            {0x30eb, "MA04AM"},
            {0x30ec, "MA05AM"},
            {0x30ed, "MA06AM"},
            {0x30ee, "MA07AM"},
            {0x30ef, "MA08AM"},
            {0x30f0, "MA09AM"},
            {0x30f1, "MA10AM"},
            {0x30f2, "MA11AM"},
            {0x30f3, "MA01AL"},
            {0x30f4, "MA02AL"},
            {0x30f5, "MA03AL"},
            {0x30f6, "MA04AL"},
            {0x30f7, "MA05AL"},
            {0x30f8, "MA06AL"},
            {0x30f9, "MA07AL"},
            {0x30fa, "MA08AL"},
            {0x30fb, "MA09AL"},
            {0x30fc, "MA10AL"},
            {0x30fd, "MA11AL"},
            {0x30fe, "MA01AX"},
            {0x30ff, "MA02AX"},
            {0x3100, "MA03AX"},
            {0x3101, "MA04AX"},
            {0x3102, "MA05AX"},
            {0x3103, "MA06AX"},
            {0x3104, "MA07AX"},
            {0x3105, "MA08AX"},
            {0x3106, "MA09AX"},
            {0x3107, "MA10AX"},
            {0x3108, "MA11AX"},
            {0x3109, "MA01BS"},
            {0x310a, "MA02BS"},
            {0x310b, "MA03BS"},
            {0x310c, "MA04BS"},
            {0x310d, "MA05BS"},
            {0x310e, "MA06BS"},
            {0x310f, "MA07BS"},
            {0x3110, "MA08BS"},
            {0x3111, "MA09BS"},
            {0x3112, "MA10BS"},
            {0x3113, "MA11BS"},
            {0x3114, "MA01BM"},
            {0x3115, "MA02BM"},
            {0x3116, "MA03BM"},
            {0x3117, "MA04BM"},
            {0x3118, "MA05BM"},
            {0x3119, "MA06BM"},
            {0x311a, "MA07BM"},
            {0x311b, "MA08BM"},
            {0x311c, "MA09BM"},
            {0x311d, "MA10BM"},
            {0x311e, "MA11BM"},
            {0x311f, "MA01BL"},
            {0x3120, "MA02BL"},
            {0x3121, "MA03BL"},
            {0x3122, "MA04BL"},
            {0x3123, "MA05BL"},
            {0x3124, "MA06BL"},
            {0x3125, "MA07BL"},
            {0x3126, "MA08BL"},
            {0x3127, "MA09BL"},
            {0x3128, "MA10BL"},
            {0x3129, "MA11BL"},
            {0x312a, "MA01BX"},
            {0x312b, "MA02BX"},
            {0x312c, "MA03BX"},
            {0x312d, "MA04BX"},
            {0x312e, "MA05BX"},
            {0x312f, "MA06BX"},
            {0x3130, "MA07BX"},
            {0x3131, "MA08BX"},
            {0x3132, "MA09BX"},
            {0x3133, "MA10BX"},
            {0x3134, "MA11BX"},
            {0x3135, "YO01AS"},
            {0x3136, "YO02AS"},
            {0x3137, "YO03AS"},
            {0x3138, "YO04AS"},
            {0x3139, "YO05AS"},
            {0x313a, "YO06AS"},
            {0x313b, "YO07AS"},
            {0x313c, "YO08AS"},
            {0x313d, "YO09AS"},
            {0x313e, "YO10AS"},
            {0x313f, "YO11AS"},
            {0x3140, "YO01AM"},
            {0x3141, "YO02AM"},
            {0x3142, "YO03AM"},
            {0x3143, "YO04AM"},
            {0x3144, "YO05AM"},
            {0x3145, "YO06AM"},
            {0x3146, "YO07AM"},
            {0x3147, "YO08AM"},
            {0x3148, "YO09AM"},
            {0x3149, "YO10AM"},
            {0x314a, "YO11AM"},
            {0x314b, "YO01AL"},
            {0x314c, "YO02AL"},
            {0x314d, "YO03AL"},
            {0x314e, "YO04AL"},
            {0x314f, "YO05AL"},
            {0x3150, "YO06AL"},
            {0x3151, "YO07AL"},
            {0x3152, "YO08AL"},
            {0x3153, "YO09AL"},
            {0x3154, "YO10AL"},
            {0x3155, "YO11AL"},
            {0x3156, "YO01AX"},
            {0x3157, "YO02AX"},
            {0x3158, "YO03AX"},
            {0x3159, "YO04AX"},
            {0x315a, "YO05AX"},
            {0x315b, "YO06AX"},
            {0x315c, "YO07AX"},
            {0x315d, "YO08AX"},
            {0x315e, "YO09AX"},
            {0x315f, "YO10AX"},
            {0x3160, "YO11AX"},
            {0x3161, "YO01BS"},
            {0x3162, "YO02BS"},
            {0x3163, "YO03BS"},
            {0x3164, "YO04BS"},
            {0x3165, "YO05BS"},
            {0x3166, "YO06BS"},
            {0x3167, "YO07BS"},
            {0x3168, "YO08BS"},
            {0x3169, "YO09BS"},
            {0x316a, "YO10BS"},
            {0x316b, "YO11BS"},
            {0x316c, "YO01BM"},
            {0x316d, "YO02BM"},
            {0x316e, "YO03BM"},
            {0x316f, "YO04BM"},
            {0x3170, "YO05BM"},
            {0x3171, "YO06BM"},
            {0x3172, "YO07BM"},
            {0x3173, "YO08BM"},
            {0x3174, "YO09BM"},
            {0x3175, "YO10BM"},
            {0x3176, "YO11BM"},
            {0x3177, "YO01BL"},
            {0x3178, "YO02BL"},
            {0x3179, "YO03BL"},
            {0x317a, "YO04BL"},
            {0x317b, "YO05BL"},
            {0x317c, "YO06BL"},
            {0x317d, "YO07BL"},
            {0x317e, "YO08BL"},
            {0x317f, "YO09BL"},
            {0x3180, "YO10BL"},
            {0x3181, "YO11BL"},
            {0x3182, "YO01BX"},
            {0x3183, "YO02BX"},
            {0x3184, "YO03BX"},
            {0x3185, "YO04BX"},
            {0x3186, "YO05BX"},
            {0x3187, "YO06BX"},
            {0x3188, "YO07BX"},
            {0x3189, "YO08BX"},
            {0x318a, "YO09BX"},
            {0x318b, "YO10BX"},
            {0x318c, "YO11BX"},
            {0x318d, "KA01AS"},
            {0x318e, "KA02AS"},
            {0x318f, "KA03AS"},
            {0x3190, "KA04AS"},
            {0x3191, "KA05AS"},
            {0x3192, "KA06AS"},
            {0x3193, "KA07AS"},
            {0x3194, "KA08AS"},
            {0x3195, "KA09AS"},
            {0x3196, "KA10AS"},
            {0x3197, "KA11AS"},
            {0x3198, "KA01AM"},
            {0x3199, "KA02AM"},
            {0x319a, "KA03AM"},
            {0x319b, "KA04AM"},
            {0x319c, "KA05AM"},
            {0x319d, "KA06AM"},
            {0x319e, "KA07AM"},
            {0x319f, "KA08AM"},
            {0x31a0, "KA09AM"},
            {0x31a1, "KA10AM"},
            {0x31a2, "KA11AM"},
            {0x31a3, "KA01AL"},
            {0x31a4, "KA02AL"},
            {0x31a5, "KA03AL"},
            {0x31a6, "KA04AL"},
            {0x31a7, "KA05AL"},
            {0x31a8, "KA06AL"},
            {0x31a9, "KA07AL"},
            {0x31aa, "KA08AL"},
            {0x31ab, "KA09AL"},
            {0x31ac, "KA10AL"},
            {0x31ad, "KA11AL"},
            {0x31ae, "KA01AX"},
            {0x31af, "KA02AX"},
            {0x31b0, "KA03AX"},
            {0x31b1, "KA04AX"},
            {0x31b2, "KA05AX"},
            {0x31b3, "KA06AX"},
            {0x31b4, "KA07AX"},
            {0x31b5, "KA08AX"},
            {0x31b6, "KA09AX"},
            {0x31b7, "KA10AX"},
            {0x31b8, "KA11AX"},
            {0x31b9, "KA01BS"},
            {0x31ba, "KA02BS"},
            {0x31bb, "KA03BS"},
            {0x31bc, "KA04BS"},
            {0x31bd, "KA05BS"},
            {0x31be, "KA06BS"},
            {0x31bf, "KA07BS"},
            {0x31c0, "KA08BS"},
            {0x31c1, "KA09BS"},
            {0x31c2, "KA10BS"},
            {0x31c3, "KA11BS"},
            {0x31c4, "KA01BM"},
            {0x31c5, "KA02BM"},
            {0x31c6, "KA03BM"},
            {0x31c7, "KA04BM"},
            {0x31c8, "KA05BM"},
            {0x31c9, "KA06BM"},
            {0x31ca, "KA07BM"},
            {0x31cb, "KA08BM"},
            {0x31cc, "KA09BM"},
            {0x31cd, "KA10BM"},
            {0x31ce, "KA11BM"},
            {0x31cf, "KA01BL"},
            {0x31d0, "KA02BL"},
            {0x31d1, "KA03BL"},
            {0x31d2, "KA04BL"},
            {0x31d3, "KA05BL"},
            {0x31d4, "KA06BL"},
            {0x31d5, "KA07BL"},
            {0x31d6, "KA08BL"},
            {0x31d7, "KA09BL"},
            {0x31d8, "KA10BL"},
            {0x31d9, "KA11BL"},
            {0x31da, "KA01BX"},
            {0x31db, "KA02BX"},
            {0x31dc, "KA03BX"},
            {0x31dd, "KA04BX"},
            {0x31de, "KA05BX"},
            {0x31df, "KA06BX"},
            {0x31e0, "KA07BX"},
            {0x31e1, "KA08BX"},
            {0x31e2, "KA09BX"},
            {0x31e3, "KA10BX"},
            {0x31e4, "KA11BX"},
            {0x31e5, "KA01CS"},
            {0x31e6, "KA02CS"},
            {0x31e7, "KA03CS"},
            {0x31e8, "KA04CS"},
            {0x31e9, "KA05CS"},
            {0x31ea, "KA06CS"},
            {0x31eb, "KA07CS"},
            {0x31ec, "KA08CS"},
            {0x31ed, "KA09CS"},
            {0x31ee, "KA10CS"},
            {0x31ef, "KA11CS"},
            {0x31f0, "KA01CM"},
            {0x31f1, "KA02CM"},
            {0x31f2, "KA03CM"},
            {0x31f3, "KA04CM"},
            {0x31f4, "KA05CM"},
            {0x31f5, "KA06CM"},
            {0x31f6, "KA07CM"},
            {0x31f7, "KA08CM"},
            {0x31f8, "KA09CM"},
            {0x31f9, "KA10CM"},
            {0x31fa, "KA11CM"},
            {0x31fb, "KA01CL"},
            {0x31fc, "KA02CL"},
            {0x31fd, "KA03CL"},
            {0x31fe, "KA04CL"},
            {0x31ff, "KA05CL"},
            {0x3200, "KA06CL"},
            {0x3201, "KA07CL"},
            {0x3202, "KA08CL"},
            {0x3203, "KA09CL"},
            {0x3204, "KA10CL"},
            {0x3205, "KA11CL"},
            {0x3206, "KA01CX"},
            {0x3207, "KA02CX"},
            {0x3208, "KA03CX"},
            {0x3209, "KA04CX"},
            {0x320a, "KA05CX"},
            {0x320b, "KA06CX"},
            {0x320c, "KA07CX"},
            {0x320d, "KA08CX"},
            {0x320e, "KA09CX"},
            {0x320f, "KA10CX"},
            {0x3210, "KA11CX"},
            {0x3211, "IN01AS"},
            {0x3212, "IN02AS"},
            {0x3213, "IN03AS"},
            {0x3214, "IN04AS"},
            {0x3215, "IN05AS"},
            {0x3216, "IN06AS"},
            {0x3217, "IN07AS"},
            {0x3218, "IN08AS"},
            {0x3219, "IN09AS"},
            {0x321a, "IN10AS"},
            {0x321b, "IN11AS"},
            {0x321c, "IN01AM"},
            {0x321d, "IN02AM"},
            {0x321e, "IN03AM"},
            {0x321f, "IN04AM"},
            {0x3220, "IN05AM"},
            {0x3221, "IN06AM"},
            {0x3222, "IN07AM"},
            {0x3223, "IN08AM"},
            {0x3224, "IN09AM"},
            {0x3225, "IN10AM"},
            {0x3226, "IN11AM"},
            {0x3227, "IN01AL"},
            {0x3228, "IN02AL"},
            {0x3229, "IN03AL"},
            {0x322a, "IN04AL"},
            {0x322b, "IN05AL"},
            {0x322c, "IN06AL"},
            {0x322d, "IN07AL"},
            {0x322e, "IN08AL"},
            {0x322f, "IN09AL"},
            {0x3230, "IN10AL"},
            {0x3231, "IN11AL"},
            {0x3232, "IN01AX"},
            {0x3233, "IN02AX"},
            {0x3234, "IN03AX"},
            {0x3235, "IN04AX"},
            {0x3236, "IN05AX"},
            {0x3237, "IN06AX"},
            {0x3238, "IN07AX"},
            {0x3239, "IN08AX"},
            {0x323a, "IN09AX"},
            {0x323b, "IN10AX"},
            {0x323c, "IN11AX"},
            {0x323d, "IN01BS"},
            {0x323e, "IN02BS"},
            {0x323f, "IN03BS"},
            {0x3240, "IN04BS"},
            {0x3241, "IN05BS"},
            {0x3242, "IN06BS"},
            {0x3243, "IN07BS"},
            {0x3244, "IN08BS"},
            {0x3245, "IN09BS"},
            {0x3246, "IN10BS"},
            {0x3247, "IN11BS"},
            {0x3248, "IN01BM"},
            {0x3249, "IN02BM"},
            {0x324a, "IN03BM"},
            {0x324b, "IN04BM"},
            {0x324c, "IN05BM"},
            {0x324d, "IN06BM"},
            {0x324e, "IN07BM"},
            {0x324f, "IN08BM"},
            {0x3250, "IN09BM"},
            {0x3251, "IN10BM"},
            {0x3252, "IN11BM"},
            {0x3253, "IN01BL"},
            {0x3254, "IN02BL"},
            {0x3255, "IN03BL"},
            {0x3256, "IN04BL"},
            {0x3257, "IN05BL"},
            {0x3258, "IN06BL"},
            {0x3259, "IN07BL"},
            {0x325a, "IN08BL"},
            {0x325b, "IN09BL"},
            {0x325c, "IN10BL"},
            {0x325d, "IN11BL"},
            {0x325e, "IN01BX"},
            {0x325f, "IN02BX"},
            {0x3260, "IN03BX"},
            {0x3261, "IN04BX"},
            {0x3262, "IN05BX"},
            {0x3263, "IN06BX"},
            {0x3264, "IN07BX"},
            {0x3265, "IN08BX"},
            {0x3266, "IN09BX"},
            {0x3267, "IN10BX"},
            {0x3268, "IN11BX"},
            {0x3269, "IN01CS"},
            {0x326a, "IN02CS"},
            {0x326b, "IN03CS"},
            {0x326c, "IN04CS"},
            {0x326d, "IN05CS"},
            {0x326e, "IN06CS"},
            {0x326f, "IN07CS"},
            {0x3270, "IN08CS"},
            {0x3271, "IN09CS"},
            {0x3272, "IN10CS"},
            {0x3273, "IN11CS"},
            {0x3274, "IN01CM"},
            {0x3275, "IN02CM"},
            {0x3276, "IN03CM"},
            {0x3277, "IN04CM"},
            {0x3278, "IN05CM"},
            {0x3279, "IN06CM"},
            {0x327a, "IN07CM"},
            {0x327b, "IN08CM"},
            {0x327c, "IN09CM"},
            {0x327d, "IN10CM"},
            {0x327e, "IN11CM"},
            {0x327f, "IN01CL"},
            {0x3280, "IN02CL"},
            {0x3281, "IN03CL"},
            {0x3282, "IN04CL"},
            {0x3283, "IN05CL"},
            {0x3284, "IN06CL"},
            {0x3285, "IN07CL"},
            {0x3286, "IN08CL"},
            {0x3287, "IN09CL"},
            {0x3288, "IN10CL"},
            {0x3289, "IN11CL"},
            {0x328a, "IN01CX"},
            {0x328b, "IN02CX"},
            {0x328c, "IN03CX"},
            {0x328d, "IN04CX"},
            {0x328e, "IN05CX"},
            {0x328f, "IN06CX"},
            {0x3290, "IN07CX"},
            {0x3291, "IN08CX"},
            {0x3292, "IN09CX"},
            {0x3293, "IN10CX"},
            {0x3294, "IN11CX"},
            {0x3295, "UN01AS"},
            {0x3296, "UN02AS"},
            {0x3297, "UN03AS"},
            {0x3298, "UN04AS"},
            {0x3299, "UN05AS"},
            {0x329a, "UN06AS"},
            {0x329b, "UN07AS"},
            {0x329c, "UN08AS"},
            {0x329d, "UN09AS"},
            {0x329e, "UN10AS"},
            {0x329f, "UN11AS"},
            {0x32a0, "UN01AM"},
            {0x32a1, "UN02AM"},
            {0x32a2, "UN03AM"},
            {0x32a3, "UN04AM"},
            {0x32a4, "UN05AM"},
            {0x32a5, "UN06AM"},
            {0x32a6, "UN07AM"},
            {0x32a7, "UN08AM"},
            {0x32a8, "UN09AM"},
            {0x32a9, "UN10AM"},
            {0x32aa, "UN11AM"},
            {0x32ab, "UN01AL"},
            {0x32ac, "UN02AL"},
            {0x32ad, "UN03AL"},
            {0x32ae, "UN04AL"},
            {0x32af, "UN05AL"},
            {0x32b0, "UN06AL"},
            {0x32b1, "UN07AL"},
            {0x32b2, "UN08AL"},
            {0x32b3, "UN09AL"},
            {0x32b4, "UN10AL"},
            {0x32b5, "UN11AL"},
            {0x32b6, "UN12AL"},
            {0x32b7, "UN13AL"},
            {0x32b8, "UN14AL"},
            {0x32b9, "UN15AL"},
            {0x32ba, "UN16AL"},
            {0x32bb, "UN01AX"},
            {0x32bc, "UN02AX"},
            {0x32bd, "UN03AX"},
            {0x32be, "UN04AX"},
            {0x32bf, "UN05AX"},
            {0x32c0, "UN06AX"},
            {0x32c1, "UN07AX"},
            {0x32c2, "UN08AX"},
            {0x32c3, "UN09AX"},
            {0x32c4, "UN10AX"},
            {0x32c5, "UN11AX"},
            {0x32c6, "UN01BS"},
            {0x32c7, "UN02BS"},
            {0x32c8, "UN03BS"},
            {0x32c9, "UN04BS"},
            {0x32ca, "UN05BS"},
            {0x32cb, "UN06BS"},
            {0x32cc, "UN07BS"},
            {0x32cd, "UN08BS"},
            {0x32ce, "UN09BS"},
            {0x32cf, "UN10BS"},
            {0x32d0, "UN11BS"},
            {0x32d1, "UN01BM"},
            {0x32d2, "UN02BM"},
            {0x32d3, "UN03BM"},
            {0x32d4, "UN04BM"},
            {0x32d5, "UN05BM"},
            {0x32d6, "UN06BM"},
            {0x32d7, "UN07BM"},
            {0x32d8, "UN08BM"},
            {0x32d9, "UN09BM"},
            {0x32da, "UN10BM"},
            {0x32db, "UN11BM"},
            {0x32dc, "UN01BL"},
            {0x32dd, "UN02BL"},
            {0x32de, "UN03BL"},
            {0x32df, "UN04BL"},
            {0x32e0, "UN05BL"},
            {0x32e1, "UN06BL"},
            {0x32e2, "UN07BL"},
            {0x32e3, "UN08BL"},
            {0x32e4, "UN09BL"},
            {0x32e5, "UN10BL"},
            {0x32e6, "UN11BL"},
            {0x32e7, "UN01BX"},
            {0x32e8, "UN02BX"},
            {0x32e9, "UN03BX"},
            {0x32ea, "UN04BX"},
            {0x32eb, "UN05BX"},
            {0x32ec, "UN06BX"},
            {0x32ed, "UN07BX"},
            {0x32ee, "UN08BX"},
            {0x32ef, "UN09BX"},
            {0x32f0, "UN10BX"},
            {0x32f1, "UN11BX"},
            {0x32f2, "UN01CS"},
            {0x32f3, "UN02CS"},
            {0x32f4, "UN03CS"},
            {0x32f5, "UN04CS"},
            {0x32f6, "UN05CS"},
            {0x32f7, "UN06CS"},
            {0x32f8, "UN07CS"},
            {0x32f9, "UN08CS"},
            {0x32fa, "UN09CS"},
            {0x32fb, "UN10CS"},
            {0x32fc, "UN11CS"},
            {0x32fd, "UN01CM"},
            {0x32fe, "UN02CM"},
            {0x32ff, "UN03CM"},
            {0x3300, "UN04CM"},
            {0x3301, "UN05CM"},
            {0x3302, "UN06CM"},
            {0x3303, "UN07CM"},
            {0x3304, "UN08CM"},
            {0x3305, "UN09CM"},
            {0x3306, "UN10CM"},
            {0x3307, "UN11CM"},
            {0x3308, "UN01CL"},
            {0x3309, "UN02CL"},
            {0x330a, "UN03CL"},
            {0x330b, "UN04CL"},
            {0x330c, "UN05CL"},
            {0x330d, "UN06CL"},
            {0x330e, "UN07CL"},
            {0x330f, "UN08CL"},
            {0x3310, "UN09CL"},
            {0x3311, "UN10CL"},
            {0x3312, "UN11CL"},
            {0x3313, "UN01CX"},
            {0x3314, "UN02CX"},
            {0x3315, "UN03CX"},
            {0x3316, "UN04CX"},
            {0x3317, "UN05CX"},
            {0x3318, "UN06CX"},
            {0x3319, "UN07CX"},
            {0x331a, "UN08CX"},
            {0x331b, "UN09CX"},
            {0x331c, "UN10CX"},
            {0x331d, "UN11CX"},
            {0x331e, "HO01AS"},
            {0x331f, "HO02AS"},
            {0x3320, "HO03AS"},
            {0x3321, "HO04AS"},
            {0x3322, "HO05AS"},
            {0x3323, "HO06AS"},
            {0x3324, "HO07AS"},
            {0x3325, "HO08AS"},
            {0x3326, "HO09AS"},
            {0x3327, "HO01AM"},
            {0x3328, "HO02AM"},
            {0x3329, "HO03AM"},
            {0x332a, "HO04AM"},
            {0x332b, "HO05AM"},
            {0x332c, "HO06AM"},
            {0x332d, "HO07AM"},
            {0x332e, "HO08AM"},
            {0x332f, "HO09AM"},
            {0x3330, "HO01AL"},
            {0x3331, "HO02AL"},
            {0x3332, "HO03AL"},
            {0x3333, "HO04AL"},
            {0x3334, "HO05AL"},
            {0x3335, "HO06AL"},
            {0x3336, "HO07AL"},
            {0x3337, "HO08AL"},
            {0x3338, "HO09AL"},
            {0x3339, "HO01AX"},
            {0x333a, "HO02AX"},
            {0x333b, "HO03AX"},
            {0x333c, "HO04AX"},
            {0x333d, "HO05AX"},
            {0x333e, "HO06AX"},
            {0x333f, "HO07AX"},
            {0x3340, "HO08AX"},
            {0x3341, "HO09AX"},
            {0x3342, "EN01AS"},
            {0x3343, "EN02AS"},
            {0x3344, "EN03AS"},
            {0x3345, "EN04AS"},
            {0x3346, "EN05AS"},
            {0x3347, "EN06AS"},
            {0x3348, "EN07AS"},
            {0x3349, "EN08AS"},
            {0x334a, "EN09AS"},
            {0x334b, "EN01AM"},
            {0x334c, "EN02AM"},
            {0x334d, "EN03AM"},
            {0x334e, "EN04AM"},
            {0x334f, "EN05AM"},
            {0x3350, "EN06AM"},
            {0x3351, "EN07AM"},
            {0x3352, "EN08AM"},
            {0x3353, "EN09AM"},
            {0x3354, "EN01AL"},
            {0x3355, "EN02AL"},
            {0x3356, "EN03AL"},
            {0x3357, "EN04AL"},
            {0x3358, "EN05AL"},
            {0x3359, "EN06AL"},
            {0x335a, "EN07AL"},
            {0x335b, "EN08AL"},
            {0x335c, "EN09AL"},
            {0x335d, "EN01AX"},
            {0x335e, "EN02AX"},
            {0x335f, "EN03AX"},
            {0x3360, "EN04AX"},
            {0x3361, "EN05AX"},
            {0x3362, "EN06AX"},
            {0x3363, "EN07AX"},
            {0x3364, "EN08AX"},
            {0x3365, "EN09AX"},
            {0x3366, "EN01BS"},
            {0x3367, "EN02BS"},
            {0x3368, "EN03BS"},
            {0x3369, "EN04BS"},
            {0x336a, "EN05BS"},
            {0x336b, "EN06BS"},
            {0x336c, "EN07BS"},
            {0x336d, "EN08BS"},
            {0x336e, "EN09BS"},
            {0x336f, "EN01BM"},
            {0x3370, "EN02BM"},
            {0x3371, "EN03BM"},
            {0x3372, "EN04BM"},
            {0x3373, "EN05BM"},
            {0x3374, "EN06BM"},
            {0x3375, "EN07BM"},
            {0x3376, "EN08BM"},
            {0x3377, "EN09BM"},
            {0x3378, "EN01BL"},
            {0x3379, "EN02BL"},
            {0x337a, "EN03BL"},
            {0x337b, "EN04BL"},
            {0x337c, "EN05BL"},
            {0x337d, "EN06BL"},
            {0x337e, "EN07BL"},
            {0x337f, "EN08BL"},
            {0x3380, "EN09BL"},
            {0x3381, "EN01BX"},
            {0x3382, "EN02BX"},
            {0x3383, "EN03BX"},
            {0x3384, "EN04BX"},
            {0x3385, "EN05BX"},
            {0x3386, "EN06BX"},
            {0x3387, "EN07BX"},
            {0x3388, "EN08BX"},
            {0x3389, "EN09BX"},
            {0x338a, "SY01CS"},
            {0x338b, "SY02CS"},
            {0x338c, "SY03CS"},
            {0x338d, "SY04CS"},
            {0x338e, "SY05CS"},
            {0x338f, "SY06CS"},
            {0x3390, "SY07CS"},
            {0x3391, "SY08CS"},
            {0x3392, "SY09CS"},
            {0x3393, "SY01CM"},
            {0x3394, "SY02CM"},
            {0x3395, "SY03CM"},
            {0x3396, "SY04CM"},
            {0x3397, "SY05CM"},
            {0x3398, "SY06CM"},
            {0x3399, "SY07CM"},
            {0x339a, "SY08CM"},
            {0x339b, "SY09CM"},
            {0x339c, "SY01CL"},
            {0x339d, "SY02CL"},
            {0x339e, "SY03CL"},
            {0x339f, "SY04CL"},
            {0x33a0, "SY05CL"},
            {0x33a1, "SY06CL"},
            {0x33a2, "SY07CL"},
            {0x33a3, "SY08CL"},
            {0x33a4, "SY09CL"},
            {0x33a5, "SY01CX"},
            {0x33a6, "SY02CX"},
            {0x33a7, "SY03CX"},
            {0x33a8, "SY04CX"},
            {0x33a9, "SY05CX"},
            {0x33aa, "SY06CX"},
            {0x33ab, "SY07CX"},
            {0x33ac, "SY08CX"},
            {0x33ad, "SY09CX"},
            {0x33ae, "EV_CO04A2"},
            {0x33af, "EV_CO05A2"},
            {0x33b0, "EV_CO06A1"},
            {0x33b1, "EV_CO06A2"},
            {0x33b2, "EV_CO06A3"},
            {0x33b3, "KA2L"},
            {0x33b4, "KA1L"},

            {0x4000, "BGM01"},
            {0x4001, "BGM01NL"},
            {0x4002, "BGM02"},
            {0x4003, "BGM02NL"},
            {0x4004, "BGM03"},
            {0x4005, "BGM03NL"},
            {0x4006, "BGM04"},
            {0x4007, "BGM04NL"},
            {0x4008, "BGM05"},
            {0x4009, "BGM05NL"},
            {0x400a, "BGM06"},
            {0x400b, "BGM06NL"},
            {0x400c, "BGM07"},
            {0x400d, "BGM07NL"},
            {0x400e, "BGM08"},
            {0x400f, "BGM08NL"},
            {0x4010, "BGM09"},
            {0x4011, "BGM09NL"},
            {0x4012, "BGM10"},
            {0x4013, "BGM10NL"},
            {0x4014, "BGM11"},
            {0x4015, "BGM11NL"},
            {0x4016, "BGM12"},
            {0x4017, "BGM12NL"},
            {0x4018, "BGM13"},
            {0x4019, "BGM13NL"},
            {0x401a, "BGM14"},
            {0x401b, "BGM14NL"},
            {0x401c, "BGM15"},
            {0x401d, "BGM15NL"},
            {0x401e, "BGM16"},
            {0x401f, "BGM16NL"},
            {0x4020, "BGM17"},
            {0x4021, "BGM17NL"},
            {0x4022, "BGM18"},
            {0x4023, "BGM18NL"},
            {0x4024, "BGM19"},
            {0x4025, "BGM19NL"},
            {0x4026, "BGM20"},
            {0x4027, "BGM20NL"},
            {0x4028, "BGM21"},
            {0x4029, "BGM21NL"},
            {0x402a, "BGM22"},
            {0x402b, "BGM22NL"},
            {0x402c, "BGM23"},
            {0x402d, "BGM23NL"},
            {0x402e, "BGM24"},
            {0x402f, "BGM24NL"},
            {0x4030, "BGM25"},
            {0x4031, "BGM25NL"},
            {0x4032, "BGM26"},
            {0x4033, "BGM26NL"},
            {0x4034, "BGM27"},
            {0x4035, "BGM27NL"},
            {0x4036, "BGM28"},
            {0x4037, "BGM28NL"},
            {0x4038, "BGM29"},
            {0x4039, "BGM29NL"},
            {0x403a, "BGM30"},
            {0x403b, "BGM30NL"},
            {0x403c, "BGM31"},
            {0x403d, "BGM31NL"},
            {0x403e, "BGM32"},
            {0x403f, "BGM32NL"},
            {0x4040, "BGM33"},
            {0x4041, "BGM33NL"},
            {0x4042, "BGM34"},
            {0x4043, "BGM34NL"},
            {0x4044, "BGM35"},
            {0x4045, "BGM35NL"},
            {0x4046, "BGM36"},
            {0x4047, "BGM36NL"},
            {0x4048, "BGM37"},
            {0x4049, "BGM37NL"},

            {0x5000, "SE00_00"},
            {0x5001, "SE00_01"},
            {0x5002, "SE00_02"},
            {0x5003, "SE00_03"},
            {0x5004, "SE00_04"},
            {0x5005, "SE00_05"},
            {0x5006, "SE00_06"},
            {0x5007, "SE00_07"},
            {0x5008, "SE00_08"},
            {0x5009, "SE00_09"},
            {0x500a, "SE00_10"},
            {0x500b, "SE00_11"},
            {0x500c, "SE00_12"},
            {0x500d, "SE00_13"},
            {0x500e, "SE00_14"},
            {0x500f, "SE00_15"},
            {0x5010, "SE00_16"},
            {0x5011, "SE00_17"},
            {0x5012, "SE00_18"},
            {0x5013, "SE00_19"},
            {0x5014, "SE00_20"},
            {0x5015, "SE00_21"},
            {0x5016, "SE01_00"},
            {0x5017, "SE01_01L"},
            {0x5018, "SE01_02"},
            {0x5019, "SE01_02L"},
            {0x501a, "SE01_03"},
            {0x501b, "SE01_04"},
            {0x501c, "SE01_05"},
            {0x501d, "SE01_06"},
            {0x501e, "SE01_07"},
            {0x501f, "SE01_08"},
            {0x5020, "SE01_09"},
            {0x5021, "SE01_09L"},
            {0x5022, "SE01_10"},
            {0x5023, "SE01_11"},
            {0x5024, "SE01_12"},
            {0x5025, "SE01_13"},
            {0x5026, "SE01_14"},
            {0x5027, "SE01_15"},
            {0x5028, "SE01_16"},
            {0x5029, "SE01_17"},
            {0x502a, "SE01_18"},
            {0x502b, "SE01_19"},
            {0x502c, "SE01_20"},
            {0x502d, "SE01_21"},
            {0x502e, "SE01_22"},
            {0x502f, "SE01_23"},
            {0x5030, "SE01_24"},
            {0x5031, "SE01_25"},
            {0x5032, "SE01_26"},
            {0x5033, "SE01_27"},
            {0x5034, "SE01_28"},
            {0x5035, "SE01_29"},
            {0x5036, "SE01_30"},
            {0x5037, "SE01_31"},
            {0x5038, "SE01_32"},
            {0x5039, "SE01_33"},
            {0x503a, "SE01_34"},
            {0x503b, "SE01_35"},
            {0x503c, "SE01_36"},
            {0x503d, "SE01_37"},
            {0x503e, "SE01_38"},
            {0x503f, "SE01_39"},
            {0x5040, "SE01_40"},
            {0x5041, "SE01_41"},
            {0x5042, "SE02_00"},
            {0x5043, "SE02_01"},
            {0x5044, "SE02_02"},
            {0x5045, "SE02_03"},
            {0x5046, "SE02_04"},
            {0x5047, "SE02_05"},
            {0x5048, "SE02_06"},
            {0x5049, "SE02_07"},
            {0x504a, "SE02_08"},
            {0x504b, "SE02_09"},
            {0x504c, "SE02_10"},
            {0x504d, "SE02_11"},
            {0x504e, "SE02_12"},
            {0x504f, "SE02_13"},
            {0x5050, "SE02_14"},
            {0x5051, "SE02_15"},
            {0x5052, "SE02_16"},
            {0x5053, "SE02_17"},
            {0x5054, "SE02_18"},
            {0x5055, "SE02_19"},
            {0x5056, "SE02_20"},
            {0x5057, "SE02_21"},
            {0x5058, "SE02_22"},
            {0x5059, "SE02_23"},
            {0x505a, "SE02_24"},
            {0x505b, "SE02_25"},
            {0x505c, "SE02_26"},
            {0x505d, "SE02_27"},
            {0x505e, "SE02_28"},
            {0x505f, "SE02_29"},
            {0x5060, "SE02_30"},
            {0x5061, "SE02_31"},
            {0x5062, "SE02_32"},
            {0x5063, "SE02_33"},
            {0x5064, "SE02_34"},
            {0x5065, "SE02_35"},
            {0x5066, "SE02_36"},
            {0x5067, "SE02_37"},
            {0x5068, "SE02_38"},
            {0x5069, "SE02_39"},
            {0x506a, "SE02_40"},
            {0x506b, "SE02_41"},
            {0x506c, "SE02_42"},
            {0x506d, "SE02_43"},
            {0x506e, "SE02_44"},
            {0x506f, "SE02_45"},
            {0x5070, "SE02_46"},
            {0x5071, "SE02_47"},
            {0x5072, "SE02_48"},
            {0x5073, "SE02_49"},
            {0x5074, "SE02_50"},
            {0x5075, "SE02_51"},
            {0x5076, "SE02_52"},
            {0x5077, "SE02_53"},
            {0x5078, "SE02_54"},
            {0x5079, "SE02_55"},
            {0x507a, "SE02_56"},
            {0x507b, "SE02_57"},
            {0x507c, "SE02_58"},
            {0x507d, "SE02_59"},
            {0x507e, "SE02_60"},
            {0x507f, "SE02_61"},
            {0x5080, "SE02_62"},
            {0x5081, "SE02_63"},
            {0x5082, "SE02_64"},
            {0x5083, "SE02_65"},
            {0x5084, "SE02_66"},
            {0x5085, "SE02_67"},
            {0x5086, "SE02_68"},
            {0x5087, "SE02_69"},
            {0x5088, "SE02_70"},
            {0x5089, "SE02_71"},
            {0x508a, "SE02_72"},
            {0x508b, "SE02_73L"},
            {0x508c, "SE02_74"},
            {0x508d, "SE02_75"},
            {0x508e, "SE02_76"},
            {0x508f, "SE02_77"},
            {0x5090, "SE02_78"},
            {0x5091, "SE02_79"},
            {0x5092, "SE02_80"},
            {0x5093, "SE02_81"},
            {0x5094, "SE02_82"},
            {0x5095, "SE02_83"},
            {0x5096, "SE02_84"},
            {0x5097, "SE02_85"},
            {0x5098, "SE02_86"},
            {0x5099, "SE02_87"},
            {0x509a, "SE02_88"},
            {0x509b, "SE02_89"},
            {0x509c, "SE02_90"},
            {0x509d, "SE02_91"},
            {0x509e, "SE02_92"},
            {0x509f, "SE02_93"},
            {0x50a0, "SE02_94"},
            {0x50a1, "SE02_95"},
            {0x50a2, "SE02_96"},
            {0x50a3, "SE02_97"},
            {0x50a4, "SE02_98"},
            {0x50a5, "SE02_99"},
            {0x50a6, "SE02_a0"},
            {0x50a7, "SE02_a1"},
            {0x50a8, "SE02_a2"},
            {0x50a9, "SE03_00"},
            {0x50aa, "SE03_01"},
            {0x50ab, "SE03_02"},
            {0x50ac, "SE03_03"},
            {0x50ad, "SE03_04"},
            {0x50ae, "SE03_05"},
            {0x50af, "SE03_06"},
            {0x50b0, "SE03_07"},
            {0x50b1, "SE03_08"},
            {0x50b2, "SE03_09"},
            {0x50b3, "SE03_10"},
            {0x50b4, "SE03_11"},
            {0x50b5, "SE03_12"},
            {0x50b6, "SE03_13"},
            {0x50b7, "SE03_14"},
            {0x50b8, "SE03_15"},
            {0x50b9, "SE03_16"},
            {0x50ba, "SE03_17"},
            {0x50bb, "SE03_18"},
            {0x50bc, "SE03_19"},
            {0x50bd, "SE03_20"},
            {0x50be, "SE03_21"},
            {0x50bf, "SE03_21L"},
            {0x50c0, "SE03_22"},
            {0x50c1, "SE03_23"},
            {0x50c2, "SE03_24"},
            {0x50c3, "SE03_25"},
            {0x50c4, "SE03_26"},
            {0x50c5, "SE03_27"},
            {0x50c6, "SE03_28"},
            {0x50c7, "SE03_29"},
            {0x50c8, "SE03_30"},
            {0x50c9, "SE03_31"},
            {0x50ca, "SE03_32"},
            {0x50cb, "SE03_33"},
            {0x50cc, "SE03_34"},
            {0x50cd, "SE03_35"},
            {0x50ce, "SE03_36"},
            {0x50cf, "SE03_37"},
            {0x50d0, "SE03_38"},
            {0x50d1, "SE03_39"},
            {0x50d2, "SE03_40"},
            {0x50d3, "SE03_41"},
            {0x50d4, "SE03_42"},
            {0x50d5, "SE03_43"},
            {0x50d6, "SE03_44"},
            {0x50d7, "SE03_45"},
            {0x50d8, "SE03_46"},
            {0x50d9, "SE03_47"},
            {0x50da, "SE03_48"},
            {0x50db, "SE03_49"},
            {0x50dc, "SE03_50"},
            {0x50dd, "SE03_51L"},
            {0x50de, "SE03_52"},
            {0x50df, "SE03_53"},
            {0x50e0, "SE03_54"},
            {0x50e1, "SE03_55"},
            {0x50e2, "SE03_56"},
            {0x50e3, "SE03_57"},
            {0x50e4, "SE03_58"},
            {0x50e5, "SE03_59"},
            {0x50e6, "SE03_60"},
            {0x50e7, "SE03_61"},
            {0x50e8, "SE03_62"},
            {0x50e9, "SE03_63"},
            {0x50ea, "SE03_64"},
            {0x50eb, "SE03_65"},
            {0x50ec, "SE03_66"},
            {0x50ed, "SE03_67"},
            {0x50ee, "SE03_68"},
            {0x50ef, "SE03_69"},
            {0x50f0, "SE03_70"},
            {0x50f1, "SE03_71"},
            {0x50f2, "SE03_72"},
            {0x50f3, "SE03_73"},
            {0x50f4, "SE03_74"},
            {0x50f5, "SE03_75"},
            {0x50f6, "SE03_76"},
            {0x50f7, "SE03_77"},
            {0x50f8, "SE03_78"},
            {0x50f9, "SE03_79"},
            {0x50fa, "SE03_80"},
            {0x50fb, "SE03_81"},
            {0x50fc, "SE03_82"},
            {0x50fd, "SE03_83"},
            {0x50fe, "SE03_84"},
            {0x50ff, "SE03_85"},
            {0x5100, "SE03_86"},
            {0x5101, "SE03_87"},
            {0x5102, "SE03_88"},
            {0x5103, "SE03_89"},
            {0x5104, "SE03_90"},
            {0x5105, "SE03_91"},
            {0x5106, "SE03_92"},
            {0x5107, "SE03_93"},
            {0x5108, "SE03_94"},
            {0x5109, "SE03_95"},
            {0x510a, "SE03_96"},
            {0x510b, "SE03_97"},
            {0x510c, "SE03_98"},
            {0x510d, "SE03_99"},
            {0x510e, "SE03_a0"},
            {0x510f, "SE03_a1"},
            {0x5110, "SE03_a2"},
            {0x5111, "SE03_a3"},
            {0x5112, "SE03_a4"},
            {0x5113, "SE03_a5"},
            {0x5114, "SE03_a6"},
            {0x5115, "SE03_a7"},
            {0x5116, "SE03_a8"},
            {0x5117, "SE03_a9"},
            {0x5118, "SE03_b0"},
            {0x5119, "SE03_b1"},
            {0x511a, "SE03_b2"},
            {0x511b, "SE03_b3"},
            {0x511c, "SE03_b4"},
            {0x511d, "SE03_b5"},
            {0x511e, "SE03_b6"},
            {0x511f, "SE03_b7"},
            {0x5120, "SE03_b8"},
            {0x5121, "SE03_b9"},
            {0x5122, "SE03_c0"},
            {0x5123, "SE03_c1"},
            {0x5124, "SE03_c2"},
            {0x5125, "SE03_c3"},
            {0x5126, "SE03_c4"},
            {0x5127, "SE03_c5"},
            {0x5128, "SE03_c6"},
            {0x5129, "SE03_c7"},
            {0x512a, "SE04_00L"},
            {0x512b, "SE04_01"},
            {0x512c, "SE04_02"},
            {0x512d, "SE04_03"},
            {0x512e, "SE04_04"},
            {0x512f, "SE04_05"},
            {0x5130, "SE04_05L"},
            {0x5131, "SE04_06"},
            {0x5132, "SE04_07"},
            {0x5133, "SE04_08L"},
            {0x5134, "SE04_09"},
            {0x5135, "SE04_10"},
            {0x5136, "SE04_11"},
            {0x5137, "SE04_12"},
            {0x5138, "SE04_13"},
            {0x5139, "SE05_00"},
            {0x513a, "SE05_01L"},
            {0x513b, "SE05_02"},
            {0x513c, "SE05_03L"},
            {0x513d, "SE05_04L"},
            {0x513e, "SE05_05"},
            {0x513f, "SE05_06"},
            {0x5140, "SE05_07"},
            {0x5141, "SE05_08"},
            {0x5142, "SE05_09"},
            {0x5143, "SE05_10"},
            {0x5144, "SE05_11"},
            {0x5145, "SE05_12"},
            {0x5146, "SE05_13"},
            {0x5147, "SE05_14"},
            {0x5148, "SE05_15"},
            {0x5149, "SE05_16"},
            {0x514a, "SE05_17"},
            {0x514b, "SE05_18"},
            {0x514c, "SE05_19"},
            {0x514d, "SE05_20"},
            {0x514e, "SE05_21"},
            {0x514f, "SE05_22L"},
            {0x5150, "SE05_23"},
            {0x5151, "SE05_24"},
            {0x5152, "SE05_25"},
            {0x5153, "SE05_26"},
            {0x5154, "SE06_00"},
            {0x5155, "SE06_01"},
            {0x5156, "SE06_02"},
            {0x5157, "SE06_03"},
            {0x5158, "SE06_04"},
            {0x5159, "SE06_05"},
            {0x515a, "SE06_06"},
            {0x515b, "SE07_00"},
            {0x515c, "SE07_00L"},
            {0x515d, "SE07_01"},
            {0x515e, "SE07_01L"},
            {0x515f, "SE07_02"},
            {0x5160, "SE07_03"},
            {0x5161, "SE07_04"},
            {0x5162, "SE07_05"},
            {0x5163, "SE07_06"},
            {0x5164, "SE07_07"},
            {0x5165, "SE07_07L"},
            {0x5166, "SE07_08"},
            {0x5167, "SE07_09"},
            {0x5168, "SE07_10"},
            {0x5169, "SE07_11"},
            {0x516a, "SE07_12"},
            {0x516b, "SE07_13"},
            {0x516c, "SE07_14"},
            {0x516d, "SE07_15"},
            {0x516e, "SE07_16"},
            {0x516f, "SE07_17"},
            {0x5170, "SE07_18"},
            {0x5171, "SE07_19"},
            {0x5172, "SE07_20"},
            {0x5173, "SE07_21"},
            {0x5174, "SE07_22"},
            {0x5175, "SE07_23L"},
            {0x5176, "SE07_24"},
            {0x5177, "SE07_24L"},
            {0x5178, "SE07_25"},
            {0x5179, "SE07_25L"},
            {0x517a, "SE07_26"},
            {0x517b, "SE08_00"},
            {0x517c, "SE08_01"},
            {0x517d, "SE08_02"},
            {0x517e, "SE08_03"},
            {0x517f, "SE09_00"},
            {0x5180, "SE09_00L"},
            {0x5181, "SE09_01"},
            {0x5182, "SE09_01L"},
            {0x5183, "SE09_02"},
            {0x5184, "SE09_02L"},
            {0x5185, "SE09_03"},
            {0x5186, "SE09_03L"},
            {0x5187, "SE09_04"},
            {0x5188, "SE09_05"},
            {0x5189, "SE09_06"},
            {0x518a, "SE09_07"},
            {0x518b, "SE09_08"},
            {0x518c, "SE09_09"},
            {0x518d, "SE09_09L"},
            {0x518e, "SE09_10"},
            {0x518f, "SE09_11"},
            {0x5190, "SE09_12"},
            {0x5191, "SE09_13"},
            {0x5192, "SE09_14"},
            {0x5193, "SE09_15"},
            {0x5194, "SE10_00"},
            {0x5195, "SE10_01"},
            {0x5196, "SE10_02"},
            {0x5197, "SE10_03"},
            {0x5198, "SE10_04"},
            {0x5199, "SE10_04L"},
            {0x519a, "SE10_05"},
            {0x519b, "SE10_06"},
            {0x519c, "SE10_07"},
            {0x519d, "SE10_08"},
            {0x519e, "SE10_09L"},
            {0x519f, "SE10_10"},
            {0x51a0, "SE10_11"},
            {0x51a1, "SE10_12"},
            {0x51a2, "SE10_13"},
            {0x51a3, "SE10_14"},
            {0x51a4, "SE10_15"},
            {0x51a5, "SE10_16"},
            {0x51a6, "ADX00"},
            {0x51a7, "ADX01"},
            {0x51a8, "ADX02"},
            {0x51a9, "ADX03"},
            {0x51aa, "ADX04"},
            {0x51ab, "ADX05"},
            {0x51ac, "ADX06"},
            {0x51ad, "ADX07"},
            {0x51ae, "ADX08"},
            {0x51af, "ADX09"},
            {0x51b0, "ADX10"},
            {0x51b1, "ADX11"},
            {0x51b2, "ADX12"},
            {0x51b3, "ADX13"},
            {0x51b4, "BGM01ADX"},
            {0x51b5, "BGM02ADX"},
            {0x51b6, "BGM03ADX"},
            {0x51b7, "BGM04ADX"},
            {0x51b8, "BGM05ADX"},
            {0x51b9, "BGM06ADX"},
            {0x51ba, "BGM07ADX"},
            {0x51bb, "BGM08ADX"},
            {0x51bc, "BGM09ADX"},
            {0x51bd, "BGM10ADX"},
            {0x51be, "BGM11ADX"},
            {0x51bf, "BGM12ADX"},
            {0x51c0, "BGM13ADX"},
            {0x51c1, "BGM14ADX"},
            {0x51c2, "BGM15ADX"},
            {0x51c3, "BGM16ADX"},
            {0x51c4, "BGM17ADX"},
            {0x51c5, "BGM18ADX"},
            {0x51c6, "BGM19ADX"},
            {0x51c7, "BGM20ADX"},
            {0x51c8, "BGM21ADX"},
            {0x51c9, "BGM22ADX"},
            {0x51ca, "BGM23ADX"},
            {0x51cb, "BGM24ADX"},
            {0x51cc, "BGM25ADX"},
            {0x51cd, "BGM27ADX"},
            {0x51ce, "BGM28ADX"},
            {0x51cf, "BGM29ADX"},
            {0x51d0, "BGM30ADX"},
            {0x51d1, "BGM31ADX"},
            {0x51d2, "BGM32ADX"},
            {0x51d3, "BGM33ADX"},
        };


        static public void OpenFile(string fileName, string fileNameJp)
        {
            ChangedFile = false;
            Tokens = new List<Token>();
            List<string[]> labels_jumped_to = new List<string[]>();
            pos = 0;
            posEndOfInstruction = -1;
            posStringBlock = -1;
            filename = fileName;

            byte[] data_jp, data_en;
            data_en = File.ReadAllBytes(fileName);
            data = data_en;

            if (fileNameJp != null)
                data_jp = File.ReadAllBytes(fileNameJp);
            else
                data_jp = null;

            while (posEndOfInstruction == -1 || pos < posEndOfInstruction)
            {
                Token token = ReadNextToken();
                if (token == null)
                    throw new Exception("No implimentation of token " + ((TokenType)data[pos]).ToString() + " at position " + pos.ToString());

                token.Offset = (UInt16)pos;
                Tokens.Add(token);

                // This is a dirty hack. Basically I'm swapping out the static data with the appropriate language. I was expecting to have to handle multiple languages. Forgive me.
                if (data_jp != null)
                {
                    if (token is TokenMsgDisp2)
                    {
                        data = data_jp;
                        var token2 = (TokenMsgDisp2) ReadNextToken();
                        (token as TokenMsgDisp2).MessageJp = token2.Message;
                        data = data_en;
                    }
                    else if (token is TokenSelectDisp2)
                    {
                        data = data_jp;
                        var token2 = (TokenSelectDisp2) ReadNextToken();
                        for (int i=0; i<token2.Entries.Count; i++)
                        {
                            (token as TokenSelectDisp2).Entries[i].MessageJp = token2.Entries[i].Message;
                        }
                        data = data_en;
                    }
                    else if (token is TokenSystemMsg)
                    {
                        data = data_jp;
                        var token2 = (TokenSystemMsg)ReadNextToken();
                        (token as TokenSystemMsg).MessageJp = token2.Message;
                        data = data_en;
                    }
                }

                pos += token.Length;


                if (token is TokenNop)
                {
                    posEndOfInstruction = pos;
                    break;
                }
                else if (posStringBlock < 0) {
                    if (token is TokenMsgDisp2)
                        posStringBlock = (token as TokenMsgDisp2).MsgPtr;
                }

                // Hold onto a list of these jumps so we can create a label for them
                if (token is TokenIf)
                {
                    labels_jumped_to.Add(new string[] { (token as TokenIf).LabelJump, token.Offset.ToString() });
                }
                else if (token is TokenInternalGoto)
                {
                    labels_jumped_to.Add(new string[] { (token as TokenInternalGoto).LabelJump, token.Offset.ToString() });
                }
                else if (token is TokenSelectDisp)
                    throw new NotImplementedException(); //@TODO: HANDLE THIS JUMP (Only used in DBG)
            }

            // Create labels for jumping points, then update the tokens at those offsets
            foreach (var t in Tokens)
            {
                string current_label = t.Offset.ToString();
                foreach (var pair in labels_jumped_to)
                {
                    if (pair[0] == current_label)
                    {
                        t.Label = current_label;
                        t.ReferingLabels.Add(pair[1]);
                    }
                    else if (pair[1] == current_label)
                    {
                        t.Label = current_label;
                    }
                }
            }

            // Check for identical Japanese
            for (int i=0; i<Tokens.Count; i++)
            {
                var token1 = Tokens[i];
                if (token1 is TokenMsgDisp2)
                {
                    var t1 = (TokenMsgDisp2)token1;
                    string msg = t1.MessageJp;

                    for (int j = i+1; j < Tokens.Count; j++)
                    {
                        var token2 = Tokens[j];
                        if (token2 is TokenMsgDisp2 && (token2 as TokenMsgDisp2).MessageJp == msg)
                        {
                            var t2 = (TokenMsgDisp2)token2;
                            t1.Label = t1.Offset.ToString();
                            t2.Label = t2.Offset.ToString();

                            t1.IdenticalJpLabels.Add(t2.Label);
                            t2.IdenticalJpLabels.Add(t1.Label);
                            t1.Data = "IDENTICAL JP FOUND";
                            t2.Data = "IDENTICAL JP FOUND";
                        }
                        else if (token2 is TokenSelectDisp2)
                        {
                            var t2 = (TokenSelectDisp2)token2;
                            foreach (var e in t2.Entries)
                            {
                                if (e.MessageJp == msg)
                                {
                                    t1.Label = t1.Offset.ToString();
                                    t2.Label = t2.Offset.ToString();

                                    t1.IdenticalJpLabels.Add(t2.Label);
                                    t2.IdenticalJpLabels.Add(t1.Label);
                                    t1.Data = "IDENTICAL JP FOUND";
                                    t2.Data = "Choices: " + t2.Entries.Count.ToString() + " IDENTICAL JP FOUND";
                                    break;
                                }
                            }
                        }
                    }
                }
                else if (token1 is TokenSelectDisp2)
                {
                    var t1 = (TokenSelectDisp2)token1;

                    for (int j = i + 1; j < Tokens.Count; j++)
                    {
                        var token2 = Tokens[j];
                        if (token2 is TokenMsgDisp2)
                        {
                            var t2 = (TokenMsgDisp2)token2;
                            foreach (var e in t1.Entries)
                                if (e.MessageJp == t2.MessageJp)
                                {
                                    t1.Label = t1.Offset.ToString();
                                    t2.Label = t2.Offset.ToString();

                                    t1.IdenticalJpLabels.Add(t2.Label);
                                    t2.IdenticalJpLabels.Add(t1.Label);
                                    t1.Data = "Choices: " + t1.Entries.Count.ToString() + " IDENTICAL JP FOUND";
                                    t2.Data = "IDENTICAL JP FOUND";
                                    break;
                                }
                        }
                        /*
                        else if (token2 is TokenSelectDisp2)
                        {
                            var t2 = (TokenSelectDisp2)token2;
                            foreach (var e in t2.Entries)
                            {
                                if (e.MessageJp == msg)
                                {
                                    t1.Label = t1.Offset.ToString();
                                    t2.Label = t2.Offset.ToString();

                                    t1.IdenticalJpLabels.Add(t2.Label);
                                    t2.IdenticalJpLabels.Add(t1.Label);
                                    t1.Data = "IDENTICAL JP FOUND";
                                    t2.Data = "IDENTICAL JP FOUND";
                                }
                            }
                        }
                        */
                    }
                }
            }

            // Hold onto the switch table
            int switch_len = posStringBlock - posEndOfInstruction;
            data_switch_block = new byte[switch_len];
            Array.Copy(data, posEndOfInstruction, data_switch_block, 0, switch_len);
            
        }

        static public void SaveFile(string filename)
        {
            var all_messages = new List<byte[]>();
            var msg_offsets = new List<int>();
            int current_offset = 0;
            Dictionary<string, UInt16> labels = new Dictionary<string, UInt16>();


            Stream stream = new MemoryStream();

            // First pass, get the bytes.
            // Just gets the total length and sets token offsets properly
            // Also gets labels that need dereferences later
            foreach (var token in Tokens)
            {
                token.Offset = (UInt16)current_offset;
                byte[] buffer = token.GetBytes();
                stream.Write(buffer, 0, buffer.Length);
                current_offset += buffer.Length;

                if (token.Label == null || token.Label == "")
                    continue;
                if (labels.ContainsKey(token.Label))
                    throw new Exception("Label \"" + token.Label + "\" is defined more than once");
                labels.Add(token.Label, token.Offset);
            }

            // Create a new data_switch_block
            int switch_block_offset = current_offset - posEndOfInstruction;
            posEndOfInstruction = current_offset;

            byte[] data_switch_block2 = new byte[data_switch_block.Length];
            int offset = data_switch_block.Length & 0x03; // Make sure the block length is a multiple of 4
            for (int i = offset; i < data_switch_block.Length; i += 4)
            {
                data_switch_block2[i + 0] = data_switch_block[i + 0];
                data_switch_block2[i + 1] = data_switch_block[i + 1];

                ushort addr = (ushort) (data_switch_block[i + 2] + (data_switch_block[i + 3] << 8));
                if (addr == 0)
                {
                    data_switch_block2[i + 2] = 0;
                    data_switch_block2[i + 3] = 0;
                    continue;
                }
                string old_label = addr.ToString();
                ushort new_address = labels[old_label];
                data_switch_block2[i + 2] = (byte) (new_address & 0xff);
                data_switch_block2[i + 3] = (byte) (new_address >> 8);
            }

            // Second pass: Dereference labels
            foreach (var token in Tokens)
            {
                if (token is TokenIf)
                {
                    var t = (TokenIf)token;
                    if (t.Action != IfAction.SwitchBreak && t.Action != IfAction.SwitchCase)
                        t.UpdateAddress(labels[t.LabelJump]);
                    else
                    {
                        t.ptr_jump = ((ushort) (t.ptr_jump + switch_block_offset));
                    }
                }
                else if (token is TokenInternalGoto)
                {
                    var t = (TokenInternalGoto)token;
                    t.UpdateAddress(labels[t.LabelJump]);
                }
                else if (token is TokenSelectDisp)
                    throw new NotImplementedException(); //@TODO: HANDLE THIS JUMP (Only used in DBG)
            }

            // Third pass: Create string pointers.
            int end_of_script = (int)stream.Length + data_switch_block2.Length;
            int msg_offset = end_of_script;
            int idx;
            stream.Seek(0, SeekOrigin.Begin);

            for (int i=0; i<Tokens.Count; i++)
            {
                // Recalculate token offsets and rewrite the stream from scratch.
                // If multiple copies of the same text are found, make them share a single pointer
                var token = Tokens[i];
                var msg = token.GetMessagesBytes();
                
                if (msg != null)
                {
                    idx = -1;
                    for (int k=0; k<all_messages.Count; k++)
                        if (all_messages[k]!=null && msg.SequenceEqual(all_messages[k]))
                        {
                            idx = k;
                            break;
                        }
                    
                    if (idx >= 0)
                    {
                        msg = null;
                        msg_offset = msg_offsets[idx];
                        token.SetMessagePointer(msg_offset);
                    }
                    else
                    {
                        msg_offset = end_of_script;
                        end_of_script = token.SetMessagePointer(msg_offset);
                    }
                        
                }

                all_messages.Add(msg);
                msg_offsets.Add(msg_offset);



                byte[] buffer = token.GetBytes();
                stream.Write(buffer, 0, buffer.Length);
            }

            // Write the switch block.
            stream.Write(data_switch_block2, 0, data_switch_block2.Length);

            // Write the final string block
            for (int i = 0; i < Tokens.Count; i++)
            {
                var msg = all_messages[i];
                if (msg != null)
                    stream.Write(msg, 0, msg.Length);
            }

            // Throw an error if total length of file exceeds UInt16 values
            if (stream.Length > UInt16.MaxValue)
                throw new Exception("File length exceeds maximum size of " + UInt16.MaxValue.ToString() + " bytes");

            // Copy the stream to a byte array
            stream.Seek(0, SeekOrigin.Begin);
            byte[] output = new byte[stream.Length];
            stream.Read(output, 0, (int)stream.Length);
            stream.Close();

            // Write to file
            var stream_out = new FileStream(filename, FileMode.Create, FileAccess.ReadWrite);
            stream_out.Write(output, 0, output.Length);
            stream_out.Close();

            ChangedFile = false;

            // Compress the file
            string bip_fname = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename)+".BIP");
            stream = new FileStream(bip_fname, FileMode.Create, FileAccess.ReadWrite);
            var lzss_comp = new LzssCompress();
            try
            {
                byte[] compressed = lzss_comp.Compress(output);
                stream.Write(compressed, 0, (int)compressed.Length);
                stream.Close();
            }
            catch (Exception ex)
            {
                stream.Close();
                throw ex;
            }
        }

        static public Token ReadNextToken()
        {
            TokenType opcode = (TokenType) data[pos]; if (!Enum.IsDefined(typeof(TokenType), opcode)) throw new ArgumentOutOfRangeException();

            switch (opcode)
            {
                case TokenNop.Type: return new TokenNop();
                case TokenEnd.Type: return new TokenEnd();
                case TokenIf.Type: return new TokenIf();
                case TokenInternalGoto.Type: return new TokenInternalGoto();
                //case TokenInternalCall.Type: return new TokenInternalCall();
                //case TokenInternalReturn.Type: return new TokenInternalReturn();
                case TokenExternalGoto.Type: return new TokenExternalGoto();
                //case TokenExternalCall.Type: return new TokenExternalCall();
                case TokenExternalReturn.Type: return new TokenExternalReturn();
                case TokenRegCalc.Type: return new TokenRegCalc();
                case TokenCountClear.Type: return new TokenCountClear();
                case TokenCountWait.Type: return new TokenCountWait();
                case TokenTimeWait.Type: return new TokenTimeWait();
                //case TokenPadWait.Type: return new TokenPadWait();
                //case TokenPadGet.Type: return new TokenPadGet();
                case TokenFileRead.Type: return new TokenFileRead();
                case TokenFileWait.Type: return new TokenFileWait();
                //case TokenMsgWindow.Type: return new TokenMsgWindow();
                case TokenMsgView.Type: return new TokenMsgView();
                case TokenMsgMode.Type: return new TokenMsgMode();
                //case TokenMsgPos.Type: return new TokenMsgPos();
                //case TokenMsgSize.Type: return new TokenMsgSize();
                case TokenMsgType.Type: return new TokenMsgType();
                //case TokenMsgCursor.Type: return new TokenMsgCursor();
                //case TokenMsgSet.Type: return new TokenMsgSet();
                //case TokenMsgWait.Type: return new TokenMsgWait();
                //case TokenMsgClear.Type: return new TokenMsgClear();
                //case TokenMsgLine.Type: return new TokenMsgLine();
                //case TokenMsgSpeed.Type: return new TokenMsgSpeed();
                //case TokenMsgColor.Type: return new TokenMsgColor();
                //case TokenMsgAnim.Type: return new TokenMsgAnim();
                //case TokenMsgDisp.Type: return new TokenMsgDisp();
                //case TokenSelectSet.Type: return new TokenSelectSet();
                //case TokenSelectEntry.Type: return new TokenSelectEntry();
                //case TokenSelectView.Type: return new TokenSelectView();
                //case TokenSelectWait.Type: return new TokenSelectWait();
                //case TokenSelectStyle.Type: return new TokenSelectStyle();
                case TokenSelectDisp.Type: return new TokenSelectDisp();
                //case TokenFadeStart.Type: return new TokenFadeStart();
                //case TokenFadeWait.Type: return new TokenFadeWait();
                //case TokenGraphSet.Type: return new TokenGraphSet();
                //case TokenGraphDel.Type: return new TokenGraphDel();
                //case TokenGraphCopy.Type: return new TokenGraphCopy();
                //case TokenGraphView.Type: return new TokenGraphView();
                case TokenGraphPos.Type: return new TokenGraphPos();
                //case TokenGraphMove.Type: return new TokenGraphMove();
                case TokenGraphPrio.Type: return new TokenGraphPrio();
                //case TokenGraphAnim.Type: return new TokenGraphAnim();
                case TokenGraphPal.Type: return new TokenGraphPal();
                //case TokenGraphLay.Type: return new TokenGraphLay();
                //case TokenGraphWait.Type: return new TokenGraphWait();
                case TokenGraphDisp.Type: return new TokenGraphDisp();
                //case TokenEffectStart.Type: return new TokenEffectStart();
                //case TokenEffectEnd.Type: return new TokenEffectEnd();
                //case TokenEffectWait.Type: return new TokenEffectWait();
                case TokenBgmSet.Type: return new TokenBgmSet();
                case TokenBgmDel.Type: return new TokenBgmDel();
                //case TokenBgmReq.Type: return new TokenBgmReq();
                case TokenBgmWait.Type: return new TokenBgmWait();
                case TokenBgmSpeed.Type: return new TokenBgmSpeed();
                case TokenBgmVolume.Type: return new TokenBgmVolume();
                case TokenSeSet.Type: return new TokenSeSet();
                case TokenSeDel.Type: return new TokenSeDel();
                case TokenSeReq.Type: return new TokenSeReq();
                case TokenSeWait.Type: return new TokenSeWait();
                case TokenSeSpeed.Type: return new TokenSeSpeed();
                case TokenSeVolume.Type: return new TokenSeVolume();
                case TokenVoiceSet.Type: return new TokenVoiceSet();
                case TokenVoiceDel.Type: return new TokenVoiceDel();
                case TokenVoiceReq.Type: return new TokenVoiceReq();
                case TokenVoiceWait.Type: return new TokenVoiceWait();
                //case TokenVoiceSpeed.Type: return new TokenVoiceSpeed();
                //case TokenVoiceVolume.Type: return new TokenVoiceVolume();
                case TokenMenuLock.Type: return new TokenMenuLock();
                case TokenSaveLock.Type: return new TokenSaveLock();
                //case TokenSaveCheck.Type: return new TokenSaveCheck();
                //case TokenSaveDisp.Type: return new TokenSaveDisp();
                //case TokenDiskChange.Type: return new TokenDiskChange();
                //case TokenSkipStart.Type: return new TokenSkipStart();
                //case TokenSkipEnd.Type: return new TokenSkipEnd();
                //case TokenTaskEntry.Type: return new TokenTaskEntry();
                //case TokenTaskDelete.Type: return new TokenTaskDelete();
                //case TokenCalDisp.Type: return new TokenCalDisp();
                case TokenTitleDisplay.Type: return new TokenTitleDisplay();
                //case TokenVibrationStart.Type: return new TokenVibrationStart();
                //case TokenVibrationEnd.Type: return new TokenVibrationEnd();
                //case TokenVibrationWait.Type: return new TokenVibrationWait();
                case TokenMoviePlay.Type: return new TokenMoviePlay();
                case TokenGraphPosAuto.Type: return new TokenGraphPosAuto();
                case TokenGraphPosSave.Type: return new TokenGraphPosSave();
                case TokenGraphUvAuto.Type: return new TokenGraphUvAuto();
                case TokenGraphUvSave.Type: return new TokenGraphUvSave();
                case TokenEffectEx.Type: return new TokenEffectEx();
                //case TokenFadeEx.Type: return new TokenFadeEx();
                case TokenVibrationEx.Type: return new TokenVibrationEx();
                case TokenClockDisp.Type: return new TokenClockDisp();
                case TokenGraphDispEx.Type: return new TokenGraphDispEx();
                case TokenQuickSave.Type: return new TokenQuickSave();
                case TokenTraceSpc.Type: return new TokenTraceSpc();
                case TokenSystemMsg.Type: return new TokenSystemMsg();
                case TokenSkipLock.Type: return new TokenSkipLock();
                case TokenKeyLock.Type: return new TokenKeyLock();
                //case TokenGraphDisp2.Type: return new TokenGraphDisp2();
                case TokenMsgDisp2.Type: return new TokenMsgDisp2();
                case TokenSelectDisp2.Type: return new TokenSelectDisp2();
                //case TokenDateDisp.Type: return new TokenDateDisp();
                case TokenEyeLock.Type: return new TokenEyeLock();
                //case TokenMsgLock.Type: return new TokenMsgLock();
                case TokenGraphScaleAuto.Type: return new TokenGraphScaleAuto();
                case TokenMovieStart.Type: return new TokenMovieStart();
                case TokenMovieEnd.Type: return new TokenMovieEnd();
                case TokenFadeExStart.Type: return new TokenFadeExStart();
                case TokenFadeExWait.Type: return new TokenFadeExWait();
                case TokenBreathLock.Type: return new TokenBreathLock();
            }

            return null;

        }

        static public byte ReadUInt8(int offset, bool from_current_pos = true)
        {
            if (from_current_pos)
                return data[pos + offset];

            return data[offset];
        }

        static public UInt16 ReadUInt16(int offset, bool from_current_pos=true)
        {
            if (from_current_pos)
                return (UInt16)(data[pos+offset] + (data[pos+offset+1] << 8));

            return (UInt16)(data[offset] + (data[offset + 1] << 8));
        }

        static public Int32 ReadUInt32(int offset)
        {
            return data[pos+offset] + (data[pos+offset+1]<<8) + (data[pos+offset+2]<<16) + (data[pos+offset+3]<<24);
        }

        static public String ReadString(int offset)
        {
            int count = 0;

            // Read until null character
            while (true)
            {
                byte c = data[offset+count];
                if (c == 0) break;
                count++;
            }

            byte[] buffer = new byte[count];
            Array.Copy(data, offset, buffer, 0, count);

            // This is a Shift-JIS string, decode it
            return Tokenizer.StringDecode(buffer);
        }

        static public string GetMemoryName(int val)
        {
            int type = val >> 12;
            if (type >= 0 && type <= 3)
                return val.ToString();
            if (type == 0x06 || type == 0x08 || type == 0x0a)
                return "[" + MemoryNames[val] + "]";
            else
                return "reg[" + val.ToString() + "]";
        }

        static public string GetSceneTitle(int val)
        {
            return SceneTitles[val * 2] + ": " + SceneTitles[val * 2 + 1];
        }

        static public string GetEvTitle(int val)
        {
            return EvTitles[val & 0x1FF];
        }

        static public string GetBgmTitle(int val)
        {
            return BgmTitles[val];
        }
        
        static public string GetFileName(int val)
        {
            if (FileNames.ContainsKey(val))
                return FileNames[val];
            return "INVALID FILE";
        }

        static public string StringSingleSpace(string input)
        {
            return input.Replace("  ", " ");
        }

        static public string StringDoubleSpace(string input)
        {
            return input.Replace(" ", "  ");
        }

        static public byte[] StringEncode(string input)
        {
            string temp = input.Replace("ï", "∇");
            temp = temp.Replace("é", "≡");
            temp = temp.Replace("ö", "≒");
            var x = Encoding.GetEncoding("shift_jis").GetBytes(temp);
                
            for (int i=0; i<x.Length-1; i++)
                if (x[i] >= 0x80)
                {
                    i++;
                    if (x[i-1] == 0x81 && x[i] == 0xe0) // ≒ -> ö
                        { x[i-1] = 0x86; x[i] = 0x40; }
                    if (x[i-1] == 0x81 && x[i] == 0xde) // ∇ -> ï
                        { x[i-1] = 0x86; x[i] = 0x43; }
                    else if (x[i-1] == 0x81 && x[i] == 0xdf) // ≡ -> é 
                        { x[i-1] = 0x86; x[i] = 0x44; }
                    else if (x[i-1] == 0x81 && x[i] == 0x61) // ∥ -> "I"
                        { x[i-1] = 0x86; x[i] = 0x78; }
                    else if (x[i-1] == 0x83 && x[i] == 0xB1) // Tau -> "t"
                        { x[i-1] = 0x86; x[i] = 0xA4; }
                    else if (x[i-1] == 0x83 && x[i] == 0xA5) // Eta -> "h"
                        { x[i-1] = 0x86; x[i] = 0x98; }
                    else if (x[i-1] == 0x83 && x[i] == 0x9F) // Alpha -> "a"
                        { x[i-1] = 0x86; x[i] = 0x91; }
                    else if (x[i-1] == 0x81 && x[i] == 0xAB) // ↓ -> "!"
                        { x[i-1] = 0x86; x[i] = 0x50; }
                    else if (x[i-1] == 0x81 && x[i] == 0x79) // 【 -> "「" 
                        { x[i-1] = 0x85; x[i] = 0xA0; }
                    else if (x[i-1] == 0x81 && x[i] == 0x7A) // 】 -> "」"
                        { x[i-1] = 0x85; x[i] = 0xA1; }
                    
                }

            return x;
        }

        static public string StringDecode(byte[] x)
        {
            for (int i=0; i<x.Length-1; i++)
                if (x[i] >= 0x80)
                {
                    i++;
                    if (x[i - 1] == 0x86 && x[i] == 0x40) // ö -> ≒
                        { x[i-1] = 0x81; x[i] = 0xe0; }
                    else if (x[i-1] == 0x86 && x[i] == 0x43) // ï -> ∇
                        { x[i-1] = 0x81; x[i] = 0xde; }
                    else if (x[i-1] == 0x86 && x[i] == 0x44) // é -> ≡
                        { x[i-1] = 0x81; x[i] = 0xdf; }
                    else if (x[i-1] == 0x86 && x[i] == 0x78) // "I" -> ∥
                        { x[i-1] = 0x81; x[i] = 0x61; }
                    else if (x[i-1] == 0x86 && x[i] == 0xA4) // "t" -> Tau
                        { x[i-1] = 0x83; x[i] = 0xB1; }
                    else if (x[i-1] == 0x86 && x[i] == 0x98) // "h" -> Eta
                        { x[i-1] = 0x83; x[i] = 0xA5; }
                    else if (x[i-1] == 0x86 && x[i] == 0x91) // "a" -> Alpha
                        { x[i-1] = 0x83; x[i] = 0x9F; }
                    else if (x[i-1] == 0x86 && x[i] == 0x50) // "!" -> ↓
                        { x[i-1] = 0x81; x[i] = 0xAB; }
                    else if (x[i-1] == 0x85 && x[i] == 0xA0) //  "「" -> 【
                        { x[i-1] = 0x81; x[i] = 0x79; }
                    else if (x[i-1] == 0x85 && x[i] == 0xA1) // "」" -> 】
                        { x[i-1] = 0x81; x[i] = 0x7A; }
                }

            var output = Encoding.GetEncoding("shift-jis").GetString(x);
            output = output.Replace("∇", "ï");
            output = output.Replace("≡", "é");
            output = output.Replace("≒", "ö");

            return output;
        }

        static public bool Search(string text, bool next, bool case_sensitive)
        {
            int idx = TokensList.SelectedIndex;
            if (idx == -1)
                idx = 0;

            text = StringDoubleSpace(text);
            if (!case_sensitive)
                text = text.ToLower();

            if (searchEndOfFile)
            {
                if (next) idx = 0;
                else idx = TokensList.Items.Count-1;
                searchEndOfFile = false;
            }

            while (true)
            {
                if (next)
                {
                    idx++;
                    if (idx >= TokensList.Items.Count-1) break;
                }
                else
                {
                    idx--;
                    if (idx < 0) break;
                }

                object t = TokensList.Items[idx];
                string msg = (t as Token).GetMessages();

                if (msg == null) continue;

                if (!case_sensitive)
                    msg = msg.ToLower();

                if (msg.Contains(text))
                {
                    // Select and focus on the token
                    TokensList.SelectedIndex = idx;
                    TokensList.UpdateLayout();
                    TokensList.ScrollIntoView(TokensList.Items[idx]);
                    return true;
                }
            }

            searchEndOfFile = true;
            //MessageBox.Show("End of file");
            return false;
        }

        static public void JumpToLabel(string label)
        {
            for (int i=0; i<Tokens.Count; i++)
            {
                var token = Tokens[i];
                if (token.Label == label)
                {
                    // Select and focus on the token
                    TokensList.SelectedIndex = i;
                    TokensList.UpdateLayout();
                    TokensList.ScrollIntoView(TokensList.Items[i]);
                    break;
                }
            }
        }

        static public void GoToNextText(bool next)
        {
            int idx = TokensList.SelectedIndex;
            if (idx == -1)
                idx = 0;

            while (true)
            {
                if (next)
                {
                    idx++;
                    if (idx >= TokensList.Items.Count - 1) break;
                }
                else
                {
                    idx--;
                    if (idx < 0) break;
                }

                object t = TokensList.Items[idx];
                string msg = (t as Token).GetMessages();

                if (msg != null)
                {
                    // Select and focus on the token
                    TokensList.SelectedIndex = idx;
                    TokensList.UpdateLayout();
                    TokensList.ScrollIntoView(TokensList.Items[idx]);

                    // Focus the message field
                    if (t is TokenMsgDisp2)
                        GetGridAtPos(4, 1).Focus();
                    else if (t is TokenSystemMsg)
                        GetGridAtPos(3, 1).Focus();
                    else if (t is TokenSelectDisp2)
                        GetGridAtPos(3, 1).Focus();

                    return;
                }
            }

            MessageBox.Show("End of file. No more text.");
        }

        static UIElement GetGridAtPos(int row, int col)
        {
            foreach (UIElement e in Grid.Children)
            {
                if (Grid.GetRow(e) == row && Grid.GetColumn(e) == col)
                    return e;
            }
            return null;
        }
    }
}
