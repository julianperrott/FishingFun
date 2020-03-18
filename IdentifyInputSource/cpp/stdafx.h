// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently, but
// are changed infrequently
//

#pragma once

// Modify the following defines if you have to target a platform prior to the ones specified below.
// Refer to MSDN for the latest info on corresponding values for different platforms.
#ifndef WINVER              // Allow use of features specific to Windows 8.1 or later.
#define WINVER 0x0603       // Change this to the appropriate value to target other versions of Windows.
#endif

#ifndef _WIN32_WINNT        // Allow use of features specific to Windows 8.1 or later.
#define _WIN32_WINNT 0x0603 // Change this to the appropriate value to target other versions of Windows.
#endif

#ifndef _WIN32_WINDOWS        // Allow use of features specific to Windows 8.1 or later.
#define _WIN32_WINDOWS 0x0603 // Change this to the appropriate value to target Windows Me or later.
#endif

#ifndef UNICODE
#define UNICODE
#endif

// Windows Header Files:
#include <windows.h>
#include <comdef.h>
#include <winuser.h>

// C RunTime Header Files
#include <stdlib.h>
#include <math.h>
#include <malloc.h>
#include <memory.h>
#include <tchar.h>
#include <strsafe.h>
