
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " DwmWindowAttribute "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Enums

    ''' <summary>
    ''' Specifies the Desktop Window Manager (DWM) window attributes used by
    ''' <see cref="NativeMethods.DwmSetWindowAttribute"/> function.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/ne-dwmapi-dwmwindowattribute"/>
    ''' </remarks>
    Public Enum DwmWindowAttribute As Integer

        ''' <summary>
        ''' Undocumented value of <see cref="DwmWindowAttribute.UseImmersiveDarkMode"/>
        ''' used by Windows 10 versions 1809 to 1909 (builds 17763 to 18363).
        ''' </summary>
        UseImmersiveDarkModeBefore20H1 = 19

        ''' <summary>
        ''' DWMWA_USE_IMMERSIVE_DARK_MODE.
        ''' <para></para>
        ''' Allows the window frame for this window to be drawn in dark mode colors when the dark mode system setting is enabled.
        ''' <para></para>
        ''' Supported starting with Windows 10 version 2004 (build 19041) and Windows 11.
        ''' </summary>
        UseImmersiveDarkMode = 20

    End Enum

End Namespace

#End Region
