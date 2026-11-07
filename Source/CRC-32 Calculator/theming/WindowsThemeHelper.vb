
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Runtime.InteropServices

Imports Microsoft.Win32

Imports DevCase.Win32.Enums
Imports DevCase.Win32.NativeMethods

#End Region

#Region " WindowsThemeHelper "

''' <summary>
''' Provides helper methods to detect the current Windows app theme (light or dark)
''' and to apply it to the non-client area (title bar) of a window.
''' </summary>
Friend Module WindowsThemeHelper

#Region " Constants "

    ''' <summary>
    ''' The registry key (under HKEY_CURRENT_USER) that stores the Windows personalization settings.
    ''' </summary>
    Friend Const PersonalizeRegistryKeyPath As String = "Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"

    ''' <summary>
    ''' The registry value that stores the Windows app theme: 0 for dark mode, 1 for light mode.
    ''' <para></para>
    ''' This value does not exist in Windows versions prior to Windows 10.
    ''' </summary>
    Friend Const AppsUseLightThemeValueName As String = "AppsUseLightTheme"

    ''' <summary>
    ''' WM_SETTINGCHANGE message.
    ''' <para></para>
    ''' Sent to all top-level windows when a system-wide setting changes (including the Windows app theme).
    ''' </summary>
    Friend Const WM_SETTINGCHANGE As Integer = &H1A

    ''' <summary>
    ''' The <c>lParam</c> string of a <see cref="WM_SETTINGCHANGE"/> message sent when the Windows app theme changes.
    ''' </summary>
    Friend Const ImmersiveColorSetParameter As String = "ImmersiveColorSet"

    ''' <summary>
    ''' SPI_SETHIGHCONTRAST.
    ''' <para></para>
    ''' The <c>wParam</c> value of a <see cref="WM_SETTINGCHANGE"/> message sent when the high contrast mode changes.
    ''' </summary>
    Friend Const SPI_SETHIGHCONTRAST As Integer = &H43

    ''' <summary>
    ''' WM_NCACTIVATE message.
    ''' <para></para>
    ''' Sent to a window when its non-client area needs to be changed to indicate an active or inactive state.
    ''' </summary>
    Private Const WM_NCACTIVATE As Integer = &H86

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Determines whether the Windows app theme of the current user is dark.
    ''' </summary>
    '''
    ''' <remarks>
    ''' Returns <see langword="False"/> when high contrast mode is active (system colors must be respected),
    ''' and in Windows versions that do not support the dark app theme.
    ''' </remarks>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the Windows app theme is dark; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function IsDarkThemeEnabled() As Boolean

        Dim appsUseLightThemeValue As Object
        Using key As RegistryKey = Registry.CurrentUser.OpenSubKey(WindowsThemeHelper.PersonalizeRegistryKeyPath, writable:=False)
            appsUseLightThemeValue = key?.GetValue(WindowsThemeHelper.AppsUseLightThemeValueName)
        End Using

        Return WindowsThemeHelper.IsDarkTheme(appsUseLightThemeValue, SystemInformation.HighContrast)
    End Function

    ''' <summary>
    ''' Determines whether the specified theme settings represent a dark theme.
    ''' </summary>
    '''
    ''' <param name="appsUseLightThemeValue">
    ''' The data of the <c>AppsUseLightTheme</c> registry value,
    ''' or <see langword="Nothing"/> if the value does not exist (Windows versions prior to Windows 10).
    ''' </param>
    '''
    ''' <param name="isHighContrast">
    ''' <see langword="True"/> if high contrast mode is active.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the settings represent a dark theme; otherwise, <see langword="False"/>.
    ''' </returns>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' The registry value has an unexpected data type (it must be a DWORD).
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function IsDarkTheme(appsUseLightThemeValue As Object, isHighContrast As Boolean) As Boolean

        ' High contrast themes must always be rendered with system colors.
        If isHighContrast Then
            Return False
        End If

        ' The value does not exist: the dark app theme is not supported by this Windows version.
        If appsUseLightThemeValue Is Nothing Then
            Return False
        End If

        If Not TypeOf appsUseLightThemeValue Is Integer Then
            Throw New InvalidOperationException(
                $"The registry value '{WindowsThemeHelper.AppsUseLightThemeValueName}' has an unexpected data type: {appsUseLightThemeValue.GetType().FullName}. A DWORD value was expected.")
        End If

        Return DirectCast(appsUseLightThemeValue, Integer) = 0
    End Function

    ''' <summary>
    ''' Determines whether the specified window message notifies a change of the Windows app theme
    ''' or of the high contrast mode.
    ''' </summary>
    '''
    ''' <param name="m">
    ''' The window message.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the message notifies a theme change; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function IsThemeChangeMessage(m As Message) As Boolean

        If m.Msg <> WindowsThemeHelper.WM_SETTINGCHANGE Then
            Return False
        End If

        If m.WParam.ToInt64() = WindowsThemeHelper.SPI_SETHIGHCONTRAST Then
            Return True
        End If

        If m.LParam = IntPtr.Zero Then
            Return False
        End If

        Dim parameter As String = Marshal.PtrToStringUni(m.LParam)
        Return String.Equals(parameter, WindowsThemeHelper.ImmersiveColorSetParameter, StringComparison.Ordinal)
    End Function

    ''' <summary>
    ''' Enables or disables the dark mode of the title bar (non-client area) of the specified form,
    ''' through the <c>DWMWA_USE_IMMERSIVE_DARK_MODE</c> attribute of the Desktop Window Manager.
    ''' </summary>
    '''
    ''' <remarks>
    ''' This method must be called from <c>OnHandleCreated</c> (or later),
    ''' because it requires the window handle.
    ''' Calling it from the <see cref="Form.Load"/> event has no effect on the title bar.
    ''' </remarks>
    '''
    ''' <param name="targetForm">
    ''' The target form.
    ''' </param>
    '''
    ''' <param name="useDarkMode">
    ''' <see langword="True"/> to draw the title bar with dark colors; <see langword="False"/> to draw it with light colors.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if the attribute was applied;
    ''' <see langword="False"/> if the current Windows version does not support a dark title bar
    ''' (Windows versions prior to Windows 10 version 1809).
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' <paramref name="targetForm"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' The window handle of <paramref name="targetForm"/> has not been created yet.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function SetDarkTitleBar(targetForm As Form, useDarkMode As Boolean) As Boolean

        If targetForm Is Nothing Then
            Throw New ArgumentNullException(NameOf(targetForm))
        End If
        If Not targetForm.IsHandleCreated Then
            Throw New InvalidOperationException("The window handle has not been created yet. Call this method from OnHandleCreated or later.")
        End If

        Dim attributeValue As Integer = If(useDarkMode, 1, 0)
        Dim attributeSize As Integer = Marshal.SizeOf(GetType(Integer))

        ' Windows 10 version 2004 (build 19041) and newer, and Windows 11.
        Dim hResult As Integer = DwmApi.DwmSetWindowAttribute(targetForm.Handle, DwmWindowAttribute.UseImmersiveDarkMode, attributeValue, attributeSize)
        If hResult = 0 Then
            Return True
        End If

        ' Windows 10 versions 1809 to 1909.
        hResult = DwmApi.DwmSetWindowAttribute(targetForm.Handle, DwmWindowAttribute.UseImmersiveDarkModeBefore20H1, attributeValue, attributeSize)
        Return hResult = 0
    End Function

    ''' <summary>
    ''' Forces the repaint of the title bar (non-client area) of the specified form,
    ''' to reflect a title bar theme change while the form is already visible.
    ''' </summary>
    '''
    ''' <remarks>
    ''' Windows 10 does not repaint the title bar after changing the <c>DWMWA_USE_IMMERSIVE_DARK_MODE</c> attribute
    ''' of a visible window until its activation state changes,
    ''' so the activation state is toggled twice (keeping the current state as the final one).
    ''' </remarks>
    '''
    ''' <param name="targetForm">
    ''' The target form.
    ''' </param>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' <paramref name="targetForm"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="InvalidOperationException">
    ''' The window handle of <paramref name="targetForm"/> has not been created yet.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Sub RedrawTitleBar(targetForm As Form)

        If targetForm Is Nothing Then
            Throw New ArgumentNullException(NameOf(targetForm))
        End If
        If Not targetForm.IsHandleCreated Then
            Throw New InvalidOperationException("The window handle has not been created yet.")
        End If

        Dim isActive As Boolean = System.Windows.Forms.Form.ActiveForm Is targetForm
        User32.SendMessage(targetForm.Handle, WindowsThemeHelper.WM_NCACTIVATE, New IntPtr(If(isActive, 0, 1)), IntPtr.Zero)
        User32.SendMessage(targetForm.Handle, WindowsThemeHelper.WM_NCACTIVATE, New IntPtr(If(isActive, 1, 0)), IntPtr.Zero)
    End Sub

#End Region

End Module

#End Region
