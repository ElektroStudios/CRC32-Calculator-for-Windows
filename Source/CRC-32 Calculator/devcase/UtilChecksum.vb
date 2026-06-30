
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.IO
Imports System.Threading
Imports System.Threading.Tasks

Imports DevCase.Core.IO.FileSystem
Imports DevCase.Core.IO.Memory
Imports DevCase.Win32

#End Region

#Region " UtilChecksum "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Security.DataIntegrity.Checksum

    ''' <summary>
    ''' Provides checksum related utilities.
    ''' </summary>
    Public NotInheritable Class UtilChecksum

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilChecksum"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

#Region " CRC-32 "

        ''' <summary>
        ''' Asynchronously computes a CRC-32 checksum for the specified file and reports completion progress.
        ''' </summary>
        '''
        ''' <example> This is a code example that demonstrates how to report progress for a single file.
        ''' <code language="VB.NET">
        ''' Option Strict On
        ''' Option Explicit On
        ''' Option Infer Off
        ''' 
        ''' Imports DevCase.Core.Security.DataIntegrity.Checksum
        ''' 
        ''' Module Module1
        ''' 
        '''     Public Sub Main()
        ''' 
        '''         Dim targetFilePath As String = "C:\big file 1.mp4"
        ''' 
        '''         Console.WriteLine($"Starting CRC-32 calculation for: {targetFilePath}")
        ''' 
        '''         ' Inline definition of the progress handler to capture and print the percentage.
        '''         Dim progressHandler As New Progress(Of Double)(
        '''             Sub(percentage As Double)
        '''                 ' Use vbCr to return the cursor to the start of the line and overwrite it.
        '''                 Console.Write($"{vbCr}Progress: {percentage:F2}%")
        '''             End Sub)
        ''' 
        '''         Try
        '''             ' Execute the async function on a background thread and block until it completes.
        '''             Dim resultCrc32 As String =
        '''                 Task.Run(Async Function() As Task(Of String)
        '''                              Return Await UtilChecksum.ComputeCRC32OfFileAsync(targetFilePath, progressHandler).ConfigureAwait(False)
        '''                          End Function).GetAwaiter().GetResult()
        ''' 
        '''             ' Break the inline line from the progress prints.
        '''             Console.WriteLine()
        '''             Console.WriteLine($"Calculation completed successfully.")
        '''             Console.WriteLine($"Resulting CRC-32 Checksum: {resultCrc32}")
        ''' 
        '''         Catch ex As Exception
        '''             Console.WriteLine()
        '''             Console.WriteLine($"An error occurred during execution: {ex.Message}")
        '''         End Try
        ''' 
        '''         ' Explicit pause at the end of execution.
        '''         Console.WriteLine()
        '''         Console.WriteLine("Press any key to exit...")
        '''         Console.ReadKey()
        '''     End Sub
        ''' 
        ''' End Module
        ''' </code>
        ''' </example>
        ''' 
        ''' <example> This is a code example that demonstrates how to report progress for multiple files using a single global progress tracker.
        ''' <code language="VB.NET">
        ''' Option Strict On
        ''' Option Explicit On
        ''' Option Infer Off
        ''' 
        ''' Imports System.IO
        ''' 
        ''' Imports DevCase.Core.Security.DataIntegrity.Checksum
        ''' 
        ''' Module Module1
        ''' 
        '''     Public Sub Main()
        ''' 
        '''         ' Define the files to be processed.
        '''         Dim filesToProcess As String() = {
        '''             "C:\big file 1.mp4",
        '''             "C:\big file 2.mp4"
        '''         }
        ''' 
        '''         Console.WriteLine($"Starting global CRC-32 calculation for {filesToProcess.Length} files...")
        ''' 
        '''         ' Calculate the total bytes across all files to enable global progress tracking.
        '''         Dim totalGlobalBytes As Long = 0L
        '''         For Each filePath As String In filesToProcess
        '''             Dim fileInfo As New FileInfo(filePath)
        '''             If fileInfo.Exists Then
        '''                 totalGlobalBytes += fileInfo.Length
        '''             Else
        '''                 Console.WriteLine($"Error: File not found -> {filePath}")
        '''                 Console.WriteLine("Press any key to exit...")
        '''                 Console.ReadKey()
        '''                 Return
        '''             End If
        '''         Next
        ''' 
        '''         ' Inline definition of the global progress handler.
        '''         Dim globalProgressHandler As New Progress(Of Double)(
        '''             Sub(globalPercentage As Double)
        '''                 ' Use vbCr to overwrite the same line in the console.
        '''                 Console.Write($"{vbCr}Global Progress: {globalPercentage:F2}%")
        '''             End Sub)
        ''' 
        '''         Try
        '''             ' Execute the async batch processing on a background thread.
        '''             Dim results As Dictionary(Of String, String) = Task.Run(
        '''                 Async Function() As Task(Of Dictionary(Of String, String))
        '''                     Return Await ProcessMultipleFilesAsync(filesToProcess, totalGlobalBytes, globalProgressHandler).ConfigureAwait(False)
        '''                 End Function).GetAwaiter().GetResult()
        ''' 
        '''             Console.WriteLine()
        '''             Console.WriteLine()
        '''             Console.WriteLine("Calculation completed successfully. Results:")
        ''' 
        '''             For Each kvp As KeyValuePair(Of String, String) In results
        '''                 Dim fileName As String = Path.GetFileName(kvp.Key)
        '''                 Console.WriteLine($"- {fileName}: {kvp.Value}")
        '''             Next
        ''' 
        '''         Catch ex As Exception
        '''             Console.WriteLine()
        '''             Console.WriteLine($"An error occurred during execution: {ex.Message}")
        '''         End Try
        ''' 
        '''         ' Explicit pause at the end of execution.
        '''         Console.WriteLine()
        '''         Console.WriteLine("Press any key to exit...")
        '''         Console.ReadKey()
        '''     End Sub
        ''' 
        '''     ''' &lt;summary&gt;
        '''     ''' Processes multiple files sequentially and scales their individual progress into a unified global progress.
        '''     ''' &lt;/summary&gt;
        '''     Private Async Function ProcessMultipleFilesAsync(filePaths As String(), totalGlobalBytes As Long, globalProgress As IProgress(Of Double)) As Task(Of Dictionary(Of String, String))
        '''         Dim results As New Dictionary(Of String, String)()
        '''         Dim accumulatedBytesProcessed As Long = 0L
        ''' 
        '''         For Each filePath As String In filePaths
        '''             Dim currentFileLength As Long = New FileInfo(filePath).Length
        ''' 
        '''             ' Create a local progress handler that translates the 0-100% of the CURRENT file
        '''             ' into the correct 0-100% of the GLOBAL batch process.
        '''             Dim localProgressHandler As New Progress(Of Double)(
        '''                 Sub(localPercentage As Double)
        '''                     If globalProgress IsNot Nothing Then
        '''                         ' Calculate how many bytes of the current file have been processed based on its local percentage.
        '''                         Dim currentFileBytesRead As Double = (localPercentage / 100.0) * CDbl(currentFileLength)
        ''' 
        '''                         ' Calculate the total global percentage.
        '''                         Dim globalPercentage As Double = ((CDbl(accumulatedBytesProcessed) + currentFileBytesRead) / CDbl(totalGlobalBytes)) * 100.0
        ''' 
        '''                         ' Cap at 100.0 to prevent floating point rounding issues.
        '''                         If globalPercentage > 100.0 Then globalPercentage = 100.0
        ''' 
        '''                         globalProgress.Report(globalPercentage)
        '''                     End If
        '''                 End Sub)
        ''' 
        '''             ' Reuse your existing single-file method, passing the scaling handler.
        '''             Dim crc32 As String = Await UtilChecksum.ComputeCRC32OfFileAsync(filePath, localProgressHandler).ConfigureAwait(False)
        '''             results.Add(filePath, crc32)
        ''' 
        '''             ' Once the file is done, permanently add its size to the accumulated total.
        '''             accumulatedBytesProcessed += currentFileLength
        '''         Next
        ''' 
        '''         Return results
        '''     End Function
        ''' 
        ''' End Module
        ''' </code>
        ''' </example>
        '''
        ''' <param name="filepath">
        ''' The filepath.
        ''' </param>
        ''' 
        ''' <param name="progress">
        ''' The progress provider that receives the completion percentage (0.0 to 100.0).
        ''' </param>
        '''
        ''' <param name="cancellationToken">
        ''' The cancellation token used to abort the asynchronous operation.
        ''' </param>
        ''' 
        ''' <returns>
        ''' An Hexadecimal representation of the resulting CRC-32 checksum.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Async Function ComputeCRC32OfFileAsync(filepath As String, progress As IProgress(Of Double),
                                                             cancellationToken As CancellationToken) As Task(Of String)

            Dim filesize As Long = New FileInfo(filepath).Length
            Dim bufferSize As Integer = UtilStream.GetOptimalFileStreamBufferSize(filesize)
            Dim buffer As Byte() = New Byte(bufferSize - 1) {}
            Dim crc32 As UInteger = 0UI

            Using fs As New FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync:=True)
                Dim totalBytes As Long = fs.Length
                Dim totalBytesRead As Long = 0L
                Dim bytesRead As Integer

                ' Initial progress report (0%)
                progress?.Report(0.0)

                Do
                    cancellationToken.ThrowIfCancellationRequested()

                    ' Asynchronously read the file chunk
                    bytesRead = Await fs.ReadAsync(buffer, 0, bufferSize, cancellationToken).ConfigureAwait(False)

                    If bytesRead > 0 Then
                        ' Update the CRC incrementally using the chunk read.
                        crc32 = NativeMethods.RtlComputeCrc32(crc32, buffer, CUInt(bytesRead))

                        ' Track progress safely
                        totalBytesRead += bytesRead

                        If progress IsNot Nothing AndAlso totalBytes > 0L Then
                            Dim progressReport As Double = (totalBytesRead / totalBytes) * 100.0
                            progress.Report(progressReport)
                        End If
                    End If
                Loop While bytesRead > 0
            End Using

            Return crc32.ToString("X8")
        End Function

        ''' <summary>
        ''' Computes a CRC-32 checksum for the specified byte array.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' Dim data as Byte() = {1, 2, 3, 4, 5}
        ''' Dim crc32 As String = ComputeCRC32OfBytes(data)
        ''' </code>
        ''' </example>
        '''
        ''' <param name="bytes">
        ''' The byte array.
        ''' </param>
        '''
        ''' <returns>
        ''' An Hexadecimal representation of the resulting CRC-32 checksum.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function ComputeCRC32OfBytes(bytes As Byte()) As String

            Const initialValue As UInteger = 0UI
            Dim bufferSize As UInteger = Convert.ToUInt32(bytes.Length)

            Dim crc32 As UInteger = NativeMethods.RtlComputeCrc32(initialValue, bytes, bufferSize)
            Return crc32.ToString("X8")
        End Function

#End Region

#End Region

    End Class

End Namespace

#End Region
