
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

Imports DevCase.Core.IO.Memory

#End Region

#Region " Util Stream "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.IO.FileSystem

    ''' <summary>
    ''' Provides <see cref="System.IO.Stream"/> related utilities.
    ''' </summary>
    Public NotInheritable Class UtilStream

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilStream"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Given a filesize, determine the optimal buffer size to boost the performance of a <see cref="System.IO.FileStream"/> operation.
        ''' </summary>
        '''
        ''' <param name="filesize">
        ''' The size, in bytes, of the data to be read or write by a <see cref="System.IO.FileStream"/>.
        ''' </param>
        '''
        ''' <returns>
        ''' The returned value is the proper buffer size, in bytes, 
        ''' that should be set as <c>BufferSize</c> parameter when instancing a <see cref="System.IO.FileStream"/>.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function GetOptimalFileStreamBufferSize(filesize As Long) As BufferSizes

            Select Case filesize

                Case Is > 1073741824L ' Greater than 1 GB.
                    Return BufferSizes.Mb1

                Case Is > 524288000L ' Greater than 500 MB.
                    Return BufferSizes.Kb512

                Case Is > 314572800L ' Greater than 300 MB.
                    Return BufferSizes.Kb256

                Case Is > 209715200L ' Greater than 200 MB.
                    Return BufferSizes.Kb128

                Case Is > 104857600L ' Greater than 100 MB.
                    Return BufferSizes.Kb64

                Case Is > 52428800L ' Greater than 50 MB.
                    Return BufferSizes.Kb32

                Case Is > 10485760L ' Greater than 10 MB.
                    Return BufferSizes.Kb16

                Case Is > 1048576L ' Greater than 1 MB.
                    Return BufferSizes.Kb8

                Case Else ' Less or Equal than 1 MB, or zero.
                    Return BufferSizes.Kb4

            End Select

        End Function
#End Region

    End Class

End Namespace

#End Region
