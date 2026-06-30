
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

''' <summary>
''' Provides globally accessible constants and shared variables for the application lifecycle.
''' </summary>
Friend Module AppGlobals

    ''' <summary>
    ''' The absolute URL targeting the official remote GitHub source code repository for this project.
    ''' </summary>
    Friend Const GitHubRepositoryUrl As String = "https://github.com/ElektroStudios/CRC32-Calculator-for-Windows"

    ''' <summary>
    ''' The standardized application title string for this project.
    ''' </summary>
    Friend ReadOnly AppTitle As String =
        $"{My.Application.Info.ProductName} {My.Application.Info.Version.ToString(fieldCount:=3)} — ElektroStudios"

End Module