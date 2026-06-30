
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text

Imports DevCase.Win32.Enums

#End Region

#Region " NativeMethods "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' ShlwApi.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    Public Module ShlwApi

        ' ReSharper disable VBWarnings::BC42309

#Region " ShlwApi.dll "

        ''' <summary>
        ''' Converts a numeric value into a string that represents the number expressed as a size value in bytes,
        ''' kilobytes, megabytes, gigabytes, petabytes or exabytes, depending on the size.
        ''' <para></para>
        ''' Extends StrFormatByteSizeW by offering the option to round to the nearest displayed digit or to discard undisplayed digits.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/api/shlwapi/nf-shlwapi-strformatbytesizeex"/>
        ''' </remarks>
        '''
        ''' <param name="number">
        ''' The numeric value to be converted.
        ''' </param>
        '''
        ''' <param name="flags">
        ''' Specifies whether to round or truncate undisplayed digits.
        ''' </param>
        '''
        ''' <param name="buffer">
        ''' A buffer that, when this function returns successfully, receives the converted number.
        ''' </param>
        '''
        ''' <param name="bufferSize">
        ''' The size of <paramref name="buffer"/>, in characters.
        ''' </param>
        '''
        ''' <returns>
        ''' If this function succeeds, it returns HResult.S_OK.
        ''' Otherwise, it returns an HResult error code.
        ''' </returns>
        <DllImport(Win32LibNames.ShlwApi, SetLastError:=False, ExactSpelling:=True, CharSet:=CharSet.Unicode)>
        Public Function StrFormatByteSizeEx(number As ULong,
                                            flags As StrFormatByteSizeFlags,
                                            buffer As StringBuilder,
                                            bufferSize As UInteger
        ) As Integer ' HResult
        End Function

#End Region

    End Module

End Namespace

#End Region
