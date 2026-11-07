
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

#Region " Win32LibNames "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32

    ''' <summary>
    ''' Provides the filenames to specify in <see cref="DllImportAttribute.Value"/> for all referenced Win32 functions.
    ''' </summary>
    Friend Module Win32LibNames

        ' ReSharper disable InconsistentNaming

        ''' <summary>
        ''' DwmApi.dll
        ''' </summary>
        Friend Const DwmApi As String = "dwmapi.dll"

        ''' <summary>
        ''' NtDll.dll
        ''' </summary>
        Friend Const NtDll As String = "ntdll.dll"

        ''' <summary>
        ''' ShlwApi.dll
        ''' </summary>
        Friend Const ShlwApi As String = "shlwapi.dll"

        ''' <summary>
        ''' User32.dll
        ''' </summary>
        Friend Const User32 As String = "user32.dll"

    End Module

End Namespace

#End Region
