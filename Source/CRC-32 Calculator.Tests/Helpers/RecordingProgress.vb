#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic

#End Region

#Region " RecordingProgress "

Namespace Crc32Calculator.Tests.Helpers

    ''' <summary>
    ''' A synchronous <see cref="IProgress(Of T)"/> implementation that records every reported value.
    ''' </summary>
    '''
    ''' <remarks>
    ''' Unlike <see cref="Progress(Of T)"/>, this implementation invokes the callback synchronously on the reporting thread,
    ''' which makes the order and the number of reports fully deterministic in unit tests.
    ''' </remarks>
    '''
    ''' <typeparam name="T">
    ''' The type of the progress values.
    ''' </typeparam>
    Friend NotInheritable Class RecordingProgress(Of T) : Implements IProgress(Of T)

#Region " Fields "

        ''' <summary>
        ''' The synchronization object that protects <see cref="RecordingProgress(Of T).reportedValues"/>.
        ''' </summary>
        Private ReadOnly syncRoot As New Object()

        ''' <summary>
        ''' The list of reported values, in reporting order.
        ''' </summary>
        Private ReadOnly reportedValues As New List(Of T)()

        ''' <summary>
        ''' An optional callback that is synchronously invoked after recording each reported value.
        ''' </summary>
        Private ReadOnly onReport As Action(Of T)

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets a snapshot of the reported values, in reporting order.
        ''' </summary>
        Friend ReadOnly Property Values As IReadOnlyList(Of T)
            Get
                SyncLock Me.syncRoot
                    Return Me.reportedValues.ToArray()
                End SyncLock
            End Get
        End Property

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RecordingProgress(Of T)"/> class.
        ''' </summary>
        Friend Sub New()
            Me.New(Nothing)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="RecordingProgress(Of T)"/> class.
        ''' </summary>
        '''
        ''' <param name="onReport">
        ''' An optional callback that is synchronously invoked after recording each reported value.
        ''' </param>
        Friend Sub New(onReport As Action(Of T))
            Me.onReport = onReport
        End Sub

#End Region

#Region " IProgress Implementation "

        ''' <summary>
        ''' Records the specified progress value and invokes the optional callback.
        ''' </summary>
        '''
        ''' <param name="value">
        ''' The reported progress value.
        ''' </param>
        Public Sub Report(value As T) Implements IProgress(Of T).Report

            SyncLock Me.syncRoot
                Me.reportedValues.Add(value)
            End SyncLock

            Me.onReport?.Invoke(value)
        End Sub

#End Region

    End Class

End Namespace

#End Region
