# CRC-32 Calculator for Windows Change Log 📋

## v1.0.1 *(current)* 🆕

#### 🚀 New Features:
* **Progress Bar:** Integrated a custom visual progress overlay that displays the real-time checksum calculation percentage for large files.
* **Command-Line & Drop-to-EXE Support:** Added support to automatically load a file path supplied via command-line arguments or by dragging and dropping a file directly onto the compiled executable file in your directory.
* **Checksum Calculation Cancellation:** Added the ability to safely abort active CRC-32 checksum calculation operations on large files by clicking the status action button when the calculation is in progress.

#### 🌟 Improvements:
* **Consecutive File Re-evaluation:** Enhanced the file path field behavior to allow re-checking the exact same file path consecutively when dragged into the window or opened multiple times back-to-back.

#### 🛠️ Fixes:
* **Race Condition Mitigation & Code Refactoring:** Executed deep internal refactoring and implemented workflow-blocking mechanisms to prevent potential asynchronous race conditions and double-triggering events during rapid control interaction.

## v1.0 🆕
Initial Release.