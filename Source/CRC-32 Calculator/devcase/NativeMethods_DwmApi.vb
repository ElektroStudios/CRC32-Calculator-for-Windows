
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security

Imports DevCase.Win32.Enums

#End Region

#Region " NativeMethods "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' DwmApi.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    Public Module DwmApi

        ' ReSharper disable VBWarnings::BC42309

#Region " DwmApi.dll "

        ''' <summary>
        ''' Sets the value of Desktop Window Manager (DWM) non-client rendering attributes for a window.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/dwmapi/nf-dwmapi-dwmsetwindowattribute"/>
        ''' </remarks>
        '''
        ''' <param name="hwnd">
        ''' The handle to the window for which the attribute value is to be set.
        ''' </param>
        '''
        ''' <param name="attribute">
        ''' The attribute to set.
        ''' </param>
        '''
        ''' <param name="attributeValue">
        ''' The value of the attribute (a Win32 <c>BOOL</c> for the dark mode attributes).
        ''' </param>
        '''
        ''' <param name="attributeSize">
        ''' The size, in bytes, of <paramref name="attributeValue"/>.
        ''' </param>
        '''
        ''' <returns>
        ''' If this function succeeds, it returns HResult.S_OK.
        ''' Otherwise, it returns an HResult error code.
        ''' </returns>
        <DllImport(Win32LibNames.DwmApi, SetLastError:=False, ExactSpelling:=True)>
        Public Function DwmSetWindowAttribute(hwnd As IntPtr,
                                              attribute As DwmWindowAttribute,
                                              ByRef attributeValue As Integer,
                                              attributeSize As Integer
        ) As Integer ' HResult
        End Function

#End Region

    End Module

End Namespace

#End Region
