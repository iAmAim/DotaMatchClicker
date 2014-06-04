using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;


namespace ConsoleApplication2
{
    class Program
    {
        static string proc = "OUTLOOK";
		
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(HandleRef hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr WindowHandle);
        public const int SW_RESTORE = 9;

        /* For chekcing windows event*/
        [DllImport("user32.dll", EntryPoint = "SetWinEventHook", SetLastError = true)]
        private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        // Callback function
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        // Enums
        public enum EventContants : uint
        {
            EVENT_SYSTEM_SOUND = 0x1,
            EVENT_SYSTEM_ALERT = 0x2,
            EVENT_SYSTEM_FOREGROUND = 0x3,
            EVENT_SYSTEM_MENUSTART = 0x4,
            EVENT_SYSTEM_MENUEND = 0x5,
            EVENT_SYSTEM_MENUPOPUPSTART = 0x6,
            EVENT_SYSTEM_MENUPOPUPEND = 0x7,
            EVENT_SYSTEM_CAPTURESTART = 0x8,
            EVENT_SYSTEM_CAPTUREEND = 0x9,
            EVENT_SYSTEM_MOVESIZESTART = 0xa,
            EVENT_SYSTEM_MOVESIZEEND = 0xb,
            EVENT_SYSTEM_CONTEXTHELPSTART = 0xc,
            EVENT_SYSTEM_CONTEXTHELPEND = 0xd,
            EVENT_SYSTEM_DRAGDROPSTART = 0xe,
            EVENT_SYSTEM_DRAGDROPEND = 0xf,
            EVENT_SYSTEM_DIALOGSTART = 0x10,
            EVENT_SYSTEM_DIALOGEND = 0x11,
            EVENT_SYSTEM_SCROLLINGSTART = 0x12,
            EVENT_SYSTEM_SCROLLINGEND = 0x13,
            EVENT_SYSTEM_SWITCHSTART = 0x14,
            EVENT_SYSTEM_SWITCHEND = 0x15,
            EVENT_SYSTEM_MINIMIZESTART = 0x16,
            EVENT_SYSTEM_MINIMIZEEND = 0x17
        }
   


        [Flags]
        internal enum SetWinEventHookParameter
        {
            WINEVENT_OUTOFCONTEXT = 0,
            WINEVENT_SKIPOWNTHREAD = 1,
            WINEVENT_SKIPOWNPROCESS = 2,
            WINEVENT_INCONTEXT = 4
        }

        private static void FocusProcess(string procName)
        {
            Process[] objProcesses = Process.GetProcessesByName(procName);
            bool processSet= false;
            IntPtr hWnd = IntPtr.Zero;

            while (!processSet)
            {
                if (objProcesses.Length > 0)
                {
                    /* TODO: If process gets an event system alert, focus the process*/

                    if (ProcessNeedsAttention(hWnd))
                    {
                        hWnd = objProcesses[0].MainWindowHandle;
                        ShowWindowAsync(new HandleRef(null, hWnd), SW_RESTORE);
                        SetForegroundWindow(objProcesses[0].MainWindowHandle);
                        processSet = true;
                    }


                }
            }
  
        }

        static void Main(string[] args)
        {
            //Process

           // CheckProcessEvent();
            FocusProcess(proc);
            

             SomeRandomStuff();

            // Console.ReadLine();
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        /************* Test Event check*********************************************/
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        public enum FlashWindow : uint
        {
            /// <summary>
            /// Stop flashing. The system restores the window to its original state. 
            /// </summary>    
            FLASHW_STOP = 0,

            /// <summary>
            /// Flash the window caption 
            /// </summary>
            FLASHW_CAPTION = 1,

            /// <summary>
            /// Flash the taskbar button. 
            /// </summary>
            FLASHW_TRAY = 2,

            /// <summary>
            /// Flash both the window caption and taskbar button.
            /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags. 
            /// </summary>
            FLASHW_ALL = 3,

            /// <summary>
            /// Flash continuously, until the FLASHW_STOP flag is set.
            /// </summary>
            FLASHW_TIMER = 4,

            /// <summary>
            /// Flash continuously until the window comes to the foreground. 
            /// </summary>
            FLASHW_TIMERNOFG = 12

        }

        static bool ProcessNeedsAttention(IntPtr hWnd)
        {
            /* if windows is flashing return true */
            FLASHWINFO fInfo = new FLASHWINFO();
            fInfo.hwnd = hWnd;

            if (fInfo.dwFlags == 0) {
                Console.WriteLine("Focus set to {0}!", proc);
                return true; 
            }
            else {

                Console.WriteLine("Focus not set to {0} because there is no flash info recorded.", proc);
                return false;
            }
            
        }


        private static WinEventDelegate winEventProc;
        private static SetWinEventHookParameter m_hhook;

        public static void CheckProcessEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            IntPtr m_hhook;
            winEventProc = new WinEventDelegate(WinEventProc);
            var handle = SetWinEventHook((uint)EventContants.EVENT_SYSTEM_FOREGROUND, (uint)EventContants.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero,
                 winEventProc, 0, 0, (uint)(SetWinEventHookParameter.WINEVENT_OUTOFCONTEXT | SetWinEventHookParameter.WINEVENT_SKIPOWNPROCESS));

            m_hhook = SetWinEventHook((uint)EventContants.EVENT_SYSTEM_FOREGROUND, (uint)EventContants.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventProc, 0, 0,
               (uint)(SetWinEventHookParameter.WINEVENT_OUTOFCONTEXT));
            
        }

        static void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            if (eventType == (uint)EventContants.EVENT_SYSTEM_FOREGROUND)
            {
                StringBuilder sb = new StringBuilder(500);
                GetWindowText(hwnd, sb, sb.Capacity);
            }
        }

       

        /************* Test Event check****************************************************/

        public static void SomeRandomStuff()
        {
            /* access ac = new access();
           Console.Write("Enter a string:\t");
           ac.str = Console.ReadLine();
           ac.print();

           //Bubble Sort
           int[] num = new int[]{5,4,3,2,1};

           Console.Write("Num contents:\n");
           foreach (var item in num)
               Console.Write(item.ToString() + " ");
           Sorting sort = new Sorting();
           sort.BubbleSort(num); 
           Console.Write("\nNum contents after sorting:\n");
           foreach (var item in num)
               Console.Write(item.ToString() + " ");
           Class1 classone = new Class1();
           classone.Hello(); */
        }
     
    }



    class access
    {
        // String Variable declared as public
        public string str;
        // Public method
        public void print()
        {
            Console.WriteLine("\nYour string is " + "\"" + str + "\"");
        }
    }
    


}
