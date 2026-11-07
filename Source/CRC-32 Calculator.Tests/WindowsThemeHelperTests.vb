#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Windows.Forms

#End Region

#Region " WindowsThemeHelperTests "

Namespace Crc32Calculator.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="WindowsThemeHelper"/> module.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class WindowsThemeHelperTests

#Region " IsDarkTheme "

        ''' <summary>
        ''' Verifies the interpretation of the <c>AppsUseLightTheme</c> registry value and the high contrast mode.
        ''' </summary>
        '''
        ''' <param name="appsUseLightTheme">
        ''' The registry value data.
        ''' </param>
        '''
        ''' <param name="isHighContrast">
        ''' A value indicating whether high contrast mode is active.
        ''' </param>
        '''
        ''' <param name="expected">
        ''' The expected result.
        ''' </param>
        <TestMethod>
        <DataRow(0, False, True, DisplayName:="Dark theme")>
        <DataRow(1, False, False, DisplayName:="Light theme")>
        <DataRow(0, True, False, DisplayName:="Dark theme with high contrast")>
        <DataRow(1, True, False, DisplayName:="Light theme with high contrast")>
        Public Sub IsDarkTheme_RegistryValue_ReturnsExpectedResult(appsUseLightTheme As Integer, isHighContrast As Boolean, expected As Boolean)

            Dim actual As Boolean = WindowsThemeHelper.IsDarkTheme(appsUseLightTheme, isHighContrast)

            Assert.AreEqual(expected, actual)
        End Sub

        ''' <summary>
        ''' Verifies that a missing registry value (Windows versions prior to Windows 10) means light theme.
        ''' </summary>
        <TestMethod>
        Public Sub IsDarkTheme_MissingRegistryValue_ReturnsFalse()

            Assert.IsFalse(WindowsThemeHelper.IsDarkTheme(Nothing, isHighContrast:=False))
        End Sub

        ''' <summary>
        ''' Verifies that a registry value with an unexpected data type throws <see cref="InvalidOperationException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub IsDarkTheme_UnexpectedRegistryValueType_ThrowsInvalidOperationException()

            Assert.ThrowsExactly(Of InvalidOperationException)(Sub() WindowsThemeHelper.IsDarkTheme("0", isHighContrast:=False))
        End Sub

        ''' <summary>
        ''' Verifies that the high contrast mode takes precedence, even over an invalid registry value.
        ''' </summary>
        <TestMethod>
        Public Sub IsDarkTheme_HighContrastWithInvalidValue_ReturnsFalse()

            Assert.IsFalse(WindowsThemeHelper.IsDarkTheme("0", isHighContrast:=True))
        End Sub

        ''' <summary>
        ''' Verifies that reading the current Windows theme does not throw.
        ''' </summary>
        <TestMethod>
        Public Sub IsDarkThemeEnabled_CurrentSystem_DoesNotThrow()

            WindowsThemeHelper.IsDarkThemeEnabled()
        End Sub

#End Region

#Region " IsThemeChangeMessage "

        ''' <summary>
        ''' Verifies the detection of the <c>WM_SETTINGCHANGE</c> notification sent when the Windows app theme changes.
        ''' </summary>
        '''
        ''' <param name="parameter">
        ''' The string passed in the <c>lParam</c> of the message.
        ''' </param>
        '''
        ''' <param name="expected">
        ''' The expected result.
        ''' </param>
        <TestMethod>
        <DataRow("ImmersiveColorSet", True)>
        <DataRow("immersivecolorset", False, DisplayName:="Different case")>
        <DataRow("Environment", False)>
        <DataRow("Policy", False)>
        Public Sub IsThemeChangeMessage_SettingChangeWithParameter_ReturnsExpectedResult(parameter As String, expected As Boolean)

            Dim parameterPtr As IntPtr = Marshal.StringToHGlobalUni(parameter)
            Try
                Dim m As Message = Message.Create(IntPtr.Zero, WindowsThemeHelper.WM_SETTINGCHANGE, IntPtr.Zero, parameterPtr)

                Assert.AreEqual(expected, WindowsThemeHelper.IsThemeChangeMessage(m))
            Finally
                Marshal.FreeHGlobal(parameterPtr)
            End Try
        End Sub

        ''' <summary>
        ''' Verifies that the high contrast change notification is detected.
        ''' </summary>
        <TestMethod>
        Public Sub IsThemeChangeMessage_HighContrastChange_ReturnsTrue()

            Dim m As Message = Message.Create(IntPtr.Zero, WindowsThemeHelper.WM_SETTINGCHANGE,
                                              New IntPtr(WindowsThemeHelper.SPI_SETHIGHCONTRAST), IntPtr.Zero)

            Assert.IsTrue(WindowsThemeHelper.IsThemeChangeMessage(m))
        End Sub

        ''' <summary>
        ''' Verifies that a <c>WM_SETTINGCHANGE</c> message without parameter string is not a theme change.
        ''' </summary>
        <TestMethod>
        Public Sub IsThemeChangeMessage_SettingChangeWithoutParameter_ReturnsFalse()

            Dim m As Message = Message.Create(IntPtr.Zero, WindowsThemeHelper.WM_SETTINGCHANGE, IntPtr.Zero, IntPtr.Zero)

            Assert.IsFalse(WindowsThemeHelper.IsThemeChangeMessage(m))
        End Sub

        ''' <summary>
        ''' Verifies that other window messages are never a theme change, even with the same parameter string.
        ''' </summary>
        <TestMethod>
        Public Sub IsThemeChangeMessage_OtherMessage_ReturnsFalse()

            Dim parameterPtr As IntPtr = Marshal.StringToHGlobalUni(WindowsThemeHelper.ImmersiveColorSetParameter)
            Try
                Const WM_PAINT As Integer = &HF
                Dim m As Message = Message.Create(IntPtr.Zero, WM_PAINT, IntPtr.Zero, parameterPtr)

                Assert.IsFalse(WindowsThemeHelper.IsThemeChangeMessage(m))
            Finally
                Marshal.FreeHGlobal(parameterPtr)
            End Try
        End Sub

#End Region

#Region " SetDarkTitleBar / RedrawTitleBar "

        ''' <summary>
        ''' Verifies that a null form throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub SetDarkTitleBar_NullForm_ThrowsArgumentNullException()

            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() WindowsThemeHelper.SetDarkTitleBar(Nothing, useDarkMode:=True))
        End Sub

        ''' <summary>
        ''' Verifies that calling the method before the window handle exists (e.g. too early)
        ''' throws <see cref="InvalidOperationException"/> instead of silently doing nothing.
        ''' </summary>
        <TestMethod>
        Public Sub SetDarkTitleBar_FormWithoutHandle_ThrowsInvalidOperationException()

            Using f As New Form()
                Assert.IsFalse(f.IsHandleCreated, "Precondition: the handle must not exist.")

                Assert.ThrowsExactly(Of InvalidOperationException)(Sub() WindowsThemeHelper.SetDarkTitleBar(f, useDarkMode:=True))
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that enabling and disabling the dark title bar of a real window does not throw
        ''' (the result depends on the Windows version).
        ''' </summary>
        <TestMethod>
        Public Sub SetDarkTitleBar_FormWithHandle_DoesNotThrow()

            Using f As New Form()
                Dim handle As IntPtr = f.Handle ' Forces the handle creation.
                Assert.AreNotEqual(IntPtr.Zero, handle)

                WindowsThemeHelper.SetDarkTitleBar(f, useDarkMode:=True)
                WindowsThemeHelper.SetDarkTitleBar(f, useDarkMode:=False)
                WindowsThemeHelper.RedrawTitleBar(f)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that redrawing the title bar of a form without handle throws <see cref="InvalidOperationException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub RedrawTitleBar_FormWithoutHandle_ThrowsInvalidOperationException()

            Using f As New Form()
                Assert.ThrowsExactly(Of InvalidOperationException)(Sub() WindowsThemeHelper.RedrawTitleBar(f))
            End Using
        End Sub

#End Region

    End Class

End Namespace

#End Region
