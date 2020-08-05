# ConsoleHub

## Introduction
A tool to help you manage your console applications.  
![Screenshot](https://raw.githubusercontent.com/1354092549/ConsoleHub/master/Screenshot.png)  

## Installation
Just go to the release page and download the binary files

## Config Auto Run
You can create a file named ConsoleHubAutoRun.txt (Encoding: UTF-8) in the same directory as that of ConsoleHub.exe with commands line by line, then ConsoleHub will execute them automatically when starting.  
Special Commands (case insensitive):   
- \*delay [milliseconds]

## Run File Format
Example:
```
cmd /k echo a
*wait 1200
cmd /k echo b
cmd /k echo c
```

## Start to tray
Use the command line argument `--tray` (or `/tray`, `-tray`)