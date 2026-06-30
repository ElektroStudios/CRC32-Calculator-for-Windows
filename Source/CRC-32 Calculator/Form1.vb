
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Imports DevCase.Core.Security.DataIntegrity.Checksum

#End Region

''' <summary>
''' Represents the main form of the CRC32 Calculator application.
''' <para></para>
''' Provides user interface initialization, drag-and-drop support, and asynchronous CRC-32 checksum calculation.
''' </summary>
Public NotInheritable Class Form1 : Inherits Form

#Region " Fields "

    ''' <summary>
    ''' Represents the custom button control embedded within the status strip container 
    ''' that handles event notifications dynamically.
    ''' </summary>
    Private WithEvents StatusToolsTripButton As ToolStripButton

    ''' <summary>
    ''' Status label control used to display additional progressive text updates.
    ''' </summary>
    Private WithEvents ProgressTextLabel As ToolStripStatusLabel

    ''' <summary>
    ''' Handles progress reporting from asynchronous operations, updating the progress bar value and custom text.
    ''' </summary>
    Private ReadOnly ProgressHandler As New Progress(Of Double)(
        Sub(percentage As Double)
            Dim currentInteger As Integer = CInt(percentage)

            ' Only write to the UI if the integer value has actually changed.
            If currentInteger <> Me.lastWrittenPercentageInteger Then
                Me.lastWrittenPercentageInteger = currentInteger

                Me.TextToolStripProgressBar1.Value = currentInteger
                Me.TextToolStripProgressBar1.CustomText = $"{currentInteger}%"
            End If
        End Sub)

    ''' <summary>
    ''' Stores the last integer percentage written to the UI progressbar to avoid redundant redraws.
    ''' </summary>
    Private lastWrittenPercentageInteger As Integer = -1

    ''' <summary>
    ''' Signals to a <see cref="CancellationToken"/> that current file checksum calculation should be canceled.
    ''' </summary>
    Private CancellationSource As CancellationTokenSource = Nothing

    ''' <summary>
    ''' Flag used to suspend the asynchronous TextChanged handler execution while resetting the control programmatically.
    ''' </summary>
    Private isUpdatingTextBoxPath As Boolean = False

#End Region

#Region " Enums "

    ''' <summary>
    ''' Defines tracking states for the status strip button to manage asynchronous execution workflows.
    ''' </summary>
    Private Enum StatusButtonTags

        ''' <summary>
        ''' Indicates the application is idle.
        ''' </summary>
        [Default]

        ''' <summary>
        ''' Indicates an asynchronous file operation is currently running and can be aborted.
        ''' </summary>
        WorkInProgress

        ''' <summary>
        ''' Indicates an asynchronous file operation has succeeded.
        ''' </summary>
        Success

        ''' <summary>
        ''' Indicates an asynchronous file operation has been aborted.
        ''' </summary>
        Canceled

    End Enum

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Handles the Load event of the Form1 control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' An <see cref="EventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Me.Text = AppGlobals.AppTitle

        Me.StatusToolsTripButton = New ToolStripButton With {
            .Name = "ToolStripButtonAction",
            .Text = "",
            .Tag = StatusButtonTags.Default,
            .Image = My.Resources.Resource1._default,
            .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText
        }
        Me.StatusStrip1.Items.Insert(1, Me.StatusToolsTripButton) ' Insert after the label with Spring = True

        ' Adjust Right padding to let the progressbar eat the blank space of the disabled sizing grip (14 pixels).
        Dim currentPadding As Padding = Me.StatusStrip1.Padding
        Dim newRightValue As Integer = currentPadding.Left
        Me.StatusStrip1.Padding = New Padding(currentPadding.Left, currentPadding.Top, newRightValue, currentPadding.Bottom)
    End Sub

    ''' <summary>
    ''' Handles the Shown event of the Form1 control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' An <see cref="EventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepperBoundary>
    Private Sub Form1_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown

        ' Environment.GetCommandLineArgs(0) is always the app path. 
        ' Index 1 represents the target file passed via command-line or dragged onto the executable.
        Dim args As String() = Environment.GetCommandLineArgs()

        If args IsNot Nothing AndAlso args.Length > 1 Then
            Dim startupFilePath As String = args(1)
            If Not String.IsNullOrWhiteSpace(startupFilePath) Then

                Dim extendedPath As String = PathHelper.GetExtendedPath(startupFilePath)

                ' Verify that it is a single valid file before running the trigger
                If File.Exists(extendedPath) Then
                    Me.SetTextBoxFilePath(startupFilePath)
                Else
                    MessageBox.Show(Me, $"ERROR: File not found: {startupFilePath}", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the DragEnter event of the Form1 control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' A <see cref="DragEventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub Form1_DragEnter(sender As Object, e As DragEventArgs) Handles MyBase.DragEnter

        Dim btn As ToolStripButton = Me.StatusToolsTripButton

        If TypeOf btn.Tag Is StatusButtonTags Then
            Dim currentStatus As StatusButtonTags = CType(btn.Tag, StatusButtonTags)

            ' ADDED FOR WORKFLOW PROTECTION: Block drag operations if a checksum is currently in progress.
            If currentStatus = StatusButtonTags.WorkInProgress Then
                e.Effect = DragDropEffects.None
                Exit Sub
            End If

            Dim isFile As Boolean = e.Data.GetDataPresent(DataFormats.FileDrop)

            If isFile Then
                Dim filePaths As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())

                If filePaths IsNot Nothing AndAlso filePaths.Length = 1 Then
                    Dim rawPath As String = filePaths(0)
                    Dim extendedPath As String = PathHelper.GetExtendedPath(rawPath)

                    If File.Exists(extendedPath) Then
                        e.Effect = DragDropEffects.Copy
                        Exit Sub
                    End If
                End If
            End If
        End If

        e.Effect = DragDropEffects.None
    End Sub

    ''' <summary>
    ''' Handles the DragDrop event of the Form1 control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' A <see cref="DragEventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub Form1_DragDrop(sender As Object, e As DragEventArgs) Handles MyBase.DragDrop

        Dim btn As ToolStripButton = Me.StatusToolsTripButton

        If TypeOf btn.Tag Is StatusButtonTags Then
            Dim currentStatus As StatusButtonTags = CType(btn.Tag, StatusButtonTags)

            ' ADDED FOR WORKFLOW PROTECTION: Guard drop execution against race conditions during processing.
            If currentStatus = StatusButtonTags.WorkInProgress Then
                Exit Sub
            End If
        End If

        Dim filePaths As String() = DirectCast(e.Data.GetData(DataFormats.FileDrop), String())
        Dim selectedPath As String = filePaths.Single()

        Me.SetTextBoxFilePath(selectedPath)
    End Sub

    ''' <summary>
    ''' Handles the Click event of the ButtonBrowse control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' A <see cref="EventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ButtonBrowse_Click(sender As Object, e As EventArgs) Handles ButtonBrowse.Click

        If Me.OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            Me.SetTextBoxFilePath(Me.OpenFileDialog1.FileName)
        End If
    End Sub

    ''' <summary>
    ''' Handles the Click event of the ButtonCopy control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' A <see cref="EventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ButtonCopy_Click(sender As Object, e As EventArgs) Handles ButtonCopy.Click

        Clipboard.SetText(Me.TextBoxCrc.Text)

        Me.ToolTip1.Hide(Me.ButtonCopy)
        Me.ToolTip1.Show("Checksum copied to clipboard.", Me.ButtonCopy, 0, +Me.ButtonCopy.Height, 1500)
    End Sub

    ''' <summary>
    ''' Handles the Click event of the ButtonClear control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' A <see cref="EventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ButtonClear_Click(sender As Object, e As EventArgs) Handles ButtonClear.Click

        Me.TextBoxFile.Clear()
        Me.StatusToolsTripButton.Image = My.Resources.Resource1._default
        Me.StatusToolsTripButton.Tag = StatusButtonTags.Default
        Me.ButtonBrowse.Focus()

        Me.ToolTip1.Hide(Me.TextBoxFile)
        Me.ToolTip1.Show("Values cleared.", Me.TextBoxFile, 0, 0, 1500)
    End Sub

    ''' <summary>
    ''' Handles the LinkClicked event of the LinkLabelGithub control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' A <see cref="LinkLabelLinkClickedEventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub LinkLabelGithub_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles LinkLabelGithub.LinkClicked

        Dim msgResult As DialogResult =
            MessageBox.Show(Me, $"Do you want to open the project's GitHub page in your default web browser?",
                            My.Application.Info.Title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2)

        If msgResult = DialogResult.Yes Then

            Try
                Process.Start(AppGlobals.GitHubRepositoryUrl)

            Catch ex As Exception
                MessageBox.Show(Me, $"ERROR: {ex.Message}", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)

            End Try
        End If
    End Sub

    ''' <summary>
    ''' Handles the Click event of the custom status strip button control.
    ''' </summary>
    ''' 
    ''' <param name="sender">The source of the event, expected to be a <see cref="ToolStripButton"/>.</param>
    ''' <param name="e">An <see cref="EventArgs"/> object containing event data.</param>
    <DebuggerStepThrough>
    Private Sub StatusToolsTripButton_Click(sender As Object, e As EventArgs) Handles StatusToolsTripButton.Click

        Dim btn As ToolStripButton = DirectCast(sender, ToolStripButton)

        If TypeOf btn.Tag Is StatusButtonTags Then

            If CType(btn.Tag, StatusButtonTags) = StatusButtonTags.Default Then
                ' Build and display an "About Box" message.
                Dim processBitness As String = If(Environment.Is64BitProcess, "64-bit", "32-bit")
                Dim aboutMessage As String =
                    $"{My.Application.Info.Title} Version {My.Application.Info.Version.ToString(fieldCount:=3)} ({processBitness} mode){Environment.NewLine}{Environment.NewLine}" &
                    $"{My.Application.Info.Description}{Environment.NewLine}{Environment.NewLine}" &
                    $"{My.Application.Info.Copyright}{Environment.NewLine}{Environment.NewLine}"

                MessageBox.Show(Me, aboutMessage, $"About {My.Application.Info.Title}", MessageBoxButtons.OK, MessageBoxIcon.Information)

            ElseIf CType(btn.Tag, StatusButtonTags) = StatusButtonTags.WorkInProgress Then
                If Me.CancellationSource IsNot Nothing Then
                    btn.Enabled = False ' Disable button to prevent multi-clicks during aborting phase.
                    Me.CancellationSource.Cancel()
                End If

            End If
        End If

    End Sub

    ''' <summary>
    ''' Handles the TextChanged event of the TextBoxFile control.
    ''' </summary>
    ''' 
    ''' <param name="sender">
    ''' The source of the event.
    ''' </param>
    ''' 
    ''' <param name="e">
    ''' A <see cref="EventArgs"/> containing the event data.
    ''' </param>
    <DebuggerStepperBoundary>
    Private Async Sub TextBoxFile_TextChanged(sender As Object, e As EventArgs) Handles TextBoxFile.TextChanged

        If Me.isUpdatingTextBoxPath Then
            Exit Sub
        End If

        Dim tb As TextBox = DirectCast(sender, TextBox)

        Dim filepath As String = tb.Text

        ' Disable UI controls.
        Me.ButtonBrowse.Enabled = False
        Me.ButtonCopy.Enabled = False
        Me.ButtonClear.Enabled = False
        Me.TextBoxFile.Enabled = False
        Me.TextBoxCrc.Enabled = False
        ' Reset specific controls to its defaults.
        Me.TextBoxCrc.Clear()
        Me.TextToolStripProgressBar1.Value = Me.TextToolStripProgressBar1.Minimum
        Me.TextToolStripProgressBar1.CustomText = "0%"
        Me.ToolStripStatusLabel1.Text = "Waiting..."

        If Not String.IsNullOrWhiteSpace(filepath) Then
            Me.UseWaitCursor = True

            Me.CancellationSource = New CancellationTokenSource()

            Try
                Me.StatusToolsTripButton.Image = My.Resources.Resource1.prohibition
                Me.StatusToolsTripButton.Tag = StatusButtonTags.WorkInProgress
                Me.StatusToolsTripButton.Text = "Cancel"

                Dim isSuccess As Boolean =
                    Await Me.ProcessFileAsync(filepath, Me.CancellationSource.Token).ConfigureAwait(continueOnCapturedContext:=True)

                If isSuccess AndAlso Not Me.CancellationSource?.IsCancellationRequested Then
                    Me.StatusToolsTripButton.Image = My.Resources.Resource1.check
                    Me.StatusToolsTripButton.Tag = StatusButtonTags.Success
                    Me.StatusToolsTripButton.Text = ""

                    Me.TextBoxCrc.Enabled = True
                    Me.ButtonCopy.Enabled = True

                    Me.Focus() ' Reclaim form focus to guarantee the ToolTip balloon is shown when the window loses foreground.
                    Me.ToolTip1.Hide(Me.StatusStrip1)
                    Me.ToolTip1.Show("File loaded.", Me.StatusStrip1, Me.TextToolStripProgressBar1.Bounds.Left - Me.TextToolStripProgressBar1.Width * 2, 0, 1500)
                End If

            Catch ex As OperationCanceledException
                Me.TextBoxCrc.Text = "CANCELED"
                Me.StatusToolsTripButton.Image = My.Resources.Resource1.warning
                Me.StatusToolsTripButton.Tag = StatusButtonTags.Canceled
                Me.StatusToolsTripButton.Text = ""
                Me.ToolTip1.Hide(Me.StatusStrip1)
                Me.ToolTip1.Show("CANCELED.", Me.StatusStrip1, Me.TextToolStripProgressBar1.Bounds.Left - Me.TextToolStripProgressBar1.Width * 2, 0, 1500)

            Finally
                If Me.CancellationSource IsNot Nothing Then
                    Me.CancellationSource.Dispose()
                    Me.CancellationSource = Nothing
                End If

                Me.StatusToolsTripButton.Enabled = True
                Me.UseWaitCursor = False
                Me.TextBoxFile.Enabled = True
                Me.ButtonClear.Enabled = True
            End Try

        Else
            Me.StatusToolsTripButton.Image = My.Resources.Resource1._default
            Me.StatusToolsTripButton.Tag = StatusButtonTags.Default
            Me.StatusToolsTripButton.Text = ""
            tb.Enabled = False

        End If

        Me.ButtonBrowse.Enabled = True
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Updates the text property of the target file path textbox, updates text selection pointers and assigns primary focus.
    ''' </summary>
    ''' 
    ''' <param name="filePath">
    ''' The absolute or relative file string path value to apply.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub SetTextBoxFilePath(filePath As String)

        ' Suspend the event execution during the Clear() phase to avoid double async triggering.
        Me.isUpdatingTextBoxPath = True

        ' Clear the current text to force a re-evaluation if the same file has been modified or replaced.
        Me.TextBoxFile.Clear()

        ' Re-enable the event trigger for the actual path assignment.
        Me.isUpdatingTextBoxPath = False
        Me.TextBoxFile.Text = filePath

        Me.TextBoxFile.SelectionStart = Me.TextBoxFile.Text.Length
        Me.TextBoxFile.SelectionLength = 0
        Me.TextBoxFile.Focus()
    End Sub

    ''' <summary>
    ''' Performs the asynchronous CRC-32 calculation and retrieves metadata information for the specified file path.
    ''' </summary>
    ''' 
    ''' <param name="filePath">
    ''' The target raw file system path string to process.
    ''' </param>
    ''' 
    ''' <param name="cancellationToken">
    ''' The cancellation token used to abort the asynchronous operation.
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="Task(Of Boolean)"/> that yields <see langword="True"/> if the operation completes successfully; 
    ''' otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepperBoundary>
    Private Async Function ProcessFileAsync(filePath As String, cancellationToken As CancellationToken) As Task(Of Boolean)

        Dim extendedFilePath As String = PathHelper.GetExtendedPath(filePath)

        Try
            Dim fileInfoString As String = FileInfoHelper.BuildFileInfoString(extendedFilePath)
            Me.ToolStripStatusLabel1.Text = fileInfoString

            Dim resultCrc32 As String =
                Await UtilChecksum.ComputeCRC32OfFileAsync(extendedFilePath, Me.ProgressHandler, cancellationToken).
                                   ConfigureAwait(continueOnCapturedContext:=True)

            Me.TextBoxCrc.Text = resultCrc32
            Return True

        Catch ex As Exception When Not (TypeOf ex Is OperationCanceledException OrElse TypeOf ex.InnerException Is OperationCanceledException)
            Me.BeginInvoke(
                Sub()
                    Me.TextBoxCrc.Text = "ERROR"
                    MessageBox.Show(Me, $"ERROR: {ex.Message.Replace(extendedFilePath, filePath)}", My.Application.Info.Title, MessageBoxButtons.OK, MessageBoxIcon.Error)
                End Sub)
            Return False

        End Try
    End Function

#End Region

End Class
