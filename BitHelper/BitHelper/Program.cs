/******************************************************************************************************
 * Author:          Daniel Stotts
 * Version:         1.0.1.8
 * Version Date:    12/2/2016
 * Utility Title:   BitHelper
 * Description:     Aids in visualization of hexadecimal strings
 *****************************************************************************************************/

using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BitHelper
{
    internal class Program
    {
        [STAThreadAttribute]
        public static void Main(string[] args)
        {
            const string version = "1.0.1.8";
            const string date = "12/2/2016";

            const int requestWidth = 111;
            const int requestHeight = 29;

            int prevHeight = Console.WindowHeight;

            bool quickHelp = true;

            const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

            Console.Title = "BitHelper " + version;

            if (Console.WindowHeight < requestHeight) Console.SetWindowSize(Console.WindowWidth, requestHeight);
            if (Console.WindowWidth < requestWidth) Console.SetWindowSize(requestWidth, Console.WindowHeight);

            Regex preProcessorRepetitionRegex = new Regex("\\[(\\d+)x([\\da-f]+)\\]", regexOptions);
            Regex preProcessorIgnoreRegex = new Regex("_", regexOptions);
            Regex bitRegex = new Regex("^t\\s(\\d+)$", regexOptions);
            Regex setRegex = new Regex("^s\\s(\\d+)$", regexOptions);
            Regex clrRegex = new Regex("^c\\s(\\d+)$", regexOptions);
            Regex rangeRegex = new Regex("^t\\s(\\d+)-(\\d+)$", regexOptions);
            Regex rangeSetRegex = new Regex("^s\\s(\\d+)-(\\d+)$", regexOptions);
            Regex rangeClrRegex = new Regex("^c\\s(\\d+)-(\\d+)$", regexOptions);
            Regex stickySetRangeRegex = new Regex("^ss\\s(\\d+)-(\\d+)$", regexOptions);
            Regex stickyClrRangeRegex = new Regex("^sc\\s(\\d+)-(\\d+)$", regexOptions);
            Regex sizeRegex = new Regex("^size\\s(\\d+)$", regexOptions);
            Regex orRegex = new Regex("^or\\s([\\da-f]+)$", regexOptions);
            Regex xorRegex = new Regex("^xor\\s([\\da-f]+)$", regexOptions);
            Regex andRegex = new Regex("^and\\s([\\da-f]+)$", regexOptions);

            BitArray bits = new BitArray(32);
            string err = "none";

            while (true)
            {
                Console.Clear();

                if (quickHelp)
                {
                    Console.WriteLine("\n  Inputs:");
                    Console.WriteLine("  t      <x> : toggle bit at position x");
                    Console.WriteLine("  t  <x>-<y> : toggle bits at positions x-y");
                    Console.WriteLine("  s      <x> : set bit at position x");
                    Console.WriteLine("  s  <x>-<y> : set bits at positions x-y");
                    Console.WriteLine("  c      <x> : clear bit at position x");
                    Console.WriteLine("  c  <x>-<y> : clear bits at positions x-y");
                    Console.WriteLine("  ss <x>-<y> : sticky set bits at x-y");
                    Console.WriteLine("  sc <x>-<y> : sticky clr bits at x-y");
                    Console.WriteLine("  or     <x> : bitwise OR with hexadecimal string x");
                    Console.WriteLine("  xor    <x> : bitwise XOR with hexadecimal string x");
                    Console.WriteLine("  and    <x> : bitwise AND with hexadecimal string x");
                    Console.WriteLine("  not        : invert entire string");
                    Console.WriteLine("  size   <x> : resize string to x number of bytes");
                    Console.WriteLine("  set        : set all bits");
                    Console.WriteLine("  clr        : clear all bits");
                    Console.WriteLine("  qh         : toggle quick help menu");
                    Console.WriteLine("  bits       : view bit schema");
                    Console.WriteLine("  help       : display help menu");
                    Console.WriteLine("  copy       : copy string to clipboard");
                    Console.WriteLine("  quit       : quit");
                    Console.WriteLine("  [<q>x<a>]  : repeat string a, q number of times");
                }
                else
                {
                    Console.WriteLine("\n  Inputs:");
                    Console.WriteLine("  qh         : toggle quick help menu");
                    Console.WriteLine("  help       : display help menu");
                    Console.WriteLine("  quit       : quit");
                }

                Console.WriteLine("\n  Current string: " + FormatHex(ToHex(bits)));
                Console.WriteLine("\n  Error: " + err);

                Console.Write("  Requested input: ");

                string read = Console.ReadLine();
                if (string.IsNullOrEmpty(read))
                {
                    err = "no input";
                    continue;
                }

                err = "none";

                int lo, hi;
                if (preProcessorRepetitionRegex.IsMatch(read))
                {
                    MatchCollection matches = preProcessorRepetitionRegex.Matches(read);
                    foreach (Match m in matches)
                    {

                        if (!int.TryParse(m.Groups[1].Value, out lo))
                        {
                            err = "Invalid quantity!";
                            break;
                        }

                        string a = "";
                        string r = m.Groups[2].Value;
                        for (int i = 0; i < lo; ++i)
                        {
                            a = a.Insert(0, r);
                        }

                        read = read.Replace(m.Value, a);
                    }
                }

                if (preProcessorIgnoreRegex.IsMatch(read))
                {
                    MatchCollection matches = preProcessorIgnoreRegex.Matches(read);
                    read = matches.Cast<Match>().Aggregate(read, (current, m) => current.Replace(m.Value, ""));
                }

                if (bitRegex.IsMatch(read))
                {
                    if (!int.TryParse(bitRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid index";
                        continue;
                    }
                    if (lo > bits.Length - 1)
                    {
                        err = "index outside of size boundary";
                        continue;
                    }
                    bits.Set(lo, !bits[lo]);
                    continue;
                }

                if (setRegex.IsMatch(read))
                {
                    if (!int.TryParse(setRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid index";
                        continue;
                    }
                    if (lo > bits.Length - 1)
                    {
                        err = "index outside of size boundary";
                        continue;
                    }
                    bits.Set(lo, true);
                    continue;
                }

                if (clrRegex.IsMatch(read))
                {
                    if (!int.TryParse(clrRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid index";
                        continue;
                    }
                    if (lo > bits.Length - 1)
                    {
                        err = "index outside of size boundary";
                        continue;
                    }
                    bits.Set(lo, false);
                    continue;
                }

                if (rangeRegex.IsMatch(read))
                {
                    if (!int.TryParse(rangeRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid range";
                        continue; 
                    }
                    if (!int.TryParse(rangeRegex.Match(read).Groups[2].Value, out hi))
                    {
                        err = "invalid range";
                        continue;
                    }

                    if (lo > hi)
                    {
                        lo = lo ^ hi;
                        hi = hi ^ lo;
                        lo = lo ^ hi;
                    }

                    int range = hi - lo;
                    if (lo + range > bits.Length - 1)
                    {
                        err = "range exceeds size boundary";
                        continue;
                    }

                    for (int i = lo; i <= lo + range; ++i)
                    {
                        bits.Set(i, !bits[i]);
                    }
                    continue;
                }

                if (rangeSetRegex.IsMatch(read))
                {
                    if (!int.TryParse(rangeSetRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid range";
                        continue;
                    }
                    if (!int.TryParse(rangeSetRegex.Match(read).Groups[2].Value, out hi))
                    {
                        err = "invalid range";
                        continue;
                    }

                    if (lo > hi)
                    {
                        lo = lo ^ hi;
                        hi = hi ^ lo;
                        lo = lo ^ hi;
                    }

                    int range = hi - lo;
                    if (lo + range > bits.Length - 1)
                    {
                        err = "range exceeds size boundary";
                        continue;
                    }

                    for (int i = lo; i <= lo + range; ++i)
                    {
                        bits.Set(i, true);
                    }
                    continue;
                }

                if (rangeClrRegex.IsMatch(read))
                {
                    if (!int.TryParse(rangeClrRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid range";
                        continue;
                    }
                    if (!int.TryParse(rangeClrRegex.Match(read).Groups[2].Value, out hi))
                    {
                        err = "invalid range";
                        continue;
                    }

                    if (lo > hi)
                    {
                        lo = lo ^ hi;
                        hi = hi ^ lo;
                        lo = lo ^ hi;
                    }

                    int range = hi - lo;
                    if (lo + range > bits.Length - 1)
                    {
                        err = "range exceeds size boundary";
                        continue;
                    }

                    for (int i = lo; i <= lo + range; ++i)
                    {
                        bits.Set(i, false);
                    }
                    continue;
                }

                if (stickySetRangeRegex.IsMatch(read))
                {
                    if (!int.TryParse(stickySetRangeRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid range";
                        continue;
                    }
                    if (!int.TryParse(stickySetRangeRegex.Match(read).Groups[2].Value, out hi))
                    {
                        err = "invalid range";
                        continue;
                    }

                    if (lo > hi)
                    {
                        lo = lo ^ hi;
                        hi = hi ^ lo;
                        lo = lo ^ hi;
                    }

                    int range = hi - lo;
                    if (lo + range > bits.Length - 1)
                    {
                        err = "range exceeds size boundary";
                        continue;
                    }

                    bool flag = false;
                    for (int i = lo; i <= lo + range && !flag; ++i)
                    {
                        if (!bits[i]) flag = true;
                    }
                    for (int i = lo; i <= lo + range; ++i)
                    {
                        bits.Set(i, flag);
                    }
                    continue;
                }

                if (stickyClrRangeRegex.IsMatch(read))
                {
                    if (!int.TryParse(stickyClrRangeRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid range";
                        continue;
                    }
                    if (!int.TryParse(stickyClrRangeRegex.Match(read).Groups[2].Value, out hi))
                    {
                        err = "invalid range";
                        continue;
                    }

                    if (lo > hi)
                    {
                        lo = lo ^ hi;
                        hi = hi ^ lo;
                        lo = lo ^ hi;
                    }

                    int range = hi - lo;
                    if (lo + range > bits.Length - 1)
                    {
                        err = "range exceeds size boundary";
                        continue;
                    }

                    bool flag = true;
                    for (int i = lo; i <= lo + range && flag; ++i)
                    {
                        if (bits[i]) flag = false;
                    }
                    for (int i = lo; i <= lo + range; ++i)
                    {
                        bits.Set(i, flag);
                    }
                    continue;
                }

                if (sizeRegex.IsMatch(read))
                {
                    if (!int.TryParse(sizeRegex.Match(read).Groups[1].Value, out lo))
                    {
                        err = "invalid size";
                        continue;
                    }
                    if (lo < 1 || lo > 32)
                    {
                        err = "invalid size (valid range: 1 - 32)";
                        continue;
                    }

                    bits = new BitArray(lo*8);
                    continue;
                }

                if (orRegex.IsMatch(read))
                {
                    string s = orRegex.Match(read).Groups[1].Value;
                    if (s.Length > bits.Length/4)
                    {
                        err = "input larger than size boundary";
                        continue;
                    }

                    s = s.PadLeft(bits.Length/4, '0');
                    BitArray b = ToBitArray(s);

                    if (b == null)
                    {
                        err = "could not convert input";
                        continue;
                    }

                    bits.Or(b);
                    continue;
                }

                if (xorRegex.IsMatch(read))
                {
                    string s = xorRegex.Match(read).Groups[1].Value;
                    if (s.Length > bits.Length / 4)
                    {
                        err = "input larger than size boundary";
                        continue;
                    }

                    s = s.PadLeft(bits.Length / 4, '0');
                    BitArray b = ToBitArray(s);

                    if (b == null)
                    {
                        err = "could not convert input";
                        continue;
                    }

                    bits.Xor(b);
                    continue;
                }

                if (andRegex.IsMatch(read))
                {
                    string s = andRegex.Match(read).Groups[1].Value;
                    if (s.Length > bits.Length / 4)
                    {
                        err = "input larger than size boundary";
                        continue;
                    }

                    s = s.PadLeft(bits.Length / 4, '0');
                    BitArray b = ToBitArray(s);

                    if (b == null)
                    {
                        err = "could not convert input";
                        continue;
                    }

                    bits.And(b);
                    continue;
                }

                if (read.ToLower() == "clr" || read.ToLower() == "cls" || read.ToLower() == "clear")
                {
                    bits.SetAll(false);
                    continue;
                }

                if (read.ToLower() == "set")
                {
                    bits.SetAll(true);
                    continue;
                }

                if (read.ToLower() == "not")
                {
                    bits.Not();
                    continue;
                }

                if (read.ToLower() == "qh" || read.ToLower() == "quick")
                {
                    int qhLines = 11;
                    quickHelp = !quickHelp;

                    if (quickHelp)
                    {
                        if (Console.WindowHeight < requestHeight) Console.SetWindowSize(Console.WindowWidth, requestHeight);
                        if (Console.WindowWidth < requestWidth) Console.SetWindowSize(requestWidth, Console.WindowHeight);
                    }
                    else if (Console.WindowHeight > prevHeight)
                    {
                        Console.SetWindowSize(Console.WindowWidth, prevHeight > qhLines ? prevHeight : qhLines);
                    }

                    continue;
                }

                if (read.ToLower() == "bits")
                {
                    int lines = bits.Length / 8 + 6;
                    int prev = Console.WindowHeight;

                    if (Console.WindowHeight < lines)
                    {
                        Console.SetWindowSize(Console.WindowWidth, lines);
                    }

                    Console.Clear();
                    Console.WriteLine("\n  Current string: " + FormatHex(ToHex(bits)));
                    Console.WriteLine();

                    int l = bits.Length;
                    while (l > 0)
                    {
                        Console.Write("  ");
                        for (int i = 0; i < 8; ++i)
                        {
                            l -= 1;

                            Console.Write("Bit " 
                                + (l < 10 ? " " : "") 
                                + l + ": " 
                                + (bits.Length > 99 && l < 100 ? " " : "") 
                                + (bits[l] ? 1 : 0));

                            Console.Write(l > 99  
                                || (bits.Length > 99 && l < 100) ? "   " : "    ");
                        }
                        Console.WriteLine();
                    }

                    Console.Write("\n  Press any key to return. ");
                    Console.ReadKey();

                    Console.WindowHeight = prev;
                    continue;
                }

                if (read.ToLower() == "help")
                {
                    int lines = 43;
                    int prev = Console.WindowHeight;

                    if (Console.WindowHeight < lines)
                    {
                        Console.SetWindowSize(Console.WindowWidth, lines);
                    }

                    Console.Clear();
                    Console.WriteLine("\n  Author:            Daniel Stotts");
                    Console.WriteLine("  Version:           " + version);
                    Console.WriteLine("  Version Date:      " + date);
                    Console.WriteLine("  Utility Title:     BitHelper");
                    Console.WriteLine("  Description:       Aids in visualization of hexadecimal strings");
                    Console.WriteLine("\n  Inputs:");
                    Console.WriteLine("  Command:    Expanded name:    Parameter(s):    Description:");
                    Console.WriteLine("  t           toggle            <x>              Toggle the bit at position x.");
                    Console.WriteLine("  t           toggle            <x>-<y>          Toggle bits at positions x through y.");
                    Console.WriteLine("  s           set               <x>              Set the bit at position x.");
                    Console.WriteLine("  s           set               <x>-<y>          Set bits at positions x through y.");
                    Console.WriteLine("  c           clear             <x>              Clear the bit at position x.");
                    Console.WriteLine("  c           clear             <x>-<y>          Clear bits at positions x through y.");
                    Console.WriteLine("  ss          sticky set        <x>-<y>          Evaluate range x-y: If any bits vary within range,");
                    Console.WriteLine("                                                 set all bits in range. Else, toggle entire range.");
                    Console.WriteLine("  sc          sticky clear      <x>-<y>          Evaluate range x-y: If any bits vary within range,");
                    Console.WriteLine("                                                 clear all bits in range. Else, toggle entire range.");
                    Console.WriteLine("  or          or                <0xValue>        Bitwise OR with hexadecimal string 0xValue.");
                    Console.WriteLine("  xor         xor               <0xValue>        Bitwise XOR with hexadecimal string 0xValue.");
                    Console.WriteLine("  and         and               <0xValue>        Bitwise AND with hexadecimal string 0xValue.");
                    Console.WriteLine("  not         not                                Invert entire bit string.");
                    Console.WriteLine("  size        byte size         <x>              Set size (in bytes) of hexadecimal output string.");
                    Console.WriteLine("  set         set all                            Set all bits.");
                    Console.WriteLine("  clr         clear all                          Clear all bits.");
                    Console.WriteLine("  qh          quick help                         Toggle quick help menu.");
                    Console.WriteLine("  bits        bit schema                         View bit layout model.");
                    Console.WriteLine("  help        help/ about                        Display help menu.");
                    Console.WriteLine("  copy        copy                               Copy hexadecimal string to clipboard.");
                    Console.WriteLine("  quit        quit                               Exit program.");
                    Console.WriteLine("\n  Preprocess:");
                    Console.WriteLine("  [<q>x<a>]   repetition        <q>, <a>         Repeat string a quantity q times.");
                    Console.WriteLine("  Examples:   or A[4xF]123, or [4x01], or [1x0][2xA01]8, or [2x01][1xF]A[0xC][2x0]");

                    Console.WriteLine("\n  Notes: ");
                    Console.WriteLine("  Underscore characters are ignored. The input 'or 1234_0000' is equivalent to 'or 12340000'.");
                    Console.WriteLine("  Range value order does not matter. The input 'sc 4-7' is equivalent to 'sc 7-4'.");
                    Console.WriteLine("  Some alias commands are included:  clear = cls = clr, exit = q = quit, quick = qh");
                     
                    Console.Write("\n  Press any key to return. ");
                    Console.ReadKey();

                    Console.WindowHeight = prev;
                    continue;
                }

                if (read.ToLower() == "copy")
                {
                    Clipboard.SetText(ToHex(bits));
                    continue;
                }

                if (read.ToLower() == "quit" || read.ToLower() == "exit" || read.ToLower() == "q")
                {
                    return;
                }

                err = "unrecognized input";
            }
        }

        public static string ToHex(BitArray bits)
        {
            if (bits.Length % 4 != 0) return null;

            StringBuilder sb = new StringBuilder(bits.Length/4);

            for (int i = bits.Length - 1; i >= 0; i -= 4)
            {
                int v = (bits[i] ? 8 : 0) |
                        (bits[i - 1] ? 4 : 0) |
                        (bits[i - 2] ? 2 : 0) |
                        (bits[i - 3] ? 1 : 0);

                sb.Append(v.ToString("X1"));
            }

            return sb.ToString();
        }

        public static string FormatHex(string a)
        {
            StringBuilder sb = new StringBuilder(a);

            for (int i = sb.Length - 4; i > 0; i -= 4)
            {
                sb.Insert(i, '_');
            }
            if (a.Length % 4 == 2)
            {
                return "xx" + sb;
            }

            return sb.ToString();
        }

        public static BitArray ToBitArray(string a)
        {
            if (a == null) return null;

            BitArray r = new BitArray(4 * a.Length);
            for (int i = a.Length; i > 0; --i)
            {
                byte b = byte.Parse(a[a.Length - i].ToString(), NumberStyles.HexNumber);
                for (int ii = 0; ii < 4; ++ii)
                {
                    int loc = i*4 - ii - 1;
                    bool v = (b & (1 << (3 - ii))) == 1 << (3 - ii);
                    r.Set(loc, v);
               }
            }

            return r;
        }
    }
}
