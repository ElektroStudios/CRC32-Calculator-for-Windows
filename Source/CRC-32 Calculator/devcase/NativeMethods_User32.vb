
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security

#End Region

#Region " NativeMethods "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' User32.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    Public Module User32

        ' ReSharper disable VBWarnings::BC42309

#Region " User32.dll "

        ''' <summary>
        ''' Sends the specified message to a window or windows.
        ''' <para></para>
        ''' The function calls the window procedure for the specified window and does not return
        ''' until the window procedure has processed the message.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-sendmessagew"/>
        ''' </remarks>
        '''
        ''' <param name="hWnd">
        ''' A handle to the window whose window procedure will receive the message.
        ''' </param>
        '''
        ''' <param name="msg">
        ''' The message to be sent.
        ''' </param>
        '''
        ''' <param name="wParam">
        ''' Additional message-specific information.
        ''' </param>
        '''
        ''' <param name="lParam">
        ''' Additional message-specific information.
        ''' </param>
        '''
        ''' <returns>
        ''' The result of the message processing; it depends on the message sent.
        ''' </returns>
        <DllImport(Win32LibNames.User32, EntryPoint:="SendMessageW", SetLastError:=False, ExactSpelling:=True)>
        Public Function SendMessage(hWnd As IntPtr,
                                    msg As Integer,
                                    wParam As IntPtr,
                                    lParam As IntPtr
        ) As IntPtr
        End Function

#End Region

    End Module

End Namespace

#End Region
