#define WIN32_LEAN_AND_MEAN
#include <windows.h>
#undef WIN32_LEAN_AND_MEAN
#include <stdio.h>

BOOL APIENTRY DllMain (HINSTANCE hInst, DWORD reason, LPVOID reserved)
{
	return TRUE;
}


BOOL APIENTRY DllMainCRTStartup (HINSTANCE hInst, DWORD reason, LPVOID reserved)
{
	return TRUE;
}

