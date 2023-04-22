using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpPrompt
{
    internal class Api
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREDUI_INFOW
        {
            public int cbSize;
            public IntPtr hwndParent;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszMessageText;
            [MarshalAs(UnmanagedType.LPWStr)] public string pszCaptionText;
            public IntPtr hbmBanner;
        }

        [Flags]
        public enum DWMWINDOWATTRIBUTE : uint
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Cloak,
            Cloaked,
            FreezeRepresentation,
            PassiveUpdateMode,
            UseHostBackdropBrush,
            UseImmersiveDarkMode = 20,
            WindowCornerPreference = 33,
            BorderColor,
            CaptionColor,
            TextColor,
            VisibleFrameBorderThickness,
            SystemBackdropType,
            Last
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LogonUserA(
            string lpszUsername,
            string lpszDomain,
            string lpszPassword,
            LogonType dwLogonType,
            LogonProvider dwLogonProvider,
            out IntPtr phToken
        );


        [Flags]
        public enum LogonProvider
        {
            LOGON32_PROVIDER_DEFAULT = 0,
            LOGON32_PROVIDER_WINNT35 = 1,
            LOGON32_PROVIDER_WINNT40 = 2,
            LOGON32_PROVIDER_WINNT50 = 3
        }

        [Flags]
        public enum LogonType
        {
            LOGON32_LOGON_INTERACTIVE = 2,
            LOGON32_LOGON_NETWORK = 3,
            LOGON32_LOGON_BATCH = 4,
            LOGON32_LOGON_SERVICE = 5,
            LOGON32_LOGON_UNLOCK = 7,
            LOGON32_LOGON_NETWORK_CLEARTEXT = 8,
            LOGON32_LOGON_NEW_CREDENTIALS = 9,
        }

        [DllImport("ole32.dll")]
        public static extern void CoTaskMemFree(IntPtr ptr);


        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        public static extern bool CredUnPackAuthenticationBuffer(int dwFlags, IntPtr pAuthBuffer, uint cbAuthBuffer, StringBuilder pszUserName, ref int pcchMaxUserName, StringBuilder pszDomainName, ref int pcchMaxDomainame, StringBuilder pszPassword, ref int pcchMaxPassword);

        [Flags]
        public enum PromptForWindowsCredentialsFlags
        {
            CREDUIWIN_GENERIC = 0x1,
            CREDUIWIN_CHECKBOX = 0x2,
            CREDUIWIN_AUTHPACKAGE_ONLY = 0x10,
            CREDUIWIN_IN_CRED_ONLY = 0x20,
            CREDUIWIN_ENUMERATE_ADMINS = 0x100,
            CREDUIWIN_ENUMERATE_CURRENT_USER = 0x200,
            CREDUIWIN_SECURE_PROMPT = 0x1000,
            CREDUIWIN_PACK_32_WOW = 0x10000000,
        }

        [DllImport("credui.dll", CharSet = CharSet.Unicode)]
        public static extern uint CredUIPromptForWindowsCredentialsW(ref CREDUI_INFOW notUsedHere,
        int authError,
        ref uint authPackage,
        IntPtr InAuthBuffer,
        uint InAuthBufferSize,
        out IntPtr refOutAuthBuffer,
        out uint refOutAuthBufferSize,
        ref bool fSave,
        PromptForWindowsCredentialsFlags flags);

        [DllImport("credui.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredPackAuthenticationBuffer(
        int dwFlags,
        string pszUserName,
        string pszPassword,
        IntPtr pPackedCredentials,
        ref int pcbPackedCredentials);
    }
}
